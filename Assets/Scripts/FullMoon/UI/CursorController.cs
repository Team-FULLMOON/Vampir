using MyBox;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FullMoon.Util;

namespace FullMoon.UI
{
    public enum CursorType
    {
        Idle,
        Attack,
        Move,
        Unit,
        Create,
        Camera
    }

    public class CursorController : MonoBehaviour
    {
        [SerializeField] private bool enable = true;
        
        [Separator("Mouse Cursor Settings")] 

        [SerializeField] private CursorType cursorType;

        [SerializeField] private List<Texture2D> textures;

        [SerializeField] private GameObject moveAnim;

        private void Start()
        {
            cursorType = CursorType.Idle;
            textures = textures.Select(tex => ScaleTexture(tex, 0.3f)).ToList();
        }

        private void Update()
        {
            UpdateCursorState();
        }

        public void SetMoveAniTarget(Vector3 pos)
        {
            ObjectPoolManager.Instance.SpawnObject(moveAnim, pos + new Vector3(0, 0.5f), Quaternion.identity);
        }

        public void SetCursorState(CursorType type)
        {
            if (cursorType is not CursorType.Idle && type is CursorType.Camera)
            {
                return;
            }
            cursorType = type;
        }

        private void UpdateCursorState()
        {
            if (!Application.isPlaying || !Application.isFocused || enable == false)
            {
                return;
            }
            
            switch (cursorType)
            {
                case CursorType.Idle:
                    Cursor.SetCursor(textures[0], Vector2.zero, CursorMode.ForceSoftware);
                    break;
                case CursorType.Attack:
                    Cursor.SetCursor(textures[1], new Vector2(textures[1].width / 2f, textures[1].height / 2f), CursorMode.ForceSoftware);
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
                case CursorType.Camera:
                    Cursor.SetCursor(textures[5], Vector2.zero, CursorMode.ForceSoftware);
                    break;
            }
        }

        // Texture2D 크기 조정
        private Texture2D ScaleTexture(Texture2D source, float scaleFactor)
        {
            if (Mathf.Approximately(scaleFactor, 1f))
            {
                return source;
            }

            if (Mathf.Approximately(scaleFactor, 0f))
            {
                return Texture2D.blackTexture;
            }

            int newWidth = Mathf.RoundToInt(source.width * scaleFactor);
            int newHeight = Mathf.RoundToInt(source.height * scaleFactor);
            
            Color[] scaledTexPixels = new Color[newWidth * newHeight];

            for (int yCord = 0; yCord < newHeight; yCord++)
            {
                float vCord = yCord / (newHeight * 1f);
                int scanLineIndex = yCord * newWidth;

                for (int xCord = 0; xCord < newWidth; xCord++)
                {
                    float uCord = xCord / (newWidth * 1f);

                    scaledTexPixels[scanLineIndex + xCord] = source.GetPixelBilinear(uCord, vCord);
                }
            }

            // Create Scaled Texture
            Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);
            result.SetPixels(scaledTexPixels, 0);
            result.Apply();

            return result;
        }
    }
}

