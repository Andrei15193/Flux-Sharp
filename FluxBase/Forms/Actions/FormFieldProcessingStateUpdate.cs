using System;
using FluxBase.Forms.Store;

namespace FluxBase.Forms.Actions
{
    /// <summary>Updates the processing state of a field.</summary>
    public class FormFieldProcessingStateUpdate : FormUpdate
    {
        /// <summary>Initializes a new instance of the <see cref="FormFieldProcessingStateUpdate"/> class.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="processingState">The processing state to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public FormFieldProcessingStateUpdate(string fieldName, string processingState)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            ProcessingState = processingState;
        }

        /// <summary>The field name to update.</summary>
        public string FieldName { get; }

        /// <summary>The processing state to set.</summary>
        public string ProcessingState { get; }

        internal override void Apply(FormData formData)
            => formData.GetField(FieldName).ProcessingState = ProcessingState;
    }
}