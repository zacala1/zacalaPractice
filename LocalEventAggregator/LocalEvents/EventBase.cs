// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace LocalEvents
{
    /// <summary>
    /// Creates a new EventKey used to broadcast T type events.
    /// </summary>
    /// <typeparam name="T">The data type of the event you wish to send</typeparam>
    public abstract class EventBase<T> : EventKeyBase
    {
        private readonly BroadcastBlock<T> broadcastBlock;

        private readonly List<IEventPublisher<T>> _pubs;
        private readonly List<IEventSubscriber<T>> _subs;
        private readonly List<IEventSubscribeHandler<T>> _suhs;
        private readonly List<IEventSubscription> _subscriptions;

        /// <summary>
        /// Gets the list of current subscriptions.
        /// </summary>
        /// <value>The current subscribers.</value>
        protected ICollection<IEventSubscription> Subscriptions
        {
            get { return _subscriptions; }
        }

        public EventBase()
        {
            broadcastBlock = new BroadcastBlock<T>(null, new DataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler });
            _subscriptions = new List<IEventSubscription>();
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

        /// <summary>
        /// Gets the event subscribe handeler
        /// </summary>
        /// <param name="action">handled action</param>
        /// <param name="options"></param>
        /// <returns></returns>
        private IEventSubscribeHandler<T> GetSubscribeHandler(Action<T> action, EventReceiverOptions options = EventReceiverOptions.None)
        {
            return new EventSubscribeHandler<T>(this, action, options);
        }

        /// <summary>
        /// Posts an item to the System.Threading.Tasks.Dataflow.ITargetBlock`1.
        /// </summary>
        /// <param name="data">The item being offered to the target.</param>
        public void Publish(T data)
        {
            broadcastBlock.Post(data);
        }

        public IEventSubscribeHandler<T> Subscribe(Action<T> action)
        {
            var subscription = GetSubscribeHandler(action);
            lock (_suhs)
            {
                _suhs.Add(subscription);
            }
            return subscription;
        }

        public void Unsubscribe(IEventSubscribeHandler<T> subscription)
        {
            lock (Subscriptions)
            {
                IEventSubscribeHandler<T> sub = Subscriptions.Cast<EventSubscribeHandler<T>>().FirstOrDefault(evt => { return evt == subscription; });
                if (sub != null)
                {
                    Subscriptions.Remove(sub);
                    sub.Dispose();
                }
            }
        }

        public void Unsubscribe(Action<T> subscription)
        {
            lock (Subscriptions)
            {
                IEventSubscribeHandler<T> sub = Subscriptions.Cast<EventSubscribeHandler<T>>().FirstOrDefault(evt => { return evt.Action == subscription; });
                if (sub != null)
                {
                    Subscriptions.Remove(sub);
                    sub.Dispose();
                }
            }
        }

        public bool Contains(IEventSubscription subscription)
        {
            lock (Subscriptions)
            {
                return Subscriptions.Contains(subscription);
            }
        }

        public void ClearConnection()
        {
            lock (Subscriptions)
            {
                for (var i = Subscriptions.Count - 1; i >= 0; i--)
                {
                    var sub = _subscriptions[i];
                    _subscriptions.RemoveAt(i);
                    sub.Dispose();
                }
            }
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
