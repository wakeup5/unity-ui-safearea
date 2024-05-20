using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Waker.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Safe Area RectTransform", 101)]
    public partial class SafeAreaRectTransform : UIBehaviour
    {
        public enum Mode
        {
            Fully,
            Simple,
            Advanced,
        }

        [Flags]
        public enum Edge
        {
            Top = 1,
            Bottom = 2,
            Left = 4,
            Right = 8
        }

        public enum AnchorType
        {
            FullScreen,
            SafeArea,
        }

        [System.Serializable]
        public class Simple
        {    
            [field:SerializeField]
            public Vector2 OffsetMin { get; set; } = new Vector2(0f, 0f);

            [field:SerializeField] 
            public Vector2 OffsetMax { get; set; } = new Vector2(0f, 0f);

            [field:SerializeField] 
            public Edge Edge { get; set; } = Edge.Top | Edge.Bottom | Edge.Left | Edge.Right;

            [field:SerializeField] 
            public Vector2 AnchorMin { get; set; } = new Vector2(0f, 0f);

            [field:SerializeField] 
            public Vector2 AnchorMax { get; set; } = new Vector2(1f, 1f);
        }

        [System.Serializable]
        public class Advanced
        {
            [field:SerializeField]
            public Vector2 OffsetMin { get; set; } = new Vector2(0f, 0f);

            [field:SerializeField] 
            public Vector2 OffsetMax { get; set; } = new Vector2(0f, 0f);

            [field:SerializeField]
            public EdgeAnchor Left { get; set; } = new EdgeAnchor(0f);
            
            [field:SerializeField]
            public EdgeAnchor Right { get; set; } = new EdgeAnchor(1f);

            [field:SerializeField]
            public EdgeAnchor Top { get; set; } = new EdgeAnchor(1f);

            [field:SerializeField]
            public EdgeAnchor Bottom { get; set; } = new EdgeAnchor(0f);
        }

        [System.Serializable]
        public class EdgeAnchor
        {
            public EdgeAnchor()
            {
                
            }

            public EdgeAnchor(float anchor)
            {
                Anchor = anchor;
            }

            [field:SerializeField]
            public float Anchor { get; set; } = 0f;

            [field:SerializeField]
            public AnchorType Min { get; set; } = AnchorType.FullScreen;

            [field:SerializeField]
            public AnchorType Max { get; set; } = AnchorType.FullScreen;
        }

        [SerializeField] private Mode _mode = Mode.Fully;
        [SerializeField] private Simple _simple = new Simple();
        [SerializeField] private Advanced _advanced = new Advanced();

        private DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

        private RectTransform rectTransform;
        private Rect safeArea;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (rectTransform == null)
                rectTransform = transform as RectTransform;

             // Control transform
            DrivenTransformProperties properties = DrivenTransformProperties.None;

            properties |= DrivenTransformProperties.AnchoredPositionX;
            properties |= DrivenTransformProperties.AnchoredPositionY;
            properties |= DrivenTransformProperties.AnchorMinX;
            properties |= DrivenTransformProperties.AnchorMinY;
            properties |= DrivenTransformProperties.AnchorMaxX;
            properties |= DrivenTransformProperties.AnchorMaxY;
            properties |= DrivenTransformProperties.SizeDeltaX;
            properties |= DrivenTransformProperties.SizeDeltaY;

            tracker.Clear();
            tracker.Add(this, rectTransform, properties);
        }

        protected override void Awake()
        {
            base.Awake();

            rectTransform = transform as RectTransform;
        }

        protected override void Start()
        {
            base.Start();

            safeArea = Screen.safeArea;
            ApplySafeArea(safeArea);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                safeArea = Screen.safeArea;
                ApplySafeArea(safeArea);
                return;
            }
