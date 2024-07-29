using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class MasterCameraHolder
    {
        public Camera MasterCamera { get; private set; }
        public Transform MasterCameraAnchor { get; private set; }

        public float ViewportLeftBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.zero).x;
        public Vector3 ViewportLeftPosition => new(ViewportLeftBound, 0f, 0f);

        public float ViewportRightBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.right).x;
        public Vector3 ViewportRightPosition => new(ViewportRightBound, 0f, 0f);

        public float ViewportUpperBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.up).y;
        public Vector3 ViewportUpperPosition => new(0f, ViewportUpperBound, 0f);

        public float ViewportLowerBound => MasterCamera == null ? 0f : MasterCamera.ViewportToWorldPoint(Vector3.zero).y;
        public Vector3 ViewportLowerPosition => new(0f, ViewportLowerBound, 0f);

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