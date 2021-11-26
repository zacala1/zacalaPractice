using System;
using System.Collections.Generic;

namespace LocalEventAggregator
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, EventKeyBase> _events = new Dictionary<Type, EventKeyBase>();

        public TEventType GetEvent<TEventType>() where TEventType : EventKeyBase, new()
        {
            lock (_events)
            {
                EventKeyBase existingEvent = null;

                if (!_events.TryGetValue(typeof(TEventType), out existingEvent))
                {
                    TEventType newEvent = new TEventType();
                    
                    _events[typeof(TEventType)] = newEvent;

                    return newEvent;
                }
                else
                {
                    return (TEventType)existingEvent;
                }
            }
        }
    }
}
