using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Waker.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public partial class SafeAreaContainer : UIBehaviour
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

        [Header("Offset")]
        [SerializeField] private float offsetTop;
        [SerializeField] private float offsetBottom;
        [SerializeField] private float offsetLeft;
        [SerializeField] private float offsetRight;

        private DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

        private RectTransform rectTransform;

        protected override void Awake()
        {
            rectTransform = transform as RectTransform;
         
            Manager.RegisterSafeAreaContainer(this);
        }

        protected override void OnDestroy()
        {
            Manager.UnregisterSafeAreaContainer(this);
        }

#if UNITY_EDITOR
        private void Update()
        {
            // Only in editor
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ApplySafeArea(Screen.safeArea);
            }
        }
#endif

        public void ApplySafeArea(Rect safeArea)
        {
            var min = safeArea.position;
            var max = min + safeArea.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            // NaN check
            if (float.IsNaN(min.x) || float.IsNaN(min.y) || float.IsNaN(max.x) || float.IsNaN(max.y))
            {
                return;
            }

            // Min
            var anchorMin = rectTransform.anchorMin;
            var offsetMin = rectTransform.offsetMin;

            if (edge.HasFlag(Edge.Left))
                anchorMin.x = min.x;
                
            offsetMin.x = offsetLeft;

            if (edge.HasFlag(Edge.Bottom))
                anchorMin.y = min.y;

            offsetMin.y = offsetBottom;

            var anchorMax = rectTransform.anchorMax;
            var offsetMax = rectTransform.offsetMax;

            // Max
            if (edge.HasFlag(Edge.Right))
                anchorMax.x = max.x;
            
            offsetMax.x = -offsetRight;

            if (edge.HasFlag(Edge.Top))
                anchorMax.y = max.y;

            offsetMax.y = -offsetTop;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            if (lockOffset)
            {
                rectTransform.offsetMin = offsetMin;
                rectTransform.offsetMax = offsetMax;
            }

            rectTransform.localScale = Vector3.one;

            // Control transform
            DrivenTransformProperties properties = DrivenTransformProperties.None;

            properties |= DrivenTransformProperties.AnchoredPositionZ;
            properties |= DrivenTransformProperties.Scale;

            if (edge.HasFlag(Edge.Top)) 
            {
                properties |= DrivenTransformProperties.AnchorMaxY;

                if (lockOffset)
                    properties |= DrivenTransformProperties.AnchoredPositionY;
            }
            
            if (edge.HasFlag(Edge.Bottom)) 
            {
                properties |= DrivenTransformProperties.AnchorMinY;

                if (lockOffset)
                    properties |= DrivenTransformProperties.SizeDeltaY;
            }
            
            if (edge.HasFlag(Edge.Left)) 
            {
                properties |= DrivenTransformProperties.AnchorMinX;

                if (lockOffset)
                    properties |= DrivenTransformProperties.AnchoredPositionX;
            }
            
            if (edge.HasFlag(Edge.Right)) 
            {
                properties |= DrivenTransformProperties.AnchorMaxX;

                if (lockOffset)
                    properties |= DrivenTransformProperties.SizeDeltaX;
            }

            tracker.Clear();
            tracker.Add(this, rectTransform, properties);
        }
    }
}