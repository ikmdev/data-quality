using Newtonsoft.Json;
using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;
using System.Text;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if an attribute's text contains only alphanumeric characters and spaces.
    /// </summary>
    public class SAM_CustomExternalAssessment : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_CustomExternalAssessment"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_CustomExternalAssessment(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates a PIQI message asynchronously using a RESTful API call.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing the message and parameters for evaluation. 
        /// Must include a parameter named "Processing URL" specifying the API endpoint.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation. 
        /// The task result contains the <see cref="PIQISAMResponse"/> returned by the API or an error if the evaluation fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>Validates that the request includes a "Processing URL" parameter.</item>
        ///   <item>Removes the "Processing URL" parameter from the request object before sending.</item>
        ///   <item>Serializes the request object to JSON.</item>
        ///   <item>Sends a POST request to the RESTful API endpoint.</item>
        ///   <item>Reads the response and deserializes it into a <see cref="PIQISAMResponse"/> object.</item>
        ///   <item>Captures and reports any exceptions that occur during processing.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if the parameter list is missing, the "Processing URL" is not provided, the request or URL is invalid, 
        /// the API call fails, or the response cannot be parsed into a <see cref="PIQISAMResponse"/>.
        /// </exception>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();

            try
            {
                // Get processing URL from the paramter list
                if (request.ParmList == null) throw new Exception("Parameter list was not supplied");
                Tuple<string, string>? arg1 = request.ParmList.Where(t => t.Item1 == "Processing URL").FirstOrDefault();
                if (arg1 == null) throw new Exception("Missing or invalid URL for RESTful API SAM.");
                string processingUrl = arg1.Item2;

                // REmove the processing url from the request object for the api call
                request.RemoveParameter("Processing URL");

                // Serialize object to JSON
                string json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                if (content == null || processingUrl == null) throw new Exception("Invalid process url or request in SAM_CustomExternalAssessment.");

                // Call the API
                var apiResponse = await _SAMService.Client.PostAsync(processingUrl, content);
                if (apiResponse == null) throw new Exception($"Failed to call SAM API: {processingUrl}");

                // Parse the response
                string responseBody = await apiResponse.Content.ReadAsStringAsync();
                PIQISAMResponse? samResponse = JsonConvert.DeserializeObject<PIQISAMResponse>(responseBody);
                if (samResponse == null) throw new Exception($"Failed to parse PIQISAMResponse from {processingUrl}.");

                result = samResponse;
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Gets the mnemonic code for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "CUSTOM_EXTERNAL_ASSESSMENT";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
