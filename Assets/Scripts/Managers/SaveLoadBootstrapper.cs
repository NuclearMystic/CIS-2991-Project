using CIS2991Project.UI;
using UnityEngine;

namespace CIS2991Project.Managers
{
    // Ensures the shared save/load panel (SaveLoadPanelController) exists in every scene, including
    // the Main Menu - same [RuntimeInitializeOnLoadMethod] + singleton-guard + DontDestroyOnLoad
    // shape as GameBootstrapper, just for a different persistent object.
    public class SaveLoadBootstrapper : MonoBehaviour
    {
        private const string PanelPrefabResourcePath = "Prefabs/UI/SaveLoadPanel";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindAnyObjectByType<SaveLoadPanelController>() != null)
            {
                return;
            }

            var prefab = Resources.Load<GameObject>(PanelPrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"SaveLoadBootstrapper: no prefab found at Resources/{PanelPrefabResourcePath}.prefab");
                return;
            }

            var panelObject = Object.Instantiate(prefab);
            panelObject.name = "SaveLoadPanel";
        }
    }
}
