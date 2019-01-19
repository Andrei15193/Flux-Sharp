using System;

namespace FluxSharp.Tests.Mocks
{
    internal class MockDelegateStore : Store
    {
        private readonly Action<ActionData> _callback;

        public MockDelegateStore(Action<ActionData> callback)
        {
            _callback = callback;
        }

        protected override void Handle(ActionData actionData)
            => _callback(actionData);
    }
}