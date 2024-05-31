using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class MasterCameraHolder
    {
        public Camera MasterCamera { get; private set; }
        public Transform MasterCameraAnchor { get; private set; }

        public float ViewportLeftBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.zero).x;
        public float ViewportRightBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.right).x;
        public float ViewportUpperBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.up).y;
        public float ViewportLowerBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.zero).y;

        public MasterCameraHolder(GameObject masterCameraPrefab)
        {
            if (masterCameraPrefab == null) throw new ArgumentNullException();

            MasterCameraAnchor = UnityEngine.Object.Instantiate(masterCameraPrefab, Vector3.back, Quaternion.identity).transform;
            MasterCamera = MasterCameraAnchor.gameObject.GetComponentInChildren<Camera>();

            if (MasterCamera == null) throw new MissingComponentException(nameof(Camera));
        }

        public float GetViewportLeftBoundWithOffset(float offsetFactor) => ViewportLeftBound * offsetFactor;

        public float GetViewportRightBoundWithOffset(float offsetFactor) => ViewportRightBound * offsetFactor;

        public float GetViewportUpperBoundWithOffset(float offsetFactor) => ViewportUpperBound * offsetFactor;

        public float GetViewportLowerBoundWithOffset(float offsetFactor) => ViewportLowerBound * offsetFactor;

        public bool InsideViewport(Vector2 position)
        {
            if (MasterCamera == null) return false;

            return position.x > ViewportLeftBound &&
                   position.x < ViewportRightBound &&
                   position.y < ViewportUpperBound &&
                   position.y > ViewportLowerBound;
        }
    }
}