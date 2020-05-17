using System;
using System.Collections.Generic;
using System.Linq;
using FluxBase.Forms.Actions;
using FluxBase.Forms.Store;
using Xunit;
using Xunit.Sdk;

namespace FluxBase.Tests.Forms.Store
{
    public class FormStoreTests
    {
        private FormStore _formStore;
        private IEnumerable<string> _storePropertyChanges;
        private IFormData _form;
        private IEnumerable<string> _formPropertyChanges;
        private IFormFieldData _field;
        private IEnumerable<string> _fieldPropertyChanges;

        public FormStoreTests()
        {
            var storePropertyChanges = new List<string>();
            _storePropertyChanges = storePropertyChanges;

            _formStore = new FormStore();
            _formStore.PropertyChanged += (sender, e) => storePropertyChanges.Add(e.PropertyName);

            var formPropertyChanges = new List<string>();
            _formPropertyChanges = formPropertyChanges;

            _form = _formStore["test-form"];
            _form.PropertyChanged += (sender, e) => formPropertyChanges.Add(e.PropertyName);

            var fieldPropertyChanges = new List<string>();
            _fieldPropertyChanges = fieldPropertyChanges;

            _field = _form["test-field"];
            _field.PropertyChanged += (sender, e) => fieldPropertyChanges.Add(e.PropertyName);
        }

        [Fact]
        public void TheFormStoreInitiallyContainsNoForms()
        {
            var formStore = new FormStore();

            Assert.False(formStore.Forms.Any());
        }

        [Fact]
        public void RetrievingAFormReturnsAnEmptyOne()
        {
            var form = _formStore["form"];

            Assert.Equal("form", form.Name);
            Assert.False(form.Errors.Any());
            Assert.Null(form.ProcessingState);
            Assert.False(form.IsProcessing);

            AssertCollections(_formStore.Forms, new[] { _form, form });
            Assert.False(form.Fields.Any());
        }

        [Fact]
        public void RetrievingAFormWithTheSameNameReturnsTheSameInstance()
        {
            var sameForm = _formStore["TEST-form"];

            Assert.Same(_form, sameForm);
        }

