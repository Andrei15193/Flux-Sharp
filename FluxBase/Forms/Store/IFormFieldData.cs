using System.ComponentModel;
using System.Collections.Generic;
#if NETCORE1_0 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472
//using System.ComponentModel.DataAnnotations;
#endif

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

#if NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472
        ///// <summary>The field validation errors.</summary>
        //IEnumerable<ValidationResult> Errors { get; }
#endif
    }
}