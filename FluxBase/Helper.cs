#if NET40 || NET45 || NET451 || NET452 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2
using System;
using System.Threading.Tasks;

namespace FluxBase
{
    internal static class Helper
    {
#if NET40
        public static Task CompletedTask
        {
            get
            {
                var taskCompletionSource = new TaskCompletionSource<object>();
                taskCompletionSource.SetResult(null);
                return taskCompletionSource.Task;
            }
        }
#endif

        public static Task TaskFromException(Exception exception)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            taskCompletionSource.SetException(exception);
            return taskCompletionSource.Task;
        }
    }
}
#endif