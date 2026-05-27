namespace PIQI.Components.Models
{
    /// <summary>
    /// Provides helper methods to build and populate a <see cref="MessageModel"/> from a <see cref="PIQIRequest"/>.
    /// </summary>
    public class MessageModelBuilder
    {
        #region Methods

        /// <summary>
        /// Loads the header of a message from the specified <see cref="PIQIRequest"/> 
        /// and returns a <see cref="MessageModel"/> containing the loaded header.
        /// </summary>
        /// <param name="piqiRequest">The PIQI request containing the message data and metadata.</param>
        /// <returns>
        /// A <see cref="MessageModel"/> containing the loaded <see cref="MessageModel"/> 
        /// if successful, or failure information if an error occurred.
        /// </returns>
        public static MessageModel LoadHeader(PIQIRequest piqiRequest)
        {
            try
            {
                // Create a message model
                MessageModel model = new MessageModel();

                // Process the header
                model.LoadHeader(piqiRequest);

                // Success
                return model;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Populates the content portion of the specified <see cref="MessageModel"/> by loading its classes, elements, and attributes.
        /// </summary>
        /// <param name="messageModel">The message model to populate.</param>
        /// <param name="referenceData">The reference data used to set recognized code systems in codeable concepts.</param>
        public static void LoadContent(MessageModel messageModel, PIQIReferenceData referenceData)
        {
            try
            {
                // Process the content
                messageModel.LoadContent(referenceData);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
