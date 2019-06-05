using System.Threading;

namespace FluxBase.Tests.Mocks
{
    internal class MockStore<TActionData, TResult> : Store where TActionData : ActionData
    {
        public static MockStore<TActionData, TResult> Instance { get; } = new MockStore<TActionData, TResult>();

        private int _invocationCount = 0;

        public int InvocationCount
            => _invocationCount;

        protected MockStore()
        {
        }

        public TResult HandleActionData(ActionData actionData)
        {
            Interlocked.Increment(ref _invocationCount);
            return default(TResult);
        }
    }

    internal class MockStore<TActionData> : Store where TActionData : ActionData
    {
        private int _invocationCount = 0;

        public int InvocationCount
            => _invocationCount;

        public MockStore()
        {
        }

        public void HandleActionData(TActionData actionData)
            => Interlocked.Increment(ref _invocationCount);
    }

    internal sealed class MockStore : MockStore<ActionData>
    {
        public MockStore()
        {
        }
    }
}