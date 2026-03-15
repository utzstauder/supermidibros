#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// One-time setup: creates URP Asset and Renderer, assigns them in Project Settings,
/// and adds the Edge Detection renderer feature. Run via menu Tools > Setup URP for Edge Detection.
/// Requires the Universal RP package to be installed.
/// </summary>
public static class SetupURPForEdgeDetection
{
    const string SettingsFolder = "Assets/Settings";
    const string PipelineAssetPath = "Assets/Settings/UniversalRenderPipelineAsset.asset";
    const string RendererAssetPath = "Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset";

    [MenuItem("Tools/Setup URP for Edge Detection")]
    public static void SetupURP()
    {
        Debug.Log("Setup URP for Edge Detection: starting...");

        if (!System.IO.Directory.Exists(SettingsFolder))
            System.IO.Directory.CreateDirectory(SettingsFolder);

        UniversalRendererData rendererData;
        UniversalRenderPipelineAsset pipelineAsset;

        // Load existing or create new renderer
        rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
        if (rendererData == null)
        {
            Debug.Log("Creating Universal Renderer Data...");
            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            if (rendererData == null)
            {
                Debug.LogError("SetupURP: Could not create UniversalRendererData. Is the Universal RP package installed?");
                return;
            }
            AssetDatabase.CreateAsset(rendererData, RendererAssetPath);
        }
        else
            Debug.Log("Using existing renderer at " + RendererAssetPath);

        // Add Edge Detection feature if not already present
        var so = new SerializedObject(rendererData);
        var listProp = so.FindProperty("m_RendererFeatures");
        bool hasEdgeDetection = false;
        if (listProp != null && listProp.isArray)
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var refVal = listProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (refVal is EdgeDetectionFeature) { hasEdgeDetection = true; break; }
            }
        }
        if (!hasEdgeDetection && listProp != null && listProp.isArray)
        {
            Debug.Log("Adding Edge Detection feature to renderer...");
            var edgeFeature = ScriptableObject.CreateInstance<EdgeDetectionFeature>();
            edgeFeature.name = "Edge Detection Feature";
            AssetDatabase.AddObjectToAsset(edgeFeature, rendererData);
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = edgeFeature;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else if (hasEdgeDetection)
            Debug.Log("Edge Detection feature already on renderer.");
        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();

        // Load existing or create new pipeline asset
        pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
        if (pipelineAsset == null)
        {
            Debug.Log("Creating URP Asset...");
            pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
            if (pipelineAsset == null)
            {
                pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                var pipelineSo = new SerializedObject(pipelineAsset);
                var rendererListProp = pipelineSo.FindProperty("m_RendererList") ?? pipelineSo.FindProperty("m_RendererDataList");
                if (rendererListProp != null && rendererListProp.isArray)
                {
                    rendererListProp.ClearArray();
                    rendererListProp.InsertArrayElementAtIndex(0);
                    rendererListProp.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
                    pipelineSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
        }
        else
            Debug.Log("Using existing pipeline asset at " + PipelineAssetPath);
        AssetDatabase.SaveAssets();

        // Assign URP Asset in Graphics Settings (Unity 6 uses m_CustomRenderPipeline)
        bool assigned = AssignPipelineInGraphicsSettings(pipelineAsset);
        if (assigned)
            Debug.Log("Assigned URP Asset to Project Settings > Graphics (Default Render Pipeline).");
        else
            Debug.LogWarning("Could not assign pipeline in Graphics Settings. Please assign " + PipelineAssetPath + " manually in Edit > Project Settings > Graphics > Default Render Pipeline.");

        AssignPipelineInQualitySettings(pipelineAsset);

        // Add UniversalAdditionalCameraData to Main Camera if missing
        AddCameraDataToMainCamera();

        AssetDatabase.Refresh();
        Debug.Log("URP setup complete. Pipeline: " + PipelineAssetPath + ". Edge Detection is on the renderer. " + (assigned ? "Graphics set. Enter Play to see the effect." : "Assign the pipeline in Graphics if the effect does not appear."));
    }

    static bool AssignPipelineInGraphicsSettings(RenderPipelineAsset pipelineAsset)
    {
        const string path = "ProjectSettings/GraphicsSettings.asset";
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets == null || assets.Length == 0) return false;
        var so = new SerializedObject(assets[0]);
        // Unity 6 stores the default pipeline in m_CustomRenderPipeline (not m_DefaultRenderPipeline)
        var prop = so.FindProperty("m_CustomRenderPipeline")
            ?? so.FindProperty("m_DefaultRenderPipeline")
            ?? so.FindProperty("m_ScriptableRenderPipelineSettings");
        if (prop != null)
        {
            prop.objectReferenceValue = pipelineAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
        return false;
    }

    static void AddCameraDataToMainCamera()
    {
        var cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null) return;
        if (cam.GetComponent<UniversalAdditionalCameraData>() != null) return;
        if (cam.gameObject.AddComponent<UniversalAdditionalCameraData>() != null)
            Debug.Log("Added Universal Additional Camera Data to " + cam.gameObject.name + ".");
    }

    static void AssignPipelineInQualitySettings(RenderPipelineAsset pipelineAsset)
    {
        const string path = "ProjectSettings/QualitySettings.asset";
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var prop = so.FindProperty("m_QualitySettings");
        if (prop != null && prop.isArray)
        {
            for (int i = 0; i < prop.arraySize; i++)
            {
                var el = prop.GetArrayElementAtIndex(i);
                var rpProp = el.FindPropertyRelative("renderPipelineAsset") ?? el.FindPropertyRelative("renderPipeline");
                if (rpProp != null)
                {
                    rpProp.objectReferenceValue = pipelineAsset;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
