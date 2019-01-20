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
        public void StoreWithMoreSpecificActionDataGetsNotifiedWhenMatchesExactly()
        {
            var store = new MockStore<MockActionData>();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new MockActionData());

            Assert.AreEqual(1, store.InvocationCount);
        }

        [TestMethod]
        public void StoreWithMoreSpecificActionDataDoesNotGetNotifiedWhenUsingBaseActionData()
        {
            var store = new MockStore<MockActionData>();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(0, store.InvocationCount);
        }

        [TestMethod]
        public void StoreWithBaseActionDataGetsNotifiedWhenUsingMoreSpecificActionData()
        {
            var store = new MockStore<ActionData>();
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(new MockActionData());

            Assert.AreEqual(1, store.InvocationCount);
        }

        [TestMethod]
        public void NonVoidReturningMethodsAreNotRegistered()
        {
            var store = MockStore<ActionData, int>.Instance;

            _Dispatcher.Dispatch(new MockActionData());

            Assert.AreEqual(0, store.InvocationCount);
        }
    }
}