using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Serialization;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedFbm : Controller, ITerrainBakedFbm, IGameCycleListener
    {
        public bool IsReady { get; private set; }
        public Texture Texture => _texture;

        private readonly ITerrainFbmRender _fbmRender;
        private readonly GraphicConfig _config;
        private readonly IWorldManager _worldManager;
        private Texture2D _texture;
        private TerrainData _terrainData;
        
        public TerrainBakedFbm(
            ITerrainFbmRender fbmRender, 
            GraphicConfig config,
            IWorldManager worldManager)
        {
            _fbmRender = fbmRender;
            _config = config;
            _worldManager = worldManager;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            WorldData world = _worldManager.GetCurrentWorld();
            TerrainData terrain = world.terrain;
            
            if (_terrainData.IsSame(terrain))
            {
                IsReady = true;
                return UniTask.CompletedTask;
            }

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
                RenderTextureData tempData = _config.terrain.fbmRenderTexture.WithSize(terrain.terrainSize);
            
                Profiler.BeginSample("CreateTempRenderTexture");
                renderTexture = BGraphic.CreateTempRenderTexture(this, tempData);
                Profiler.EndSample();
            
                Profiler.BeginSample("fbmRender.Render");
                _fbmRender.Render(new TerrainFbmRenderTask
                {
                    terrain = terrain,
                    outputTexture = renderTexture,
                    quality01 = 1
                });
                Profiler.EndSample();
            }

            if (_texture == null)
            {
                Texture2DParameters permData = _config.terrain.fbmTexture2D.WithSize(terrain.terrainSize);
            
                Profiler.BeginSample("CreateTexture2D");
                _texture = BGraphic.CreateTexture2D(this, permData);
                Profiler.EndSample();
            }

            if (fbmCacheExists)
            {
                using IBinaryReader reader = _worldManager.OpenCacheForReading(world.guid, FileName.TERRAIN_FBM);
                
                Profiler.BeginSample("ReadTextureCache");
                NativeArray<byte> bytes = reader.ReadNativeBytesAndLength(Allocator.Temp);
                Profiler.EndSample();
                
                Profiler.BeginSample("LoadRawTextureData");
                _texture.SetPixelData(bytes, 0);
                _texture.Apply(_texture.mipmapCount > 0);
                Profiler.EndSample();
                
                bytes.Dispose();
            }
            else
            {
                Profiler.BeginSample("BlitRenderTextureTo2D");
                BGraphic.BlitRenderTextureTo2D(renderTexture, _texture);
                Profiler.EndSample();
                
                BGraphic.Dispose(renderTexture);
            }

            _terrainData = terrain;

            IsReady = true;

            return UniTask.CompletedTask;
        }
        
        private void _gameSave(BlockingContext context)
        {
            WorldData world = _worldManager.GetCurrentWorld();

            if (_worldManager.CacheExists(world.guid, FileName.TERRAIN_FBM))
            {
                return;
            }

            using IBinaryWriter writer = _worldManager.OpenCacheForWriting(world.guid, FileName.TERRAIN_FBM);
            
            writer.WriteNativeBytesAndLength(_texture.GetPixelData<byte>(0));
        }

        private void _dispose(BlockingContext context)
        {
            IsReady = false;

            BGraphic.TryDispose(ref _texture);
        }
    }
}