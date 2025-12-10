using System.Collections.Generic;
using UnityEngine;

namespace Gley.CameraCulling
{
    [RequireComponent(typeof(Camera))]
    public class LayerCulling : MonoBehaviour
    {
        public CullingMaskSettings cullingMaskSettings;
        public Transform activeCamera;
        public List<LightObject> sceneLights;

        private int lightDistance;

        private void OnEnable()
        {
            Camera camera = GetComponent<Camera>();
            API.Initialize((QualityLevel)QualitySettings.GetQualityLevel(), cullingMaskSettings, camera);
            lightDistance = cullingMaskSettings.GetLightDistance((QualityLevel)QualitySettings.GetQualityLevel());
        }

        private void Update()
        {
            API.UpdateLights(sceneLights, activeCamera, lightDistance);
        }
    }
}