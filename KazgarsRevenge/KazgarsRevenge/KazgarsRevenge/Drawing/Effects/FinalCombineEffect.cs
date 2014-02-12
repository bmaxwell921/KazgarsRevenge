using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class FinalCombineEffect
    {
        private Effect _effect;

        private EffectParameter _halfPixelParameter;
        private EffectParameter _lightMultiplier;
        private EffectParameter _ambientFactor;
        private EffectParameter _diffuseFactor;

        private EffectParameter _colorMap;
        private EffectParameter _highlightMap;
        private EffectParameter _lightMap;

        #region Properties

        public Vector2 HalfPixel
        {
            get { return _halfPixelParameter.GetValueVector2(); }
            set { _halfPixelParameter.SetValue(value); }
        }

        public float LightMultiplier
        {
            get { return _lightMultiplier.GetValueSingle(); }
            set { _lightMultiplier.SetValue(value); }
        }

        public float AmbientFactor
        {
            get { return _ambientFactor.GetValueSingle(); }
            set { _ambientFactor.SetValue(value); }
        }

        public float DiffuseFactor
        {
            get { return _diffuseFactor.GetValueSingle(); }
            set { _diffuseFactor.SetValue(value); }
        }

        public Texture2D ColorMap
        {
            get { return _colorMap.GetValueTexture2D(); }
            set { _colorMap.SetValue(value); }
        }

        public Texture2D HighlightMap
        {
            get { return _highlightMap.GetValueTexture2D(); }
            set { _highlightMap.SetValue(value); }
        }

        public Texture2D LightMap
        {
            get { return _lightMap.GetValueTexture2D(); }
            set { _lightMap.SetValue(value); }
        }

        #endregion

        public FinalCombineEffect(Effect effect)
        {
            _effect = effect;
            CacheShaderParameters();
        }

        private FinalCombineEffect(FinalCombineEffect other)
        {
            _effect = other._effect.Clone();
            CacheShaderParameters();
        }

        public FinalCombineEffect Clone()
        {
            return new FinalCombineEffect(this);
        }

        public void Apply()
        {
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        private void CacheShaderParameters()
        {
            _halfPixelParameter = _effect.Parameters["halfPixel"];
            _lightMultiplier = _effect.Parameters["lightMultiplier"];
            _ambientFactor = _effect.Parameters["ambientFactor"];
            _diffuseFactor = _effect.Parameters["diffuseFactor"];

            _colorMap = _effect.Parameters["ColorMap"];
            _lightMap = _effect.Parameters["LightMap"];
            _highlightMap = _effect.Parameters["HighlightMap"];
            _lightMultiplier = _effect.Parameters["LightMultiplier"];
        }
    }
}
