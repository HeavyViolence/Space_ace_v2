using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class MasterCameraHolder : MonoBehaviour
    {
        [SerializeField] private Camera _masterCamera;

        public Camera MasterCamera => _masterCamera;

        private float _viewportLeftBound = float.NaN;
        public float ViewportLeftBound
        {
            get
            {
                if (_viewportLeftBound == float.NaN)
                    _viewportLeftBound = _masterCamera.ViewportToWorldPoint(Vector3.zero).x;

                return _viewportLeftBound;
            }
        }

        private float _viewportRightBound = float.NaN;
        public float ViewportRightBound
        {
            get
            {
                if (_viewportRightBound == float.NaN)
                    _viewportRightBound = _masterCamera.ViewportToWorldPoint(Vector3.right).x;

                return _viewportRightBound;
            }
        }

        private float _viewportUpperBound = float.NaN;
        public float ViewportUpperBound
        {
            get
            {
                if (_viewportUpperBound == float.NaN)
                    _viewportUpperBound = _masterCamera.ViewportToWorldPoint(Vector3.up).y;

                return _viewportUpperBound;
            }
        }

        private float _viewportLowerBound = float.NaN;
        public float ViewportLowerBound
        {
            get
            {
                if (_viewportLowerBound == float.NaN)
                    _viewportLowerBound = _masterCamera.ViewportToWorldPoint(Vector3.zero).y;

                return _viewportLowerBound;
            }
        }

        public float GetShiftedViewportLeftBound(float offsetFactor) => ViewportLeftBound * offsetFactor;

        public float GetShiftedViewportRightBound(float offsetFactor) => ViewportRightBound * offsetFactor;

        public float GetShiftedViewportUpperBound(float offsetFactor) => ViewportUpperBound * offsetFactor;

        public float GetShiftedViewportLowerBound(float offsetFactor)  => ViewportLowerBound * offsetFactor;
    }
}