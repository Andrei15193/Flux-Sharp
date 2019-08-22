using System.Collections.Generic;
using System.ComponentModel;

namespace FluxBase.Forms.Store
{
    /// <summary>Represents a store responsible with managing form states.</summary>
    public interface IFormStore : INotifyPropertyChanged
    {
        /// <summary>Gets an <see cref="IFormData"/> for the given <paramref name="formName"/>.</summary>
        /// <param name="formName">The name of the form to retrieve.</param>
        /// <remarks>Multiple calls with the same form name (case insensitive) will retrieve the same form.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="formName"/> is <c>null</c>.</exception>
        IFormData this[string formName] { get; }

        /// <summary>The forms exposed by the store.</summary>
        /// <remarks>
        /// Forms are added dynamically as they are requested, the value of this property
        /// may not change but the contents of the collection can change.
        /// The <see cref="INotifyPropertyChanged.PropertyChanged"/> event can be raised in both cases.
        /// </remarks>
        IEnumerable<IFormData> Forms { get; }
    }
}