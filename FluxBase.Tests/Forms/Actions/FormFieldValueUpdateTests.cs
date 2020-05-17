using System;
using FluxBase.Forms.Actions;
using Xunit;

namespace FluxBase.Tests.Forms.Actions
{
    public class FormFieldValueUpdateTests
    {
        [Fact]
        public void CreatingAFormFieldValueUpdateWithNullFieldNameThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new FormFieldValueUpdate(null, null));
            Assert.Equal(new ArgumentNullException("fieldName").Message, exception.Message);
        }
    }
}