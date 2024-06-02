using MyBox;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using FullMoon.Entities.Unit;

namespace FullMoon.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UnitUIController : MonoBehaviour
    {
        [Separator("HP")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Color playerColor = Color.blue;
        [SerializeField] private Color enemyColor = Color.red;

        private BaseUnitController Unit { get; set; }

        private void Start()
        {
            GetComponent<Canvas>().worldCamera = UnityEngine.Camera.main;
            Unit = GetComponentInParent<BaseUnitController>();

            hpSlider.gameObject.SetActive(true);
            hpFillImage.color = Unit.unitData.UnitType == "Player" ? playerColor : enemyColor;
            hpSlider.maxValue = Unit.unitData.MaxHp;
            hpSlider.value = Unit.unitData.MaxHp;
        }

        private void Update()
        {
            if (Unit.Hp != (int)hpSlider.value)
            {
                hpSlider.DOValue(Unit.Hp, 0.3f).SetEase(Ease.OutQuad);
            }
        }
    }
}