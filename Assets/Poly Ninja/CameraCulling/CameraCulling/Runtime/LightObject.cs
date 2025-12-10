using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Gley.CameraCulling
{
    [System.Serializable]
    public class LightObject
    {
        public bool active;
        public Light light;

        public LightObject(Light light, bool active) 
        { 
            this.light = light;
            this.active = active;
        }
    }
}
