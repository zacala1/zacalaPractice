// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LocalEventAggregator
{
    /// <summary>
    /// Event subscriber. It must be disposed when it unsubscribes or deletes
    /// </summary>
    /// <typeparam name="T">The type of data the <see cref="EventBase{T}"/> will send</typeparam>
    public sealed class EventSubscriber<T> : EventSubscriber, IEventSubscriber<T>, IDisposable
    {
        private IDisposable link;

        internal BufferBlock<T> BufferBlock;

        public EventBase<T> Key { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly DataflowBlockOptions CapacityOptions = new DataflowBlockOptions
        {
            BoundedCapacity = 1,
            TaskScheduler = EventTaskScheduler.Scheduler,
        };

        internal EventSubscriber(EventBase<T> key, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Key = key;

            BufferBlock = options.HasFlag(EventReceiverOptions.Buffered) ?
                new BufferBlock<T>(new DataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler }) :
                new BufferBlock<T>(CapacityOptions);

            link = key.Connect(this);

            _ = TryReceive(out _); //clear any previous event, we don't want to receive old events, as broadcast block will always send us the last avail event on connect
        }

        public async Task<T> ReceiveAsync()
        {
            var res = await BufferBlock.ReceiveAsync();

            return res;
        }

        public async Task<T> ReceiveAsync(CancellationToken cancellationToken)
        {
            var res = await BufferBlock.ReceiveAsync(cancellationToken);

            return res;
        }

        public async Task<T> ReceiveAsync(TimeSpan timeout)
        {
            var res = await BufferBlock.ReceiveAsync(timeout);

            return res;
        }

        public async Task<T> ReceiveAsync<TOutput>(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var res = await BufferBlock.ReceiveAsync(timeout, cancellationToken);

            return res;
        }

        public EventReceiverAwaiter<T> GetAwaiter()
        {
            return new EventReceiverAwaiter<T>(ReceiveAsync().GetAwaiter());
        }

        /// <summary>
        /// Returns the count of currently buffered events
        /// </summary>
        public int Count => BufferBlock.Count;

        public bool TryReceive(out T data)
        {
            if (BufferBlock.Count == 0)
            {
                data = default(T);
                return false;
            }

            data = BufferBlock.Receive();

            return true;
        }

        public int TryReceiveAll(ICollection<T> collection)
        {
            IList<T> result;
            if (!BufferBlock.TryReceiveAll(out result))
            {
                return 0;
            }

            var count = 0;
            foreach (var e in result)
            {
                count++;
                collection?.Add(e);
            }

            return count;
        }

        /// <summary>
        /// Clears all currently buffered events.
        /// </summary>
        public void Reset()
        {
            //consume all in one go
            IList<T> result;
            BufferBlock.TryReceiveAll(out result);
        }

        ~EventSubscriber()
        {
            Dispose();
        }

        public void Dispose()
        {
            link?.Dispose();

            GC.SuppressFinalize(this);
        }

        internal override Task<bool> GetPeakTask()
        {
            return BufferBlock.OutputAvailableAsync();
        }

        internal override bool TryReceiveOneInternal(out object obj)
        {
            T res;
            if (!TryReceive(out res))
            {
                obj = null;
                return false;
            }

            obj = res;
            return true;
        }
    }

    /// <summary>
    /// Base class for EventReceivers
    /// </summary>
    public abstract class EventSubscriber
    {
        internal abstract Task<bool> GetPeakTask();

        internal abstract bool TryReceiveOneInternal(out object obj);

        /// <summary>
        /// Combines multiple receivers in one call and tries to receive the first available events among all the passed receivers
        /// </summary>
        /// <param name="events">The events you want to listen to</param>
        /// <returns></returns>
        public static async Task<EventData> ReceiveOne(params EventSubscriber[] events)
        {
            while (true)
            {
                var tasks = new Task[events.Length];
                for (var i = 0; i < events.Length; i++)
                {
                    tasks[i] = events[i].GetPeakTask();
                }

                await Task.WhenAny(tasks);

                for (var i = 0; i < events.Length; i++)
                {
                    if (!tasks[i].IsCompleted) continue;

                    object data;
                    if (!events[i].TryReceiveOneInternal(out data)) continue;

                    var res = new EventData
                    {
                        Data = data,
                        Receiver = events[i],
                    };
                    return res;
                }
            }
        }
    }
}
