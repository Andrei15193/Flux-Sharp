using System;
using FluxBase.Forms.Actions;
using Xunit;

namespace FluxBase.Tests.Forms.Actions
{
    public class FormFieldProcessingStateUpdateTests
    {
        [Fact]
        public void CreatingAFormFieldProcessingStateUpdateWithNullFieldNameThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new FormFieldProcessingStateUpdate(null, null));
            Assert.Equal(new ArgumentNullException("fieldName").Message, exception.Message);
        }
    }
}