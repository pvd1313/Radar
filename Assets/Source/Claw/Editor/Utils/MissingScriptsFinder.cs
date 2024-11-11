using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Utils
{
    public static class MissingScriptsFinder
    {
        [MenuItem("Tools/Find Missing Scripts in Project")]
        public static void FindMissingScriptsInProject()
        {
            string[] allPrefabPaths = AssetDatabase.GetAllAssetPaths();
            int missingCount = 0;

            foreach (string path in allPrefabPaths)
            {
                if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        Component[] components = prefab.GetComponentsInChildren<Component>(true);

                        for (int i = 0; i < components.Length; i++)
                        {
                            if (components[i] == null)
                            {
                                UnityEngine.Debug.LogWarning($"Prefab '{path}' has a missing script attached!", prefab);
                                missingCount++;
                            }
                        }
                    }
                }
            }

            if (missingCount == 0)
            {
                UnityEngine.Debug.Log("No missing scripts found in the project.");
            }
            else
            {
                UnityEngine.Debug.Log($"Total missing scripts found in the project: {missingCount}");
            }
        }
    }
}