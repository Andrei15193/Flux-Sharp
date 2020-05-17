using System;
using FluxBase.Forms.Actions;
using Xunit;

namespace FluxBase.Tests.Forms.Actions
{
    public class FormFieldErrorsUpdateTests
    {
        [Fact]
        public void CreatingAFormFieldValueUpdateWithNullFieldNameThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new FormFieldErrorsUpdate(null, null));
            Assert.Equal(new ArgumentNullException("fieldName").Message, exception.Message);
        }
    }
}