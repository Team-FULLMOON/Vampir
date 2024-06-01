using Unity.Burst;

namespace FullMoon.Entities.Building
{
    [BurstCompile]
    public class LumberBuildingController : BaseBuildingController
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            ShowFrame(5f).Forget();
        }
    }
}