        [Fact]
        public void RetrievingANullNamedFormThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _formStore[null]);
            Assert.Equal(new ArgumentNullException("formName").Message, exception.Message);
        }

        [Fact]
        public void RetrievingAFieldReturnsAnEmptyOne()
        {
            var field = _form["field"];

            Assert.Null(field.Value);
            Assert.Equal("field", field.Name);
            Assert.False(field.Errors.Any());
            Assert.Null(field.ProcessingState);
            Assert.False(field.IsProcessing);
        }

        [Fact]
        public void RetrievingAFieldWithTheSameNameReturnsTheSameInstance()
        {
            var sameField = _form["test-FIELD"];

            Assert.Same(_field, sameField);
        }

        [Fact]
        public void RetrievingANullNamedFieldThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _form[null]);
            Assert.Equal(new ArgumentNullException("fieldName").Message, exception.Message);
        }

        [Fact]
        public void UpdatingAFormFieldValueSetsItsValue()
        {
            var value = new object();
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldValue("test-field", value)
                )
            );

            Assert.Same(value, _form["test-field"].Value);
            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms),
                    "Items['test-form']['test-field'].Value"
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields),
                    "Items['test-field'].Value"
                }
            );
            AssertCollections(
                _fieldPropertyChanges,
                new[]
                {
                    nameof(IFormFieldData.Value)
                }
            );
        }

        [Fact]
        public void SettingTheSameValueDoesNotRaisePropertyChangedEvents()
        {
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldValue("test-field", _field.Value)
                )
            );

            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms)
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields)
                }
            );
            Assert.False(_fieldPropertyChanges.Any());
        }

        [Fact]
        public void UpdatingAFormFieldErrorSetsItsErrors()
        {
            var errors = new[] { "error 1", "error 2" };
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldErrors("test-field", errors)
                )
            );

            Assert.Same(errors, _form["test-field"].Errors);
            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms),
                    "Items['test-form']['test-field'].Errors"
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields),
                    "Items['test-field'].Errors"
                }
            );
            AssertCollections(
                _fieldPropertyChanges,
                new[]
                {
                    nameof(IFormFieldData.Errors)
                }
            );
        }

        [Fact]
        public void SettingTheSameErrorsDoesNotRaisePropertyChangedEvents()
        {
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldErrors("test-field", _field.Errors)
                )
            );

            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms)
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields)
                }
            );
            Assert.False(_fieldPropertyChanges.Any());
        }

        [Fact]
        public void SettingNullErrorsSetsEmptyCollection()
        {
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldErrors("test-field", new[] { "error" }),
                    FormUpdate.FieldErrors("test-field", null)
                )
            );

            Assert.False(_form["test-field"].Errors.Any());
            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms),
                    "Items['test-form']['test-field'].Errors",
                    "Items['test-form']['test-field'].Errors"
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields),
                    "Items['test-field'].Errors",
                    "Items['test-field'].Errors"
                }
            );
            AssertCollections(
                _fieldPropertyChanges,
                new[]
                {
                    nameof(IFormFieldData.Errors),
                    nameof(IFormFieldData.Errors)
                }
            );
        }

        [Fact]
        public void UpdatingAFormFieldProcessingStateSetsItsProcessingStateAndRelatedFlag()
        {
            var value = new object();
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldProcessingState("test-field", "loading")
                )
            );

            Assert.Equal("loading", _form["test-field"].ProcessingState);
            Assert.True(_form["test-field"].IsProcessing);
            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms),
                    "Items['test-form']['test-field'].ProcessingState",
                    "Items['test-form']['test-field'].IsProcessing"
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields),
                    "Items['test-field'].ProcessingState",
                    "Items['test-field'].IsProcessing"
                }
            );
            AssertCollections(
                _fieldPropertyChanges,
                new[]
                {
                    nameof(IFormFieldData.ProcessingState),
                    nameof(IFormFieldData.IsProcessing)
                }
            );
        }

        [Fact]
        public void SettingTheSameProcessingStateDoesNotRaisePropertyChangedEvents()
        {
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldProcessingState("test-field", _field.ProcessingState)
                )
            );

            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms)
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields)
                }
            );
            Assert.False(_fieldPropertyChanges.Any());
        }

        [Fact]
        public void ChangingAFormFieldProcessingStateDoesNotChangeRelatedFlag()
        {
            var value = new object();
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldProcessingState("test-field", "loading"),
                    FormUpdate.FieldProcessingState("test-field", "processing")
                )
            );

            Assert.Equal("processing", _form["test-field"].ProcessingState);
            Assert.True(_form["test-field"].IsProcessing);
            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms),
                    "Items['test-form']['test-field'].ProcessingState",
                    "Items['test-form']['test-field'].IsProcessing",
                    "Items['test-form']['test-field'].ProcessingState"
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields),
                    "Items['test-field'].ProcessingState",
                    "Items['test-field'].IsProcessing",
                    "Items['test-field'].ProcessingState"
                }
            );
            AssertCollections(
                _fieldPropertyChanges,
                new[]
                {
                    nameof(IFormFieldData.ProcessingState),
                    nameof(IFormFieldData.IsProcessing),
                    nameof(IFormFieldData.ProcessingState)
                }
            );
        }

        [Fact]
        public void SettingTheProcessingStateOfAFieldToNullResetsTheRelatedFlag()
        {
            var value = new object();
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldProcessingState("test-field", "loading"),
                    FormUpdate.FieldProcessingState("test-field", null)
                )
            );

            Assert.Null(_form["test-field"].ProcessingState);
            Assert.False(_form["test-field"].IsProcessing);
            AssertCollections(
                _storePropertyChanges,
                new[]
                {
                    nameof(IFormStore.Forms),
                    "Items['test-form']['test-field'].ProcessingState",
                    "Items['test-form']['test-field'].IsProcessing",
                    "Items['test-form']['test-field'].ProcessingState",
                    "Items['test-form']['test-field'].IsProcessing"
                }
            );
            AssertCollections(
                _formPropertyChanges,
                new[]
                {
                    nameof(IFormData.Fields),
                    "Items['test-field'].ProcessingState",
                    "Items['test-field'].IsProcessing",
                    "Items['test-field'].ProcessingState",
                    "Items['test-field'].IsProcessing"
                }
            );
            AssertCollections(
                _fieldPropertyChanges,
                new[]
                {
                    nameof(IFormFieldData.ProcessingState),
                    nameof(IFormFieldData.IsProcessing),
                    nameof(IFormFieldData.ProcessingState),
                    nameof(IFormFieldData.IsProcessing)
                }
            );
        }

        private static void AssertCollections<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            using (IEnumerator<T> expectedItem = expected.GetEnumerator(), actualItem = actual.GetEnumerator())
            {
                var hasExpectedItem = expectedItem.MoveNext();
                var hasActualItem = actualItem.MoveNext();

                while (hasExpectedItem && hasActualItem)
                {
                    Assert.Equal(expectedItem.Current, actualItem.Current);

                    hasExpectedItem = expectedItem.MoveNext();
                    hasActualItem = actualItem.MoveNext();
                }

                if (hasExpectedItem)
                    throw new XunitException($"Expected: {expectedItem.Current}");
                if (hasActualItem)
                    throw new XunitException($"Unexpected: {actualItem.Current}");
            }
        }
    }
}