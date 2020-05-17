using System.ComponentModel;
using System.Collections.Generic;

namespace FluxBase.Forms.Store
{
    /// <summary>Exposes data for a form field.</summary>
    public interface IFormFieldData : INotifyPropertyChanged
    {
        /// <summary>The field name.</summary>
        string Name { get; }

        /// <summary>The field value.</summary>
        object Value { get; }

        /// <summary>The field validation errors.</summary>
        IEnumerable<string> Errors { get; }

        /// <summary>The field processing state (e.g.: validating, fetching etc.)</summary>
        string ProcessingState { get; }

        /// <summary>Indicates whether the filed is in a processing state or not.</summary>
        /// <value>The value of this property is equivalent to <c>ProcessingState != null</c>.</value>
        bool IsProcessing { get; }
    }
}