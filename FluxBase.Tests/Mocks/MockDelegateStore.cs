using System;

namespace FluxBase.Tests.Mocks
{
    internal class MockDelegateStore : Store
    {
        private readonly Action<ActionData> _callback;

        public MockDelegateStore(Action<ActionData> callback)
        {
            _callback = callback;
        }

        public override void Handle(ActionData actionData)
            => _callback(actionData);
    }
}