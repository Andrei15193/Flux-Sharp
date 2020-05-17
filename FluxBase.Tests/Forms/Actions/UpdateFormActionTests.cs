using System;
using FluxBase.Forms.Actions;
using Xunit;

namespace FluxBase.Tests.Forms.Actions
{
    public class UpdateFormActionTests
    {
        [Fact]
        public void CreatingAnActionWithNullFormNameThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new UpdateFormAction(null));
            Assert.Equal(new ArgumentNullException("formName").Message, exception.Message);
        }

        [Fact]
        public void CreatingAnActionWithNullUpdatesThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new UpdateFormAction("formName", null));
            Assert.Equal(new ArgumentNullException("updates").Message, exception.Message);
        }
    }
}