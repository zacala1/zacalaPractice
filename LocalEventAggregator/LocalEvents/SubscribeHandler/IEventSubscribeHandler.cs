using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalEvents
{
    public interface IEventSubscribeHandler<T> : IEventSubscription, IDisposable
    {
        /// <summary>
        /// Subscribes a delegate to an event that will be published
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is published.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        //SubscriptionToken Subscribe(Action<T> action);

        /// <summary>
        /// Removes the subscriber matching the <see cref="SubscriptionToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SubscriptionToken"/> returned by <see cref="EventBase{T}"/> while subscribing to the event.</param>
        //void Unsubscribe(SubscriptionToken token);

        /// <summary>
        /// Removes the first subscriber matching <see cref="Action{T}"/> from the subscribers' list.
        /// </summary>
        /// <param name="subscriber">The <see cref="Action{T}"/> used when subscribing to the event.</param>
        //void Unsubscribe(Action<T> subscriber);

        /// <summary>
        /// Returns <see langword="true"/> if there is a subscriber matching <see cref="Action{T}"/>.
        /// </summary>
        /// <param name="subscriber">The <see cref="Action{T}"/> used when subscribing to the event.</param>
        /// <returns><see langword="true"/> if there is an <see cref="Action{T}"/> that matches; otherwise <see langword="false"/>.</returns>
        //bool Contains(Action<T> subscriber);

        void Invoke(T data);
    }
}
