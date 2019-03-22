using FluxBase.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxBase.Tests
{
    [TestClass]
    public class StoreTests
    {
        private Dispatcher _Dispatcher { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            _Dispatcher = new Dispatcher();
        }

        [TestMethod]
        public void StoreWithMoreSpecificActionGetsNotifiedWhenMatchesExactly()
        {
            var store = new MockStore();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new object());

            Assert.AreEqual(1, store.InvocationCount);
        }

        [TestMethod]
        public void StoreWithMoreSpecificActionDoesNotGetNotifiedWhenUsingBaseAction()
        {
            var store = new MockStore<string>();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new object());

            Assert.AreEqual(0, store.InvocationCount);
        }

        [TestMethod]
        public void StoreWithBaseActionGetsNotifiedWhenUsingMoreSpecificAction()
        {
            var store = new MockStore();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new object());

            Assert.AreEqual(1, store.InvocationCount);
        }

        [TestMethod]
        public void NonVoidReturningMethodsAreNotRegistered()
        {
            var store = MockStore<object, int>.Instance;

            _Dispatcher.Dispatch(new object());

            Assert.AreEqual(0, store.InvocationCount);
        }
    }
}