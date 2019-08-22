using System.Collections.Generic;
using System.ComponentModel;

namespace FluxBase.Forms.Store
{
    internal class FormFieldData : IFormFieldData
    {
        private object _value;
        private IEnumerable<string> _errors = new string[0];
        private string _processingState;

        public FormFieldData(string name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }

        public object Value
        {
            get => _value;
            internal set
            {
                if (_value == value)
                    return;

                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public IEnumerable<string> Errors
        {
            get => _errors;
            internal set
            {
                if (value == _errors)
                    return;

                var notifyChanged = false;
                if (value != null)
                {
                    notifyChanged = true;
                    _errors = value;
                }
                else
                    using (var error = _errors.GetEnumerator())
                        if (error.MoveNext())
                        {
                            _errors = new string[0];
                            notifyChanged = true;
                        }

                if (notifyChanged)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Errors)));
            }
        }

        public string ProcessingState
        {
            get => _processingState;
            internal set
            {
                if (value == _processingState)
                    return;

                var wasProcessing = IsProcessing;
                _processingState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessingState)));
                if (wasProcessing != IsProcessing)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProcessing)));
            }
        }

        public bool IsProcessing
            => _processingState != null;
    }
}