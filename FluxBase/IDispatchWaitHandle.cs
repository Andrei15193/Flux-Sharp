using System;
using System.Collections.Generic;

namespace FluxBase
{
    /// <summary>Represents an interface for waiting after action handlers to process a current dispatch.</summary>
    public interface IDispatchWaitHandle
    {
        /// <summary>Waits for the registered action handler with the provided <paramref name="id"/> to complete.</summary>
        /// <param name="id">The ID object identifying the action handler to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks if the referred callback is registered and has not yet been executed.</remarks>
        void WaitFor(object id);

        /// <summary>Waits for the registered action handlers with the provided <paramref name="ids"/> to complete.</summary>
        /// <param name="ids">A collection of IDs identifying the action handlers to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred callbacks that are registered and have not yet been executed.</remarks>
        void WaitFor(IEnumerable<object> ids);

        /// <summary>Waits for the registered action handlers with the provided <paramref name="ids"/> to complete.</summary>
        /// <param name="ids">A collection of IDs identifying the action handlers to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred callbacks that are registered and have not yet been executed.</remarks>
        void WaitFor(params object[] ids);

        /// <summary>Waits for the provided <paramref name="store"/> to complete.</summary>
        /// <param name="store">A previously subscribed <see cref="Store"/> to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred <paramref name="store"/> is registered and has not yet handled the current action dispatch.</remarks>
        void WaitFor(Store store);

        /// <summary>Waits for the provided <paramref name="stores"/> to complete.</summary>
        /// <param name="stores">A collection of previously subscribed <see cref="Store"/>s to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stores"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred <paramref name="stores"/> that are registered and have not yet handled the current action dispatch.</remarks>
        void WaitFor(IEnumerable<Store> stores);

        /// <summary>Waits for the provided <paramref name="stores"/> to complete.</summary>
        /// <param name="stores">A collection of previously subscribed <see cref="Store"/>s to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stores"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred <paramref name="stores"/> that are registered and have not yet handled the current action dispatch.</remarks>
        void WaitFor(params Store[] stores);
    }
}