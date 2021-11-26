using System;
using System.Threading.Channels;

namespace LocalEventAggregator2
{
    public class EventBase<T>
    {
        private Channel<T> _chennel;

        public EventBase()
        {
            _chennel = Channel.CreateUnbounded<T>();
        }

        public ChannelReader<T> GetSubscriber()
        {
            return _chennel.Reader;
        }
    }
}
