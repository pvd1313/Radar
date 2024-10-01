using Beast.Claw.CG;
using Unity.Mathematics;
using UnityEngine;

public class IsometricRender : MonoBehaviour
{
    public Shader shader;
    public Mesh quadMesh;
    
    [Space]
    public Texture2D tileAtlas;
    public int2 tileAtlasSize;
    public float2 tileCenter;
    public float tileAtlasPPU;
    
    [Space]
    public int2 gridSize;

    private float2 renderSize;
    public float2 renderOffset01;
    public float renderTileHeight;

    [Space]
    public Texture2D worldTexture;
    public float2 worldTextureSize; 
    public float2 worldTextureOffset;
    
    [Space]
    public Texture2D noiseTexture;
    public float2 noiseTextureSize;
    public float2 noiseTextureOffset;

    private Material _material;
    
    private void Start()
    {
        _material = new Material(shader);

        Application.targetFrameRate = 4096;
    }

    private void Update()
    {
        RenderParams renderParams = new ()
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one),
            material = _material,
            matProps = new MaterialPropertyBlock()
        };

        renderSize = new float2(Screen.width, Screen.height);

        int2 gSize = gridSize;
        gSize.x = Mathf.Max(1, gridSize.x);
        gSize.y = Mathf.Max(1, gridSize.y);
        
        renderParams.matProps.SetVector("_GridSize", gSize);
        
        renderParams.matProps.SetTexture("_TileAtlas", tileAtlas);
        renderParams.matProps.SetVector("_TileAtlasSize", tileAtlasSize);
        renderParams.matProps.SetVector("_TileCenter", tileCenter);
        renderParams.matProps.SetFloat("_TileAtlasPPU", tileAtlasPPU);
        
        renderParams.matProps.SetTexture("_WorldTexture", worldTexture);
        renderParams.matProps.SetVector("_WorldTextureSize", worldTextureSize);
        renderParams.matProps.SetVector("_WorldTextureOffset", worldTextureOffset);
        
        renderParams.matProps.SetVector("_RenderSize", renderSize);
        renderParams.matProps.SetVector("_RenderOffset01", renderOffset01);
        renderParams.matProps.SetFloat("_RenderTileHeight", renderTileHeight);
        
        renderParams.matProps.SetTexture("_NoiseTexture", noiseTexture);
        renderParams.matProps.SetVector("_NoiseTextureSize", noiseTextureSize);
        renderParams.matProps.SetVector("_NoiseTextureOffset", noiseTextureOffset);

        Graphics.RenderMeshPrimitives(renderParams, quadMesh, 0, GetTileCount(gSize));
    }

    private int GetTileCount(int2 gSize)
    {
        return gSize.x * gSize.y + (gSize.x - 1) * (gSize.y - 1);
    }
}
