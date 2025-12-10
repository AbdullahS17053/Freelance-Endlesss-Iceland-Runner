using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.CameraCulling
{
    public class CullingMaskSettings : ScriptableObject
    {
        public int lastEdited;
        public List<DistanceSettings> settings = new List<DistanceSettings>();

        public List<LayerCullDistance> GetLayerCullDistances(QualityLevel qualityLevel)
        {
            DistanceSettings item = settings.FirstOrDefault(cond => cond.quality == (int)qualityLevel);
            if (item != null)
            {
                return item.layerCullDistances;
            }
            else
            {
                return new List<LayerCullDistance>();
            }
        }

        public void SetLayerCullingDistance(int qualityLevel, List<LayerCullDistance> layerCullDistances, int lightDistance)
        {
            DistanceSettings item = settings.FirstOrDefault(cond => cond.quality == (int)qualityLevel);
            if (item == null)
            {
                settings.Add(new DistanceSettings(qualityLevel, layerCullDistances, lightDistance));
            }
            else
            {
                item.layerCullDistances = layerCullDistances;
                item.lightDistance = lightDistance;
            }
        }
        public int GetLightDistance(QualityLevel qualityLevel)
        {
            DistanceSettings item = settings.FirstOrDefault(cond => cond.quality == (int)qualityLevel);
            if (item != null)
            {
                return item.lightDistance;
            }
            return 0;
        }
    }
}

