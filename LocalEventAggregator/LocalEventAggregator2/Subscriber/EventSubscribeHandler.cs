// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LocalEventAggregator
{
    /// <summary>
    /// Event subscribe handler. It must be disposed when it unsubscribes or deletes
    /// </summary>
    /// <typeparam name="T">The type of data the <see cref="EventBase{T}"/> will send</typeparam>
    public sealed class EventSubscribeHandler<T> : IEventSubscribeHandler<T>
    {
        private IDisposable link;

        internal ActionBlock<T> ActionBlock;

        public EventBase<T> Key { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ExecutionDataflowBlockOptions CapacityOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1,
            TaskScheduler = EventTaskScheduler.Scheduler,
        };

        internal EventSubscribeHandler(EventBase<T> key, Action<T> action, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Key = key;
            Action<T> blockAction = (x) => { }; //clear any previous event, we don't want to receive old events, as broadcast block will always send us the last avail event on connect
            ActionBlock = options.HasFlag(EventReceiverOptions.Buffered) ?
                new ActionBlock<T>((x) => blockAction(x), new ExecutionDataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler }) :
                new ActionBlock<T>((x) => blockAction(x), CapacityOptions);

            link = key.Connect(this);
            blockAction = action;
        }

        ~EventSubscribeHandler()
        {
            Dispose();
        }

        public void Dispose()
        {
            link?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Invoke(T data)
        {
            ActionBlock.Post(data);
        }
    }
}
