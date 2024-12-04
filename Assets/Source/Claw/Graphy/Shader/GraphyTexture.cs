using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Graphy
{
    public class GraphyTexture : Controller
    {
        private const int ARRAY_MAX_SIZE_FULL = 512;
        private const int ARRAY_MAX_SIZE_LIGHT = 128;

        private int _arrayMaxSize;

        public float[] shaderArrayValues;

        public Material _material;

        public float average = 0;

        public float goodThreshold = 0;
        public float cautionThreshold = 0;

        public Color goodColor = Color.white;
        public Color cautionColor = Color.white;
        public Color criticalColor = Color.white;


        private static readonly int AveragePropertyId = Shader.PropertyToID("Average");

        private static readonly int GoodThresholdPropertyId = Shader.PropertyToID("_GoodThreshold");
        private static readonly int CautionThresholdPropertyId = Shader.PropertyToID("_CautionThreshold");

        private static readonly int GoodColorPropertyId = Shader.PropertyToID("_GoodColor");
        private static readonly int CautionColorPropertyId = Shader.PropertyToID("_CautionColor");
        private static readonly int CriticalColorPropertyId = Shader.PropertyToID("_CriticalColor");

        private static readonly int GraphValues = Shader.PropertyToID("GraphValues");
        private static readonly int GraphValuesLength = Shader.PropertyToID("GraphValues_Length");

        private RenderTexture _texture;
        private EMode _mode;

        public GraphyTexture(GraphyConfig config)
        {
            config.renderTextureData.ValidateThrowException(nameof(config.renderTextureData));
            _texture = BGraphic.CreateTempRenderTexture(this, config.renderTextureData);
            _material = new Material(config.shader);
            _material.SetTexture("_MainTex", _texture);
            _mode = config.graphyMode;
        }

        public void Blit()
        {
            BGraphic.ClearTexture(_texture);

            BGraphic.Blit(new BlitParameters
            {
                material = _material,
                shaderPass = 0,
                targetTexture = _texture
            });
        }

        /// <summary>
        /// This is done to avoid a design problem that arrays in shaders have, 
        /// and should be called before initializing any shader graph.
        /// The first time that you use initialize an array, the size of the array in the shader is fixed.
        /// This is why sometimes you will get a warning saying that the array size will be capped.
        /// It shouldn't generate any issues, but in the worst case scenario just reset the Unity Editor
        /// (if for some reason the shaders reload).
        /// I also cache the Property IDs, that make access faster to modify shader parameters.
        /// </summary>
        public void InitializeShader()
        {
            if (_mode is EMode.FULL)
            {
                _arrayMaxSize = ARRAY_MAX_SIZE_FULL;
                _material.EnableKeyword("FULL");
            }
            else
            {
                _arrayMaxSize = ARRAY_MAX_SIZE_LIGHT;
                _material.DisableKeyword("FULL");
            }
            
            _material.SetFloatArray(GraphValues, new float[_arrayMaxSize]);
        }

        /// <summary>
        /// Updates the GraphValuesLength parameter in the material with the current length of the
        /// ShaderArrayValues float[] array.
        /// </summary>
        public void UpdateArrayValuesLength()
        {
            _material.SetInt(GraphValuesLength, shaderArrayValues.Length);
        }

        /// <summary>
        /// Updates the average parameter in the material.
        /// </summary>
        public void UpdateAverage()
        {
            _material.SetFloat(AveragePropertyId, average);
        }

        /// <summary>
        /// Updates the thresholds in the material.
        /// </summary>
        public void UpdateThresholds()
        {
            _material.SetFloat(GoodThresholdPropertyId, goodThreshold);
            _material.SetFloat(CautionThresholdPropertyId, cautionThreshold);
        }

        /// <summary>
        /// Updates the colors in the material.
        /// </summary>
        public void UpdateColors()
        {
            _material.SetColor(GoodColorPropertyId, goodColor);
            _material.SetColor(CautionColorPropertyId, cautionColor);
            _material.SetColor(CriticalColorPropertyId, criticalColor);
        }

        /// <summary>
        /// Updates the points in the graph with the set array of values.
        /// </summary>
        public void UpdatePoints()
        {
            _material.SetFloatArray(GraphValues, shaderArrayValues);
        }

        public void Dispose()
        {
            BGraphic.TryDispose(ref _texture);
            BGraphic.TryDispose(ref _material);
        }
    }
}