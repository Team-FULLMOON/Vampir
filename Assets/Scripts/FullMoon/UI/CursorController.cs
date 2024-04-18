using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FullMoon.Util;
using MyBox;
using Unity.VisualScripting;

namespace FullMoon.UI
{
    public enum CursorType
    {
        Idle,
        Attack,
        Move,
    }

    public class CursorController : MonoBehaviour
    {
        [Header("Mouse Cursor Settings")] 
        CursorType cursorType;

        [Header("Mouse Cursor Image")]
        [SerializeField] Texture2D[] textures;

        void Start()
        {
            cursorType = CursorType.Idle;
            textures = textures.Select(tex => ScaleTexture(tex, 0.3f)).ToArray();
        }

        void Update()
        {
            if (!Application.isPlaying)
                return;

            UpdateCursorState();
        }

        public void SetCursorState(CursorType type)
        {
            cursorType = type;
        }

        public void UpdateCursorState()
        {
            switch (cursorType)
            {
                case CursorType.Idle:
                    Cursor.SetCursor(textures[0], Vector2.zero, CursorMode.ForceSoftware);
                    break;
                case CursorType.Attack:
                    Cursor.SetCursor(textures[1], new Vector2(textures[1].width / 2, textures[1].height / 2), CursorMode.ForceSoftware);
                    break;
                case CursorType.Move:
                    Cursor.SetCursor(textures[2], Vector2.zero, CursorMode.ForceSoftware);
                    break;
            }
        }

        // Texture2D 크기 조정
        public Texture2D ScaleTexture(Texture2D source, float _scaleFactor)
        {
            if (_scaleFactor == 1f)
            {
                return source;
            }
            else if (_scaleFactor == 0f)
            {
                return Texture2D.blackTexture;
            }

            int _newWidth = Mathf.RoundToInt(source.width * _scaleFactor);
            int _newHeight = Mathf.RoundToInt(source.height * _scaleFactor);


            
            Color[] _scaledTexPixels = new Color[_newWidth * _newHeight];

            for (int _yCord = 0; _yCord < _newHeight; _yCord++)
            {
                float _vCord = _yCord / (_newHeight * 1f);
                int _scanLineIndex = _yCord * _newWidth;

                for (int _xCord = 0; _xCord < _newWidth; _xCord++)
                {
                    float _uCord = _xCord / (_newWidth * 1f);

                    _scaledTexPixels[_scanLineIndex + _xCord] = source.GetPixelBilinear(_uCord, _vCord);
                }
            }

            // Create Scaled Texture
            Texture2D result = new Texture2D(_newWidth, _newHeight, source.format, false);
            result.SetPixels(_scaledTexPixels, 0);
            result.Apply();

            return result;
        }
    }
}

