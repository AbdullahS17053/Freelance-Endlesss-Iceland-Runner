using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Gley.CameraCulling
{

    [CustomEditor(typeof(LayerCulling))]
    public class LayerCullingEditor : Editor
    {
        private const string FOLDER_NAME = "CameraCulling";
        private const string PARENT_FOLDER = "Poly Ninja/CameraCulling";
        private static string rootFolder;
        private static string rootWithoutAssets;
        string[] qualityLevelNames;
        LayerCulling targetScript;

        int currentQualitySettings;
        List<DistanceSettings> settings;
        int lightDistance;
        int min, max;

        List<LayerCullDistance> layerCullDistances;

        private void OnEnable()
        {
            targetScript = (LayerCulling)target;

            Initialize();
        }

        void Initialize()
        {
            LoadRootFolder();
            if (targetScript.cullingMaskSettings == null)
            {
                string path = EditorUtility.SaveFilePanel("Save Culling Distance Asset", $"{rootFolder}/Runtime", "CullingDistances", "asset");

                if (path.Length != 0)
                {
                    // Create a new ScriptableObject
                    CullingMaskSettings asset = ScriptableObject.CreateInstance<CullingMaskSettings>();

                    // Save the asset to the selected path
                    string relativePath = "Assets" + path.Replace(Application.dataPath, "");

                    AssetDatabase.CreateAsset(asset, relativePath);
                    AssetDatabase.SaveAssets();
                    targetScript.cullingMaskSettings = asset;
                    EditorUtility.SetDirty(target);
                }
            }
            //load values
            settings = targetScript.cullingMaskSettings.settings;
            currentQualitySettings = targetScript.cullingMaskSettings.lastEdited;
            layerCullDistances = targetScript.cullingMaskSettings.GetLayerCullDistances((QualityLevel)currentQualitySettings);
            lightDistance = targetScript.cullingMaskSettings.GetLightDistance((QualityLevel)currentQualitySettings);

            //load quality levels
            qualityLevelNames = QualitySettings.names;

            //load camera settings
            Camera camera = targetScript.GetComponent<Camera>();
            min = ((int)camera.nearClipPlane);
            max = ((int)camera.farClipPlane);
            int mask = camera.cullingMask;


            //apply loaded settings
            for (int i = 0; i < 32; i++)
            {
                // Check if the i-th bit of the mask is set
                if ((mask & (1 << i)) != 0)
                {
                    LayerCullDistance layer = layerCullDistances.FirstOrDefault(cond => cond.layer == i);
                    if (layer == null)
                    {
                        layerCullDistances.Add(new LayerCullDistance(i, LayerMask.LayerToName(i), max));
                    }
                    else
                    {
                        if(layer.name!= LayerMask.LayerToName(i))
                        {
                            layer.name = LayerMask.LayerToName(i);
                        }
                    }
                }
                else
                {
                    LayerCullDistance elem = layerCullDistances.FirstOrDefault(cond => cond.layer == i);
                    if (elem != null)
                    {
                        layerCullDistances.Remove(elem);
                    }
                }
            }

            layerCullDistances = layerCullDistances.OrderBy(x => x.layer).ToList();
        }


        static bool LoadRootFolder()
        {
            rootFolder = Gley.Common.EditorUtilities.FindFolder(FOLDER_NAME, PARENT_FOLDER);
            if (rootFolder == null)
            {
                Debug.LogError($"Folder Not Found:'{PARENT_FOLDER}/{FOLDER_NAME}'");
                return false;
            }
            rootWithoutAssets = rootFolder.Substring(7, rootFolder.Length - 7);
            return true;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                Initialize();
            }
            EditorGUILayout.Space();

            currentQualitySettings = EditorGUILayout.Popup("Quality", currentQualitySettings, qualityLevelNames);
            if (currentQualitySettings != targetScript.cullingMaskSettings.lastEdited)
            {
                targetScript.cullingMaskSettings.lastEdited = currentQualitySettings;
                layerCullDistances = targetScript.cullingMaskSettings.GetLayerCullDistances((QualityLevel)currentQualitySettings);
            }
            EditorGUILayout.Space();

            GUILayout.Label("Cull Distance:");
            for (int i = 0; i < layerCullDistances.Count; i++)
            {
                layerCullDistances[i].distance = EditorGUILayout.IntSlider(layerCullDistances[i].name, layerCullDistances[i].distance, min, max);
            }

            lightDistance = EditorGUILayout.IntSlider("Light Distance", lightDistance, 0, max);

            if (lightDistance > 0)
            {
                if (GUILayout.Button("Load Scene Lights"))
                {
                    LoadLights();
                }
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Save"))
            {
                SaveSettings();
            }

            //enumIndex = (int)myEnum;

            // Draw the default inspector elements for the target Monobehavior script
            DrawDefaultInspector();
        }

        private void SaveSettings()
        {
            if (lightDistance == 0)
            {
                targetScript.sceneLights = new List<LightObject>();
            }
            CreateEnumFiles();
            targetScript.cullingMaskSettings.SetLayerCullingDistance(currentQualitySettings, layerCullDistances, lightDistance);
            EditorUtility.SetDirty(targetScript.cullingMaskSettings);
        }

        private void LoadLights()
        {
            targetScript.sceneLights = new List<LightObject>();
            var lights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type != LightType.Directional)
                {
                    targetScript.sceneLights.Add(new LightObject(light, true));
                }
            }
        }

        private void CreateEnumFiles()
        {
            string text =
            "public enum QualityLevel\n" +
            "{\n";
            for (int i = 0; i < qualityLevelNames.Length; i++)
            {
                qualityLevelNames[i] = qualityLevelNames[i].Replace(" ", "");
                text += "\t" + qualityLevelNames[i] + ",\n";
            }
            text += "}";
            File.WriteAllText($"{Application.dataPath}/{rootWithoutAssets}/Runtime/QualityLevel.cs", text);

            AssetDatabase.Refresh();
        }
    }
}

