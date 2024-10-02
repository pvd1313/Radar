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
    public int2 tileAtlasSize;
    public float2 tileCenter;
    public float tileAtlasPPU;
    
    [Space]
    public int2 gridSize;

    private float2 screenSize;
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

    public float4 layoutMatrix;

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
        screenSize = new float2(Screen.width, Screen.height);
        
        int2 gSize = gridSize;
        gSize.x = Mathf.Max(1, gridSize.x);
        gSize.y = Mathf.Max(1, gridSize.y);

        int tileCount = GetTileCount(gSize);
        
        RenderParams renderParams = new ()
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one),
            material = _renderMaterial,
            matProps = new MaterialPropertyBlock()
        };
        
        renderParams.matProps.SetVector("_GridSize", gSize);
        
        renderParams.matProps.SetTexture("_TileAtlas", tileAtlas);
        renderParams.matProps.SetVector("_TileAtlasSize", tileAtlasSize);
        renderParams.matProps.SetVector("_TileCenter", tileCenter);
        renderParams.matProps.SetFloat("_TileAtlasPPU", tileAtlasPPU);
        
        renderParams.matProps.SetTexture("_WorldTexture", worldTexture);
        renderParams.matProps.SetVector("_WorldTextureSize", worldTextureSize);
        renderParams.matProps.SetVector("_WorldTextureOffset", worldTextureOffset);
        
        renderParams.matProps.SetVector("_RenderSize", screenSize);
        renderParams.matProps.SetVector("_RenderOffset01", renderOffset01);
        renderParams.matProps.SetFloat("_RenderTileHeight", renderTileHeight);
        
        renderParams.matProps.SetTexture("_NoiseTexture", noiseTexture);
        renderParams.matProps.SetVector("_NoiseTextureSize", noiseTextureSize);
        renderParams.matProps.SetVector("_NoiseTextureOffset", noiseTextureOffset);
        
        renderParams.matProps.SetVector("_LayoutMatrix", layoutMatrix);
        
        BeastLog.Int(this, "tileCount", tileCount);
        Graphics.RenderMeshPrimitives(renderParams, quadMesh, 0, tileCount);
        

        int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
        
        _solverMaterial.SetTexture("_WorldTexture", worldTexture);
        _solverMaterial.SetVector("_WorldTextureSize", worldTextureSize);
        _solverMaterial.SetVector("_WorldTextureOffset", worldTextureOffset);
        
        _solverMaterial.SetVector("_GridSize", gridSize);
        
        _solverMaterial.SetFloat("_OutputTextureSize", solverTextureSize);
        
        RenderTexture solverTexture = BeastCG.NewRenderTextureTemporary(this, new NewTempRenderTextureData
        {
            size = new int2(solverTextureSize, solverTextureSize),
            antiAliasing = 1,
            readWrite = RenderTextureReadWrite.Linear, 
            format = RenderTextureFormat.ARGB32,
            filterMode = FilterMode.Point
        });
        
        BeastCG.Blit(new BlitParameters
        {
            material = _solverMaterial,
            shaderPass = 0,
            targetTexture = solverTexture
        });
        
        _unlitMaterial.mainTexture = solverTexture;
        
        BeastCG.DrawMesh(new DrawMeshParameters
        {
            mesh = quadMesh,
            material = _unlitMaterial,
            matrix = float4x4.identity,
        });
        
        BeastCG.ReleaseTemporaryRenderTexture(solverTexture);
    }

    private int GetTileCount(int2 gSize)
    {
        return gSize.x * gSize.y + (gSize.x - 1) * (gSize.y - 1);
    }
}
