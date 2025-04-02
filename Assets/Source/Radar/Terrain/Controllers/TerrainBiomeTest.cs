using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBiomeTest : Actor
    {
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly IGizmoManager _gizmoManager;
        [Required] private readonly IDeviceManager _deviceManager;

        private IGizmoHandle _gizmo;

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            return UniTask.CompletedTask;
        }

        protected override void _render(BlockingContext context)
        {
            if (!_biome.IsReady)
            {
                return;
            }
            
            _gizmo.Clear();

            PackedAxis activeAxis = _deviceManager.ActiveDevice.GetActiveAxis();

            if (activeAxis.GetAxisType() != AxisType.DRAG)
            {
                return;
            }
            
            activeAxis.Unpack(out DragAxis drag);
            
            float2 worldPoint = _camera.ViewportToWorld(drag.position);
            
            // 1 km
            int2 worldPoint1km = BMath.FloorToInt(worldPoint);
            _gizmo.SetColor(Color.red);
            _gizmo.AddScreenPoint(worldPoint1km);
            _gizmo.AddScreenPoint(worldPoint1km + new float2(0, 1));
            _gizmo.AddScreenPoint(worldPoint1km + new float2(1, 0));
            _gizmo.AddScreenPoint(worldPoint1km + new float2(1, 1));
            
            BDebug.Track(this, "WorldPoint_1KM", worldPoint1km);
            
            ETerrainBiome biomeIndex = _biome.Terrain1kmCell.GetPixelClamped(worldPoint1km).Biome;
            
            BDebug.Track(this, "Biome_1KM", biomeIndex.ToString());
            
            // 8-16 km
            int cellSize = _biome.Terrain8kmCell.cellSize;
            BDebug.Track(this, "CellSize8km", cellSize);
            
            int2 worldPoint8km = BMath.FloorToInt(worldPoint / cellSize) * cellSize;
            _gizmo.SetColor(Color.green);
            _gizmo.AddScreenPoint(worldPoint8km);
            _gizmo.AddScreenPoint(worldPoint8km + new float2(0, cellSize));
            _gizmo.AddScreenPoint(worldPoint8km + new float2(cellSize, 0));
            _gizmo.AddScreenPoint(worldPoint8km + new float2(cellSize, cellSize));
            
            BDebug.Track(this, "WorldPoint_8KM", worldPoint8km);
            
            biomeIndex = _biome.Terrain8kmCell.GetPixelClampedWorld(worldPoint1km).Biome;
            
            BDebug.Track(this, "Biome_8KM", biomeIndex.ToString());
        }

        protected override void _dispose(BlockingContext context)
        {
            _gizmo.Dispose();
        }
    }
}