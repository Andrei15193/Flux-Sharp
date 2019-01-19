using System;
using System.Threading;
using FluxSharp.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxSharp.Tests
{
    [TestClass]
    public class DispatcherTests
    {
        private Dispatcher _Dispatcher { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            _Dispatcher = new Dispatcher();
        }

        [TestMethod]
        public void RegisteringToDispatcherInvokesCallback()
        {
            var invocationCount = 0;

            _Dispatcher.Register(actionData => Interlocked.Increment(ref invocationCount));
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationCount);
        }

        [TestMethod]
        public void RegisteringStoreToDispatcherInvokesHandler()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(actionData => Interlocked.Increment(ref invocationCount));

            _Dispatcher.Register(store);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationCount);
        }

        [TestMethod]
        public void RegisteringToDispatcherTwiceInvokesCallbackOnce()
        {
            var invocationCount = 0;

            void Callback(ActionData actionData) => Interlocked.Increment(ref invocationCount);

            var firstRegistrationId = _Dispatcher.Register(Callback);
            var secondRegistrationId = _Dispatcher.Register(Callback);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationCount);
            Assert.AreEqual(firstRegistrationId, secondRegistrationId);
        }

        [TestMethod]
        public void RegisteringStoreTwiceToDispatcherInvokesHandlerOnce()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(actionData => Interlocked.Increment(ref invocationCount));

            var firstRegistrationId = _Dispatcher.Register(store);
            var secondRegistrationId = _Dispatcher.Register(store);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationCount);
            Assert.AreEqual(firstRegistrationId, secondRegistrationId);
        }

        [TestMethod]
        public void UnregisteringFromDispatcherNoLongerInvokesCallback()
        {
            var invocationCount = 0;

            var registrationId = _Dispatcher.Register(actionData => Interlocked.Increment(ref invocationCount));
            _Dispatcher.Unregister(registrationId);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(0, invocationCount);
        }

        [TestMethod]
        public void UnregisteringStoreFromDispatcherNoLongerInvokesHandler()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(actionData => Interlocked.Increment(ref invocationCount));

            _Dispatcher.Register(store);
            _Dispatcher.Unregister(store);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(0, invocationCount);
        }

        [TestMethod]
        public void UnregisteringTwiceFromDispatcherReturnsFalseTheSecondTime()
        {
            var invocationCount = 0;

            var registrationId = _Dispatcher.Register(actionData => Interlocked.Increment(ref invocationCount));
            Assert.IsTrue(_Dispatcher.Unregister(registrationId));

            Assert.IsFalse(_Dispatcher.Unregister(registrationId));
        }

        [TestMethod]
        public void UnregisteringStoreTwiceFromDispatcherReturnsFalseTheSecondTime()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(actionData => Interlocked.Increment(ref invocationCount));

            var registrationId = _Dispatcher.Register(store);

            Assert.IsTrue(_Dispatcher.Unregister(store));
            Assert.IsFalse(_Dispatcher.Unregister(store));
        }

        [TestMethod]
        public void DispatchingNullPassesNull()
        {
            ActionData actualActionData = null;

            _Dispatcher.Register(actionData => Interlocked.Exchange(ref actualActionData, actionData));
            _Dispatcher.Dispatch(null);

            Assert.IsNull(actualActionData);
        }

        [TestMethod]
        public void DispatchPassesSameActionData()
        {
            var expectedActionData = new MockActionData();
            ActionData actualActionData = null;

            _Dispatcher.Register(actionData => Interlocked.Exchange(ref actualActionData, actionData));
            _Dispatcher.Dispatch(expectedActionData);

            Assert.AreSame(expectedActionData, actualActionData);
        }

        [TestMethod]
        public void RegisteringNullCallbackThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.Register(callback: null));
            Assert.AreEqual(new ArgumentNullException("callback").Message, exception.Message);
        }

        [TestMethod]
        public void RegisteringNullStoreThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.Register(store: null));
            Assert.AreEqual(new ArgumentNullException("store").Message, exception.Message);
        }

        [TestMethod]
        public void UnregisteringNullIdThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.Unregister(id: null));
            Assert.AreEqual(new ArgumentNullException("id").Message, exception.Message);
        }

        [TestMethod]
        public void UnregisteringNullStoreThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.Unregister(store: null));
            Assert.AreEqual(new ArgumentNullException("store").Message, exception.Message);
        }
    }
}