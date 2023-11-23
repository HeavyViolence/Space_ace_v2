using UnityEngine;

namespace SpaceAce.Main
{
    public interface IMasterCameraHolder
    {
        Camera MasterCamera { get; }
        GameObject MasterCameraObject { get; }

        float ViewportLeftBound { get; }
        float ViewportRightBound { get; }
        float ViewportTopBound { get; }
        float ViewportBottomBound { get; }

        float GetShiftedViewportLeftBound(float offsetFactor);
        float GetShiftedViewportRightBound(float offsetFactor);
        float GetShiftedViewportTopBound(float offsetFactor);
        float GetShiftedViewportBottomBound(float offsetFactor);

        bool InsideViewport(Vector2 position, float delta = 0f);
    }
}