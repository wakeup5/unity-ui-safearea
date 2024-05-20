using UnityEditor;
using UnityEngine;

namespace Waker.UI
{
    [CustomPropertyDrawer(typeof(SafeAreaRectTransform.EdgeAnchor))]
    public class SafeAreaRectTransformEdgeAnchorPropertyDrawer : PropertyDrawer
    {
        private static readonly GUIContent[] _labels = new GUIContent[]
        {
            new GUIContent(""),
            new GUIContent("Min"),
            new GUIContent("Max"),
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty anchor = property.FindPropertyRelative("<Anchor>k__BackingField");
            EditorGUI.MultiPropertyField(position, _labels, anchor, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}