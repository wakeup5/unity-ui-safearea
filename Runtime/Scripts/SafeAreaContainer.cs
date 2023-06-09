using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Waker.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaContainer : UIBehaviour
    {
        [Flags]
        public enum Edge
        {
            Top = 1,
            Bottom = 2,
            Left = 4,
            Right = 8
        }

        [SerializeField] private Edge edge = Edge.Top | Edge.Bottom | Edge.Left | Edge.Right;
        [SerializeField] private bool lockOffset;

        private Rect lastSafeArea;
        private RectTransform parentRectTransform;
        private DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

        private RectTransform rectTransform;

        protected override void Awake()
        {
            base.Awake();

            rectTransform = transform as RectTransform;
            parentRectTransform = this.GetComponentInParent<RectTransform>();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            ApplySafeArea();
#else
            if (lastSafeArea != Screen.safeArea)
            {
                ApplySafeArea();
            }
#endif
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;

            var min = safeArea.position;
            var max = min + safeArea.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            // Min
            var anchorMin = rectTransform.anchorMin;
            var offsetMin = rectTransform.offsetMin;

            if (edge.HasFlag(Edge.Left))
            {
                anchorMin.x = min.x;
                offsetMin.x = 0f;
            }

            if (edge.HasFlag(Edge.Bottom))
            {
                anchorMin.y = min.y;
                offsetMin.y = 0f;
            }

            var anchorMax = rectTransform.anchorMax;
            var offsetMax = rectTransform.offsetMax;

            // Max
            if (edge.HasFlag(Edge.Right))
            {
                anchorMax.x = max.x;
                offsetMax.x = 0f;
            }

            if (edge.HasFlag(Edge.Top))
            {
                anchorMax.y = max.y;
                offsetMax.y = 0f;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            if (lockOffset)
            {
                rectTransform.offsetMin = offsetMin;
                rectTransform.offsetMax = offsetMax;
            }

            rectTransform.localScale = Vector3.one;

            // Control transform
            var properties = DrivenTransformProperties.AnchoredPositionZ | DrivenTransformProperties.Scale;

            if (edge.HasFlag(Edge.Top)) properties |= DrivenTransformProperties.AnchorMaxY;// | DrivenTransformProperties.AnchoredPositionY;
            if (edge.HasFlag(Edge.Bottom)) properties |= DrivenTransformProperties.AnchorMinY;// | DrivenTransformProperties.ScaleY;
            if (edge.HasFlag(Edge.Left)) properties |= DrivenTransformProperties.AnchorMinX;// | DrivenTransformProperties.AnchoredPositionX;
            if (edge.HasFlag(Edge.Right)) properties |= DrivenTransformProperties.AnchorMaxX;// | DrivenTransformProperties.ScaleX;

            tracker.Clear();

            tracker.Add(this, rectTransform, properties);
        }
    }
}