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
            Automation,
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
            [field: SerializeField]
            public Vector2 OffsetMin { get; set; } = new Vector2(0f, 0f);

            [field: SerializeField]
            public Vector2 OffsetMax { get; set; } = new Vector2(0f, 0f);

            [field: SerializeField]
            public Edge Edge { get; set; } = Edge.Top | Edge.Bottom | Edge.Left | Edge.Right;

            [field: SerializeField]
            public Vector2 AnchorMin { get; set; } = new Vector2(0f, 0f);

            [field: SerializeField]
            public Vector2 AnchorMax { get; set; } = new Vector2(1f, 1f);
        }

        [System.Serializable]
        public class Advanced
        {
            [field: SerializeField]
            public Vector2 OffsetMin { get; set; } = new Vector2(0f, 0f);

            [field: SerializeField]
            public Vector2 OffsetMax { get; set; } = new Vector2(0f, 0f);

            [field: SerializeField]
            public EdgeAnchor Left { get; set; } = new EdgeAnchor(0f);

            [field: SerializeField]
            public EdgeAnchor Right { get; set; } = new EdgeAnchor(1f);

            [field: SerializeField]
            public EdgeAnchor Top { get; set; } = new EdgeAnchor(1f);

            [field: SerializeField]
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

            [field: SerializeField]
            public float Anchor { get; set; } = 0f;

            [field: SerializeField]
            public AnchorType Min { get; set; } = AnchorType.FullScreen;

            [field: SerializeField]
            public AnchorType Max { get; set; } = AnchorType.FullScreen;
        }

        [SerializeField] private Mode _mode = Mode.Simple;
        [SerializeField] private Simple _simple = new Simple();
        [SerializeField] private Advanced _advanced = new Advanced();

        private Canvas _rootCanvas;
        private RectTransform rectTransform;
        private Rect safeArea;

#if UNITY_EDITOR
        private DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();
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
#endif

        protected override void Awake()
        {
            base.Awake();

            rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                Debug.LogError("SafeAreaRectTransform requires a RectTransform component.");
                return;
            }

            _rootCanvas = GetRootCanvas();
            if (_rootCanvas == null)
            {
                Debug.LogError("SafeAreaRectTransform requires a Canvas component in the hierarchy.");
                return;
            }
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
                case Mode.Automation:
                    ApplySafeAreaAutomation();
                    break;
                case Mode.Simple:
                    ApplySafeAreaSimple();
                    break;
                case Mode.Advanced:
                    ApplySafeAreaAdvanced();
                    break;
            }
        }

        private void ApplySafeAreaAutomation()
        {
            Vector2 safeAreaMin = new Vector2();
            Vector2 safeAreaMax = new Vector2();

            GetSafeAreaAnchor(safeArea, out safeAreaMin, out safeAreaMax);

            // rectTransform.anchorMin = safeAreaMin;
            // rectTransform.anchorMax = safeAreaMax;

            // rectTransform.offsetMin = new Vector2(0f, 0f);
            // rectTransform.offsetMax = new Vector2(0f, 0f);

            SetAnchorAndOffset(safeAreaMin, safeAreaMax, Vector2.zero, Vector2.zero);
        }

        private void ApplySafeAreaSimple()
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

            // rectTransform.anchorMin = finalAnchorMin;
            // rectTransform.anchorMax = finalAnchorMax;

            // rectTransform.offsetMin = offsetMin;
            // rectTransform.offsetMax = offsetMax;

            SetAnchorAndOffset(finalAnchorMin, finalAnchorMax, offsetMin, offsetMax);
        }

        private void ApplySafeAreaAdvanced()
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

            // rectTransform.anchorMin = finalAnchorMin;
            // rectTransform.anchorMax = finalAnchorMax;

            // rectTransform.offsetMin = offsetMin;
            // rectTransform.offsetMax = offsetMax;

            SetAnchorAndOffset(finalAnchorMin, finalAnchorMax, offsetMin, offsetMax);
        }

        private void SetAnchorAndOffset(Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            // rectTransform.anchorMin = anchorMin;
            // rectTransform.anchorMax = anchorMax;

            // rectTransform.offsetMin = offsetMin;
            // rectTransform.offsetMax = offsetMax;

            var canvasSize = GetCanvasSize();

            Vector2 anchorMinSize = new Vector2(canvasSize.x * anchorMin.x, canvasSize.y * anchorMin.y);
            Vector2 anchorMaxSize = new Vector2(canvasSize.x * anchorMax.x, canvasSize.y * anchorMax.y) - canvasSize;

            rectTransform.offsetMin = offsetMin + anchorMinSize;
            rectTransform.offsetMax = offsetMax + anchorMaxSize;
        }

        private float GetAnchor(EdgeAnchor edge, Vector2 fullscreen, Vector2 safeArea)
        {
            float min = Choose(edge.Min == AnchorType.SafeArea, safeArea.x, fullscreen.x);
            float max = Choose(edge.Max == AnchorType.SafeArea, safeArea.y, fullscreen.y);

            return Mathf.Lerp(min, max, edge.Anchor);
        }

        private Canvas GetRootCanvas()
        {
            Canvas parentCanvas = rectTransform.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                return null;
            }
            
            return parentCanvas.rootCanvas;
        }

        private Vector2 GetCanvasSize()
        {
            if (_rootCanvas != null)
            {
                return new Vector2(Screen.width, Screen.height);
            }

            return new Vector2(Screen.width, Screen.height);
        }
    }
}
