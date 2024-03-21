using System;

namespace FullMoon.Util
{
    public class GenericEventSystem<T>
    {
        public delegate void GenericDelegate(T item);

        public event GenericDelegate OnEventTriggered;

        public void TriggerEvent(T item)
        {
            OnEventTriggered?.Invoke(item);
        }

        public void AddEvent(GenericDelegate newEvent)
        {
            OnEventTriggered += newEvent;
        }

        public void RemoveEvent(GenericDelegate existingEvent)
        {
            OnEventTriggered -= existingEvent;
        }
    }
}