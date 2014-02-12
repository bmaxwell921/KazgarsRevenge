using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class LightEffect : Effect
    {
        private EffectParameter _color;
        private EffectParameter _camPosition;

        private EffectParameter _colorMap;
        private EffectParameter _normalMap;
        private EffectParameter _depthMap;
        private EffectParameter _highlightMap;

        private EffectParameter _halfPixel;
        private EffectParameter _inverseViewProjection;

        public Color Color
        {
            get { return new Color(_color.GetValueVector3()); }
            set { _color.SetValue(value.ToVector3()); }
        }

        public Vector3 CameraPosition
        {
            get { return _camPosition.GetValueVector3(); }
            set { _camPosition.SetValue(value); }
        }

        public Texture2D ColorMap
        {
            get { return _colorMap.GetValueTexture2D(); }
            set { _colorMap.SetValue(value); }
        }

        public Texture2D NormalMap
        {
            get { return _normalMap.GetValueTexture2D(); }
            set { _normalMap.SetValue(value); }
        }

        public Texture2D DepthMap
        {
            get { return _depthMap.GetValueTexture2D(); }
            set { _depthMap.SetValue(value); }
        }

        public Texture2D HighlightsMap
        {
            get { return _highlightMap.GetValueTexture2D(); }
            set { _highlightMap.SetValue(value); }
        }

        public Vector2 HalfPixel
        {
            get { return _halfPixel.GetValueVector2(); }
            set { _halfPixel.SetValue(value); }
        }

        public Matrix InverseViewProjection
        {
            get { return _inverseViewProjection.GetValueMatrix(); }
            set { _inverseViewProjection.SetValue(value); }
        }

        public LightEffect(Effect effect)
            : base(effect)
        {
            CacheShaderParameters();
        }

        protected virtual void CacheShaderParameters()
        {
            _color = Parameters["Color"];
            _camPosition = Parameters["CameraPosition"];

            _colorMap = Parameters["ColorMap"];
            _normalMap = Parameters["NormalMap"];
            _depthMap = Parameters["DepthMap"];
            _highlightMap = Parameters["HighlightMap"];

            _halfPixel = Parameters["halfPixel"];
            _inverseViewProjection = Parameters["InverseViewProjection"];
        }
    }
}
