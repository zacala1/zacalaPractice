﻿using System;
using System.Collections.Generic;

namespace LocalEventAggregator
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, EventKeyBase> _events = new();

        public TEventType GetEvent<TEventType>() where TEventType : EventKeyBase, new()
        {
            lock (_events)
            {
                if (!_events.TryGetValue(typeof(TEventType), out var existingEvent))
                {
                    TEventType newEvent = new();
                    
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
