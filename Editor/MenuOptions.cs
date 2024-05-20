using UnityEditor;
using UnityEngine;

namespace Waker.UI
{
    internal static class MenuOptions
    {
        [MenuItem("GameObject/UI/Safe Area Container", false, 101)]
        public static void CreateSafeAreaContainer()
        {
            if (Selection.activeGameObject == null)
            {
                return;
            }

            var parent = Selection.activeGameObject.GetComponent<RectTransform>();

            if (parent == null)
            {
                return;
            }

            var obj = new GameObject("Safe Area Container", typeof(SafeAreaRectTransform));
            obj.transform.SetParent(parent, false);

            Selection.activeGameObject = obj;
        }
    }
}