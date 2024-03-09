using System.Collections.Generic;
using UnityEngine;

namespace Waker.UI
{
    public partial class SafeAreaContainer
    {
        public class Manager : MonoBehaviour
        {
            private static Manager instance;

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialize()
            {
                var go = new GameObject("SafeAreaContainerManager");
                instance = go.AddComponent<Manager>();
                DontDestroyOnLoad(go);
            }

            public static void RegisterSafeAreaContainer(SafeAreaContainer container)
            {
                if (container == null)
                {
                    return;
                }

                if (instance == null)
                {
                    return;
                }

                instance.containers.Add(container);
            }

            public static void UnregisterSafeAreaContainer(SafeAreaContainer container)
            {
                if (container == null)
                {
                    return;
                }

                if (instance == null)
                {
                    return;
                }

                instance.containers.Remove(container);
            }

            private Rect lastSafeArea;
            private List<SafeAreaContainer> containers = new List<SafeAreaContainer>();

            private void Update()
            {
                if (lastSafeArea == Screen.safeArea)
                {
                    return;
                }

                lastSafeArea = Screen.safeArea;

                foreach (var container in containers)
                {
                    if (container != null && container.isActiveAndEnabled)
                    {
                        container.ApplySafeArea(lastSafeArea);
                    }
                }
            }
        }
    }
}