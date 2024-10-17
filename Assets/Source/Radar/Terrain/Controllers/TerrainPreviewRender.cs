using System;
using Beast.Claw.Graphic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainPreviewRender : ITerrainPreviewRender
    {
        // private readonly ITerrainBaker _terrainBaker;
        // private readonly ITerrainBiomeRender _biomeRender;
        //
        // public TerrainPreviewRender(ITerrainBaker terrainBaker, ITerrainBiomeRender biomeRender)
        // {
        //     _terrainBaker = terrainBaker;
        //     _biomeRender = biomeRender;
        // }
        //
        // public RenderTexture CreateTexture<TOwner>([NotNull]TOwner owner, int2 pixelSize)
        // {
        //     if (pixelSize.x <= 0 || pixelSize.y <= 0)
        //     {
        //         throw new ArgumentException(nameof(pixelSize));
        //     }
        //     
        //     return BGraphic.CreatePermanentRenderTexture(owner, new PermRenderTextureData
        //     {
        //         size = pixelSize,
        //         format = RenderTextureFormat.ARGB32
        //     });
        // }
        //
        // public void Render(RenderTexture texture, TerrainData terrain)
        // {
        //     RenderTexture renderTexture = _terrainBaker.Create(this, texture.Size(), true);
        //
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