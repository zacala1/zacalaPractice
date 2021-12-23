// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LocalEvents
{
    public struct EventReceiverAwaiter<T> : INotifyCompletion
    {
        private TaskAwaiter<T> task;

        public EventReceiverAwaiter(TaskAwaiter<T> task)
        {
            this.task = task;
        }

        public void OnCompleted(Action continuation)
        {
            task.OnCompleted(continuation);
        }

        public bool IsCompleted => task.IsCompleted;

        public T GetResult()
        {
            return task.GetResult();
        }
    }

    /// <summary>
    /// When using EventReceiver.ReceiveOne, this structure is used to contain the received data
    /// </summary>
    public struct EventData
    {
        public EventSubscriber Receiver { get; internal set; }

        public object Data { get; internal set; }
    }
}
