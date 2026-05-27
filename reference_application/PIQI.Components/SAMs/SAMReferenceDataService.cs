using PIQI.Components.Models;
using System.Net;
using System.Text.Json;

namespace PIQI.Components.Services
{
    /// <summary>
    /// Service for handling reference data lookups and FHIR server interactions
    /// used during SAM evaluation.
    /// </summary>
    public class SAMService
    {
        #region Properties

        /// <summary>
        /// Provides access to the <see cref="IFHIRClientProvider"/> service
        /// used for performing FHIR resource lookups.
        /// </summary>
        protected readonly IFHIRClientProvider _fhirClientProvider;

        /// <summary>
        /// Gets the <see cref="HttpClient"/> instance used to make HTTP requests to the FHIR server.
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Gets or sets the reference data used for PIQI processing.
        /// </summary>
        /// <value>
        /// A <see cref="PIQIMessage"/> instance containing lookup values, code systems,
        /// and other contextual information needed for SAM evaluation; or <c>null</c> if not assigned.
        /// </value>
        public PIQIMessage? Message { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SAMService"/> class
        /// with the specified <see cref="IFHIRClientProvider"/>.
        /// </summary>
        /// <param name="fhirClientProvider">
        /// An implementation of <see cref="IFHIRClientProvider"/> used to make FHIR API calls.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fhirClientProvider"/> is <c>null</c>.
        /// </exception>
        public SAMService(IFHIRClientProvider fhirClientProvider, HttpClient client)
        {
            _fhirClientProvider = fhirClientProvider ?? throw new ArgumentNullException(nameof(fhirClientProvider));
            Client = client;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the <see cref="Coding.IsInteroperable"/> property for each <see cref="Coding"/>
        /// in the given list based on the specified interoperability code systems.
        /// </summary>
        /// <param name="codingList">
        /// The list of <see cref="Coding"/> objects to update. Each coding must have
        /// <see cref="Coding.RecognizedCodeSystem"/> set to be checked for interoperability.
        /// </param>
        /// <param name="systemsList">
        /// The list of code system identifiers considered interoperable.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Message.RefData"/> is <c>null</c> or invalid.
        /// </exception>
        public void UpdateInteroperability(List<Coding> codingList, List<string> systemsList)
        {
            try
            {
                if (Message?.RefData == null)
                    throw new InvalidOperationException("Missing or invalid reference data for interoperability check.");

                // Update all codings against the list
                foreach (Coding coding in codingList)
                {
                    coding.IsInteroperable = coding.RecognizedCodeSystem != null ?
                        systemsList.Where(s => Message.RefData.GetCodeSystem(s) == Message.RefData.GetCodeSystem(coding.RecognizedCodeSystem)).Any()
                        : false;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Populates the <see cref="Coding.ReferenceDisplayList"/> for each <see cref="Coding"/> 
        /// in the given <see cref="CodeableConcept"/> using the FHIR $lookup operation
        /// if it has not already been called.
        /// </summary>
        /// <param name="codeableConcept">
        /// The <see cref="CodeableConcept"/> containing a list of <see cref="Coding"/> objects 
        /// for which the reference display values will be populated.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. 
        /// The method completes when all codings have their reference display lists populated.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <see cref="Message.RefData"/> property is <c>null</c> or invalid.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if the FHIR $lookup call returns an unexpected status code other than <see cref="HttpStatusCode.BadRequest"/>.
        /// </exception>
        public async Task LookupCodeAsync(CodeableConcept codeableConcept)
        {
            try
            {
                if (Message?.RefData == null)
                    throw new InvalidOperationException("Missing or invalid reference data for FHIR server check.");

                // Check each coding code against their code system lists
                foreach (Coding coding in codeableConcept.CodingList)
                {
                    // Get all code system list identifiers based on the coding's recognized code system
                    var codeSystem = coding.HasRecognizedCodeSystem? Message.RefData.GetCodeSystem(coding.RecognizedCodeSystem) : null;

                    if (codeSystem?.FhirUri != null)
                    {
                        var response = await _fhirClientProvider.LookupCodeAsync(coding.CodeValue, codeSystem.FhirUri);

                        if (response.IsSuccessStatusCode)
                        {
                            coding.LookupResponse = response;
                            coding.IsValid = true;
                            coding.SetReferenceDisplayList();
                            coding.SetStatus();
                        }
                        else if (response.StatusCode != HttpStatusCode.BadRequest)
                        {
                            throw new Exception($"Unexpected status code from FHIR client provider: {response.StatusCode}");
                        }
                    }
                    else
                    {
                        var codeSystemList = codeSystem?.CodeSystemIdentifiers ?? [];
                        foreach (string codeSystemIdentifier in codeSystemList)
                        {
                            // Make API request to check code against code system
                            var response = await _fhirClientProvider.LookupCodeAsync(coding.CodeValue, codeSystemIdentifier);

                            if (response.IsSuccessStatusCode)
                            {
                                coding.LookupResponse = response;
                                coding.IsValid = true;
                                coding.SetReferenceDisplayList();
                                coding.SetStatus();
                                break;
                            }
                            else if (response.StatusCode != HttpStatusCode.BadRequest)
                            {
                                throw new Exception($"Unexpected status code from FHIR client provider: {response.StatusCode}");
                            }
                        }
                    }
                }

                codeableConcept.FHIRServerCalled = true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="ValueSet"/> by its mnemonic or FHIR URI. 
        /// </summary>
        /// <param name="valueSetMnemonic">The mnemonic identifier of the value set to retrieve.</param>
        /// <returns>
        /// A <see cref="Task{ValueSet}"/> representing the asynchronous operation.
        /// The task result contains the <see cref="ValueSet"/> object, including its codings if retrieved from a FHIR server.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the FHIR client provider returns an unsuccessful HTTP status code or if the value set cannot be retrieved.
        /// </exception>
        public async Task<ValueSet> GetValueSetAsync(string valueSetMnemonic)
        {
            try
            {
                // Get the value set from the mnemonic
                var valueSet = Message.RefData.GetValueSet(valueSetMnemonic);
                if (valueSet == null) valueSet = new ValueSet();

                HttpResponseMessage response = null;

                if (valueSet?.FhirUri != null)
                    response = await _fhirClientProvider.GetValueSetAsync(valueSet.FhirUri);
                else
                    response = await _fhirClientProvider.GetValueSetAsync(valueSetMnemonic);

                if (response == null || !response.IsSuccessStatusCode)
                    throw new Exception($"Unexpected status code from FHIR client provider: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(content);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("expansion", out JsonElement expansion))
                {
                    if (expansion.TryGetProperty("contains", out JsonElement codeList) && codeList.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var coding in codeList.EnumerateArray())
                            valueSet.CodingList.Add(new Coding(coding));
                    }
                }

                return valueSet;
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
