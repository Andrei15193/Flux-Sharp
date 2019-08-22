using System;
using System.Collections.Generic;
using System.ComponentModel;
using FluxBase.Forms.Actions;

namespace FluxBase.Forms.Store
{
    /// <summary>Represents a store responsible with managing form states.</summary>
    public class FormStore : FluxBase.Store, IFormStore
    {
        private readonly IDictionary<string, FormData> _forms;

        /// <summary>Initializes a new instance of the <see cref="FormStore"/> class.</summary>
        public FormStore()
        {
            _forms = new Dictionary<string, FormData>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Gets an <see cref="IFormData"/> for the given <paramref name="formName"/>.</summary>
        /// <param name="formName">The name of the form to retrieve.</param>
        /// <remarks>Multiple calls with the same form name (case insensitive) will retrieve the same form.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formName"/> is <c>null</c>.</exception>
        public IFormData this[string formName]
            => GetForm(formName);

        /// <summary>The forms exposed by the store.</summary>
        /// <remarks>
        /// Forms are added dynamically as they are requested, the value of this property
        /// may not change but the contents of the collection can change.
        /// The <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> event can be raised in both cases.
        /// </remarks>
        public IEnumerable<IFormData> Forms
            => (IEnumerable<IFormData>)_forms.Values;

        /// <summary>Handles the given <paramref name="updateFormAction"/>.</summary>
        /// <param name="updateFormAction">The update action to handle.</param>
        public void Handle(UpdateFormAction updateFormAction)
        {
            if (updateFormAction != null)
            {
                var form = GetForm(updateFormAction.FormName);
                foreach (var update in updateFormAction.Updates)
                    update.Apply(form);
            }
        }

        internal FormData GetForm(string formName)
        {
            if (formName == null)
                throw new ArgumentNullException(nameof(formName));

            if (!_forms.TryGetValue(formName, out var form))
            {
                form = new FormData(formName);
                form.PropertyChanged += _FormPropertyChanged;
                _forms.Add(formName, form);
                NotifyPropertyChanged(nameof(Forms));
            }
            return form;
        }

        private void _FormPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IFormData formData && (e?.PropertyName?.StartsWith("Items['", StringComparison.OrdinalIgnoreCase) ?? false))
                NotifyPropertyChanged($"Items['{formData.Name}']{e.PropertyName.Substring("Items".Length)}");
        }
    }
}