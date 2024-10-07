using System;
using Unity.Mathematics;
using UnityEngine;

public class NoManSky : MonoBehaviour
{
    [SerializeField] private Shader _shader;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Texture _texture;
    
    private Material _material;

    private float _yAxis;
    private float _tiltAxis;
    
    private void Start()
    {
        _material = new Material(_shader)
        {
            mainTexture = _texture
        };

        _meshRenderer.sharedMaterial = _material;
        _tiltAxis = 0.5f;
    }

    private void Update()
    {
        _yAxis += Input.GetAxis("Horizontal") * Time.deltaTime * 0.2f;
        _tiltAxis += Input.GetAxis("Vertical") * Time.deltaTime * 0.2f;

        _tiltAxis %= 1;
        _yAxis %= 1;

        _material.mainTextureScale = new Vector2(0.5f, 0.5f);
        _material.mainTextureOffset = 
            new Vector2(
                0.5f + _yAxis, 
                0.5f + _tiltAxis);
    }
}
