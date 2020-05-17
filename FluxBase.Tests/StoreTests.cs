using FluxBase.Tests.Mocks;
using Xunit;

namespace FluxBase.Tests
{
    public class StoreTests
    {
        private Dispatcher _Dispatcher { get; } = new Dispatcher();

        [Fact]
        public void StoreWithMoreSpecificActionGetsNotifiedWhenMatchesExactly()
        {
            var store = new MockStore();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new object());

            Assert.Equal(1, store.InvocationCount);
        }

        [Fact]
        public void StoreWithMoreSpecificActionDoesNotGetNotifiedWhenUsingBaseAction()
        {
            var store = new MockStore<string>();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new object());

            Assert.Equal(0, store.InvocationCount);
        }

        [Fact]
        public void StoreWithBaseActionGetsNotifiedWhenUsingMoreSpecificAction()
        {
            var store = new MockStore();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new object());

            Assert.Equal(1, store.InvocationCount);
        }

        [Fact]
        public void NonVoidReturningMethodsAreNotRegistered()
        {
            var store = MockStore<object, int>.Instance;

            _Dispatcher.Dispatch(new object());

            Assert.Equal(0, store.InvocationCount);
        }
    }
}