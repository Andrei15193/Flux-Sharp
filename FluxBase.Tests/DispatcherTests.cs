using FluxBase.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluxBase.Tests
{
    public class DispatcherTests
    {
        [Fact]
        public async Task RegisteringToDispatcherInvokesCallback()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;

                    testDispatcher.Register(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    await dispatch(null);

                    Assert.Equal(1, invocationCount);
                }
            );

        [Fact]
        public async Task RegisteringStoreToDispatcherInvokesHandler()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    testDispatcher.Register(store);

                    await dispatch(null);

                    Assert.Equal(1, invocationCount);
                }
            );

        [Fact]
        public async Task RegisteringToDispatcherTwiceInvokesCallbackOnce()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;

                    void Callback(object action) => Interlocked.Increment(ref invocationCount);

                    var firstRegistrationId = testDispatcher.Register(Callback);
                    var secondRegistrationId = testDispatcher.Register(Callback);

                    await dispatch(null);

                    Assert.Equal(1, invocationCount);
                    Assert.Equal(firstRegistrationId, secondRegistrationId);
                }
            );

        [Fact]
        public async Task RegisteringStoreTwiceToDispatcherInvokesHandlerOnce()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    var firstRegistrationId = testDispatcher.Register(store);
                    var secondRegistrationId = testDispatcher.Register(store);

                    await dispatch(null);

                    Assert.Equal(1, invocationCount);
                    Assert.Equal(firstRegistrationId, secondRegistrationId);
                }
            );

        [Fact]
        public async Task UnregisteringFromDispatcherNoLongerInvokesCallback()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;

                    var registrationId = testDispatcher.Register(
                        action => Interlocked.Increment(ref invocationCount)
                    );
                    testDispatcher.Unregister(registrationId);

                    await dispatch(null);

                    Assert.Equal(0, invocationCount);
                }
            );

        [Fact]
        public async Task UnregisteringStoreFromDispatcherNoLongerInvokesHandler()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var invocationCount = 0;
                    var store = new MockDelegateStore(
                        action => Interlocked.Increment(ref invocationCount)
                    );

                    testDispatcher.Register(store);
                    testDispatcher.Unregister(store);

                    await dispatch(null);

                    Assert.Equal(0, invocationCount);
                }
            );

        [Fact]
        public void UnregisteringTwiceFromDispatcherReturnsFalseTheSecondTime()
        {
            var testDispatcher = new Dispatcher();

            var invocationCount = 0;

            var registrationId = testDispatcher.Register(
                action => Interlocked.Increment(ref invocationCount)
            );
            Assert.True(testDispatcher.Unregister(registrationId));

            Assert.False(testDispatcher.Unregister(registrationId));
        }

        [Fact]
        public void UnregisteringStoreTwiceFromDispatcherReturnsFalseTheSecondTime()
        {
            var testDispatcher = new Dispatcher();

            var invocationCount = 0;
            var store = new MockDelegateStore(
                action => Interlocked.Increment(ref invocationCount)
            );

            var registrationId = testDispatcher.Register(store);

            Assert.True(testDispatcher.Unregister(store));
            Assert.False(testDispatcher.Unregister(store));
        }

        [Fact]
        public async Task DispatchingNullPassesNull()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    object actualAction = null;

                    testDispatcher.Register(
                        action => Interlocked.Exchange(ref actualAction, action)
                    );

                    await dispatch(null);

                    Assert.Null(actualAction);
                }
            );

        [Fact]
        public async Task DispatchPassesSameAction()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    var expectedAction = new object();
                    object actualAction = null;

                    testDispatcher.Register(
                        action => Interlocked.Exchange(ref actualAction, action)
                    );

                    await dispatch(expectedAction);

                    Assert.Same(expectedAction, actualAction);
                }
            );

        [Fact]
        public async Task WaitForBlocksUntilAwaitedHandlerCompletes()
            => await _AssertAsync(
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

                    Assert.Equal(2, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(first, invocationsList[1]);
                }
            );

        [Fact]
        public async Task WaitForBlocksUntilHandlersThatThemselvesWaitAwaitsTheirCompletion()
            => await _AssertAsync(
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

                    Assert.Equal(chainedHandlersCount * 2, invocationsList.Count);
                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        Assert.Equal($"Blocked {chainedHandlersCount - index - 1}", invocationsList[index]);
                        Assert.Equal($"Not blocked {index}", invocationsList[index + chainedHandlersCount]);
                    }
                }
            );

        [Fact]
        public async Task WaitForCausingDeadlockIsDetected()
            => await _AssertAsync(
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

                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.Equal(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [Fact]
        public async Task WaitForCausingDeadlockThroughChainedBlocksIsDetected()
            => await _AssertAsync(
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

                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.Equal(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [Fact]
        public async Task WaitForBlocksUntilAwaitedHandlerCompletesWithTwoSeparateDependencyChains()
            => await _AssertAsync(
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

                    Assert.Equal(4, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(first, invocationsList[1]);
                    Assert.Equal(fourth, invocationsList[2]);
                    Assert.Equal(third, invocationsList[3]);
                }
            );

        [Fact]
        public async Task WaitForDoesNotBlockIfHandlerWasAlreadyExecuted()
            => await _AssertAsync(
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

                    Assert.Equal(2, invocationsList.Count);
                    Assert.Equal(first, invocationsList[0]);
                    Assert.Equal(second, invocationsList[1]);
                }
            );

        [Fact]
        public async Task WaitForDoesNotBlockIfHandlerWasUnregistered()
            => await _AssertAsync(
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

                    Assert.Single(invocationsList);
                    Assert.Equal(second, invocationsList[0]);
                }
            );

        [Fact]
        public async Task WaitForStoreBlocksUntilAwaitedHandlerCompletes()
            => await _AssertAsync(
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

                    Assert.Equal(2, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(first, invocationsList[1]);
                }
            );

        [Fact]
        public async Task WaitForStoreBlocksUntilHandlersThatThemselvesWaitAwaitsTheirCompletion()
            => await _AssertAsync(
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

                    Assert.Equal(chainedHandlersCount * 2, invocationsList.Count);
                    for (var index = 0; index < chainedHandlersCount; index++)
                    {
                        Assert.Equal($"Blocked {chainedHandlersCount - index - 1}", invocationsList[index]);
                        Assert.Equal($"Not blocked {index}", invocationsList[index + chainedHandlersCount]);
                    }
                }
            );

        [Fact]
        public async Task WaitForStoreCausingDeadlockIsDetected()
            => await _AssertAsync(
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

                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.Equal(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [Fact]
        public async Task WaitForStoreCausingDeadlockThroughChainedBlocksIsDetected()
            => await _AssertAsync(
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

                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatch(null));

                    Assert.Equal(
                        new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.").Message,
                        exception.Message
                    );
                }
            );

        [Fact]
        public async Task WaitForStoreBlocksUntilAwaitedHandlerCompletesWithTwoSeparateDependencyChains()
            => await _AssertAsync(
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

                    Assert.Equal(4, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(first, invocationsList[1]);
                    Assert.Equal(fourth, invocationsList[2]);
                    Assert.Equal(third, invocationsList[3]);
                }
            );

        [Fact]
        public async Task WaitForStoreDoesNotBlockIfHandlerWasAlreadyExecuted()
            => await _AssertAsync(
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

                    Assert.Equal(2, invocationsList.Count);
                    Assert.Equal(first, invocationsList[0]);
                    Assert.Equal(second, invocationsList[1]);
                }
            );

        [Fact]
        public async Task WaitForStoreDoesNotBlockIfHandlerWasUnregistered()
            => await _AssertAsync(
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

                    Assert.Single(invocationsList);
                    Assert.Equal(second, invocationsList[0]);
                }
            );

        [Fact]
        public async Task HandlerWaitingForStoreBlocksUntilAwaitedStoreCompletes()
            => await _AssertAsync(
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

                    Assert.Equal(2, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(first, invocationsList[1]);
                }
            );

        [Fact]
        public async Task StoreWaitingForHandlerBlocksUntilAwaitedHandlerCompletes()
            => await _AssertAsync(
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

                    Assert.Equal(2, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(first, invocationsList[1]);
                }
            );

        [Fact]
        public async Task WaitingForMultipleIdsWaitsUntilEachCompletes()
            => await _AssertAsync(
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

                    Assert.Equal(3, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(third, invocationsList[1]);
                    Assert.Equal(first, invocationsList[2]);
                }
            );

        [Fact]
        public async Task WaitingForMultipleStoresWaitsUntilEachCompletes()
            => await _AssertAsync(
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

                    Assert.Equal(3, invocationsList.Count);
                    Assert.Equal(second, invocationsList[0]);
                    Assert.Equal(third, invocationsList[1]);
                    Assert.Equal(first, invocationsList[2]);
                }
            );

        [Fact]
        public async Task IsDispatchingIsUpdatedWhileNotifyingSubscribers()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    Assert.False(testDispatcher.IsDispatching);

                    testDispatcher.Register(
                        action => Assert.True(testDispatcher.IsDispatching)
                    );

                    await dispatch(null);

                    Assert.False(testDispatcher.IsDispatching);
                }
            );

        [Fact]
        public async Task IsDispatchingIsSetToFalseEvenIfAHandlerThrowsExcetpion()
            => await _AssertAsync(
                async (dispatch, testDispatcher) =>
                {
                    Assert.False(testDispatcher.IsDispatching);

                    testDispatcher.Register(
                        action => throw new Exception()
                    );

                    await Assert.ThrowsAsync<Exception>(() => dispatch(null));

                    Assert.False(testDispatcher.IsDispatching);
                }
            );

        [Fact]
        public void RegisteringNullCallbackThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.Register(callback: null));
            Assert.Equal(new ArgumentNullException("callback").Message, exception.Message);
        }

        [Fact]
        public void RegisteringNullStoreThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.Register(store: null));
            Assert.Equal(new ArgumentNullException("store").Message, exception.Message);
        }

        [Fact]
        public void UnregisteringNullIdThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.Unregister(id: null));
            Assert.Equal(new ArgumentNullException("id").Message, exception.Message);
        }

        [Fact]
        public void UnregisteringNullStoreThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.Unregister(store: null));
            Assert.Equal(new ArgumentNullException("store").Message, exception.Message);
        }

        [Fact]
        public void ConfiguringNullMiddlewareThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.Use(middleware: null));
            Assert.Equal(new ArgumentNullException("middleware").Message, exception.Message);
        }

        [Fact]
        public void ConfiguringTypedNullMiddlewareThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.Use<object>(middleware: null));
            Assert.Equal(new ArgumentNullException("middleware").Message, exception.Message);
        }

        [Fact]
        public void WaitForNullThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.WaitFor(id: null));
            Assert.Equal(new ArgumentNullException("id").Message, exception.Message);
        }

        [Fact]
        public void WaitForNullStoreThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.WaitFor(store: null));
            Assert.Equal(new ArgumentNullException("store").Message, exception.Message);
        }

        [Fact]
        public void WaitForMultipleIdsWithNullCollectionThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.WaitFor(ids: null));
            Assert.Equal(new ArgumentNullException("ids").Message, exception.Message);
        }

        [Fact]
        public void WaitForMultipleIdsContainingNullValuesThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentException>(() => testDispatcher.WaitFor(new object[] { null }));
            Assert.Equal(new ArgumentException("Cannot contain 'null' ids.", "ids").Message, exception.Message);
        }

        [Fact]
        public void WaitForMultipleStoresWithNullCollectionThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentNullException>(() => testDispatcher.WaitFor(stores: null));
            Assert.Equal(new ArgumentNullException("stores").Message, exception.Message);
        }

        [Fact]
        public void WaitForMultipleStoresContainingNullValuesThrowsException()
        {
            var testDispatcher = new Dispatcher();

            var exception = Assert.Throws<ArgumentException>(() => testDispatcher.WaitFor(new Store[] { null }));
            Assert.Equal(new ArgumentException("Cannot contain 'null' stores.", "stores").Message, exception.Message);
        }

        [Fact]
        public void WaitForOneIdWhileThereIsNoActiveDispatchThrowsException()
        {
            var dispatcher = new TestDispatcher();
            var actionHandlerId = dispatcher.Register(delegate { });

            var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.WaitFor(actionHandlerId));

            Assert.Equal(new InvalidOperationException("Cannot wait for action handler when there is no active dispatch.").Message, exception.Message);
        }

        [Fact]
        public void WaitForMultipleIdsWhileThereIsNoActiveDispatchThrowsException()
        {
            var dispatcher = new TestDispatcher();
            var firstActionHandlerId = dispatcher.Register(delegate { });
            var secondActionHandlerId = dispatcher.Register(delegate { });

            var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.WaitFor(firstActionHandlerId, secondActionHandlerId));

            Assert.Equal(new InvalidOperationException("Cannot wait for action handler when there is no active dispatch.").Message, exception.Message);
        }

        [Fact]
        public void WaitForOneStoreWhileThereIsNoActiveDispatchThrowsException()
        {
            var dispatcher = new TestDispatcher();
            Store store = new MockDelegateStore(delegate { });
            dispatcher.Register(store);

            var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.WaitFor(store));

            Assert.Equal(new InvalidOperationException("Cannot wait for store when there is no active dispatch.").Message, exception.Message);
        }

        [Fact]
        public void WaitForMultipleStoresWhileThereIsNoActiveDispatchThrowsException()
        {
            var dispatcher = new TestDispatcher();
            Store firstStore = new MockDelegateStore(delegate { });
            dispatcher.Register(firstStore);

            Store secondStore = new MockDelegateStore(delegate { });
            dispatcher.Register(secondStore);

            var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.WaitFor(firstStore, secondStore));

            Assert.Equal(new InvalidOperationException("Cannot wait for store when there is no active dispatch.").Message, exception.Message);
        }

        [Fact]
        public async Task MiddlewareIsBeingCalledBeforeActualDispatch()
            => await _AssertAsync(
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
                            },
                            async (context, cancellationToken) =>
                            {
                                callList.Add("middleware-before-1");
                                await context.NextAsync(cancellationToken);
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
                            },
                            async (context, cancellationToken) =>
                            {
                                callList.Add("middleware-before-2");
                                await context.NextAsync(new object(), cancellationToken);
                                callList.Add("middleware-after-2");
                            }
                        )
                    );

                    await dispatch(null);

                    Assert.True(
                        callList.SequenceEqual(new[]
                        {
                            "middleware-before-1",
                            "middleware-before-2",
                            "dispatch",
                            "middleware-after-2",
                            "middleware-after-1"
                        }),
                        $"Actual: {string.Join(", ", callList)}"
                    );
                }
            );

        [Fact]
        public async Task ModifyingTheActionPropagatesToAllFutureMiddleware()
            => await _AssertAsync(
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
                            },
                            async (context, cancellationToken) =>
                            {
                                firstAction = context.Action;
                                await context.NextAsync(new object(), cancellationToken);
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware(
                            context => secondAction = context.Action,
                            (context, cancellationToken) =>
                            {
                                secondAction = context.Action;

                                return Task.FromResult<object>(null);
                            }
                        )
                    );

                    await dispatch(initialAction);

                    Assert.Same(initialAction, firstAction);
                    Assert.NotSame(firstAction, secondAction);
                }
            );

        [Fact]
        public async Task UsingConcreteActionMiddlewareGetsCalledOnlyWhenCompatible()
            => await _AssertAsync(
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
                            },
                            async (context, cancellationToken) =>
                            {
                                callList.Add("middleware-1");
                                await context.NextAsync(cancellationToken);
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware<object>(
                            context =>
                            {
                                callList.Add("middleware-2");
                                context.Next();
                            },
                            async (context, cancellationToken) =>
                            {
                                callList.Add("middleware-2");
                                await context.NextAsync(cancellationToken);
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware<string>(
                            context =>
                            {
                                callList.Add("middleware-3");
                                context.Next();
                            },
                            async (context, cancellationToken) =>
                            {
                                callList.Add("middleware-3");
                                await context.NextAsync(cancellationToken);
                            }
                        )
                    );
                    testDispatcher.Use(
                        new MockMiddleware<int>(
                            context =>
                            {
                                callList.Add("middleware-4");
                                context.Next();
                            },
                            async (context, cancellationToken) =>
                            {
                                callList.Add("middleware-4");
                                await context.NextAsync(cancellationToken);
                            }
                        )
                    );

                    await dispatch(null);
                    await dispatch(string.Empty);

                    Assert.True(
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
                    ((Dispatcher)syncDispatcher).Dispatch(action);
                    return Task.FromResult<object>(null);
                },
                syncDispatcher
            );

            var asyncDispatcher = new TestDispatcher();
            await callback(((Dispatcher)asyncDispatcher).DispatchAsync, asyncDispatcher);
        }

        private sealed class TestDispatcher : Dispatcher
        {
            [Obsolete("Use the provided 'dispatch' callback to dispatch actions.", true)]
            public new void Dispatch(object action)
                => base.Dispatch(action);

            [Obsolete("Use the provided 'dispatch' callback to dispatch actions.", true)]
            public new Task DispatchAsync(object action)
                => base.DispatchAsync(action);

            [Obsolete("Use the provided 'dispatch' callback to dispatch actions.", true)]
            public new Task DispatchAsync(object action, CancellationToken cancellationToken)
                => base.DispatchAsync(action, cancellationToken);
        }
    }
}