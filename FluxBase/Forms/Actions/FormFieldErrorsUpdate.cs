using System;
using System.Collections.Generic;
using FluxBase.Forms.Store;

namespace FluxBase.Forms.Actions
{
    /// <summary>Updates the errors of a field.</summary>
    public class FormFieldErrorsUpdate : FormUpdate
    {
        /// <summary>Initializes a new instance of the <see cref="FormFieldErrorsUpdate"/> class.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="errors">The errors to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public FormFieldErrorsUpdate(string fieldName, IEnumerable<string> errors)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            Errors = errors;
        }

        /// <summary>Initializes a new instance of the <see cref="FormFieldErrorsUpdate"/> class.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="errors">The errors to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public FormFieldErrorsUpdate(string fieldName, params string[] errors)
            : this(fieldName, (IEnumerable<string>)errors)
        {
        }

        /// <summary>The field name to update.</summary>
        public string FieldName { get; }

        /// <summary>The errors to set.</summary>
        public IEnumerable<string> Errors { get; }

        internal override void Apply(FormData formData)
            => formData.GetField(FieldName).Errors = Errors;
    }
}