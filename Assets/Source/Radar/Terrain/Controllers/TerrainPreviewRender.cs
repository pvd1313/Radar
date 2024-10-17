using System;
using Beast.Claw;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainPreviewRender : ITerrainPreviewRender, IGameCycleListener
    {
        private readonly ITerrainBaker _terrainBaker;
        private readonly ITerrainBiomeRender _biomeRender;
        private readonly TerrainRenderConfig _config;

        public TerrainPreviewRender(
            ITerrainBaker terrainBaker, 
            ITerrainBiomeRender biomeRender, 
            TerrainRenderConfig config)
        {
            _terrainBaker = terrainBaker;
            _biomeRender = biomeRender;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            
        }
        
        public RenderTexture CreateTexture<TOwner>(TOwner owner, int2 pixelSize)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            if (pixelSize.x <= 0 || pixelSize.y <= 0)
            {
                throw new ArgumentException(nameof(pixelSize));
            }

            RenderTextureData textureData = _config.colorTexture.WithSize(pixelSize);
            
            return BGraphic.CreatePermanentRenderTexture(owner, textureData);
        }
        
        // public void Render(RenderTexture texture, TerrainData terrain)
        // {
        //     try
        //     {
        //         _terrainBaker.Bake(terrain, renderTexture);
        //         
        //         _biomeRender.
        //     }
        //     finally
        //     {
        //         BGraphic.Dispose(renderTexture);
        //     }
        // }
    }
}