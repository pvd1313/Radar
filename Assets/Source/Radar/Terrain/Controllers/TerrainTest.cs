using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Gizmos;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.InputHandling;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainTest : Controller, IGizmosDrawer
    {
        private enum BiomeName
        {
            Empty = 0,
            Plains = 1,
            Coast = 2,
            Sand = 6,
            Shallow = 3,
            Deep = 4,
            Grass = 5,
            Forest = 7
        }
        
        public Color GizmosColor { get; } = Color.red;
        
        private readonly IInputSystem _input;
        private readonly ICameraTransform _camera;
        private readonly ITerrainBackedBiome _biome;

        public TerrainTest(IInputSystem input, ICameraTransform camera, ITerrainBackedBiome biome)
        {
            _input = input;
            _camera = camera;
            _biome = biome;
        }

        public void Draw(GizmosHandle handle)
        {
            if (!_biome.IsReady)
            {
                return;
            }
            
            float2 worldPoint = _camera.ScreenToWorld(_input.GameInput.ScreenPoint.Value);
            
            worldPoint = BMath.Floor(worldPoint);

            handle.DrawScreenDot(worldPoint);
            handle.DrawScreenDot(worldPoint + new float2(0, 1));
            handle.DrawScreenDot(worldPoint + new float2(1, 0));
            handle.DrawScreenDot(worldPoint + new float2(1, 1));
            
            BDebug.Track(this, "worldPoint", worldPoint);
            
            byte biomeIndex = _biome.Terrain1kmCell.GetPixelClamped(BMath.FloorToInt(worldPoint));
            
            BDebug.Track(this, "Biome", ((BiomeName) biomeIndex).ToString());
        }
    }
}