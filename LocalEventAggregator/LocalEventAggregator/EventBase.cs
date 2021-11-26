// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace LocalEventAggregator
{
    /// <summary>
    /// Creates a new EventKey used to broadcast T type events.
    /// </summary>
    /// <typeparam name="T">The data type of the event you wish to send</typeparam>
    public abstract class EventBase<T> : EventKeyBase, IEventSubscribeHandler<T>, IDisposable
    {
        private readonly BroadcastBlock<T> broadcastBlock;
        private readonly IEventSubscribeHandler<T> eventSubscribeHandler;

        public EventBase()
        {
            broadcastBlock = new BroadcastBlock<T>(null, new DataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler });
            eventSubscribeHandler = new EventSubscribeHandler<T>(this);
        }

        ~EventBase()
        {
            Dispose();
        }

        public void Dispose()
        {
            eventSubscribeHandler.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Links the System.Threading.Tasks.Dataflow.ISourceBlock`1 to the specified System.Threading.Tasks.Dataflow.ITargetBlock
        /// </summary>
        /// <param name="source">The source from which to link.</param>
        /// <returns>An System.IDisposable that, upon calling Dispose, 
        /// will unlink the source from the target.</returns>
        /// <exception cref="System.ArgumentNullException">The source is null. -or- The target is null.</exception>
        internal IDisposable Connect(EventPublisher<T> source)
        {
            return source.BroadcastBlock.LinkTo(broadcastBlock);
        }

        /// <summary>
        /// Links the System.Threading.Tasks.Dataflow.ISourceBlock`1 to the specified System.Threading.Tasks.Dataflow.ITargetBlock
        /// </summary>
        /// <param name="target">The target to which to link.</param>
        /// <returns>An System.IDisposable that, upon calling Dispose, 
        /// will unlink the source from the target.</returns>
        /// <exception cref="System.ArgumentNullException">The source is null. -or- The target is null.</exception>
        internal IDisposable Connect(EventSubscribeHandler<T> target)
        {
            return broadcastBlock.LinkTo(target.ActionBlock);
        }

        /// <summary>
        /// Links the System.Threading.Tasks.Dataflow.ISourceBlock`1 to the specified System.Threading.Tasks.Dataflow.ITargetBlock
        /// </summary>
        /// <param name="target">The target to which to link.</param>
        /// <returns>An System.IDisposable that, upon calling Dispose, 
        /// will unlink the source from the target.</returns>
        /// <exception cref="System.ArgumentNullException">The source is null. -or- The target is null.</exception>
        internal IDisposable Connect(EventSubscriber<T> target)
        {
            return broadcastBlock.LinkTo(target.BufferBlock);
        }

        /// <summary>
        /// Posts an item to the System.Threading.Tasks.Dataflow.ITargetBlock`1.
        /// </summary>
        /// <param name="data">The item being offered to the target.</param>
        public void Publish(T data)
        {
            broadcastBlock.Post(data);
        }

        /// <summary>
        /// Gets the event publisher
        /// </summary>
        /// <returns>The event publisher instance</returns>
        public IEventPublisher<T> GetPublisher()
        {
            return new EventPublisher<T>(this);
        }

        /// <summary>
        /// Gets the event subscriber
        /// </summary>
        /// <param name="options"></param>
        /// <returns>The event subscriber instance</returns>
        public IEventSubscriber<T> GetSubscriber(EventReceiverOptions options = EventReceiverOptions.Buffered)
        {
            return new EventSubscriber<T>(this, options);
        }

        public SubscriptionToken Subscribe(Action<T> action)
        {
            return eventSubscribeHandler.Subscribe(action);
        }

        public void Unsubscribe(SubscriptionToken token)
        {
            eventSubscribeHandler.Unsubscribe(token);
        }

        public void Unsubscribe(Action<T> subscriber)
        {
            eventSubscribeHandler.Unsubscribe(subscriber);
        }

        public bool Contains(Action<T> subscriber)
        {
            return eventSubscribeHandler.Contains(subscriber);
        }
    }

    public abstract class EventKeyBase
    {
        internal readonly ulong EventId = EventKeyCounter.New();
    }

    internal static class EventKeyCounter
    {
        private static long eventKeysCounter;

        public static ulong New()
        {
            return (ulong)Interlocked.Increment(ref eventKeysCounter);
        }
    }
}
