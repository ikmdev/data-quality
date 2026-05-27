using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI.Components.SAMs
{
    /// <summary>
    /// Base class for all SAM (Scoring and Analysis Model) implementations.
    /// Provides common properties and a default evaluation method.
    /// </summary>
    public abstract class SAMBase : ISAMWorker
    {
        /// <summary>
        /// Gets the <see cref="SAM"/> object associated with this evaluator.
        /// </summary>
        public SAM SAMObject { get; }

        /// <summary>
        /// Gets the mnemonic identifier associated with this instance.
        /// </summary>
        public abstract string Mnemonic { get; }

        /// <summary>
        /// Provides access to the <see cref="SAMService"/> service
        /// used for accessing reference data and performing FHIR resource lookups.
        /// </summary>
        protected readonly SAMService _SAMService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SAMBase"/> class.
        /// </summary>
        /// <param name="sam">
        /// The parent <see cref="SAM"/> object that defines the configuration and context
        /// for this evaluator.
        /// </param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        protected SAMBase(SAM sam, SAMService samService)
        {
            SAMObject = sam;
            _SAMService = samService;
        }

        /// <summary>
        /// Asynchronously evaluates the specified request against the SAM logic.
        /// </summary>
        /// <param name="request">
        /// A <see cref="PIQISAMRequest"/> containing the message data and contextual parameters
        /// used for evaluation.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> that represents the asynchronous operation.
        /// The response indicates the outcome of the SAM evaluation.
        /// </returns>
        /// <remarks>
        /// The default implementation always returns an error response with the message
        /// <c>"Method not implemented."</c>. Derived classes must override this method to
        /// provide custom evaluation logic.
        /// </remarks>
        public virtual async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            result.Error("Method not implemented.");
            return result;
        }

    }
}
