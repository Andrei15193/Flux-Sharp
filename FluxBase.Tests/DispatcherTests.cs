using System;
using System.Collections.Generic;
using System.Threading;
using FluxBase.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluxBase.Tests
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

            _Dispatcher.Register(
                actionData => Interlocked.Increment(ref invocationCount)
            );
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationCount);
        }

        [TestMethod]
        public void RegisteringStoreToDispatcherInvokesHandler()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(
                actionData => Interlocked.Increment(ref invocationCount)
            );

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
            var store = new MockDelegateStore(
                actionData => Interlocked.Increment(ref invocationCount)
            );

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

            var registrationId = _Dispatcher.Register(
                actionData => Interlocked.Increment(ref invocationCount)
            );
            _Dispatcher.Unregister(registrationId);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(0, invocationCount);
        }

        [TestMethod]
        public void UnregisteringStoreFromDispatcherNoLongerInvokesHandler()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(
                actionData => Interlocked.Increment(ref invocationCount)
            );

            _Dispatcher.Register(store);
            _Dispatcher.Unregister(store);
            _Dispatcher.Dispatch(null);

            Assert.AreEqual(0, invocationCount);
        }

        [TestMethod]
        public void UnregisteringTwiceFromDispatcherReturnsFalseTheSecondTime()
        {
            var invocationCount = 0;

            var registrationId = _Dispatcher.Register(
                actionData => Interlocked.Increment(ref invocationCount)
            );
            Assert.IsTrue(_Dispatcher.Unregister(registrationId));

            Assert.IsFalse(_Dispatcher.Unregister(registrationId));
        }

        [TestMethod]
        public void UnregisteringStoreTwiceFromDispatcherReturnsFalseTheSecondTime()
        {
            var invocationCount = 0;
            var store = new MockDelegateStore(
                actionData => Interlocked.Increment(ref invocationCount)
            );

            var registrationId = _Dispatcher.Register(store);

            Assert.IsTrue(_Dispatcher.Unregister(store));
            Assert.IsFalse(_Dispatcher.Unregister(store));
        }

        [TestMethod]
        public void DispatchingNullPassesNull()
        {
            ActionData actualActionData = null;

            _Dispatcher.Register(
                actionData => Interlocked.Exchange(ref actualActionData, actionData)
            );
            _Dispatcher.Dispatch(null);

            Assert.IsNull(actualActionData);
        }

        [TestMethod]
        public void DispatchPassesSameActionData()
        {
            var expectedActionData = new MockActionData();
            ActionData actualActionData = null;

            _Dispatcher.Register(
                actionData => Interlocked.Exchange(ref actualActionData, actionData)
            );
            _Dispatcher.Dispatch(expectedActionData);

            Assert.AreSame(expectedActionData, actualActionData);
        }

        [TestMethod]
        public void WaitForBlocksUntilAwaitedHandlerCompletes()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            object secondSubscriptionId = null;
            var firstSubscriptionId = _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(secondSubscriptionId);
                    invocationsList.Add(first);
                }
            );
            secondSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(second)
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(2, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(first, invocationsList[1]);
        }

        [TestMethod]
        public void WaitForBlocksUntilHandlersThatThemselvesWaitAwaitsTheirCompletion()
        {
            const int chainedHandlersCount = 500;
            var registrationIds = new List<object>();
            var invocationsList = new List<string>();

            for (var index = 0; index < chainedHandlersCount; index++)
            {
                var indexCopy = index;
                registrationIds.Add(
                    _Dispatcher.Register(
                        actionData =>
                        {
                            if (indexCopy < chainedHandlersCount - 1)
                                _Dispatcher.WaitFor(registrationIds[indexCopy + 1]);
                            invocationsList.Add($"Blocked {indexCopy}");
                        }
                    )
                );
                _Dispatcher.Register(
                    actionData => invocationsList.Add($"Not blocked {indexCopy}")
                );
            }

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(chainedHandlersCount * 2, invocationsList.Count);
            for (var index = 0; index < chainedHandlersCount; index++)
            {
                Assert.AreEqual($"Blocked {chainedHandlersCount - index - 1}", invocationsList[index]);
                Assert.AreEqual($"Not blocked {index}", invocationsList[index + chainedHandlersCount]);
            }
        }

        [TestMethod]
        public void WaitForCausingDeadlockIsDetected()
        {
            object firstSubscriptionId = null;
            object secondSubscriptionId = null;
            firstSubscriptionId = _Dispatcher.Register(
                actionData => _Dispatcher.WaitFor(secondSubscriptionId)
            );
            secondSubscriptionId = _Dispatcher.Register(
                actionData => _Dispatcher.WaitFor(firstSubscriptionId)
            );

            var exception = Assert.ThrowsException<InvalidOperationException>(() => _Dispatcher.Dispatch(null));

            Assert.AreEqual(
                new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                exception.Message
            );
        }

        [TestMethod]
        public void WaitForCausingDeadlockThroughChainedBlocksIsDetected()
        {
            const int chainedHandlersCount = 500;
            var registrationIds = new List<object>();

            for (var index = 0; index < chainedHandlersCount; index++)
            {
                var indexCopy = index;
                registrationIds.Add(
                    _Dispatcher.Register(
                        actionData => _Dispatcher.WaitFor(registrationIds[(indexCopy + 1) % chainedHandlersCount])
                    )
                );
            }

            var exception = Assert.ThrowsException<InvalidOperationException>(() => _Dispatcher.Dispatch(null));

            Assert.AreEqual(
                new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                exception.Message
            );
        }

        [TestMethod]
        public void WaitForBlocksUntilAwaitedHandlerCompletesWithTwoSeparateDependencyChains()
        {
            const string first = "first";
            const string second = "second";
            const string third = "third";
            const string fourth = "fourth";
            var invocationsList = new List<string>();

            object secondSubscriptionId = null;
            object fourthSubscriptionId = null;
            var firstSubscriptionId = _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(secondSubscriptionId);
                    invocationsList.Add(first);
                }
            );
            secondSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(second)
            );
            var thirdSubscriptionId = _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(fourthSubscriptionId);
                    invocationsList.Add(third);
                }
            );
            fourthSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(fourth)
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(4, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(first, invocationsList[1]);
            Assert.AreEqual(fourth, invocationsList[2]);
            Assert.AreEqual(third, invocationsList[3]);
        }

        [TestMethod]
        public void WaitForDoesNotBlockIfHandlerWasAlreadyExecuted()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            var firstSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(first)
            );
            var secondSubscriptionId = _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(firstSubscriptionId);
                    invocationsList.Add(second);
                }
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(2, invocationsList.Count);
            Assert.AreEqual(first, invocationsList[0]);
            Assert.AreEqual(second, invocationsList[1]);
        }

        [TestMethod]
        public void WaitForDoesNotBlockIfHandlerWasUnregistered()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            var firstSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(first)
            );
            var secondSubscriptionId = _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(firstSubscriptionId);
                    invocationsList.Add(second);
                }
            );
            _Dispatcher.Unregister(firstSubscriptionId);

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
        }

        [TestMethod]
        public void WaitForStoreBlocksUntilAwaitedHandlerCompletes()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            object secondSubscriptionId = null;
            var firstSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(secondSubscriptionId);
                        invocationsList.Add(first);
                    }
                )
            );
            secondSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => invocationsList.Add(second)
                )
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(2, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(first, invocationsList[1]);
        }

        [TestMethod]
        public void WaitForStoreBlocksUntilHandlersThatThemselvesWaitAwaitsTheirCompletion()
        {
            const int chainedHandlersCount = 500;
            var registrationIds = new List<object>();
            var invocationsList = new List<string>();

            for (var index = 0; index < chainedHandlersCount; index++)
            {
                var indexCopy = index;
                registrationIds.Add(
                    _Dispatcher.Register(
                        new MockDelegateStore(
                            actionData =>
                            {
                                if (indexCopy < chainedHandlersCount - 1)
                                    _Dispatcher.WaitFor(registrationIds[indexCopy + 1]);
                                invocationsList.Add($"Blocked {indexCopy}");
                            }
                        )
                    )
                );
                _Dispatcher.Register(
                    new MockDelegateStore(
                        actionData => invocationsList.Add($"Not blocked {indexCopy}")
                    )
                );
            }

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(chainedHandlersCount * 2, invocationsList.Count);
            for (var index = 0; index < chainedHandlersCount; index++)
            {
                Assert.AreEqual($"Blocked {chainedHandlersCount - index - 1}", invocationsList[index]);
                Assert.AreEqual($"Not blocked {index}", invocationsList[index + chainedHandlersCount]);
            }
        }

        [TestMethod]
        public void WaitForStoreCausingDeadlockIsDetected()
        {
            object firstSubscriptionId = null;
            object secondSubscriptionId = null;
            firstSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => _Dispatcher.WaitFor(secondSubscriptionId)
                )
            );
            secondSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => _Dispatcher.WaitFor(firstSubscriptionId)
                )
            );

            var exception = Assert.ThrowsException<InvalidOperationException>(() => _Dispatcher.Dispatch(null));

            Assert.AreEqual(
                new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                exception.Message
            );
        }

        [TestMethod]
        public void WaitForStoreCausingDeadlockThroughChainedBlocksIsDetected()
        {
            const int chainedHandlersCount = 500;
            var registrationIds = new List<object>();

            for (var index = 0; index < chainedHandlersCount; index++)
            {
                var indexCopy = index;
                registrationIds.Add(
                    _Dispatcher.Register(
                        new MockDelegateStore(
                            actionData => _Dispatcher.WaitFor(registrationIds[(indexCopy + 1) % chainedHandlersCount])
                        )
                    )
                );
            }

            var exception = Assert.ThrowsException<InvalidOperationException>(() => _Dispatcher.Dispatch(null));

            Assert.AreEqual(
                new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                exception.Message
            );
        }

        [TestMethod]
        public void WaitForStoreBlocksUntilAwaitedHandlerCompletesWithTwoSeparateDependencyChains()
        {
            const string first = "first";
            const string second = "second";
            const string third = "third";
            const string fourth = "fourth";
            var invocationsList = new List<string>();

            object secondSubscriptionId = null;
            object fourthSubscriptionId = null;
            var firstSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(secondSubscriptionId);
                        invocationsList.Add(first);
                    }
                )
            );
            secondSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => invocationsList.Add(second)
                )
            );
            var thirdSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(fourthSubscriptionId);
                        invocationsList.Add(third);
                    }
                )
            );
            fourthSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => invocationsList.Add(fourth)
                )
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(4, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(first, invocationsList[1]);
            Assert.AreEqual(fourth, invocationsList[2]);
            Assert.AreEqual(third, invocationsList[3]);
        }

        [TestMethod]
        public void WaitForStoreDoesNotBlockIfHandlerWasAlreadyExecuted()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            var firstSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => invocationsList.Add(first)
                )
            );
            var secondSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(firstSubscriptionId);
                        invocationsList.Add(second);
                    }
                )
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(2, invocationsList.Count);
            Assert.AreEqual(first, invocationsList[0]);
            Assert.AreEqual(second, invocationsList[1]);
        }

        [TestMethod]
        public void WaitForStoreDoesNotBlockIfHandlerWasUnregistered()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            var firstSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => invocationsList.Add(first)
                )
            );
            var secondSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(firstSubscriptionId);
                        invocationsList.Add(second);
                    }
                )
            );
            _Dispatcher.Unregister(firstSubscriptionId);

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(1, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
        }

        [TestMethod]
        public void HandlerWaitingForStoreBlocksUntilAwaitedStoreCompletes()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            var store = new MockDelegateStore(
                actionData => invocationsList.Add(second)
            );
            var firstSubscriptionId = _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(store);
                    invocationsList.Add(first);
                }
            );
            _Dispatcher.Register(store);

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(2, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(first, invocationsList[1]);
        }

        [TestMethod]
        public void StoreWaitingForHandlerBlocksUntilAwaitedHandlerCompletes()
        {
            const string first = "first";
            const string second = "second";
            var invocationsList = new List<string>();

            object secondSubscriptionId = null;
            _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(secondSubscriptionId);
                        invocationsList.Add(first);
                    }
                )
            );
            secondSubscriptionId = _Dispatcher.Register(
                new MockDelegateStore(
                    actionData => invocationsList.Add(second)
                )
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(2, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(first, invocationsList[1]);
        }

        [TestMethod]
        public void WaitingForMultipleIdsWaitsUntilEachCompletes()
        {
            const string first = "first";
            const string second = "second";
            const string third = "second";
            var invocationsList = new List<string>();

            object secondSubscriptionId = null;
            object thirdSubscriptionId = null;
            _Dispatcher.Register(
                actionData =>
                {
                    _Dispatcher.WaitFor(secondSubscriptionId, thirdSubscriptionId);
                    invocationsList.Add(first);
                }
            );
            secondSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(second)
            );
            thirdSubscriptionId = _Dispatcher.Register(
                actionData => invocationsList.Add(third)
            );

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(3, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(third, invocationsList[1]);
            Assert.AreEqual(first, invocationsList[2]);
        }

        [TestMethod]
        public void WaitingForMultipleStoresWaitsUntilEachCompletes()
        {
            const string first = "first";
            const string second = "second";
            const string third = "second";
            var invocationsList = new List<string>();

            Store secondStore = null;
            Store thirdStore = null;
            _Dispatcher.Register(
                new MockDelegateStore(
                    actionData =>
                    {
                        _Dispatcher.WaitFor(secondStore, thirdStore);
                        invocationsList.Add(first);
                    }
                )
            );
            secondStore = new MockDelegateStore(
                actionData => invocationsList.Add(second)
            );
            thirdStore = new MockDelegateStore(
                actionData => invocationsList.Add(third)
            );
            _Dispatcher.Register(secondStore);
            _Dispatcher.Register(thirdStore);

            _Dispatcher.Dispatch(null);

            Assert.AreEqual(3, invocationsList.Count);
            Assert.AreEqual(second, invocationsList[0]);
            Assert.AreEqual(third, invocationsList[1]);
            Assert.AreEqual(first, invocationsList[2]);
        }

        [TestMethod]
        public void IsDispatchingIsUpdatedWhileNotifyingSubscribers()
        {
            Assert.IsFalse(_Dispatcher.IsDispatching);

            _Dispatcher.Register(
                actionData => Assert.IsTrue(_Dispatcher.IsDispatching)
            );
            _Dispatcher.Dispatch(null);

            Assert.IsFalse(_Dispatcher.IsDispatching);
        }

        [TestMethod]
        public void IsDispatchingIsSetToFalseEvenIfAHandlerThrowsExcetpion()
        {
            Assert.IsFalse(_Dispatcher.IsDispatching);

            _Dispatcher.Register(
                actionData => throw new Exception()
            );
            Assert.ThrowsException<Exception>(() => _Dispatcher.Dispatch(null));

            Assert.IsFalse(_Dispatcher.IsDispatching);
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

        [TestMethod]
        public void WaitForNullThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.WaitFor(id: null));
            Assert.AreEqual(new ArgumentNullException("id").Message, exception.Message);
        }

        [TestMethod]
        public void WaitForNullStoreThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.WaitFor(store: null));
            Assert.AreEqual(new ArgumentNullException("store").Message, exception.Message);
        }

        [TestMethod]
        public void WaitForMultipleIdsWithNullCollectionThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.WaitFor(ids: null));
            Assert.AreEqual(new ArgumentNullException("ids").Message, exception.Message);
        }

        [TestMethod]
        public void WaitForMultipleIdsContainingNullValuesThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => _Dispatcher.WaitFor(new object[] { null }));
            Assert.AreEqual(new ArgumentException("Cannot contain 'null' ids.", "ids").Message, exception.Message);
        }

        [TestMethod]
        public void WaitForMultipleStoresWithNullCollectionThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => _Dispatcher.WaitFor(stores: null));
            Assert.AreEqual(new ArgumentNullException("stores").Message, exception.Message);
        }

        [TestMethod]
        public void WaitForMultipleStoresContainingNullValuesThrowsException()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => _Dispatcher.WaitFor(new Store[] { null }));
            Assert.AreEqual(new ArgumentException("Cannot contain 'null' stores.", "stores").Message, exception.Message);
        }
    }
}