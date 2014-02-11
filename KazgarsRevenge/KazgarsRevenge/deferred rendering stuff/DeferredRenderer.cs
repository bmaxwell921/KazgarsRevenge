using System;
using System.Collections.Generic;
using CastleCraftGame.Core.Content;
using CastleCraftGame.Rendering.Common;
using CastleCraftGame.Rendering.Components;
using CastleCraftGame.Rendering.Effects;
using CastleCraftGame.Rendering.Effects.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CastleCraftGame.Rendering.Renderers
{
    public enum RenderingMode
    {
        DeferredAlbedo,
        DeferredNormals,
        DeferredDepth,
        DeferredHighlights,
        DeferredLighting,
        DeferredComposite,
    }

    internal class DeferredRenderer : IRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private CameraComponent _activeCamera;

        private FinalCombineEffect _finalCombineEffect;
        private ClearGBufferEffect _clearGBufferEffect;

        private SpriteBatch _spriteBatch;
        private QuadRenderer _quadRenderer;
        private BlendState _lightBlendState;

        public DeferredRenderer(IServiceProvider serviceProvider)
        {
            ContentManagerExt contentManager = new ContentManagerExt(serviceProvider);
            _graphicsDevice = ((GraphicsDeviceManager)serviceProvider.GetService(typeof(GraphicsDeviceManager))).GraphicsDevice;

            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _quadRenderer = new QuadRenderer(_graphicsDevice);

            _clearGBufferEffect = new ClearGBufferEffect(contentManager);
            _finalCombineEffect = new FinalCombineEffect(contentManager);
            _lightBlendState = new BlendState();
            _lightBlendState.AlphaBlendFunction = BlendFunction.Add;
            _lightBlendState.AlphaSourceBlend = Blend.One;
            _lightBlendState.AlphaDestinationBlend = Blend.One;
            _lightBlendState.ColorBlendFunction = BlendFunction.Add;
            _lightBlendState.ColorSourceBlend = Blend.One;
            _lightBlendState.ColorDestinationBlend = Blend.One;
        }

        public void Begin(CameraComponent camera)
        {
            _activeCamera = camera;

            GBufferTarget_32_ADHeN gbuffer = _activeCamera.RenderTargets;
            RenderTarget2D[] renderTargets = gbuffer.GetRenderTargets();
            _graphicsDevice.SetRenderTargets(renderTargets[0], renderTargets[1], renderTargets[2], renderTargets[3]);

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.Viewport = _activeCamera.Viewport;



            _clearGBufferEffect.ClearColor = camera.ClearColor.ToVector3();
            _clearGBufferEffect.Apply();
            _quadRenderer.Render(-Vector2.One, Vector2.One);

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void Draw(SceneAnalyzer sceneAnalyzer)
        {
            List<MeshRendererComponent> meshes = sceneAnalyzer.MeshRendererComponents[RenderingStage.Deferred];
            foreach (var meshRenderer in meshes)
            {
                MeshRenderer.RenderMesh(_graphicsDevice, meshRenderer, _activeCamera);
            }

            //Resolve GBuffer
            _graphicsDevice.SetRenderTarget(null);

            BeginLightPass();

            //Draw lights
            foreach (LightComponent lightComponent in sceneAnalyzer.LightComponents[RenderingStage.Deferred])
            {
                if (!lightComponent.Enabled)
                    continue;

                lightComponent.SynchronizeWithTransform();
                
                LightEffect lightEffect = lightComponent.LightEffect;
                lightEffect.ColorMap = _activeCamera.RenderTargets.Albedo;
                lightEffect.NormalMap = _activeCamera.RenderTargets.Normals;
                lightEffect.HighlightsMap = _activeCamera.RenderTargets.Highlights;
                lightEffect.DepthMap = _activeCamera.RenderTargets.Depth;
                lightEffect.CameraPosition = _activeCamera.Transform.WorldTransform.Translation;
                lightEffect.HalfPixel = _activeCamera.RenderTargets.HalfPixel;
                lightEffect.InverseViewProjection = _activeCamera.InverseViewProjection;
                
                foreach (EffectPass pass in lightEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                _quadRenderer.Render(-Vector2.One, Vector2.One);
            }
        }

        /// <summary>
        /// Prepares to start the lighting pass.
        /// </summary>
        private void BeginLightPass()
        {
            _graphicsDevice.SetRenderTarget(_activeCamera.RenderTargets.Lighting);
            _graphicsDevice.BlendState = _lightBlendState;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.Clear(Color.Transparent);
        }

        /// <summary>
        /// Finishes rendering the scene.
        /// </summary>
        public void End()
        {
            _graphicsDevice.SetRenderTarget(_activeCamera.OutputTarget);

            switch (_activeCamera.RenderingMode)
            {
                case(RenderingMode.DeferredComposite):
                    EndComposite();
                    return;
                case(RenderingMode.DeferredAlbedo):
                    EndCompositeGBufferComponent(_activeCamera.RenderTargets.Albedo);
                    return;
                case (RenderingMode.DeferredNormals):
                    EndCompositeGBufferComponent(_activeCamera.RenderTargets.Normals);
                    return;
                case (RenderingMode.DeferredHighlights):
                    EndCompositeGBufferComponent(_activeCamera.RenderTargets.Highlights);
                    return;
                case (RenderingMode.DeferredDepth):
                    EndCompositeGBufferComponent(_activeCamera.RenderTargets.Depth);
                    return;
                case (RenderingMode.DeferredLighting):
                    EndCompositeGBufferComponent(_activeCamera.RenderTargets.Lighting);
                    return;
            }
        }

        private void EndComposite()
        {
            _graphicsDevice.BlendState = BlendState.Opaque;

            var gbuffer = _activeCamera.RenderTargets;

            _finalCombineEffect.ColorMap = gbuffer.Albedo;
            _finalCombineEffect.HighlightMap = gbuffer.Highlights;
            _finalCombineEffect.LightMap = gbuffer.Lighting;
            _finalCombineEffect.HalfPixel = gbuffer.HalfPixel;

            _finalCombineEffect.Apply();

            Vector2 bL, tR;
            ConvertViewportToNormalizedVectors(_activeCamera.Viewport, out bL, out tR);

            _quadRenderer.Render(bL, tR);
        }

        public void EndCompositeGBufferComponent(RenderTarget2D gbufferTarget)
        {
            _graphicsDevice.BlendState = BlendState.Opaque;

            var gbuffer = _activeCamera.RenderTargets;
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(gbufferTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        private void ConvertViewportToNormalizedVectors(Viewport viewport, out Vector2 bottomLeft, out Vector2 topRight)
        {
            Vector2 viewportBottomLeft = new Vector2(viewport.X, viewport.Y + viewport.Height);
            Vector2 viewportTopRight = new Vector2(viewport.X + viewport.Width, viewport.Y);

            Vector2 graphicsDeviceDimensions = new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

            // now between 0 and 1
            viewportBottomLeft /= graphicsDeviceDimensions;
            viewportTopRight /= graphicsDeviceDimensions;

            viewportBottomLeft *= 2;
            viewportBottomLeft -= Vector2.One;

            viewportBottomLeft.Y *= -1;

            viewportTopRight *= 2;
            viewportTopRight -= Vector2.One;
            viewportTopRight.Y *= -1;

            bottomLeft = viewportBottomLeft;
            topRight = viewportTopRight;
        }
    }
}
