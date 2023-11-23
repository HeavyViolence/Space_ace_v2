using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class MasterCameraHolder : IMasterCameraHolder
    {
        public Camera MasterCamera { get; private set; }
        public GameObject MasterCameraObject { get; private set; }

        private float _viewportLeftBound = float.NaN;
        public float ViewportLeftBound
        {
            get
            {
                if (_viewportLeftBound == float.NaN)
                    _viewportLeftBound = MasterCamera.ViewportToWorldPoint(Vector3.zero).x;

                return _viewportLeftBound;
            }
        }

        private float _viewportRightBound = float.NaN;
        public float ViewportRightBound
        {
            get
            {
                if (_viewportRightBound == float.NaN)
                    _viewportRightBound = MasterCamera.ViewportToWorldPoint(Vector3.right).x;

                return _viewportRightBound;
            }
        }

        private float _viewportTopBound = float.NaN;
        public float ViewportTopBound
        {
            get
            {
                if (_viewportTopBound == float.NaN)
                    _viewportTopBound = MasterCamera.ViewportToWorldPoint(Vector3.up).y;

                return _viewportTopBound;
            }
        }

        private float _viewportBottomBound = float.NaN;
        public float ViewportBottomBound
        {
            get
            {
                if (_viewportBottomBound == float.NaN)
                    _viewportBottomBound = MasterCamera.ViewportToWorldPoint(Vector3.zero).y;

                return _viewportBottomBound;
            }
        }

        public MasterCameraHolder(GameObject masterCameraPrefab)
        {
            if (masterCameraPrefab == null)
                throw new ArgumentNullException("Attempted to pass an empty master camera prefab!");

            MasterCameraObject = UnityEngine.Object.Instantiate(masterCameraPrefab, Vector3.zero, Quaternion.identity);
            MasterCamera = MasterCameraObject.GetComponentInChildren<Camera>();

            if (MasterCamera == null)
                throw new MissingComponentException($"Passed master camera prefab is missing {typeof(Camera)}!");
        }

        public float GetShiftedViewportLeftBound(float offsetFactor) => ViewportLeftBound * offsetFactor;

        public float GetShiftedViewportRightBound(float offsetFactor) => ViewportRightBound * offsetFactor;

        public float GetShiftedViewportTopBound(float offsetFactor) => ViewportTopBound * offsetFactor;

        public float GetShiftedViewportBottomBound(float offsetFactor) => ViewportBottomBound * offsetFactor;

        public bool InsideViewport(Vector2 position, float delta = 0f) => position.x + delta < ViewportLeftBound &&
                                                                          position.x - delta > ViewportRightBound &&
                                                                          position.y - delta > ViewportTopBound &&
                                                                          position.y + delta < ViewportBottomBound;
    }
}