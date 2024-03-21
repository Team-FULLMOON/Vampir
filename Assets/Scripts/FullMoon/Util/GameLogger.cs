using TMPro;
using UnityEngine;

namespace FullMoon.Util
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GameLogger : MonoBehaviour
    {
        private static TextMeshProUGUI _logText;
    
        void Start()
        {
            _logText = GetComponent<TextMeshProUGUI>();
        }
        
        // Use Info : GameLogger.PrintLog(string)
        public static void PrintLog(string text)    
        {
            _logText.text = "Log : " + text;
        }
    }
}
