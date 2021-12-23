// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly List<EventSubscribeAction<T>> _subscriptions = new List<EventSubscribeAction<T>>();

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
                foreach (var subscription in Subscriptions)
                {
                    actionList.Add(subscription.Action);
                }
            }

            foreach (var action in actionList)
            {
                action.Invoke(data);
            }
        }

        private void InternalPublish(T data)
        {
            ActionBlock.Post(data);
        }

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

        public void Unsubscribe(Action<T> subscriber)
        {
            var subscription = Subscriptions.FirstOrDefault(sub => sub.Action == subscriber);
            if (subscription != null)
            {
                Subscriptions.Remove(subscription);
            }
        }

        public bool Contains(Action<T> subscriber)
        {
            bool returnValue = false;
            lock (Subscriptions)
            {
                returnValue = Subscriptions.Any((evt) => evt.Action == subscriber);
            }
            return returnValue;
        }

        public void Prune()
        {
            lock (Subscriptions)
            {
                for (var i = Subscriptions.Count - 1; i >= 0; i--)
                {
                    _subscriptions.RemoveAt(i);
                }
            }
        }
    }
}
