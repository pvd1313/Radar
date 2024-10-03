using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

public class IsometricRender : MonoBehaviour
{
    public Shader solverShader;
    public Shader renderShader;
    public Shader unlitShader;
    public Mesh quadMesh;
    
    [Space]
    public Texture2D tileAtlas;
    public int2 tileAtlasWidthHeight;
    public int2 tileAtlasTileCount;
    public float2 tileCenter;
    public float tileAtlasPPU;
    
    [Space]
    public int2 tileGridSize;

    public float2 renderOffset;
    public float renderTileHeight;

    [Space]
    public Texture2D terrainTexture;
    public float2 terrainSize; 
    public float2 terrainOffset;
    
    [Space]
    public Texture2D noiseTexture;
    public float2 noiseTextureSize;
    public float2 noiseTextureOffset;

    private Material _renderMaterial;
    private Material _solverMaterial;
    private Material _unlitMaterial;
    
    private void Start()
    {
        _renderMaterial = new Material(renderShader);
        _solverMaterial = new Material(solverShader);
        _unlitMaterial = new Material(unlitShader);

        Application.targetFrameRate = 4096;
    }

    private void Update()
    {
        float2 screenSize = new float2(Screen.width, Screen.height);
        
        int2 gSize = tileGridSize;
        gSize.x = Mathf.Max(1, tileGridSize.x);
        gSize.y = Mathf.Max(1, tileGridSize.y);

        int tileCount = GetTileCount(gSize);
        BeastLog.Int(this, "tileCount", tileCount);
        
        // -- Solver \/
        
        int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
        _solverMaterial.SetVector("_TileGridSize", tileGridSize);
        _solverMaterial.SetTexture("_TileAtlas", tileAtlas);
        _solverMaterial.SetVector("_TileAtlasWidthHeight", tileAtlasWidthHeight);
        _solverMaterial.SetTexture("_TerrainTexture", terrainTexture);
        _solverMaterial.SetVector("_TerrainSize", terrainSize);
        _solverMaterial.SetVector("_TerrainOffset", terrainOffset);
        _solverMaterial.SetFloat("_OutputTextureSize", solverTextureSize);
        
        RenderTexture solverTexture = BeastCG.NewRenderTextureTemporary(this, new NewTempRenderTextureData
        {
            size = new int2(solverTextureSize, solverTextureSize),
            antiAliasing = 1,
            readWrite = RenderTextureReadWrite.Linear, 
            format = RenderTextureFormat.RGFloat,
            filterMode = FilterMode.Point
        });
        
        BeastCG.Blit(new BlitParameters
        {
            material = _solverMaterial,
            shaderPass = 0,
            targetTexture = solverTexture
        });
        
        // -- Render \/
        
        RenderParams renderParams = new ()
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one),
            material = _renderMaterial,
            matProps = new MaterialPropertyBlock()
        };
        
        renderParams.matProps.SetVector("_TileGridSize", gSize);
        renderParams.matProps.SetTexture("_TileAtlas", tileAtlas);
        renderParams.matProps.SetVector("_TileAtlasWidthHeight", tileAtlasWidthHeight);
        renderParams.matProps.SetVector("_TileAtlasTileCount", tileAtlasTileCount);
        renderParams.matProps.SetVector("_TileCenter", tileCenter);
        renderParams.matProps.SetFloat("_TileAtlasPPU", tileAtlasPPU);
        renderParams.matProps.SetTexture("_SolverTexture", solverTexture);
        renderParams.matProps.SetFloat("_SolverSize", solverTextureSize);
        renderParams.matProps.SetVector("_OutputSize", screenSize);
        renderParams.matProps.SetVector("_OutputOffset", renderOffset);
        renderParams.matProps.SetFloat("_OutputTileHeight", renderTileHeight);
        
        Graphics.RenderMeshPrimitives(renderParams, quadMesh, 0, tileCount);
        
        // _unlitMaterial.mainTexture = solverTexture;
        //
        // BeastCG.DrawMesh(new DrawMeshParameters
        // {
        //     mesh = quadMesh,
        //     material = _unlitMaterial,
        //     matrix = float4x4.identity,
        // });
        
        BeastCG.ReleaseTemporaryRenderTexture(solverTexture);
    }

    private int GetTileCount(int2 gSize)
    {
        return gSize.x * gSize.y + (gSize.x - 1) * (gSize.y - 1);
    }
}
