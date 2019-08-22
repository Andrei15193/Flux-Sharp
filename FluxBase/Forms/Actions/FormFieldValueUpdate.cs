using System;
using FluxBase.Forms.Store;

namespace FluxBase.Forms.Actions
{
    /// <summary>Updates the value of a field.</summary>
    public class FormFieldValueUpdate : FormUpdate
    {
        /// <summary>Initializes a new instance of the <see cref="FormFieldValueUpdate"/> class.</summary>
        /// <param name="fieldName">The field to update.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        public FormFieldValueUpdate(string fieldName, object value)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            Value = value;
        }

        /// <summary>The field name to update.</summary>
        public string FieldName { get; }

        /// <summary>The value to set.</summary>
        public object Value { get; }

        internal override void Apply(FormData formData)
            => formData.GetField(FieldName).Value = Value;
    }
}