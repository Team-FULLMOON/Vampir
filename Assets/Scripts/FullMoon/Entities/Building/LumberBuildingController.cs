using Unity.Burst;

namespace FullMoon.Entities.Building
{
    [BurstCompile]
    public class LumberBuildingController : BaseBuildingController
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            ShowFrame(buildingData.BuildTime).Forget();
        }
    }
}