#endif

            if (safeArea != Screen.safeArea)
            {
                safeArea = Screen.safeArea;
                ApplySafeArea(safeArea);
            }
        }

        private static void GetSafeAreaAnchor(Rect safeArea, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        }

        private static T Choose<T>(bool condition, T a, T b)
        {
            return condition ? a : b;
        }

        public void ApplySafeArea(Rect safeArea)
        {
            switch (_mode)
            {
                case Mode.Fully:
                    ApplySafeAreaFully();
                    break;
                case Mode.Simple:
                    ApplySafeAreaSimpleMode();
                    break;
                case Mode.Advanced:
                    ApplySafeAreaAdvancedMode();
                    break;
            }
        }

        private void ApplySafeAreaFully()
        {
            Vector2 safeAreaMin = new Vector2();
            Vector2 safeAreaMax = new Vector2();

            GetSafeAreaAnchor(safeArea, out safeAreaMin, out safeAreaMax);

            rectTransform.anchorMin = safeAreaMin;
            rectTransform.anchorMax = safeAreaMax;

            rectTransform.offsetMin = new Vector2(0f, 0f);
            rectTransform.offsetMax = new Vector2(0f, 0f);
        }

        private void ApplySafeAreaSimpleMode()
        {
            // Variables
            Vector2 offsetMin = _simple.OffsetMin;
            Vector2 offsetMax = _simple.OffsetMax;

            Vector2 anchorMin = _simple.AnchorMin;
            Vector2 anchorMax = _simple.AnchorMax;

            Edge edge = _simple.Edge;

            // Apply
            Vector2 fullscreenMin = new Vector2(0f, 0f);
            Vector2 fullscreenMAx = new Vector2(1f, 1f);

            Vector2 safeAreaMin = new Vector2();
            Vector2 safeAreaMax = new Vector2();

            GetSafeAreaAnchor(safeArea, out safeAreaMin, out safeAreaMax);

            float minX = Choose(edge.HasFlag(Edge.Left), safeAreaMin.x, fullscreenMin.x);
            float minY = Choose(edge.HasFlag(Edge.Bottom), safeAreaMin.y, fullscreenMin.y);
            float maxX = Choose(edge.HasFlag(Edge.Right), safeAreaMax.x, fullscreenMAx.x);
            float maxY = Choose(edge.HasFlag(Edge.Top), safeAreaMax.y, fullscreenMAx.y);

            Vector2 finalAnchorMin = new Vector2();
            Vector2 finalAnchorMax = new Vector2();

            finalAnchorMin.x = Mathf.Lerp(minX, maxX, anchorMin.x);
            finalAnchorMin.y = Mathf.Lerp(minY, maxY, anchorMin.y);
            finalAnchorMax.x = Mathf.Lerp(minX, maxX, anchorMax.x);
            finalAnchorMax.y = Mathf.Lerp(minY, maxY, anchorMax.y);

            rectTransform.anchorMin = finalAnchorMin;
            rectTransform.anchorMax = finalAnchorMax;

            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private void ApplySafeAreaAdvancedMode()
        {
            Vector2 offsetMin = _advanced.OffsetMin;
            Vector2 offsetMax = _advanced.OffsetMax;

            // Apply
            Vector2 fullscreenMin = new Vector2(0f, 0f);
            Vector2 fullscreenMax = new Vector2(1f, 1f);

            Vector2 safeAreaMin = new Vector2();
            Vector2 safeAreaMax = new Vector2();

            GetSafeAreaAnchor(safeArea, out safeAreaMin, out safeAreaMax);

            Vector2 fullScreenX = new Vector2(fullscreenMin.x, fullscreenMax.x);
            Vector2 fullScreenY = new Vector2(fullscreenMin.y, fullscreenMax.y);

            Vector2 safeAreaX = new Vector2(safeAreaMin.x, safeAreaMax.x);
            Vector2 safeAreaY = new Vector2(safeAreaMin.y, safeAreaMax.y);

            Vector2 finalAnchorMin = new Vector2();
            finalAnchorMin.x = GetAnchor(_advanced.Left, fullScreenX, safeAreaX);
            finalAnchorMin.y = GetAnchor(_advanced.Bottom, fullScreenY, safeAreaY);

            Vector2 finalAnchorMax = new Vector2();
            finalAnchorMax.x = GetAnchor(_advanced.Right, fullScreenX, safeAreaX);
            finalAnchorMax.y = GetAnchor(_advanced.Top, fullScreenY, safeAreaY);

            rectTransform.anchorMin = finalAnchorMin;
            rectTransform.anchorMax = finalAnchorMax;

            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private float GetAnchor(EdgeAnchor edge, Vector2 fullscreen, Vector2 safeArea)
        {
            float min = Choose(edge.Min == AnchorType.SafeArea, safeArea.x, fullscreen.x);
            float max = Choose(edge.Max == AnchorType.SafeArea, safeArea.y, fullscreen.y);

            return Mathf.Lerp(min, max, edge.Anchor);
        }
    }
}