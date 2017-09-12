﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.DI;
using Aggregates.Messages;
using System.Collections.Concurrent;
using Aggregates.Extensions;

namespace Aggregates.Internal
{
    class Processor : IProcessor
    {
        static readonly ConcurrentDictionary<Type, object> Processors = new ConcurrentDictionary<Type, object>();

        [DebuggerStepThrough]
        public Task<TResponse> Process<TQuery, TResponse>(TQuery query) where TQuery : IQuery<TResponse>
        {
            var handlerType = typeof(IHandleQueries<,>).MakeGenericType(typeof(TQuery), typeof(TResponse));

            var handlerFunc = (Func<object, TQuery, Task<TResponse>>)Processors.GetOrAdd(handlerType, t => ReflectionExtensions.MakeQueryHandler<TQuery, TResponse>(handlerType));
            var handler = TinyIoCContainer.Current.Resolve(handlerType);

            return handlerFunc(handler, query);
        }
    }
}