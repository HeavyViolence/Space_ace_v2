using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class MasterCameraHolder
    {
        public Camera MasterCamera { get; private set; }
        public GameObject MasterCameraObject { get; private set; }

        public float ViewportLeftBound => MasterCamera.ViewportToWorldPoint(Vector3.zero).x;
        public float ViewportRightBound => MasterCamera.ViewportToWorldPoint(Vector3.right).x;
        public float ViewportUpperBound => MasterCamera.ViewportToWorldPoint(Vector3.up).y;
        public float ViewportLowerBound => MasterCamera.ViewportToWorldPoint(Vector3.zero).y;

        public MasterCameraHolder(GameObject masterCameraPrefab)
        {
            if (masterCameraPrefab == null) throw new ArgumentNullException();

            MasterCameraObject = UnityEngine.Object.Instantiate(masterCameraPrefab, Vector3.back, Quaternion.identity);
            MasterCamera = MasterCameraObject.GetComponentInChildren<Camera>();

            if (MasterCamera == null)
                throw new MissingComponentException($"Passed master camera prefab is missing {typeof(Camera)}!");
        }

        public float GetViewportLeftBoundWithOffset(float offsetFactor) => ViewportLeftBound * offsetFactor;

        public float GetViewportRightBoundWithOffset(float offsetFactor) => ViewportRightBound * offsetFactor;

        public float GetViewportUpperBoundWithOffset(float offsetFactor) => ViewportUpperBound * offsetFactor;

        public float GetViewportLowerBoundWithOffset(float offsetFactor) => ViewportLowerBound * offsetFactor;

        public bool InsideViewport(Vector2 position, float delta = 0f) => position.x + delta < ViewportLeftBound &&
                                                                          position.x - delta > ViewportRightBound &&
                                                                          position.y - delta > ViewportUpperBound &&
                                                                          position.y + delta < ViewportLowerBound;
    }
}