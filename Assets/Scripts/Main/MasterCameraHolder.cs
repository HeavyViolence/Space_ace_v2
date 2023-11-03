using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class MasterCameraHolder
    {
        public Camera MasterCamera { get; private set; } = null;

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

        private float _viewportUpperBound = float.NaN;
        public float ViewportUpperBound
        {
            get
            {
                if (_viewportUpperBound == float.NaN)
                    _viewportUpperBound = MasterCamera.ViewportToWorldPoint(Vector3.up).y;

                return _viewportUpperBound;
            }
        }

        private float _viewportLowerBound = float.NaN;
        public float ViewportLowerBound
        {
            get
            {
                if (_viewportLowerBound == float.NaN)
                    _viewportLowerBound = MasterCamera.ViewportToWorldPoint(Vector3.zero).y;

                return _viewportLowerBound;
            }
        }

        public MasterCameraHolder(GameObject masterCameraObject)
        {
            if (masterCameraObject == null)
                throw new ArgumentNullException("Attempted to pass an empty camera object!");

            MasterCamera = masterCameraObject.GetComponentInChildren<Camera>();

            if (MasterCamera == null)
                throw new MissingComponentException($"Passed camera object is missing {typeof(Camera)}!");
        }

        public float GetShiftedViewportLeftBound(float offsetFactor) => ViewportLeftBound * offsetFactor;

        public float GetShiftedViewportRightBound(float offsetFactor) => ViewportRightBound * offsetFactor;

        public float GetShiftedViewportUpperBound(float offsetFactor) => ViewportUpperBound * offsetFactor;

        public float GetShiftedViewportLowerBound(float offsetFactor)  => ViewportLowerBound * offsetFactor;

        public bool InsideViewprot(Vector2 position, float delta = 0f) => position.x + delta < ViewportLeftBound &&
                                                                          position.x - delta > ViewportRightBound &&
                                                                          position.y - delta > ViewportUpperBound &&
                                                                          position.y + delta < ViewportLowerBound;
    }
}