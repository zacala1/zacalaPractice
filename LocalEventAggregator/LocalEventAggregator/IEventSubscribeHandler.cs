using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalEventAggregator
{
    public interface IEventSubscribeHandler<T> : IDisposable
    {
        void Invoke(T data);
    }
}
