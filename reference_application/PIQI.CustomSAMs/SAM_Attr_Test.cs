using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that is an example of how to set up a custom SAM. 
    /// This SAM serves no purpose other than to demonstrate how to set up the evaluation.
    /// In this test SAM, it will always return true. This SAM's definition would also need to be 
    /// added to the SAMS.json file and the Evaluation for it to be used.
    /// </summary>
    public class SAM_Attr_Test : SAMBase
    {
        public SAM_Attr_Test(SAM sam, SAMService samService) : base(sam, samService) { }

        
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = true;

           
            // Update result
            result.Done(passed);
           
            return result;
        }

        /// <summary>
        /// Gets the mnemonic code for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "ATTR_TEST_1";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
