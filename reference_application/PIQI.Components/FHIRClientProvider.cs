namespace PIQI.Components.Services
{
    /// <summary>
    /// Contract for a provider that supplies a configured FHIR <see cref="HttpClient"/>.
    /// </summary>
    public interface IFHIRClientProvider
    {
        /// <summary>
        /// Gets the configured <see cref="HttpClient"/> instance for FHIR server communication.
        /// </summary>
        HttpClient Client { get; }

        /// <summary>
        /// Performs a FHIR CodeSystem $lookup operation for a given code and code system.
        /// </summary>
        /// <param name="code">The code value to look up.</param>
        /// <param name="system">The code system URI.</param>
        /// <param name="properties">Optional properties to include in the lookup.</param>
        /// <returns>The HTTP response from the FHIR server.</returns>
        Task<HttpResponseMessage> LookupCodeAsync(string code, string system, params string[] properties);

        /// <summary>
        /// Asynchronously retrieves a <see cref="ValueSet"/> by its mnemonic identifier.
        /// </summary>
        /// <param name="valueSetMnemonic">The mnemonic of the value set to retrieve.</param>
        /// <returns>
        /// A <see cref="Task{HttpResponseMessage}"/> representing the asynchronous operation. 
        /// The task result contains the HTTP response from the service, which should include the requested value set.
        /// </returns>
        Task<HttpResponseMessage> GetValueSetAsync(string valueSetMnemonic);

    }

    /// <summary>
    /// Provides an HTTP client configured for interacting with a FHIR server.
    /// </summary>
    public class FHIRClientProvider : IFHIRClientProvider
    {
        /// <summary>
        /// Gets the configured <see cref="HttpClient"/> instance for FHIR interactions.
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FHIRClientProvider"/> class.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> instance provided by dependency injection.</param>
        public FHIRClientProvider(HttpClient client)
        {
            Client = client;
        }

        /// <summary>
        /// Asynchronously looks up a code in a FHIR CodeSystem and retrieves specified properties.
        /// </summary>
        /// <param name="code">The code to look up.</param>
        /// <param name="system">The URI of the code system that contains the code.</param>
        /// <param name="properties">
        /// Optional list of properties to retrieve for the code, such as "display", "designations", or "status".
        /// If none are specified, defaults to "display", "designations", and "status".
        /// </param>
        /// <returns>
        /// A <see cref="Task{HttpResponseMessage}"/> representing the asynchronous operation.
        /// The task result contains the HTTP response from the FHIR server with the code information.
        /// </returns>
        /// <exception cref="Exception">Propagates any exceptions thrown during the HTTP request.</exception>
        public async Task<HttpResponseMessage> LookupCodeAsync(string code, string system, params string[] properties)
        {
            try
            {
                if (properties == null || properties.Length == 0)
                    properties = new[] { "display", "designations", "status", "inactive" };

                // Build query string
                var query = $"CodeSystem/$lookup?code={Uri.EscapeDataString(code)}&system={Uri.EscapeDataString(system)}";
                if (properties != null && properties.Length > 0)
                {
                    foreach (var prop in properties)
                        query += $"&property={Uri.EscapeDataString(prop)}";
                }

                return await Client.GetAsync(query);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves and expands a FHIR ValueSet by its mnemonic identifier.
        /// </summary>
        /// <param name="valueSetMnemonic">The mnemonic of the value set to retrieve and expand.</param>
        /// <returns>
        /// A <see cref="Task{HttpResponseMessage}"/> representing the asynchronous operation.
        /// The task result contains the HTTP response from the FHIR server, which includes the expanded value set.
        /// </returns>
        /// <exception cref="Exception">Propagates any exceptions thrown during the HTTP request.</exception>
        public async Task<HttpResponseMessage> GetValueSetAsync(string valueSetMnemonic)
        {
            try
            {
                // Build query string
                var query = $"ValueSet/$expand/{valueSetMnemonic}";
                return await Client.GetAsync(query);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
