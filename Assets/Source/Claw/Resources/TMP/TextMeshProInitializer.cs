using UnityEditor;
using UnityEngine;

public static class TextMeshProInitializer {

    private static TextMeshProResourcesAPI resourcesAPI;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitializeRuntime() {

        RegisterAPI();
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void InitializeEditor() {

        RegisterAPI();
    }
#endif

    private static void RegisterAPI() {

        resourcesAPI = new TextMeshProResourcesAPI();

        ResourcesAPI.overrideAPI = resourcesAPI;
    }
}