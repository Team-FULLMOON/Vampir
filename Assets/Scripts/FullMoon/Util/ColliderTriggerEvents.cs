using MyBox;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FullMoon.Util
{
    public class ColliderTriggerEvents : MonoBehaviour
    {
        [SerializeField, Tag] private List<string> filterTags;
        [SerializeField, Space(10)] private bool executeEnterAfterFrame;
        [SerializeField] private UnityEvent<Collider> onEnterEvent;
        [SerializeField, Space(10)] private bool executeExitAfterFrame;
        [SerializeField] private UnityEvent<Collider> onExitEvent;

        public List<string> GetFilterTags => filterTags;
        
        private async void OnTriggerEnter(Collider other)
        {
            bool filterResult = false;
            foreach (var filterTag in filterTags)
            {
                if (other.CompareTag(filterTag))
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
                await UniTask.NextFrame();
                onEnterEvent?.Invoke(other);
            }
        
            onEnterEvent?.Invoke(other);
        }

        private async void OnTriggerExit(Collider other)
        {
            bool filterResult = false;
            foreach (var filterTag in filterTags)
            {
                if (other.CompareTag(filterTag))
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
                await UniTask.NextFrame();
                onEnterEvent?.Invoke(other);
                return;
            }

            onExitEvent?.Invoke(other);
        }
    }
}