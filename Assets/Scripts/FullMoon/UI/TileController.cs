using System.Collections;
using System.Collections.Generic;
using FullMoon.Util;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace FullMoon.UI
{
    [DefaultExecutionOrder(-1)]
    public class TileController : ComponentSingleton<TileController>
    {
        Rect castleRect;
        private Vector2 castleMiddlePos;
        private Vector2 castleSize;
        private float rectLength = 4;

        void Start()
        {
            castleMiddlePos = new Vector2(-1f, -6f);
            castleSize = new Vector2(8f, 8f);
            castleRect = new Rect(castleMiddlePos, castleSize * 2);
        }

        // 월드 좌표 -> 타일 좌표
        public Vector2 WorldToTileVector(Vector3 pos)
        {
            Vector2 vector = new Vector2(pos.x, pos.z);
            Vector2 value = new Vector2(0, 0);

            if (castleRect.Contains(vector))
            {
            }
            else
            {
                if (vector.x < castleRect.center.x)
                {
                    value.y = Mathf.Abs((int)(castleRect.center.x - vector.x) / rectLength);
                }
                else if (vector.x > castleRect.center.x)
                {
                    value.y = (int)((castleRect.center.x - vector.x) / rectLength);
                }
                if (vector.y < castleRect.center.y)
                {
                    value.x = Mathf.Abs((int)(castleRect.center.y - vector.y) / rectLength);
                }
                else if (vector.y >= castleRect.center.y)
                {
                    value.x = (int)((castleRect.center.y - vector.y) / rectLength);
                }
            }
            
            return value;
        }

        // 타일 좌표 -> 월드 좌표(타일 중앙 값)
        public Vector2 TileToWorldVector(Vector3 pos)
        {
            Vector2 vector = WorldToTileVector(pos);

            return new Vector2(castleRect.center.x + (vector.x * rectLength), 
                               castleRect.center.y + (vector.y * rectLength));
        }
    }
}