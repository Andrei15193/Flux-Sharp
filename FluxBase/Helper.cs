using System;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETCOREAPP1_0 || NETCOREAPP1_1
using System.Reflection;
#endif

namespace FluxBase
{
    internal static class Helper
    {
        public static bool IsCompatible<T>(object value)
        {
            if (value is T)
                return true;
            else if (value == null)
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETCOREAPP1_0 || NETCOREAPP1_1
                if (typeof(T).GetTypeInfo().IsValueType)
#else
                if (typeof(T).IsValueType)
#endif
                    return Nullable.GetUnderlyingType(typeof(T)) != null;
                else
                    return true;
            else
                return false;
        }

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

#if NET40 || NET45 || NET451 || NET452 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2
        public static Task TaskFromException(Exception exception)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            taskCompletionSource.SetException(exception);
            return taskCompletionSource.Task;
        }
#endif
    }
}