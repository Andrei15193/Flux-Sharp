using FluxBase.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase.Tests
{
    [TestClass]
    public class DispatcherTests
    {
        [TestMethod]
        public Task RegisteringToDispatcherInvokesCallback()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;

                    testDispatcher.Register(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    await dispatch(null);

                    Assert.AreEqual(1, invocationCount);
                }
            );

        [TestMethod]
        public Task RegisteringStoreToDispatcherInvokesHandler()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    testDispatcher.Register(store);

                    await dispatch(null);

                    Assert.AreEqual(1, invocationCount);
                }
            );

        [TestMethod]
        public Task RegisteringToDispatcherTwiceInvokesCallbackOnce()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;

                    void Callback(object action) => Interlocked.Increment(ref invocationCount);

                    var firstRegistrationId = testDispatcher.Register(Callback);
                    var secondRegistrationId = testDispatcher.Register(Callback);

                    await dispatch(null);

                    Assert.AreEqual(1, invocationCount);
                    Assert.AreEqual(firstRegistrationId, secondRegistrationId);
                }
            );

        [TestMethod]
        public Task RegisteringStoreTwiceToDispatcherInvokesHandlerOnce()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    var firstRegistrationId = testDispatcher.Register(store);
                    var secondRegistrationId = testDispatcher.Register(store);

                    await dispatch(null);

                    Assert.AreEqual(1, invocationCount);
                    Assert.AreEqual(firstRegistrationId, secondRegistrationId);
                }
            );

        [TestMethod]
        public Task UnregisteringFromDispatcherNoLongerInvokesCallback()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;

                    var registrationId = testDispatcher.Register(
                        action => Interlocked.Increment(ref invocationCount)
                    );
                    testDispatcher.Unregister(registrationId);

                    await dispatch(null);

                    Assert.AreEqual(0, invocationCount);
                }
            );

        [TestMethod]
        public Task UnregisteringStoreFromDispatcherNoLongerInvokesHandler()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    testDispatcher.Register(store);
                    testDispatcher.Unregister(store);

                    await dispatch(null);

                    Assert.AreEqual(0, invocationCount);
                }
            );

        [TestMethod]
        public Task UnregisteringTwiceFromDispatcherReturnsFalseTheSecondTime()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var invocationCount = 0;

                    var registrationId = testDispatcher.Register(
                        action => Interlocked.Increment(ref invocationCount)
                    );
                    Assert.IsTrue(testDispatcher.Unregister(registrationId));

                    Assert.IsFalse(testDispatcher.Unregister(registrationId));
                }
            );

        [TestMethod]
        public Task UnregisteringStoreTwiceFromDispatcherReturnsFalseTheSecondTime()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    var registrationId = testDispatcher.Register(store);

                    Assert.IsTrue(testDispatcher.Unregister(store));
                    Assert.IsFalse(testDispatcher.Unregister(store));
                }
            );

        [TestMethod]
        public Task DispatchingNullPassesNull()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    object actualAction = null;

                    testDispatcher.Register(
                        action => Interlocked.Exchange(ref actualAction, action)
                    );

                    await dispatch(null);

                    Assert.IsNull(actualAction);
                }
            );

        [TestMethod]
        public Task DispatchPassesSameAction()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var expectedAction = new object();
                    object actualAction = null;

                    testDispatcher.Register(
                        action => Interlocked.Exchange(ref actualAction, action)
                    );

                    await dispatch(expectedAction);

                    Assert.AreSame(expectedAction, actualAction);
                }
            );

        [TestMethod]
        public Task WaitForBlocksUntilAwaitedHandlerCompletes()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    object secondSubscriptionId = null;
                    var firstSubscriptionId = testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(secondSubscriptionId);
                            invocationsList.Add(first);
                        }
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(second)
                    );

                    await dispatch(null);

                    Assert.AreEqual(2, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(first, invocationsList[1]);
                }
            );

        [TestMethod]
        public Task WaitForBlocksUntilHandlersThatThemselvesWaitAwaitsTheirCompletion()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const int chainedHandlersCount = 500;
                    var registrationIds = new List<object>();
                    var invocationsList = new List<string>();

                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        var indexCopy = index;
                        registrationIds.Add(
                            testDispatcher.Register(
                                action =>
                                {
                                    if (indexCopy < chainedHandlersCount - 1)
                                        testDispatcher.WaitFor(registrationIds[indexCopy + 1]);
                                    invocationsList.Add($"Blocked {indexCopy}");
                                }
                            )
                        );
                        testDispatcher.Register(
                            action => invocationsList.Add($"Not blocked {indexCopy}")
                        );
                    }

                    await dispatch(null);

                    Assert.AreEqual(chainedHandlersCount * 2, invocationsList.Count);
                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        Assert.AreEqual($"Blocked {chainedHandlersCount - index - 1}", invocationsList[index]);
                        Assert.AreEqual($"Not blocked {index}", invocationsList[index + chainedHandlersCount]);
                    }
                }
            );

        [TestMethod]
        public Task WaitForCausingDeadlockIsDetected()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    object firstSubscriptionId = null;
                    object secondSubscriptionId = null;
                    firstSubscriptionId = testDispatcher.Register(
                        action => testDispatcher.WaitFor(secondSubscriptionId)
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        action => testDispatcher.WaitFor(firstSubscriptionId)
                    );

                    var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.AreEqual(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [TestMethod]
        public Task WaitForCausingDeadlockThroughChainedBlocksIsDetected()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const int chainedHandlersCount = 500;
                    var registrationIds = new List<object>();

                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        var indexCopy = index;
                        registrationIds.Add(
                            testDispatcher.Register(
                                action => testDispatcher.WaitFor(registrationIds[(indexCopy + 1) % chainedHandlersCount])
                            )
                        );
                    }

                    var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.AreEqual(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [TestMethod]
        public Task WaitForBlocksUntilAwaitedHandlerCompletesWithTwoSeparateDependencyChains()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    const string third = "third";
                    const string fourth = "fourth";
                    var invocationsList = new List<string>();

                    object secondSubscriptionId = null;
                    object fourthSubscriptionId = null;
                    var firstSubscriptionId = testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(secondSubscriptionId);
                            invocationsList.Add(first);
                        }
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(second)
                    );
                    var thirdSubscriptionId = testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(fourthSubscriptionId);
                            invocationsList.Add(third);
                        }
                    );
                    fourthSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(fourth)
                    );

                    await dispatch(null);

                    Assert.AreEqual(4, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(first, invocationsList[1]);
                    Assert.AreEqual(fourth, invocationsList[2]);
                    Assert.AreEqual(third, invocationsList[3]);
                }
            );

        [TestMethod]
        public Task WaitForDoesNotBlockIfHandlerWasAlreadyExecuted()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    var firstSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(first)
                    );
                    var secondSubscriptionId = testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(firstSubscriptionId);
                            invocationsList.Add(second);
                        }
                    );

                    await dispatch(null);

                    Assert.AreEqual(2, invocationsList.Count);
                    Assert.AreEqual(first, invocationsList[0]);
                    Assert.AreEqual(second, invocationsList[1]);
                }
            );

        [TestMethod]
        public Task WaitForDoesNotBlockIfHandlerWasUnregistered()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    var firstSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(first)
                    );
                    var secondSubscriptionId = testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(firstSubscriptionId);
                            invocationsList.Add(second);
                        }
                    );
                    testDispatcher.Unregister(firstSubscriptionId);

                    await dispatch(null);

                    Assert.AreEqual(1, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                }
            );

        [TestMethod]
        public Task WaitForStoreBlocksUntilAwaitedHandlerCompletes()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    object secondSubscriptionId = null;
                    var firstSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(secondSubscriptionId);
                                invocationsList.Add(first);
                            }
                        )
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => invocationsList.Add(second)
                        )
                    );

                    await dispatch(null);

                    Assert.AreEqual(2, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(first, invocationsList[1]);
                }
            );

        [TestMethod]
        public Task WaitForStoreBlocksUntilHandlersThatThemselvesWaitAwaitsTheirCompletion()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const int chainedHandlersCount = 500;
                    var registrationIds = new List<object>();
                    var invocationsList = new List<string>();

                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        var indexCopy = index;
                        registrationIds.Add(
                            testDispatcher.Register(
                                new MockDelegateStore(
                                    action =>
                                    {
                                        if (indexCopy < chainedHandlersCount - 1)
                                            testDispatcher.WaitFor(registrationIds[indexCopy + 1]);
                                        invocationsList.Add($"Blocked {indexCopy}");
                                    }
                                )
                            )
                        );
                        testDispatcher.Register(
                            new MockDelegateStore(
                                action => invocationsList.Add($"Not blocked {indexCopy}")
                            )
                        );
                    }

                    await dispatch(null);

                    Assert.AreEqual(chainedHandlersCount * 2, invocationsList.Count);
                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        Assert.AreEqual($"Blocked {chainedHandlersCount - index - 1}", invocationsList[index]);
                        Assert.AreEqual($"Not blocked {index}", invocationsList[index + chainedHandlersCount]);
                    }
                }
            );

        [TestMethod]
        public Task WaitForStoreCausingDeadlockIsDetected()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    object firstSubscriptionId = null;
                    object secondSubscriptionId = null;
                    firstSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => testDispatcher.WaitFor(secondSubscriptionId)
                        )
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => testDispatcher.WaitFor(firstSubscriptionId)
                        )
                    );

                    var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.AreEqual(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [TestMethod]
        public Task WaitForStoreCausingDeadlockThroughChainedBlocksIsDetected()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const int chainedHandlersCount = 500;
                    var registrationIds = new List<object>();

                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        var indexCopy = index;
                        registrationIds.Add(
                            testDispatcher.Register(
                                new MockDelegateStore(
                                    action => testDispatcher.WaitFor(registrationIds[(indexCopy + 1) % chainedHandlersCount])
                                )
                            )
                        );
                    }

                    var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.AreEqual(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [TestMethod]
        public Task WaitForStoreBlocksUntilAwaitedHandlerCompletesWithTwoSeparateDependencyChains()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    const string third = "third";
                    const string fourth = "fourth";
                    var invocationsList = new List<string>();

                    object secondSubscriptionId = null;
                    object fourthSubscriptionId = null;
                    var firstSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(secondSubscriptionId);
                                invocationsList.Add(first);
                            }
                        )
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => invocationsList.Add(second)
                        )
                    );
                    var thirdSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(fourthSubscriptionId);
                                invocationsList.Add(third);
                            }
                        )
                    );
                    fourthSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => invocationsList.Add(fourth)
                        )
                    );

                    await dispatch(null);

                    Assert.AreEqual(4, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(first, invocationsList[1]);
                    Assert.AreEqual(fourth, invocationsList[2]);
                    Assert.AreEqual(third, invocationsList[3]);
                }
            );

        [TestMethod]
        public Task WaitForStoreDoesNotBlockIfHandlerWasAlreadyExecuted()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    var firstSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => invocationsList.Add(first)
                        )
                    );
                    var secondSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(firstSubscriptionId);
                                invocationsList.Add(second);
                            }
                        )
                    );

                    await dispatch(null);

                    Assert.AreEqual(2, invocationsList.Count);
                    Assert.AreEqual(first, invocationsList[0]);
                    Assert.AreEqual(second, invocationsList[1]);
                }
            );

        [TestMethod]
        public Task WaitForStoreDoesNotBlockIfHandlerWasUnregistered()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    var firstSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => invocationsList.Add(first)
                        )
                    );
                    var secondSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(firstSubscriptionId);
                                invocationsList.Add(second);
                            }
                        )
                    );
                    testDispatcher.Unregister(firstSubscriptionId);

                    await dispatch(null);

                    Assert.AreEqual(1, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                }
            );

        [TestMethod]
        public Task HandlerWaitingForStoreBlocksUntilAwaitedStoreCompletes()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    var store = new MockDelegateStore(
                        action => invocationsList.Add(second)
                    );
                    var firstSubscriptionId = testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(store);
                            invocationsList.Add(first);
                        }
                    );
                    testDispatcher.Register(store);

                    await dispatch(null);

                    Assert.AreEqual(2, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(first, invocationsList[1]);
                }
            );

        [TestMethod]
        public Task StoreWaitingForHandlerBlocksUntilAwaitedHandlerCompletes()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    var invocationsList = new List<string>();

                    object secondSubscriptionId = null;
                    testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(secondSubscriptionId);
                                invocationsList.Add(first);
                            }
                        )
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        new MockDelegateStore(
                            action => invocationsList.Add(second)
                        )
                    );

                    await dispatch(null);

                    Assert.AreEqual(2, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(first, invocationsList[1]);
                }
            );

        [TestMethod]
        public Task WaitingForMultipleIdsWaitsUntilEachCompletes()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    const string third = "second";
                    var invocationsList = new List<string>();

                    object secondSubscriptionId = null;
                    object thirdSubscriptionId = null;
                    testDispatcher.Register(
                        action =>
                        {
                            testDispatcher.WaitFor(secondSubscriptionId, thirdSubscriptionId);
                            invocationsList.Add(first);
                        }
                    );
                    secondSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(second)
                    );
                    thirdSubscriptionId = testDispatcher.Register(
                        action => invocationsList.Add(third)
                    );

                    await dispatch(null);

                    Assert.AreEqual(3, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(third, invocationsList[1]);
                    Assert.AreEqual(first, invocationsList[2]);
                }
            );

        [TestMethod]
        public Task WaitingForMultipleStoresWaitsUntilEachCompletes()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    const string first = "first";
                    const string second = "second";
                    const string third = "second";
                    var invocationsList = new List<string>();

                    Store secondStore = null;
                    Store thirdStore = null;
                    testDispatcher.Register(
                        new MockDelegateStore(
                            action =>
                            {
                                testDispatcher.WaitFor(secondStore, thirdStore);
                                invocationsList.Add(first);
                            }
                        )
                    );
                    secondStore = new MockDelegateStore(
                        action => invocationsList.Add(second)
                    );
                    thirdStore = new MockDelegateStore(
                        action => invocationsList.Add(third)
                    );
                    testDispatcher.Register(secondStore);
                    testDispatcher.Register(thirdStore);

                    await dispatch(null);

                    Assert.AreEqual(3, invocationsList.Count);
                    Assert.AreEqual(second, invocationsList[0]);
                    Assert.AreEqual(third, invocationsList[1]);
                    Assert.AreEqual(first, invocationsList[2]);
                }
            );

        [TestMethod]
        public Task IsDispatchingIsUpdatedWhileNotifyingSubscribers()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    Assert.IsFalse(testDispatcher.IsDispatching);

                    testDispatcher.Register(
                        action => Assert.IsTrue(testDispatcher.IsDispatching)
                    );

                    await dispatch(null);

                    Assert.IsFalse(testDispatcher.IsDispatching);
                }
            );

        [TestMethod]
        public Task IsDispatchingIsSetToFalseEvenIfAHandlerThrowsExcetpion()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    Assert.IsFalse(testDispatcher.IsDispatching);

                    testDispatcher.Register(
                        action => throw new Exception()
                    );

                    await Assert.ThrowsExceptionAsync<Exception>(() => dispatch(null));

                    Assert.IsFalse(testDispatcher.IsDispatching);
                }
            );

        [TestMethod]
        public Task RegisteringNullCallbackThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.Register(callback: null));
                    Assert.AreEqual(new ArgumentNullException("callback").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task RegisteringNullStoreThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.Register(store: null));
                    Assert.AreEqual(new ArgumentNullException("store").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task UnregisteringNullIdThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.Unregister(id: null));
                    Assert.AreEqual(new ArgumentNullException("id").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task UnregisteringNullStoreThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.Unregister(store: null));
                    Assert.AreEqual(new ArgumentNullException("store").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task WaitForNullThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.WaitFor(id: null));
                    Assert.AreEqual(new ArgumentNullException("id").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task WaitForNullStoreThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.WaitFor(store: null));
                    Assert.AreEqual(new ArgumentNullException("store").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task WaitForMultipleIdsWithNullCollectionThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.WaitFor(ids: null));
                    Assert.AreEqual(new ArgumentNullException("ids").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task WaitForMultipleIdsContainingNullValuesThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentException>(() => testDispatcher.WaitFor(new object[] { null }));
                    Assert.AreEqual(new ArgumentException("Cannot contain 'null' ids.", "ids").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task WaitForMultipleStoresWithNullCollectionThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentNullException>(() => testDispatcher.WaitFor(stores: null));
                    Assert.AreEqual(new ArgumentNullException("stores").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task WaitForMultipleStoresContainingNullValuesThrowsException()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    await Task.Yield();
                    var exception = Assert.ThrowsException<ArgumentException>(() => testDispatcher.WaitFor(new Store[] { null }));
                    Assert.AreEqual(new ArgumentException("Cannot contain 'null' stores.", "stores").Message, exception.Message);
                }
            );

        [TestMethod]
        public Task MiddlewareIsBeingCalledBeforeActualDispatch()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var callList = new List<string>();

                    testDispatcher.Register(action => callList.Add("dispatch"));
                    testDispatcher.Use(
                        new MockMiddleware(
                            context =>
                            {
                                callList.Add("middleware-before-1");
                                context.Next();
                                callList.Add("middleware-after-1");
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware(
                            context =>
                            {
                                callList.Add("middleware-before-2");
                                context.Next(new object());
                                callList.Add("middleware-after-2");
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware(
                            context =>
                            {
                                callList.Add("middleware-before-3");
                                context.Dispatch(new object());
                                callList.Add("middleware-after-3");
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware(
                            context => throw new InvalidOperationException()
                        )
                    );

                    await dispatch(null);

                    Assert.IsTrue(
                        callList.SequenceEqual(new[]
                        {
                            "middleware-before-1",
                            "middleware-before-2",
                            "middleware-before-3",
                            "dispatch",
                            "middleware-after-3",
                            "middleware-after-2",
                            "middleware-after-1"
                        }),
                        $"Actual: {string.Join(", ", callList)}"
                    );
                }
            );

        [TestMethod]
        public Task ModifyingTheActionPropagatesToAllFutureMiddleware()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var initialAction = new object();
                    object firstAction = null;
                    object secondAction = null;

                    testDispatcher.Use(
                        new MockMiddleware(
                            context =>
                            {
                                firstAction = context.Action;
                                context.Next(new object());
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware(
                            context => secondAction = context.Action
                        )
                    );

                    await dispatch(initialAction);

                    Assert.AreSame(initialAction, firstAction);
                    Assert.AreNotSame(firstAction, secondAction);
                }
            );

        [TestMethod]
        public Task UsingConcreteActionMiddlewareGetsCalledOnlyWhenCompatible()
            => _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var callList = new List<string>();

                    testDispatcher.Register(action => callList.Add("dispatch"));
                    testDispatcher.Use(
                        new MockMiddleware<int?>(
                            context =>
                            {
                                callList.Add("middleware-1");
                                context.Next();
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware<object>(
                            context =>
                            {
                                callList.Add("middleware-2");
                                context.Next();
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware<string>(
                            context =>
                            {
                                callList.Add("middleware-3");
                                context.Next();
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware<int>(
                            context =>
                            {
                                callList.Add("middleware-4");
                                context.Next();
                            }
                        )
                    );

                    await dispatch(null);
                    await dispatch(string.Empty);

                    Assert.IsTrue(
                        callList.SequenceEqual(new[]
                        {
                            "middleware-1",
                            "middleware-2",
                            "middleware-3",
                            "dispatch",
                            "middleware-2",
                            "middleware-3",
                            "dispatch"
                        }),
                        $"Actual: {string.Join(", ", callList)}"
                    );
                }
            );

        private static async Task _AssertAsync(Func<Func<object, Task>, TestDispatcher, Task> callback)
        {
            var syncDispatcher = new TestDispatcher();
            await callback(
                action =>
                {
                    syncDispatcher.Dispatch(action);
                    return Task.FromResult<object>(null);
                },
                syncDispatcher
            );

            var asyncDispatcher = new TestDispatcher();
            await callback(asyncDispatcher.DispatchAsync, asyncDispatcher);
        }

        private sealed class TestDispatcher : Dispatcher
        {
            public new void Dispatch(object action)
            {
                throw new InvalidOperationException("Use the provided 'dispatch' callback to dispatch actions.");
            }

            public new Task DispatchAsync(object action)
                => DispatchAsync(action, CancellationToken.None);

            public new Task DispatchAsync(object action, CancellationToken cancellationToken)
            {
                Dispatch(action);
                return Task.FromResult<object>(null);
            }
        }
    }
}