using Beast.Claw.CG;
using Unity.Mathematics;
using UnityEngine;

public class IsometricRender : MonoBehaviour
{
    public Shader shader;

    public int2 gridSize;
    public Texture2D tileTexture;
    public float2 tileCenter;
    public Texture2D noiseTexture;
    public float vertexSize;
    public Mesh quadMesh;

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

        Vector2 screenSize = new (Screen.width, Screen.height);

        Vector4 gSize = new Vector4(gridSize.x, gridSize.y);
        gSize.x = Mathf.Max(1, gridSize.x);
        gSize.y = Mathf.Max(1, gridSize.y);
        
        renderParams.matProps.SetVector("_GridSize", gSize);
        renderParams.matProps.SetVector("_RenderSize", screenSize);
        renderParams.matProps.SetTexture("_TileTexture", tileTexture);
        renderParams.matProps.SetTexture("_NoiseTexture", noiseTexture);
        renderParams.matProps.SetVector("_NoiseTextureSize", noiseTexture.WidthHeightFloat());
        renderParams.matProps.SetFloat("_VertexSize", vertexSize);
        renderParams.matProps.SetVector("_TileCenter", tileCenter);

        Graphics.RenderMeshPrimitives(renderParams, quadMesh, 0, (int)(gSize.x * gSize.y) + (int)(gSize.x - 1) * (int)(gSize.y - 1));
    }
}
