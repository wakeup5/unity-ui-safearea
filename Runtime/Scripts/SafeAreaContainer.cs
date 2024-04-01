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

        [Header("Top")]
        [SerializeField] private bool enableTop;
        [SerializeField] private float anchorTopInSafeArea = 1f;
        [SerializeField] private float offsetTop;

        [Header("Bottom")]
        [SerializeField] private bool enableBottom;
        [SerializeField] private float anchorBottomInSafeArea = 0f;
        [SerializeField] private float offsetBottom;

        [Header("Left")]
        [SerializeField] private bool enableLeft;
        [SerializeField] private float anchorLeftInSafeArea = 0f;
        [SerializeField] private float offsetLeft;

        [Header("Right")]
        [SerializeField] private bool enableRight;
        [SerializeField] private float anchorRightInSafeArea = 1f;
        [SerializeField] private float offsetRight;

        private DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

        private RectTransform rectTransform;

        private void ApplySafeAreaTop(Rect safeArea, Vector2 safeAreaAnchorMinMaxY, ref DrivenTransformProperties property)
        {
            var anchorMax = rectTransform.anchorMax;
            var offsetMax = rectTransform.offsetMax;

            
        }

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
            // Clamp
            anchorTopInSafeArea = Mathf.Clamp01(anchorTopInSafeArea);
            anchorBottomInSafeArea = Mathf.Clamp01(anchorBottomInSafeArea);
            anchorLeftInSafeArea = Mathf.Clamp01(anchorLeftInSafeArea);
            anchorRightInSafeArea = Mathf.Clamp01(anchorRightInSafeArea);

            // Apply
            bool et = enableTop || edge.HasFlag(Edge.Top);
            bool eb = enableBottom || edge.HasFlag(Edge.Bottom);
            bool el = enableLeft || edge.HasFlag(Edge.Left);
            bool er = enableRight || edge.HasFlag(Edge.Right);

            float at = anchorTopInSafeArea;
            float ab = anchorBottomInSafeArea;
            float al = anchorLeftInSafeArea;
            float ar = anchorRightInSafeArea;

            if (lockOffset)
            {
                at = 1f;
                ab = 0f;
                al = 0f;
                ar = 1f;
            }

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

            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;

            var offsetMin = rectTransform.offsetMin;            
            var offsetMax = rectTransform.offsetMax;

             // Control transform
            DrivenTransformProperties properties = DrivenTransformProperties.None;

            properties |= DrivenTransformProperties.AnchoredPositionZ;
            properties |= DrivenTransformProperties.Scale;

            offsetMax.y = -offsetTop;
            if (et)
            {
                anchorMax.y = Mathf.Lerp(min.y, max.y, at);

                properties |= DrivenTransformProperties.AnchorMaxY;
                properties |= DrivenTransformProperties.AnchoredPositionY;
            }

            offsetMin.y = offsetBottom;
            if (eb)
            {
                anchorMin.y = Mathf.Lerp(min.y, max.y, ab);

                properties |= DrivenTransformProperties.AnchorMinY;
                properties |= DrivenTransformProperties.SizeDeltaY;
            }

            offsetMin.x = offsetLeft;
            if (el)
            {
                anchorMin.x = Mathf.Lerp(min.x, max.x, al);

                properties |= DrivenTransformProperties.AnchorMinX;
                properties |= DrivenTransformProperties.AnchoredPositionX;
            }

            offsetMax.x = -offsetRight;
            if (er)
            {
                anchorMax.x = Mathf.Lerp(min.x, max.x, ar);

                properties |= DrivenTransformProperties.AnchorMaxX;
                properties |= DrivenTransformProperties.SizeDeltaX;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            
            rectTransform.localScale = Vector3.one;

            tracker.Clear();
            tracker.Add(this, rectTransform, properties);
        }
    }
}