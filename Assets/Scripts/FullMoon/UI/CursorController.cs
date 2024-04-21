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
        Unit,
        Create,
    }

    public class CursorController : MonoBehaviour
    {
        [Header("Mouse Cursor Settings")] 
        CursorType cursorType;

        [Foldout("Mouse Cursor Image")]
        [SerializeField] Texture2D[] textures;

        [Foldout("Mouse Cursor Image")] 
        [SerializeField] GameObject moveAnim;

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

        public void SetMoveAniTarget(Vector3 pos)
        {
            ObjectPoolManager.SpawnObject(moveAnim, pos + new Vector3(0, 0.5f), Quaternion.Euler(90, 100, 0));
        }

        public void SetCursorState(CursorType type)
        {
            cursorType = type;
        }

        private void UpdateCursorState()
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
                case CursorType.Unit:
                    Cursor.SetCursor(textures[3], Vector2.zero, CursorMode.ForceSoftware);
                    break;
                case CursorType.Create:
                    Cursor.SetCursor(textures[4], Vector2.zero, CursorMode.ForceSoftware);
                    break;
            }
        }

        // Texture2D 크기 조정
        private Texture2D ScaleTexture(Texture2D source, float _scaleFactor)
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

