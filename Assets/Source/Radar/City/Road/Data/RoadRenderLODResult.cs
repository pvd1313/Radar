namespace Beast.Radar.City
{
    public struct RoadRenderLODResult
    {
        public int renderedMeshCount;
        public int renderedVertexCount;

        public RoadRenderLODResult(int meshCount, int vertexCount)
        {
            renderedMeshCount = meshCount;
            renderedVertexCount = vertexCount;
        }
    }
}