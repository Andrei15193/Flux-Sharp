using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
using System.Linq;
#endif
#if !NET20 && !NET30
using System.Linq.Expressions;
#endif

namespace FluxBase
{
    /// <summary>Represents a store, responsible with managing application view state.</summary>
    public abstract class Store : INotifyPropertyChanged
    {
#if NET20 || NET30 || NET35
        private volatile IEnumerable<DispatchHandlerInfo> _dispatchHandlers;
#else
        private readonly Lazy<IEnumerable<DispatchHandlerInfo>> _dispatchHandlers;
#endif

        /// <summary>Initializes a new instance of the <see cref="Store"/> class.</summary>
        protected Store()
        {
#if NET20 || NET30 || NET35
            _dispatchHandlers = null;
#else
            _dispatchHandlers = new Lazy<IEnumerable<DispatchHandlerInfo>>(_GetHandlerInfos);
#endif
        }

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Handles the provided <paramref name="action"/>.</summary>
        /// <param name="action">The action that was dispatched.</param>
        /// <remarks>
        /// <para>
        ///     The default implementation maps all public methods that return <see cref="void"/>
        ///     and have one parameter and calls the method whose parameter is closest to the
        ///     actual type of the provided <paramref name="action"/>.
        /// </para>
        /// <para>
        ///     If a method where the actual type of the <paramref name="action"/> matches exactly then that method is called,
        ///     otherwise the method with the most sepcific base class (i.e.: the closest base type in the inheritance chain)
        ///     is called if one can be found.
        /// </para>
        /// <para>
        ///     The search includes methods defined in base <see cref="Store"/>s even if they are private.
        /// </para>
        /// </remarks>
        public virtual void Handle(object action)
            => _TryFindDispatchHandler(action?.GetType() ?? typeof(object))?.Invoke(action);

        /// <summary>Notifies that a property was changed.</summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
#if !NET20 && !NET30 && !NET35 && !NET40
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
#else
        protected void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
#endif

#if !NET20 && !NET30
        /// <summary>Dynamically updates a property and notifies observers about the change.</summary>
        /// <typeparam name="TProperty">The type of the property that was changed.</typeparam>
        /// <param name="property">The property to update.</param>
        /// <param name="value">The new value to set to the property.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="property"/> does not resolve to a property of the current instance.</exception>
        /// <remarks>
        /// This method simplifies stores by removing the boilerplate code for writing properties that notify
        /// observers upon change. Using this method the store class can have less clutter. Without using this
        /// method the store would look something like this:
        /// <code>
        /// public class MyStore : Store
        /// {
        ///     private int _value1;
        /// 
        ///     public int Property1
        ///     {
        ///         get => _value1;
        ///         set
        ///         {
        ///             _value1 = value;
        ///             NotifyPropertyChanged(nameof(Property1)); // or just NotifyPropertyChanged()
        ///         }
        ///     }
        /// 
        ///     protected override void Handle(Action action)
        ///     {
        ///         Property1++;
        ///     }
        /// }
        /// </code>
        /// Using <c>SetProperty</c> will simplify this to the following:
        /// <code>
        /// public class MyStore : Store
        /// {
        ///     public int Property1 { get; private set; }
        /// 
        ///     protected override void Handle(Action action)
        ///     {
        ///         SetProperty(() => Property1, Property1 + 1);
        ///     }
        /// }
        /// </code>
        /// There is no need to explicitly back properties with a field and manually raise the
        /// <see cref="PropertyChanged"/> event. This is all done by the <c>SetProperty</c> method.
        /// </remarks>
        protected void SetProperty<TProperty>(Expression<Func<TProperty>> property, TProperty value)
        {
            switch (property?.Body)
            {
                case MemberExpression memberExpression when
                        memberExpression.Member is PropertyInfo propertyInfo
                        && propertyInfo.CanWrite
                        && memberExpression.Expression is ConstantExpression constantExpression
                        && ReferenceEquals(constantExpression.Value, this):

                    propertyInfo.SetValue(this, value, null);
                    NotifyPropertyChanged(propertyInfo.Name);
                    break;

                case null:
                    throw new ArgumentNullException(nameof(property));

                default:
                    throw new ArgumentException("Property expression does not resolve to a settable property of the current store.", nameof(property));
            }
        }
#endif

