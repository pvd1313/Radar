using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadSplineRenderLOD : Controller
    {
        public RoadRenderLODConfig Config => _config;

        private readonly RoadRenderLODConfig _config;
        
        private CellGrid<int> _roadGrid;
        private Dictionary<int2, Mesh> _gridMeshes;
        private Mesh _globalMesh;

        public RoadSplineRenderLOD(float terrainSize, int lodIndex, RoadRenderLODConfig config, RoadSplineMap roadMap)
        {
            _config = config;
            
            if (_config.isGlobalLOD)
            {
                _globalMesh = RoadMeshBuilder.BuildGlobalRoadMesh(
                    this, 
                    "RoadRender_LOD" + lodIndex, 
                    roadMap, 
                    config.segmentLength, 
                    EPrecisionMode.Float);
                
                return;
            }

            BProfiler.BeginSample("_roadGrid");
            _roadGrid = new CellGrid<int>(_config.cellSize, 256, Allocator.Persistent);
            for (int i = 0; i < roadMap.roads.Length; i++)
            {
                _roadGrid.AddItem(i, roadMap.roads[i].bounds);
            }
            BProfiler.EndSample();

            _gridMeshes = RoadMeshBuilder.BuildRoadGridMeshes(
                this,
                roadMap,
                "RoadRender_LOD" + lodIndex,
                _roadGrid,
                terrainSize,
                config.segmentLength,
                EPrecisionMode.Float);
        }

        public RoadRenderLODResult RenderToCamera(Material material, Bounds2D viewBounds)
        {
            if (_config.isGlobalLOD)
            {
                BGraphic.DrawMeshDelayed(new DrawMeshTask
                {
                    material = material,
                    mesh = _globalMesh,
                    matrix = float4x4.identity
                });

                return new RoadRenderLODResult(1, _globalMesh.vertexCount);
            }

            RoadRenderLODResult renderResult = default;
            
            _roadGrid.GetCellBounds(viewBounds, out int2 lowerBound, out int2 upperBound);
            
            for (int x = lowerBound.x; x <= upperBound.x; x++)
            for (int y = lowerBound.y; y <= upperBound.y; y++)
            {
                if (_gridMeshes.TryGetValue(new int2(x, y), out Mesh mesh))
                {
                    BGraphic.DrawMeshDelayed(new DrawMeshTask
                    {
                        material = material,
                        mesh = mesh,
                        matrix = float4x4.identity
                    });

                    renderResult.renderedMeshCount++;
                    renderResult.renderedVertexCount += mesh.vertexCount;
                }
            }

            return renderResult;
        }

        public void Dispose()
        {
            if (_gridMeshes != null)
            {
                foreach (Mesh mesh in _gridMeshes.Values)
                {
                    BGraphic.Dispose(mesh);
                }
                
                _gridMeshes.Clear();
                _gridMeshes = null;
            }

            BGraphic.TryDispose(ref _globalMesh);
            
            _roadGrid.Dispose();
        }
    }
}