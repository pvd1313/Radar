using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Serialization;
using Beast.Radar.Serialization;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedFbm : Actor, ITerrainBakedFbm
    {
        public bool IsReady { get; private set; }
        public Texture Texture => _texture;

        [Required] private readonly ITerrainFbmRender _fbmRender;
        [Required] private readonly TerrainRenderConfig _config;
        [Required] private readonly IWorldManager _worldManager;
        
        private Texture2D _texture;
        private TerrainData _bakedForTerrain;
        
        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            WorldData world = _worldManager.GetCurrentWorld();
            TerrainData terrain = world.terrain;
            
            if (_bakedForTerrain.IsSame(terrain))
            {
                IsReady = true;
                return UniTask.CompletedTask;
            }

            _bakedForTerrain = terrain;

            IsReady = false;

            if (_texture != null &&
                (_texture.width != terrain.terrainSize || 
                _texture.height != terrain.terrainSize))
            {
                BGraphic.Dispose(_texture);
                _texture = null;
            }

            bool fbmCacheExists = _worldManager.CacheExists(world.guid, FileName.TERRAIN_FBM);

            RenderTexture renderTexture = null;

            if (!fbmCacheExists)
            {
                RenderTextureData tempData = _config.fbmRenderTexture.WithSize(terrain.terrainSize);
            
                BProfiler.BeginSample("CreateTempRenderTexture");
                renderTexture = BGraphic.CreateTempRenderTexture(this, tempData);
                BProfiler.EndSample();
            
                BProfiler.BeginSample("fbmRender.Render");
                _fbmRender.Render(new TerrainFbmRenderTask
                {
                    terrain = terrain,
                    outputTexture = renderTexture,
                    quality01 = 1
                });
                BProfiler.EndSample();
            }

            if (_texture == null)
            {
                Texture2DParameters permData = _config.fbmTexture2D.WithSize(terrain.terrainSize);
            
                BProfiler.BeginSample("CreateTexture2D");
                _texture = BGraphic.CreateTexture2D(this, permData);
                BProfiler.EndSample();
            }

            if (fbmCacheExists)
            {
                using IBinaryReader reader = _worldManager.StartCacheReading(world.guid, FileName.TERRAIN_FBM);
                
                BProfiler.BeginSample("ReadTextureCache");
                NativeArray<byte> bytes = reader.ReadNativeBytesAndLength(Allocator.Temp);
                BProfiler.EndSample();
                
                BProfiler.BeginSample("LoadRawTextureData");
                _texture.SetPixelData(bytes, 0);
                _texture.Apply(_texture.mipmapCount > 0);
                BProfiler.EndSample();
                
                bytes.Dispose();
            }
            else
            {
                BProfiler.BeginSample("BlitRenderTextureTo2D");
                BGraphic.BlitRenderTextureTo2D(renderTexture, _texture);
                BProfiler.EndSample();
                
                BGraphic.Dispose(renderTexture);
            }

            IsReady = true;

            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext context)
        {
            WorldData world = _worldManager.GetCurrentWorld();

            if (_worldManager.CacheExists(world.guid, FileName.TERRAIN_FBM))
            {
                return;
            }

            using IBinaryWriter writer = _worldManager.StartCacheWriting(world.guid, FileName.TERRAIN_FBM);
            
            writer.WriteNativeBytesAndLength(_texture.GetPixelData<byte>(0));
        }

        protected override void _dispose(BlockingContext context)
        {
            IsReady = false;

            BGraphic.TryDispose(ref _texture);
        }
    }
}