using UnityEngine;

namespace FullMoon.Entities.Unit
{
    public class RespawnController : MonoBehaviour
    {
        public int ManaCost { get; private set; }
        public float CreatePrepareTime { get; private set; }
        public float SummonTime { get; private set; }
        public GameObject UnitTransformObject { get; private set; }

        public void Setup(int manaCost, float createPrepareTime, float summonTime, GameObject unitTransformObject)
        {
            ManaCost = manaCost;
            CreatePrepareTime = createPrepareTime;
            SummonTime = summonTime;
            UnitTransformObject = unitTransformObject;
        }
    }
}
