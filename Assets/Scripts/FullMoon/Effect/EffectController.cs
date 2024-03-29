using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;

namespace FullMoon.Effect
{
    [Serializable]
    public class EffectDetail
    {
        public GameObject effect;
        
        [Separator("Movement Type")]
        
        [DefinedValues("Static", "Straight")] public string movementType;
        
        [ConditionalField(nameof(movementType), false, "Straight")]
        public float movementSpeed;
    }
    
    public class EffectController : MonoBehaviour
    {
        [SerializeField] private List<EffectDetail> effectDetails;
    }
}
