using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace FullMoon.Util
{
    public class ToastManager : ComponentSingleton<ToastManager>
    {
        public GameObject toastPrefab;
        public Transform toastContainer;
        public float displayDuration = 2.0f;
        public float fadeDuration = 0.5f;

        private Queue<GameObject> toasts = new();

        public void ShowToast(string message, string color = "white")
        {
            GameObject toastInstance = Instantiate(toastPrefab, toastContainer);
            toastInstance.transform.SetSiblingIndex(0);
            TextMeshProUGUI toastText = toastInstance.GetComponent<TextMeshProUGUI>();
            toastText.raycastTarget = false;
            toastText.text = $"<color={color}>{message}</color>";
        
            toasts.Enqueue(toastInstance);
            if (toasts.Count > 5)
            {
                GameObject oldToast = toasts.Dequeue();
                Destroy(oldToast);
            }

            CanvasGroup canvasGroup = toastInstance.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = toastInstance.AddComponent<CanvasGroup>();
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1, fadeDuration))
                .AppendInterval(displayDuration)
                .Append(canvasGroup.DOFade(0, fadeDuration))
                .OnComplete(() => Destroy(toastInstance));
        }
    }
}
