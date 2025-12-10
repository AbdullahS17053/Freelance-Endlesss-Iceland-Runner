using System;
using System.Collections.Generic;
using UnityEngine;
namespace Gley.CameraCulling
{
    public class API
    {
        public static void Initialize(QualityLevel qualityLevel, CullingMaskSettings cullingMaskSettings, Camera camera)
        {
            Debug.Log("QUALITY LEVEL: " + qualityLevel);

            List<LayerCullDistance> layerCullDistances = cullingMaskSettings.GetLayerCullDistances(qualityLevel);

            float[] distances = new float[32];
            int mask = 0;

            for (int i = 0; i < layerCullDistances.Count; i++)
            {
                distances[layerCullDistances[i].layer] = layerCullDistances[i].distance;
                mask |= 1 << LayerMask.NameToLayer(layerCullDistances[i].name);
            }

            camera.layerCullDistances = distances;
            camera.cullingMask = mask;
        }

        internal static void UpdateLights(List<LightObject> sceneLights, Transform activeCamera, int distance)
        {
            foreach (LightObject light in sceneLights)
            {
                if (light.active == true)
                {
                    if ((light.light.transform.position - activeCamera.position).sqrMagnitude > distance * distance)
                    {
                        if (light.light.enabled == true)
                        {
                            light.light.enabled = false;
                        }
                    }
                    else
                    {
                        if (light.light.enabled == false)
                        {
                            light.light.enabled = true;
                        }
                    }
                }
            }
        }
    }
}