using System;
using FluxBase.Forms.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxBase.Tests.Forms.Actions
{
    [TestClass]
    public class UpdateFormActionTests
    {
        [TestMethod]
        public void CreatingAnActionWithNullFormNameThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new UpdateFormAction(null));
            Assert.AreEqual(new ArgumentNullException("formName").Message, exception.Message);
        }

        [TestMethod]
        public void CreatingAnActionWithNullUpdatesThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new UpdateFormAction("formName", null));
            Assert.AreEqual(new ArgumentNullException("updates").Message, exception.Message);
        }
    }
}