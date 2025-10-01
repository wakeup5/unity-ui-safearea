using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Waker.UI
{
    [CustomEditor(typeof(SafeAreaRectTransform))]
    public class SafeAreaRectTransformEditor : Editor
    {
        private SerializedProperty _mode;
        private SerializedProperty _simple;
        private SerializedProperty _advanced;

        private void OnEnable()
        {
            _mode = serializedObject.FindProperty("_mode");
            _simple = serializedObject.FindProperty("_simple");
            _advanced = serializedObject.FindProperty("_advanced");
        }

        public override void OnInspectorGUI()
        {
            PrintWarning();

            serializedObject.Update();

            EditorGUILayout.PropertyField(_mode);
            SafeAreaRectTransform.Mode mode = (SafeAreaRectTransform.Mode)_mode.enumValueIndex;

            switch (mode)
            {
                case SafeAreaRectTransform.Mode.Automation:
                    EditorGUILayout.HelpBox("Safe Area RectTransform is fully enabled.", MessageType.Info);
                    break;
                case SafeAreaRectTransform.Mode.Simple:
                    DrawStructProperty(_simple);
                    break;
                case SafeAreaRectTransform.Mode.Advanced:
                    DrawStructProperty(_advanced);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStructProperty(SerializedProperty property)
        {
            if (property != null)
            {
                // 필드 그리기
                SerializedProperty iterator = property.Copy();
                iterator.NextVisible(true); // 첫 번째 자식으로 이동

                EditorGUILayout.LabelField(property.displayName, EditorStyles.boldLabel);
                
                do
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
                while (iterator.NextVisible(false) && iterator.propertyPath.StartsWith(property.propertyPath));
            }
        }

        private void PrintWarning()
        {
            SafeAreaRectTransform target = (SafeAreaRectTransform)this.target;
            CanvasScaler canvasScaler = target.GetComponentInParent<CanvasScaler>();

            if (canvasScaler is not SafeAreaCanvasScaler)
            {
                EditorGUILayout.HelpBox("Safe Area RectTransform must be used with Safe Area CanvasScaler.", MessageType.Warning);
            }

            if (IsAllFilledRect(target.transform.parent as RectTransform))
            {
                // 모든 부모 RectTransform은 화면 전체를 채워야 한다.
                EditorGUILayout.HelpBox("All parent RectTransforms must fill the entire screen.", MessageType.Warning);
            }
        }

        private bool IsAllFilledRect(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return false;
            }

            foreach (RectTransform parent in IteratorParent(rectTransform))
            {
                if (parent.parent == null)
                {
                    continue;
                }

                if (parent.anchorMin != Vector2.zero || 
                    parent.anchorMax != Vector2.one ||
                    parent.offsetMin != Vector2.zero ||
                    parent.offsetMax != Vector2.zero)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<RectTransform> IteratorParent(RectTransform rectTransform)
        {
            RectTransform parent = rectTransform.parent as RectTransform;

            while (parent != null)
            {
                yield return parent;
                parent = parent.parent as RectTransform;
            }
        }
    }
}
