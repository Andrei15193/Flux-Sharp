using System;
using System.Collections.Generic;
using System.Linq;
using FluxBase.Forms.Actions;
using FluxBase.Forms.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxBase.Tests.Forms.Store
{
    [TestClass]
    public class FormStoreTests
    {
        private FormStore _formStore;
        private IEnumerable<string> _storePropertyChanges;
        private IFormData _form;
        private IEnumerable<string> _formPropertyChanges;
        private IFormFieldData _field;
        private IEnumerable<string> _fieldPropertyChanges;

        [TestInitialize]
        public void TestInitialize()
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

        [TestMethod]
        public void TheFormStoreInitiallyContainsNoForms()
        {
            var formStore = new FormStore();

            Assert.IsFalse(formStore.Forms.Any());
        }

        [TestMethod]
        public void RetrievingAFormReturnsAnEmptyOne()
        {
            var form = _formStore["form"];

            Assert.AreEqual("form", form.Name);
            Assert.IsFalse(form.Errors.Any());
            Assert.IsNull(form.ProcessingState);
            Assert.IsFalse(form.IsProcessing);

            AssertCollections(_formStore.Forms, new[] { _form, form });
            Assert.IsFalse(form.Fields.Any());
        }

        [TestMethod]
        public void RetrievingAFormWithTheSameNameReturnsTheSameInstance()
        {
            var sameForm = _formStore["TEST-form"];

            Assert.AreSame(_form, sameForm);
        }

        [TestMethod]
        public void RetrievingANullNamedFormThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _formStore[null]);
            Assert.AreEqual(new ArgumentNullException("formName").Message, exception.Message);
        }

        [TestMethod]
        public void RetrievingAFieldReturnsAnEmptyOne()
        {
            var field = _form["field"];

            Assert.IsNull(field.Value);
            Assert.AreEqual("field", field.Name);
            Assert.IsFalse(field.Errors.Any());
            Assert.IsNull(field.ProcessingState);
            Assert.IsFalse(field.IsProcessing);
        }

        [TestMethod]
        public void RetrievingAFieldWithTheSameNameReturnsTheSameInstance()
        {
            var sameField = _form["test-FIELD"];

            Assert.AreSame(_field, sameField);
        }

        [TestMethod]
        public void RetrievingANullNamedFieldThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _form[null]);
            Assert.AreEqual(new ArgumentNullException("fieldName").Message, exception.Message);
        }

        [TestMethod]
        public void UpdatingAFormFieldValueSetsItsValue()
        {
            var value = new object();
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldValue("test-field", value)
                )
            );

            Assert.AreSame(value, _form["test-field"].Value);
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

        [TestMethod]
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
            Assert.IsFalse(_fieldPropertyChanges.Any());
        }

        [TestMethod]
        public void UpdatingAFormFieldErrorSetsItsErrors()
        {
            var errors = new[] { "error 1", "error 2" };
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldErrors("test-field", errors)
                )
            );

            Assert.AreSame(errors, _form["test-field"].Errors);
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

        [TestMethod]
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
            Assert.IsFalse(_fieldPropertyChanges.Any());
        }

        [TestMethod]
        public void SettingNullErrorsSetsEmptyCollection()
        {
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldErrors("test-field", new[] { "error" }),
                    FormUpdate.FieldErrors("test-field", null)
                )
            );

            Assert.IsFalse(_form["test-field"].Errors.Any());
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

        [TestMethod]
        public void UpdatingAFormFieldProcessingStateSetsItsProcessingStateAndRelatedFlag()
        {
            var value = new object();
            _formStore.Handle(
                new UpdateFormAction(
                    "test-form",
                    FormUpdate.FieldProcessingState("test-field", "loading")
                )
            );

            Assert.AreEqual("loading", _form["test-field"].ProcessingState);
            Assert.IsTrue(_form["test-field"].IsProcessing);
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

        [TestMethod]
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
            Assert.IsFalse(_fieldPropertyChanges.Any());
        }

        [TestMethod]
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

            Assert.AreEqual("processing", _form["test-field"].ProcessingState);
            Assert.IsTrue(_form["test-field"].IsProcessing);
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

        [TestMethod]
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

            Assert.IsNull(_form["test-field"].ProcessingState);
            Assert.IsFalse(_form["test-field"].IsProcessing);
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
                    Assert.AreEqual(expectedItem.Current, actualItem.Current);

                    hasExpectedItem = expectedItem.MoveNext();
                    hasActualItem = actualItem.MoveNext();
                }

                if (hasExpectedItem)
                    Assert.Fail($"Expected: {expectedItem.Current}");
                if (hasActualItem)
                    Assert.Fail($"Unexpected: {actualItem.Current}");
            }
        }
    }
}