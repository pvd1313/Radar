using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.ECS;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Sprite
{
    public class SpriteRender : Actor, ISpriteAtlas
    {
        public NativeSlice<SpriteInfo> Sprites => _atlasManager.Sprites;

        public const int PIXEL_PER_SPRITE = 2;
        public const int HALF_PER_SPRITE = PIXEL_PER_SPRITE * 4;
        
        [Required] private readonly SpriteRenderConfig _config;
        [Required] private readonly IEntityManager _entityManager;
        [Required] private readonly IScreen _screen;
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly GraphicLibrary _library;

        private EntityQuery _entityQuery;
        private HalfStream _halfStream;
        private HalfTextureProvider _halfTextureProvider;
        private Material _renderMaterial;

        private SpriteAtlasManager _atlasManager;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _atlasManager = new SpriteAtlasManager(this, _config.atlasTexture);
            
            _entityQuery = _entityManager.CreateEntityQuery(
                typeof(PositionComponent), 
                typeof(SpriteComponent));
            _halfStream = new HalfStream(PIXEL_PER_SPRITE, Allocator.Persistent, true);
            _halfTextureProvider = new HalfTextureProvider(this, "DataTexture");
            _renderMaterial = BGraphic.CreateMaterial(this, _config.renderShader);
            
            return UniTask.CompletedTask;
        }

        public ushort CreateSprite(SpriteSetup spriteSetup)
        {
            return _atlasManager.Create(spriteSetup);
        }

        public void ReleaseSprite(ushort index)
        {
            _atlasManager.Release(index);
        }

        public void UpdateAtlas()
        {
            _atlasManager.PackGaps();
        }

        protected override void _tick(BlockingContext context)
        {
            _atlasManager.PackGapsIterative();
        }

        protected override void _render(BlockingContext context)
        {
            Profiler.BeginSample("PrepareBuffer");

            NativeArray<ArchetypeChunk> chunks = _entityQuery.ToArchetypeChunkArray(Allocator.TempJob);
            NativeArray<int> writeOffsets = BNative.TempJobArray<int>(chunks.Length);

            int entityCount = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                writeOffsets[i] = entityCount;
                entityCount += chunks[i].Count;
            }

            NativeArray<ushort> pixels = BNative.TempJobArray<ushort>(entityCount * HALF_PER_SPRITE);
            NativeArray<int> entityCounters = BNative.TempJobArray<int>(chunks.Length);

            SpriteRenderPrepareJob job = new()
            {
                inChunks = chunks,
                inWriteOffset = writeOffsets,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteComponent>(true),
                spriteOffsetHandle = _entityManager.GetComponentTypeHandle<SpriteOffsetComponent>(true),
                spritesData = _atlasManager.Sprites,
                outEntityCounters = entityCounters,
                outData = pixels
            };

            Profiler.EndSample();

            Profiler.BeginSample("SpriteRenderPrepareJob");
            job.ExecuteOn(chunks);
            Profiler.EndSample();

            Profiler.BeginSample("UnionHalfStream");
            _halfStream.Clear();

            entityCount = 0;

            for (int i = 0; i < chunks.Length; i++)
            {
                int offset = writeOffsets[i] * HALF_PER_SPRITE;
                int entityInChunk = entityCounters[i];

                NativeSlice<ushort> slice = pixels.Slice(offset, entityInChunk * HALF_PER_SPRITE);

                _halfStream.AddSlice(slice);

                entityCount += entityInChunk;
            }

            pixels.Dispose();
            chunks.Dispose();
            writeOffsets.Dispose();
            entityCounters.Dispose();
            Profiler.EndSample();

            BDebug.Track(this, "EntityCount", entityCount);

            if (entityCount == 0)
            {
                return;
            }

            World2ClipMatrix matrix = new()
            {
                screenSize = _screen.Size,
                cameraPosition = _camera.Position,
                cameraViewDistance = _camera.ViewDistance,
                cameraRotationRad = _camera.RotationRad
            };

            Profiler.BeginSample("ApplyWorld2ClipMatrix");
            matrix.Apply(_renderMaterial);
            Profiler.EndSample();

            int2 textureSize = _halfStream.GetRequiredTextureSize();
            Texture2D dataTexture = _halfTextureProvider.GetTexture(textureSize);

            Profiler.BeginSample("ApplyPixelTexture");
            _halfStream.CopyToTexture(dataTexture);
            Profiler.EndSample();

            _renderMaterial.SetTexture("_DataTexture", dataTexture);
            _renderMaterial.SetFloat2("_DataTextureSize", textureSize);

            _renderMaterial.SetFloat("_AtlasTexelSize", 1f / _atlasManager.AtlasTexture.width);
            _renderMaterial.SetTexture("_AtlasTexture", _atlasManager.AtlasTexture);

            RenderParams renderParams = new()
            {
                material = _renderMaterial,
                worldBounds = new Bounds(Vector3.zero, Vector3.one)
            };

            BDebug.Track(this, "EntityCount", entityCount);
            BDebug.Track(this, "DataTextureSize", textureSize);

            Profiler.BeginSample("RenderMeshPrimitives");
            Graphics.RenderMeshPrimitives(renderParams, _library.quad, 0, entityCount);
            Profiler.EndSample();
        }

        protected override void _dispose(BlockingContext obj)
        {
            BGraphic.TryDispose(ref _renderMaterial);
            
            _entityQuery.Dispose();
            _atlasManager.Dispose();
            _halfStream.Dispose();
            _halfTextureProvider.Dispose();
        }
    }
}