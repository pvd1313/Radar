using Beast.Claw.CG;
using Unity.Mathematics;
using UnityEngine;

public class IsometricRender2 : MonoBehaviour
{
    public Shader shader;

    public int2 gridSize;
    public Camera cameraObj;
    public Texture2D texture;
    public Texture2D noiseTexture;
    public float vertexSize;
    public Vector4 layoutMatrix;

    private Material _material;

    public Mesh mesh;
    
    private void Start()
    {
        _material = new Material(shader);

        Application.targetFrameRate = 4096;
    }

    private void Update()
    {
        RenderParams renderParams = new RenderParams();

        float screenAspect = Screen.width / (float)Screen.height;

        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10);
        renderParams.material = _material;
        renderParams.matProps = new MaterialPropertyBlock();

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        Vector4 gSize = new Vector4(gridSize.x, gridSize.y);
        gSize.x = Mathf.Max(1, gridSize.x);
        gSize.y = Mathf.Max(1, gridSize.y);
        
        renderParams.matProps.SetVector("_GridSize", gSize);
        renderParams.matProps.SetVector("_CameraDirection", cameraObj.transform.forward);
        renderParams.matProps.SetVector("_RenderSize", screenSize);
        renderParams.matProps.SetTexture("_TileTexture", texture);
        renderParams.matProps.SetTexture("_Texture", texture);
        renderParams.matProps.SetTexture("_NoiseTexture", noiseTexture);
        renderParams.matProps.SetVector("_NoiseTextureSize", noiseTexture.WidthHeight());
        renderParams.matProps.SetFloat("_VertexSize", vertexSize);
        renderParams.matProps.SetVector("_LayoutMatrix", layoutMatrix);

        Graphics.RenderMeshPrimitives(renderParams, mesh, 0, (int)(gSize.x * gSize.y) + (int)(gSize.x - 1) * (int)(gSize.y - 1));
    }
}
