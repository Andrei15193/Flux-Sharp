using System;
using FluxBase.Forms.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxBase.Tests.Forms.Actions
{
    [TestClass]
    public class FormFieldProcessingStateUpdateTests
    {
        [TestMethod]
        public void CreatingAFormFieldProcessingStateUpdateWithNullFieldNameThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new FormFieldProcessingStateUpdate(null, null));
            Assert.AreEqual(new ArgumentNullException("fieldName").Message, exception.Message);
        }
    }
}