using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class GBufferTarget
    {
        private GraphicsDevice graphicsDevice;
        private Viewport _viewport;
        private int _width;
        private int _height;
        private Vector2 _halfPixel;


        public GBufferTarget(GraphicsDevice graphicsDevice, Viewport viewport)
        {
            this.graphicsDevice = graphicsDevice;
            _viewport = viewport;
            graphicsDevice.DeviceReset += new EventHandler<EventArgs>(_graphicsDevice_DeviceReset);

            GBuffer = new RenderTarget2D[5];
            InitializeRenderTargets();
        }

        void _graphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            DisposeRenderTargets();
            InitializeRenderTargets();
        }

        private void DisposeRenderTargets()
        {
            foreach (var renderTarget in GBuffer)
            {
                if (!renderTarget.IsDisposed)
                    renderTarget.Dispose();
            }
        }

        private void InitializeRenderTargets()
        {
            _width = _viewport.Width;
            _height = _viewport.Height;
            _halfPixel = new Vector2(0.5f / _width, 0.5f / _height);

            Depth = new RenderTarget2D(graphicsDevice, _width, _height, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            Albedo = new RenderTarget2D(graphicsDevice, _width, _height, false, SurfaceFormat.Color, DepthFormat.None);
            Highlights = new RenderTarget2D(graphicsDevice, _width, _height, false, SurfaceFormat.Color, DepthFormat.None);
            Normals = new RenderTarget2D(graphicsDevice, _width, _height, false, SurfaceFormat.HalfVector2, DepthFormat.None);
            Lighting = new RenderTarget2D(graphicsDevice, _width, _height, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
        }

        internal Vector2 HalfPixel
        {
            get { return _halfPixel; }
        }

        internal RenderTarget2D[] GBuffer
        {
            get;
            set;
        }

        internal RenderTarget2D Depth
        {
            get { return GBuffer[0]; }
            private set { GBuffer[0] = value; }
        }

        internal RenderTarget2D Albedo
        {
            get { return GBuffer[1]; }
            private set { GBuffer[1] = value; }
        }

        internal RenderTarget2D Highlights
        {
            get { return GBuffer[2]; }
            private set { GBuffer[2] = value; }
        }

        internal RenderTarget2D Normals
        {
            get { return GBuffer[3]; }
            private set { GBuffer[3] = value; }
        }

        internal RenderTarget2D Lighting
        {
            get { return GBuffer[4]; }
            private set { GBuffer[4] = value; }
        }

        public RenderTarget2D[] GetRenderTargets()
        {
            return GBuffer;
        }
    }
}
