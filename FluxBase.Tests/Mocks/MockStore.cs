using System.Threading;

namespace FluxBase.Tests.Mocks
{
    internal class MockStore<TAction, TResult> : Store
    {
        public static MockStore<TAction, TResult> Instance { get; } = new MockStore<TAction, TResult>();

        private int _invocationCount = 0;

        public int InvocationCount
            => _invocationCount;

        protected MockStore()
        {
        }

        public TResult HandleAction(object action)
        {
            Interlocked.Increment(ref _invocationCount);
            return default(TResult);
        }

        private TResult _HandleAction(object action)
        {
            Interlocked.Increment(ref _invocationCount);
            return default(TResult);
        }
    }

    internal class MockStore<TAction> : Store
    {
        private int _invocationCount = 0;

        public int InvocationCount
            => _invocationCount;

        public void HandleAction(TAction action)
            => Interlocked.Increment(ref _invocationCount);

        private void _HandleAction(TAction action)
            => Interlocked.Increment(ref _invocationCount);
    }

    internal sealed class MockStore : MockStore<object>
    {
        public MockStore()
        {
        }
    }
}