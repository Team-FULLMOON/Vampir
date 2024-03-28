using MyBox;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace FullMoon.Util
{
    public class ColliderTriggerEvents : MonoBehaviour
    {
        [SerializeField, Tag] private List<string> filterTags;
        [SerializeField, Space(10)] private bool executeEnterAfterFrame;
        [SerializeField] private UnityEvent<Collider> onEnterEvent;
        [SerializeField, Space(10)] private bool executeExitAfterFrame;
        [SerializeField] private UnityEvent<Collider> onExitEvent;

        private IEnumerator ExecuteAfterFrame(UnityEvent<Collider> unityEvent, Collider other)
        {
            yield return null;
            unityEvent?.Invoke(other);
        }
    
        private void OnTriggerEnter(Collider other)
        {
            bool filterResult = false;
            foreach (var tag in filterTags)
            {
                if (other.CompareTag(tag))
                {
                    filterResult = true;
                }
            }

            if (filterResult == false)
            {
                return;
            }
        
            if (executeEnterAfterFrame)
            {
                StartCoroutine(ExecuteAfterFrame(onEnterEvent, other));
                return;
            }
        
            onEnterEvent?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            bool filterResult = false;
            foreach (var tag in filterTags)
            {
                if (other.CompareTag(tag))
                {
                    filterResult = true;
                }
            }

            if (filterResult == false)
            {
                return;
            }
        
            if (executeExitAfterFrame)
            {
                StartCoroutine(ExecuteAfterFrame(onExitEvent, other));
                return;
            }

            onExitEvent?.Invoke(other);
        }
    }
}