using CastleCraftGame.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CastleCraftGame.Rendering.Renderers;
using CastleCraftGame.Core;

namespace CastleCraftGame.Rendering.Components
{
    public sealed class CameraComponent : RendererComponent
    {
        private float _fieldOfView = MathHelper.ToRadians(60);
        private float _nearPlane = 1;
        private float _farPlane = 100;
        
        private Matrix _viewMatrix = Matrix.Identity;
        private bool _viewMatrixDirty = true;

        private Matrix _inverseViewMatrix;
        private bool _inverseViewMatrixDirty = true;

        private Matrix _inverseViewProjection;
        private bool _inverseViewProjectionDirty = true;

        private GraphicsDevice _graphics;
        private GBufferTarget_32_ADHeN _gbufferTarget;

        private RenderingMode _renderingMode;

        private Viewport _viewport;
        private float _cameraLayer = 0;

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                _fieldOfView = MathHelper.Clamp(_fieldOfView, MathHelper.ToRadians(1), MathHelper.ToRadians(179));
                buildProjection();
            }
        }

        public Viewport Viewport
        {
            get { return _viewport; }
        }

        public float NearPlane
        {
            get { return _nearPlane; }
            set
            {
                _nearPlane = value;
                buildProjection();
            }
        }

        public float FarPlane
        {
            get { return _farPlane; }
            set
            {
                _farPlane = value;
                buildProjection();
            }
        }

        public RenderingMode RenderingMode
        {
            get { return _renderingMode; }
            set { _renderingMode = value; }
        }

        public LayerMask CullingMask
        {
            get;
            set;
        }

        public float Layer
        {
            get { return _cameraLayer; }
            set { _cameraLayer = value; }
        }

        public CameraComponent(Entity owner, RenderingStage renderingStage, Viewport? viewport, Color clearColor, float nearPlaneDistance, float farPlaneDistance, float layer)
            : base(owner)
        {
            // TODO use -1 to 1 normalized viewport. Map to graphics card viewport.
            // TODO enforce viewport bounds.
            _graphics = RenderingModule.ActiveGraphicsDevice;
            _viewport = viewport ?? _graphics.Viewport;

            _renderingMode = Renderers.RenderingMode.DeferredComposite;
            _gbufferTarget = GBufferTarget_32_ADHeN.CreateGBufferTarget(_graphics, _viewport);
            OutputTarget = new RenderTarget2D(_graphics, _viewport.Width, _viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
            RenderingStage = renderingStage;
            ClearColor = clearColor;
            NearPlane = nearPlaneDistance;
            FarPlane = farPlaneDistance;
            _cameraLayer = layer;
            buildProjection();
        }

        public CameraComponent(Entity owner, RenderingStage renderingStage, Viewport? viewport=null)
            : this(owner,renderingStage,viewport, Color.CornflowerBlue,1f,1000f,0.0f)
        {
        }

        public Color ClearColor
        {
            get;
            set;
        }

        public Matrix View
        {
            get
            {
                if (_viewMatrixDirty)
                {
                    Vector3 position = Transform.WorldTransform.Translation;
                    Vector3 target = position + Transform.WorldTransform.Forward;
                    _viewMatrix = Matrix.CreateLookAt(position, target, Transform.WorldTransform.Up);
                }
                return _viewMatrix;
            }
        }

        public Matrix Projection
        {
            get;
            private set;
        }

        public Matrix InverseView
        {
            get
            {
                if (_inverseViewMatrixDirty)
                {
                    _inverseViewMatrix = Matrix.Invert(View);
                }
                return _inverseViewMatrix;
            }
        }

        public Matrix InverseViewProjection
        {
            get
            {
                if (_inverseViewProjectionDirty)
                {
                    _inverseViewProjection = Matrix.Invert(View * Projection);
                }
                return _inverseViewProjection;
            }
        }

        private void buildProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _viewport.AspectRatio, _nearPlane, _farPlane);
        }

        internal GBufferTarget_32_ADHeN RenderTargets
        {
            get { return _gbufferTarget; }
        }

        public RenderTarget2D OutputTarget
        {
            get;
            private set;
        }
    }
}
