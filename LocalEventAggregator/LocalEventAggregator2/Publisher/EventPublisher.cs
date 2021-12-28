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
    /// Event publisher. It must be disposed when it unsubscribes or deletes
    /// </summary>
    /// <typeparam name="T">The type of data the <see cref="EventBase{T}"/> will send</typeparam>
    public class EventPublisher<T> : IEventPublisher<T>
    {
        private readonly IDisposable link;

        internal BroadcastBlock<T> BroadcastBlock;

        public EventBase<T> Key { get; private set; }

        internal EventPublisher(EventBase<T> key)
        {
            Key = key;
            
            BroadcastBlock = new BroadcastBlock<T>(null, new DataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler });

            link = key.Connect(this);
        }

        ~EventPublisher()
        {
            Dispose();
        }

        public void Dispose()
        {
            link?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Broadcast(T data)
        {
            BroadcastBlock.Post(data);
        }
    }
}
