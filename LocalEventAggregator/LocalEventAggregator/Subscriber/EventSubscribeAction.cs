// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#pragma warning disable SA1402 // File may only contain a single type
using System;

namespace LocalEventAggregator
{
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
