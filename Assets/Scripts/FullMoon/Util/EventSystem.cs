using System;

namespace FullMoon.Util
{
    public class SimpleEventSystem
    {
        private event Action OnEventTriggered;

        public void TriggerEvent()
        {
            OnEventTriggered?.Invoke();
        }

        public void AddEvent(Action newEvent)
        {
            OnEventTriggered += newEvent;
        }

        public void RemoveEvent(Action existingEvent)
        {
            OnEventTriggered -= existingEvent;
        }
    }
    
    public class GenericEventSystem<T>
    {
        private event Action<T> OnEventTriggered;

        public void TriggerEvent(T item)
        {
            OnEventTriggered?.Invoke(item);
        }

        public void AddEvent(Action<T> newEvent)
        {
            OnEventTriggered += newEvent;
        }

        public void RemoveEvent(Action<T> existingEvent)
        {
            OnEventTriggered -= existingEvent;
        }
    }
}