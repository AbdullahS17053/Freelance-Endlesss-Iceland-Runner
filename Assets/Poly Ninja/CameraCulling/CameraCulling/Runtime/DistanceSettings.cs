using System.Collections.Generic;

namespace Gley.CameraCulling
{
    [System.Serializable]
    public class DistanceSettings
    {
        public int quality;
        public List<LayerCullDistance> layerCullDistances = new List<LayerCullDistance>();
        public int lightDistance;

        public DistanceSettings(int quality, List<LayerCullDistance> layerCullDistances, int lightDistance)
        {
            this.quality = quality;
            this.layerCullDistances = layerCullDistances;
            this.lightDistance = lightDistance;
        }
    }
}