        private Action<object> _TryFindDispatchHandler(Type actionType)
        {
            Action<object> _bestMatch = null;
            int _acceptableMatchPrecision = 0;
            Action<object> _acceptableMatch = null;

#if NET20 || NET30 || NET35
            if (_dispatchHandlers == null)
                _dispatchHandlers = _GetHandlerInfos();
            using (var dispatchHandlerInfo = _dispatchHandlers.GetEnumerator())
#else
            using (var dispatchHandlerInfo = _dispatchHandlers.Value.GetEnumerator())
#endif
                while (dispatchHandlerInfo.MoveNext() && _bestMatch == null)
                {
                    if (dispatchHandlerInfo.Current.ParameterType == actionType)
                        _bestMatch = dispatchHandlerInfo.Current.DispatchHandler;
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
                    else if (dispatchHandlerInfo.Current.ParameterType.GetTypeInfo().IsAssignableFrom(actionType.GetTypeInfo()))
#else
                    else if (dispatchHandlerInfo.Current.ParameterType.IsAssignableFrom(actionType))
#endif
                    {
                        var matchPrecision = _GetMatchPrecisionBetween(dispatchHandlerInfo.Current.ParameterType, actionType);
                        if (matchPrecision < _acceptableMatchPrecision || _acceptableMatch == null)
                        {
                            _acceptableMatchPrecision = matchPrecision;
                            _acceptableMatch = dispatchHandlerInfo.Current.DispatchHandler;
                        }
                    }
                }

            return _bestMatch ?? _acceptableMatch;

            int _GetMatchPrecisionBetween(Type target, Type actual)
            {
                var precision = 0;

                var current = actual;
                while (current != target)
                {
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
                    current = current.GetTypeInfo().BaseType;
#else
                    current = current.BaseType;
#endif
                    precision++;
                }

                return precision;
            }
        }


        private IEnumerable<DispatchHandlerInfo> _GetHandlerInfos()
        {
            var handlerInfos = new List<DispatchHandlerInfo>();
            var storeType = this.GetType();
            while (storeType != typeof(Store))
            {
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
                foreach (var method in storeType.GetTypeInfo().DeclaredMethods.Where(method => method.IsPublic))
#else
                foreach (var method in storeType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
#endif
                    if (method.ReturnType == typeof(void) && !method.IsStatic)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1)
                        {
                            var actionType = parameters[0].ParameterType;
                            handlerInfos.Add(new DispatchHandlerInfo(actionType, _CreateDispatchHandler(method, actionType)));
                        }
                    }
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
                storeType = storeType.GetTypeInfo().BaseType;
#else
                storeType = storeType.BaseType;
#endif
            }
            return handlerInfos;
        }

        private static Action<object> _CreateHandler<TAction>(object target, MethodInfo handlerMethodInfo)
        {
#if NET20 || NET30 || NET35 || NET40
            var concreteHandler = (Action<TAction>)Delegate.CreateDelegate(typeof(Action<TAction>), target, handlerMethodInfo);
#else
            var concreteHandler = (Action<TAction>)handlerMethodInfo.CreateDelegate(typeof(Action<TAction>), target);
#endif
            return action => concreteHandler((TAction)action);
        }

        private sealed class DispatchHandlerInfo
        {
            public DispatchHandlerInfo(Type parameterType, Action<object> dispatchHandler)
            {
                ParameterType = parameterType;
                DispatchHandler = dispatchHandler;
            }

            public Type ParameterType { get; }

            public Action<object> DispatchHandler { get; }
        }

        private Action<object> _CreateDispatchHandler(MethodInfo dispatchHandlerMethodInfo, Type actionType)
        {
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
            var factoryMethod = typeof(Store).GetTypeInfo().GetDeclaredMethods(nameof(_CreateHandler)).Single();
#else
            var factoryMethod = typeof(Store).GetMethod(nameof(_CreateHandler), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
#endif
            var dispatchHandler = (Action<object>)factoryMethod
                .MakeGenericMethod(actionType)
                .Invoke(this, new object[] { this, dispatchHandlerMethodInfo });
            return dispatchHandler;
        }
    }
}