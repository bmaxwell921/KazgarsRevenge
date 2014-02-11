using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class PointLightEffect : LightEffect
    {
        private EffectParameter _lightPosition;

        public Vector3 LightPosition
        {
            get { return _lightPosition.GetValueVector3(); }
            set { _lightPosition.SetValue(value); }
        }

        public PointLightEffect(Effect effect)
            : base(effect)
        {
        }

        protected override void CacheShaderParameters()
        {
            _lightPosition = Parameters["LightPosition"];
            base.CacheShaderParameters();
        }
    }
}
