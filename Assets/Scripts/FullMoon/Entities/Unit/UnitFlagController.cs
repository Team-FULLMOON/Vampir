using MyBox;
using UnityEngine;
using System.Collections.Generic;

namespace FullMoon.Entities.Unit
{
    public class UnitFlagController : MonoBehaviour
    {
        [AutoProperty] public List<BaseUnitController> unit;
    }
}