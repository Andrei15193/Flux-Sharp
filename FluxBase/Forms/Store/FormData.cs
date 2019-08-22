using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FluxBase.Forms.Store
{
    internal class FormData : IFormData
    {
        private readonly IDictionary<string, FormFieldData> _fields;
        private IEnumerable<string> _errors = new string[0];
        private string _processingState;

        public FormData(string name)
        {
            Name = name;
            _fields = new Dictionary<string, FormFieldData>(StringComparer.OrdinalIgnoreCase);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }

        public IFormFieldData this[string fieldName]
            => GetField(fieldName);

        public IEnumerable<string> Errors
        {
            get => _errors;
            internal set
            {
                _errors = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Errors)));
            }
        }

        public string ProcessingState
        {
            get => _processingState;
            internal set
            {
                _processingState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessingState)));
            }
        }

        public bool IsProcessing
            => _processingState != null;

        public IEnumerable<IFormFieldData> Fields
            => (IEnumerable<IFormFieldData>)_fields.Values;

        internal FormFieldData GetField(string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (!_fields.TryGetValue(fieldName, out var field))
            {
                field = new FormFieldData(fieldName);
                field.PropertyChanged += _FieldPropertyChanged;
                _fields.Add(fieldName, field);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Fields)));
            }
            return field;
        }

        private void _FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IFormFieldData fieldData && e?.PropertyName != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Items['{fieldData.Name}'].{e.PropertyName}"));
        }
    }
}