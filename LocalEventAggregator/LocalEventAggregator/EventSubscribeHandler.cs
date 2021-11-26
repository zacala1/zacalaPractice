// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LocalEventAggregator
{
    /// <summary>
    /// Event subscribe handler. It must be disposed when it unsubscribes or deletes
    /// </summary>
    /// <typeparam name="T">The type of data the <see cref="EventBase{T}"/> will send</typeparam>
    public sealed class EventSubscribeHandler<T> : IEventSubscribeHandler<T>, IDisposable
    {
        private readonly IDisposable link;

        private readonly List<EventSubscribeAction<T>> _subscriptions = new();

        protected ICollection<EventSubscribeAction<T>> Subscriptions
        {
            get { return _subscriptions; }
        }

        internal ActionBlock<T> ActionBlock;

        public EventBase<T> Key { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ExecutionDataflowBlockOptions CapacityOptions = new()
        {
            BoundedCapacity = 1,
            TaskScheduler = EventTaskScheduler.Scheduler,
        };

        internal EventSubscribeHandler(EventBase<T> key, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Key = key;
            
            ActionBlock = options.HasFlag(EventReceiverOptions.Buffered) ?
                new ActionBlock<T>(InternalInvoke, new ExecutionDataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler }) :
                new ActionBlock<T>(InternalInvoke, CapacityOptions);

            link = key.Connect(this);
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

        private void InternalInvoke(T data)
        {
            List<Action<T>> actionList = new();
            lock (Subscriptions)
            {
                foreach(var subscription in Subscriptions)
                {
                    actionList.Add(subscription.Action);
                }
            }

            foreach(var action in actionList)
            {
                action.Invoke(data);
            }
        }

        private void InternalPublish(T data)
        {
            ActionBlock.Post(data);
        }

        /// <summary>
        /// Subscribes a delegate to an event that will be published on the <see cref="ThreadOption.PublisherThread"/>.
        /// <see cref="PubSubEvent{T}"/> will maintain a <see cref="WeakReference"/> to the target of the supplied <paramref name="action"/> delegate.
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is published.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        /// <remarks>
        /// The PubSubEvent collection is thread-safe.
        /// </remarks>
        public SubscriptionToken Subscribe(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var eventSubscription = new EventSubscribeAction<T>(action)
            {
                SubscriptionToken = new SubscriptionToken(Unsubscribe)
            };

            lock (Subscriptions)
            {
                Subscriptions.Add(eventSubscription);
            }
            return eventSubscription.SubscriptionToken;
        }

        /// <summary>
        /// Removes the subscriber matching the <see cref="SubscriptionToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SubscriptionToken"/> returned by <see cref="EventBase"/> while subscribing to the event.</param>
        public void Unsubscribe(SubscriptionToken token)
        {
            lock (Subscriptions)
            {
                var subscription = Subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);
                if (subscription != null)
                {
                    Subscriptions.Remove(subscription);
                }
            }
        }

        /// <summary>
        /// Removes the first subscriber matching <see cref="Action{T}"/> from the subscribers' list.
        /// </summary>
        /// <param name="subscriber">The <see cref="Action{T}"/> used when subscribing to the event.</param>
        public void Unsubscribe(Action<T> subscriber)
        {
            var subscription = Subscriptions.FirstOrDefault(sub => sub.Action == subscriber);
            if (subscription != null)
            {
                Subscriptions.Remove(subscription);
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if there is a subscriber matching <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="subscriber">The <see cref="Action{T}"/> used when subscribing to the event.</param>
        /// <returns><see langword="true"/> if there is an <see cref="Action{T}"/> that matches; otherwise <see langword="false"/>.</returns>
        public bool Contains(Action<T> subscriber)
        {
            bool returnValue = false;
            lock (Subscriptions)
            {
                returnValue = Subscriptions.Any((evt) => evt.Action == subscriber);
            }
            return returnValue;
        }
    }

    public class EventSubscribeAction<T>
    {
        /// <summary>
        /// Gets the target <see cref="System.Action"/> that is referenced by the <see cref="IDelegateReference"/>.
        /// </summary>
        /// <value>An <see cref="System.Action"/> or <see langword="null" /> if the referenced target is not alive.</value>
        public Action<T> Action { get; }
        /// <summary>
        /// Gets or sets a <see cref="SubscriptionToken"/> that identifies this <see cref="IEventSubscription"/>.
        /// </summary>
        /// <value>A token that identifies this <see cref="IEventSubscription"/>.</value>
        public SubscriptionToken SubscriptionToken { get; set; }

        public EventSubscribeAction(Action<T> action)
        {
            Action = action;
        }
    }
}
