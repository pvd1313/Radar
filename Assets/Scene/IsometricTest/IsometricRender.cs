using UnityEngine;

public class IsometricRender : MonoBehaviour
{
    public Shader shader;

    public int gridSize;
    public Camera cameraObj;

    private Material _material;

    public Mesh mesh;
    
    private void Start()
    {
        _material = new Material(shader);

        Application.targetFrameRate = 666;
        
        Debug.Log(Shader.GetGlobalMatrix("UNITY_MATRIX_V"));
        Debug.Log(Shader.GetGlobalMatrix("unity_ObjectToWorld"));
    }

    private void Update()
    {
        RenderParams renderParams = new RenderParams();

        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 100000);
        renderParams.material = _material;
        renderParams.matProps = new MaterialPropertyBlock();

        
        
        

        renderParams.matProps.SetFloat("_GridSize", gridSize);
        renderParams.matProps.SetVector("_CameraDirection", cameraObj.transform.forward);
        
        Graphics.RenderMeshPrimitives(renderParams, mesh, 0, gridSize * gridSize);
    }
}
