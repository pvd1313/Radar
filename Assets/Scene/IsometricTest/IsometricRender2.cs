using Beast.Claw.CG;
using UnityEngine;

public class IsometricRender2 : MonoBehaviour
{
    public Shader shader;

    public int gridSize;
    public Camera cameraObj;
    public Texture2D texture;
    public Texture2D noiseTexture;
    public float vertexSize;

    private Material _material;

    public Mesh mesh;
    
    private void Start()
    {
        _material = new Material(shader);

        Application.targetFrameRate = 666;
    }

    private void Update()
    {
        RenderParams renderParams = new RenderParams();

        float screenAspect = Screen.width / (float)Screen.height;

        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10);
        renderParams.material = _material;
        renderParams.matProps = new MaterialPropertyBlock();

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        renderParams.matProps.SetFloat("_GridSize", gridSize);
        renderParams.matProps.SetVector("_CameraDirection", cameraObj.transform.forward);
        renderParams.matProps.SetVector("_RenderSize", screenSize);
        renderParams.matProps.SetTexture("_TileTexture", texture);
        renderParams.matProps.SetTexture("_NoiseTexture", noiseTexture);
        renderParams.matProps.SetVector("_NoiseTextureSize", noiseTexture.WidthHeight());
        renderParams.matProps.SetFloat("_VertexSize", vertexSize);
        
        Graphics.RenderMeshPrimitives(renderParams, mesh, 0, gridSize * gridSize);
    }
}
