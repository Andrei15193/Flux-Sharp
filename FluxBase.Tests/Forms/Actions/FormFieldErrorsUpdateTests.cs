using System;
using FluxBase.Forms.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxBase.Tests.Forms.Actions
{
    [TestClass]
    public class FormFieldErrorsUpdateTests
    {
        [TestMethod]
        public void CreatingAFormFieldValueUpdateWithNullFieldNameThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new FormFieldErrorsUpdate(null, null));
            Assert.AreEqual(new ArgumentNullException("fieldName").Message, exception.Message);
        }
    }
}