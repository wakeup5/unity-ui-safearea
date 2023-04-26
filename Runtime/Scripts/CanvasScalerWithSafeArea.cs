using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Canvas))]
    [ExecuteAlways]
    [AddComponentMenu("Layout/Canvas Scaler With SafeArea", 101)]
    [DisallowMultipleComponent]
    public class CanvasScalerWithSafeArea : CanvasScaler
    {
        private Canvas canvas;
        private const float kLogBase = 2;

        protected override void Awake()
        {
            base.Awake();
            canvas = GetComponent<Canvas>();
        }

        protected override void HandleScaleWithScreenSize()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            Vector2 screenSize = canvas.renderingDisplaySize;

            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            int displayIndex = canvas.targetDisplay;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                Display disp = Display.displays[displayIndex];
                screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
            }

            Vector2 safeArea = new Vector2(Screen.safeArea.width, Screen.safeArea.height);


            float scaleFactor = 0;
            var x = safeArea.x / screenSize.x;
            var y = safeArea.y / screenSize.y;

            switch (m_ScreenMatchMode)
            {
                case ScreenMatchMode.MatchWidthOrHeight:
                {
                    // We take the log of the relative width and height before taking the average.
                    // Then we transform it back in the original space.
                    // the reason to transform in and out of logarithmic space is to have better behavior.
                    // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                    // In normal space the average would be (0.5 + 2) / 2 = 1.25
                    // In logarithmic space the average is (-1 + 1) / 2 = 0
                    float logWidth = Mathf.Log((screenSize.x * x) / m_ReferenceResolution.x, kLogBase);
                    float logHeight = Mathf.Log((screenSize.y * y) / m_ReferenceResolution.y, kLogBase);
                    float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
                    scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                    break;
                }
                case ScreenMatchMode.Expand:
                {
                    scaleFactor = Mathf.Min((screenSize.x * x) / m_ReferenceResolution.x, (screenSize.y * y) / m_ReferenceResolution.y);
                    break;
                }
                case ScreenMatchMode.Shrink:
                {
                    scaleFactor = Mathf.Max((screenSize.x * x) / m_ReferenceResolution.x, (screenSize.y * y) / m_ReferenceResolution.y);
                    break;
                }
            }

            SetScaleFactor(scaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }
    }
}
