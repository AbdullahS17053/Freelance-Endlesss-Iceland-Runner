namespace Gley.CameraCulling
{
    [System.Serializable]
    public class LayerCullDistance
    {
        public int layer;
        public int distance;
        public string name;

        public LayerCullDistance(int layer, string name, int distance)
        {
            this.layer = layer;
            this.name = name;
            this.distance = distance;
        }
    }
}