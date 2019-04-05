#if !NET20 && !NET30 && !NET35
using System.Threading;
using System.Threading.Tasks;
#endif

namespace FluxBase
{
    /// <summary>
    /// Represents a middleware pipeline element for handling actions before and after they are actually dispatched to action handlers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A middleware pipeline can read the action that is currently being dispatched and decide to modify it, skip all the following middleware
    /// handlers or dispatch an action bypassing all following middleware handlers.
    /// </para>
    /// <para>
    /// Middleware is similar to filters as they can be used in the same way for error reporting or logging, but can be used to split an action
    /// dispatch into multiple actual action dispatches.
    /// </para>
    /// <para>
    /// A middleware implementation must cover both synchronous and asynchronous flows as neither can be adapted to the other. This is mostly
    /// because asynchronous methods cannot be adapted to synchronous ones if continuations need to execute on the same thread.
    /// </para>
    /// <para>
    /// The problem boils down to "blocking" the asynchronous call until it completes when we are using the synchronous method. This is impossible
    /// because all dispatches must be carried out on the UI thread (when a dispatch is initiated from the UI thread, this is the expected behaviour)
    /// in order execute action handlers (stores) on the UI thread that eventually notify the UI components and update themselves (through binding expressions).
    /// </para>
    /// <para>
    /// This restriction implies that a synchronous dispatch initiated on the UI thread must complete after the action was handled by all
    /// registered stores (or action handlers). If along the way we have an asynchronous operation (e.g.: an async middleware) then control
    /// is returned from the async method before it completes and that async middleware may dispatch actions that need to be handled on the
    /// UI thread.
    /// </para>
    /// <para>
    /// Waiting for the async middleware to execute on the UI thread creates a deadlock because all methods (and fragments resulting from
    /// async transformation by the compiler) execute on the UI thread and the method blocking the execution by waiting the async middleware
    /// will actually wait until a method fragment completes that is placed after itself in the execution queue. The fragment does not execute
    /// until the method waiting for it completes.
    /// </para>
    /// <para>
    /// A synchronous method can be adapted to be asynchronous to some extent. For instance, we can create a TaskCompletionSource to create the
    /// task result of the asynchronous adapter and simply call the synchronous method.
    /// </para>
    /// <code lang="c#">
    /// public Task MethodAsync()
    /// {
    ///     var taskCompletionSource = new TaskCompletionSource&lt;object&gt;();
    ///     try
    ///     {
    ///         Method(); // The synchronous method we are adapting
    ///         taskCompletionSource.SetResult(null); // Can be the result from the sync method
    ///     }
    ///     catch (Exception exception)
    ///     {
    ///         taskCompletionSource.SetException(exception);
    ///     }
    ///     return taskCompletionSource.Task;
    /// }
    /// </code>
    /// <para>
    /// Unfortunately we cannot adapt our synchronous middleware to async one because if we want to continue to the next middleware element in the
    /// pipeline we need to call <c>context.Next()</c> which is synchronous in this case. This is the same issue, <c>context.Next()</c> should needs
    /// to adapt asynchronous middleware from the pipeline in the same meaner a synchronous dispatch needs to adapt asynchronous middleware.
    /// It cannot be done since all code needs to execute on the UI thread by default.
    /// </para>
    /// <para>
    /// The asynchronous overflow is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.
    /// </para>
    /// </remarks>
    public interface IMiddleware
    {
        /// <summary>Handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        void Handle(IMiddlewareContext context);

#if !NET20 && !NET30 && !NET35
        /// <summary>Asynchronously handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleAsync(IMiddlewareAsyncContext context, CancellationToken cancellationToken);
#endif
    }

    /// <summary>Represents a middleware pipeline element for handling actions before and after they are actually dispatched to action handlers.</summary>
    /// <typeparam name="TAction">The type of action to handle.</typeparam>
    /// <remarks>
    /// <para>
    /// A middleware pipeline can read the action that is currently being dispatched and decide to modify it, skip all the following middleware
    /// handlers or dispatch an action bypassing all following middleware handlers.
    /// </para>
    /// <para>
    /// Middleware is similar to filters as they can be used in the same way for error reporting or logging, but can be used to split an action
    /// dispatch into multiple actual action dispatches.
    /// </para>
    /// <para>
    /// A middleware implementation must cover both synchronous and asynchronous flows as neither can be adapted to the other. This is mostly
    /// because asynchronous methods cannot be adapted to synchronous ones if continuations need to execute on the same thread.
    /// </para>
    /// <para>
    /// The problem boils down to "blocking" the asynchronous call until it completes when we are using the synchronous method. This is impossible
    /// because all dispatches must be carried out on the UI thread (when a dispatch is initiated from the UI thread, this is the expected behaviour)
    /// in order execute action handlers (stores) on the UI thread that eventually notify the UI components and update themselves (through binding expressions).
    /// </para>
    /// <para>
    /// This restriction implies that a synchronous dispatch initiated on the UI thread must complete after the action was handled by all
    /// registered stores (or action handlers). If along the way we have an asynchronous operation (e.g.: an async middleware) then control
    /// is returned from the async method before it completes and that async middleware may dispatch actions that need to be handled on the
    /// UI thread.
    /// </para>
    /// <para>
    /// Waiting for the async middleware to execute on the UI thread creates a deadlock because all methods (and fragments resulting from
    /// async transformation by the compiler) execute on the UI thread and the method blocking the execution by waiting the async middleware
    /// will actually wait until a method fragment completes that is placed after itself in the execution queue. The fragment does not execute
    /// until the method waiting for it completes.
    /// </para>
    /// <para>
    /// A synchronous method can be adapted to be asynchronous to some extent. For instance, we can create a TaskCompletionSource to create the
    /// task result of the asynchronous adapter and simply call the synchronous method.
    /// </para>
    /// <code lang="c#">
    /// public Task MethodAsync()
    /// {
    ///     var taskCompletionSource = new TaskCompletionSource&lt;object&gt;();
    ///     try
    ///     {
    ///         Method(); // The synchronous method we are adapting
    ///         taskCompletionSource.SetResult(null); // Can be the result from the sync method
    ///     }
    ///     catch (Exception exception)
    ///     {
    ///         taskCompletionSource.SetException(exception);
    ///     }
    ///     return taskCompletionSource.Task;
    /// }
    /// </code>
    /// <para>
    /// Unfortunately we cannot adapt our synchronous middleware to async one because if we want to continue to the next middleware element in the
    /// pipeline we need to call <c>context.Next()</c> which is synchronous in this case. This is the same issue, <c>context.Next()</c> should needs
    /// to adapt asynchronous middleware from the pipeline in the same meaner a synchronous dispatch needs to adapt asynchronous middleware.
    /// It cannot be done since all code needs to execute on the UI thread by default.
    /// </para>
    /// <para>
    /// The asynchronous overflow is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.
    /// </para>
    /// </remarks>
    public interface IMiddleware<TAction>
    {
        /// <summary>Handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        void Handle(IMiddlewareContext<TAction> context);

#if !NET20 && !NET30 && !NET35
        /// <summary>Asynchronously handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleAsync(IMiddlewareAsyncContext<TAction> context, CancellationToken cancellationToken);
#endif
    }
}