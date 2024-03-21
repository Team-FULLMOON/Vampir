using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace FullMoon.Util
{
    public class GameManage : MonoBehaviour
    {
        private static Text _logText;
    
        void Start()
        {
            _logText = GameObject.Find("LogText").GetComponent<Text>();
        }
        
        // Use Info : GameManage.PrintLog(string)
        public static void PrintLog(string text)
        {
            _logText.text = "Log : " + text;
        }
    }
}
