using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalEventAggregator
{
    public interface IEventReceiver<T> : IDisposable
    {
        /// <summary>
        /// Clears all currently buffered events.
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets an awaiter used to await this System.Threading.Tasks.Task`1.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        EventReceiverAwaiter<T> GetAwaiter();

        /// <summary>
        /// Awaits a single event
        /// </summary>
        /// <returns></returns>
        Task<T> ReceiveAsync();

        /// <summary>
        /// Asynchronously receives a value from a specified source and provides a token
        /// to cancel the operation.
        /// </summary>
        /// <param name="cancellationToken">The token to use to cancel the receive operation.</param>
        /// <returns>
        /// A task that represents the asynchronous receive operation. When a value is successfully
        /// received from the source, the returned task is completed and its System.Threading.Tasks.Task`1.Result
        /// returns the value. If a value cannot be retrieved because cancellation was requested,
        /// the returned task is canceled. If the value cannot be retrieved because the source
        /// is empty and completed , an System.InvalidOperationException exception is thrown
        /// in the returned task.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source is null.</exception>
        /// <exception cref="System.InvalidOperationException">If the value cannot be retrieved
        /// because the source is empty and completed</exception>
        Task<T> ReceiveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously receives a value from a specified source, observing an optional
        /// time-out period.
        /// </summary>
        /// <param name="timeout">The maximum time interval, in milliseconds, to wait for the synchronous operation
        /// to complete, or an interval that represents -1 milliseconds to wait indefinitely.</param>
        /// <returns>
        /// A task that represents the asynchronous receive operation. When a value is successfully
        /// received from the source, the returned task is completed and its System.Threading.Tasks.Task`1.Result
        /// returns the value. If a value cannot be retrieved because the time-out expired,
        /// the returned task is canceled. If the value cannot be retrieved because the source
        /// is empty and completed , an System.InvalidOperationException exception is thrown
        /// in the returned task.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">timeout is a negative number other than -1 milliseconds,
        /// which represents an infinite time-out period. -or- timeout is greater than System.Int32.MaxValue.</exception>
        Task<T> ReceiveAsync(TimeSpan timeout);

        /// <summary>
        /// Asynchronously receives a value from a specified source, providing a token to
        /// cancel the operation and observing an optional time-out interval.
        /// </summary>
        /// <param name="timeout">The maximum time interval, in milliseconds, to wait for the synchronous operation
        /// to complete, or an interval that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The token which may be used to cancel the receive operation.</param>
        /// <returns>
        /// A task that represents the asynchronous receive operation. When a value is successfully
        /// received from the source, the returned task is completed and its System.Threading.Tasks.Task`1.Result
        /// returns the value. If a value cannot be retrieved because the time-out expired
        /// or cancellation was requested, the returned task is canceled. If the value cannot
        /// be retrieved because the source is empty and completed, an System.InvalidOperationException
        /// exception is thrown in the returned task.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">source is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">timeout is a negative number other than -1 milliseconds, which represents an
        /// infinite time-out period. -or- timeout is greater than System.Int32.MaxValue.</exception>
        Task<T> ReceiveAsync<TOutput>(TimeSpan timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Receives one event from the buffer, useful specially in Sync scripts
        /// </summary>
        /// <param name="data">The item received from the source.</param>
        /// <returns>true if an item could be received; otherwise, false.</returns>
        public bool TryReceive(out T data);

        /// <summary>
        /// Receives all the events from the buffer(if buffered was true during creations), 
        /// useful mostly only in Sync scripts
        /// </summary>
        /// <param name="collection">The item collection received from the source.</param>
        /// <returns>true if an item could be received; otherwise, false.</returns>
        public int TryReceiveAll(ICollection<T> collection);
    }
}