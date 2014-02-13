using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class DirectionalLightEffect : LightEffect
    {
        private EffectParameter _direction;

        public Vector3 Direction
        {
            get { return _direction.GetValueVector3(); }
            set { _direction.SetValue(value); }
        }

        public DirectionalLightEffect(Effect effect)
            : base(effect)
        {
        }

        protected override void CacheShaderParameters()
        {
            _direction = Parameters["LightDirection"];

            base.CacheShaderParameters();
        }
    }
}
