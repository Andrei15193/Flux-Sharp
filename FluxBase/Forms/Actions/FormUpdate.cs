using System.Collections.Generic;
using FluxBase.Forms.Store;

namespace FluxBase.Forms.Actions
{
    /// <summary>Represents a field update.</summary>
    public abstract class FormUpdate
    {
        /// <summary>Gets an <see cref="FormFieldValueUpdate"/> for setting the <paramref name="value"/> of a field.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>Returns an <see cref="FormFieldValueUpdate"/> for setting the <paramref name="value"/> of a field.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public static FormFieldValueUpdate FieldValue(string fieldName, object value)
            => new FormFieldValueUpdate(fieldName, value);

        /// <summary>Gets an <see cref="FormFieldErrorsUpdate"/> for setting the <paramref name="errors"/> of a field.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="errors">The errors to set.</param>
        /// <returns>Returns an <see cref="FormFieldErrorsUpdate"/> for setting the <paramref name="errors"/> of a field.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public static FormFieldErrorsUpdate FieldErrors(string fieldName, IEnumerable<string> errors)
            => new FormFieldErrorsUpdate(fieldName, errors);

        /// <summary>Gets an <see cref="FormFieldErrorsUpdate"/> for setting the <paramref name="errors"/> of a field.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="errors">The errors to set.</param>
        /// <returns>Returns an <see cref="FormFieldErrorsUpdate"/> for setting the <paramref name="errors"/> of a field.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public static FormFieldErrorsUpdate FieldErrors(string fieldName, params string[] errors)
            => new FormFieldErrorsUpdate(fieldName, errors);

        /// <summary>Gets an <see cref="FormFieldProcessingStateUpdate"/> for setting the <paramref name="processingState"/> of a field.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="processingState">The processing state to set.</param>
        /// <returns>Returns an <see cref="FormFieldProcessingStateUpdate"/> for setting the <paramref name="processingState"/> of a field.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public static FormFieldProcessingStateUpdate FieldProcessingState(string fieldName, string processingState)
            => new FormFieldProcessingStateUpdate(fieldName, processingState);

        internal abstract void Apply(FormData formData);
    }
}