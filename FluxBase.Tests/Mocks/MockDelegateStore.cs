using System;

namespace FluxBase.Tests.Mocks
{
    internal class MockDelegateStore : Store
    {
        private readonly Action<object> _callback;

        public MockDelegateStore(Action<object> callback)
        {
            _callback = callback;
        }

        public override void Handle(object action)
            => _callback(action);
    }
}