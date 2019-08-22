using System.Collections.Generic;
using System.ComponentModel;

namespace FluxBase.Forms.Store
{
    /// <summary>Exposes data a form.</summary>
    public interface IFormData : INotifyPropertyChanged
    {
        /// <summary>The form name.</summary>
        string Name { get; }

        /// <summary>Gets an <see cref="IFormFieldData"/> for the given <paramref name="fieldName"/>.</summary>
        /// <param name="fieldName">The name of the field to retrieve.</param>
        /// <remarks>Multiple calls with the same field name (case insensitive) will retrieve the same field.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fieldName"/> is <c>null</c>.</exception>
        IFormFieldData this[string fieldName] { get; }

        /// <summary>The form validation errors. Field validation errors are not included.</summary>
        IEnumerable<string> Errors { get; }

        /// <summary>The form processing state (e.g.: validating, fetching etc.)</summary>
        string ProcessingState { get; }

        /// <summary>Indicates whether the form is in a processing state or not.</summary>
        /// <value>The value of this property is equivalent to <c>ProcessingState != null</c>.</value>
        bool IsProcessing { get; }

        /// <summary>The fields exposed by the form.</summary>
        /// <remarks>Fields are added dynamically as they are requested, the value of this property
        /// may not change but the contents of the collection can change.
        /// The <see cref="INotifyPropertyChanged.PropertyChanged"/> event can be raised in both cases.</remarks>
        IEnumerable<IFormFieldData> Fields { get; }
    }
}