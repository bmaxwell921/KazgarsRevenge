using CastleCraftGame.Core;
using CastleCraftGame.Rendering.Effects.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CastleCraftGame.Rendering.Components
{
    public abstract class LightComponent : RendererComponent
    {
        private Color _color = Color.White;

        // TODO does having an internal default constructor prevent other projects from deriving the abstract class?
        internal LightComponent(Entity owner)
            : base(owner)
        {
        }

        public Color Color
        {
            get { return LightEffect.Color; }
            set { LightEffect.Color = value; }
        }

        public bool CastsShadows
        {
            get;
            set;
        }

        internal abstract void SynchronizeWithTransform();

        internal RenderTarget2D ShadowTarget
        {
            get;
            set;
        }

        internal LightEffect LightEffect
        {
            get;
            set;
        }
    }
}
