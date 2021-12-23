using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalEvents
{
    public interface IEventPublisher<T> : IEventSubscription, IDisposable
    {
        /// <summary>
        /// Publishs an item to the System.Threading.Tasks.Dataflow.ITargetBlock`1.
        /// </summary>
        /// <param name="data">The item being offered to the target.</param>
        void Broadcast(T data);
    }
}
