using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Manager for DrawableComponentBillboard objects
    /// </summary>
    public class BillBoardManager : GameComponent
    {
        List<DrawableComponentBillboard> components = new List<DrawableComponentBillboard>();
        public BillBoardManager(KazgarsRevengeGame game)
            : base(game)
        {
            this.UpdateOrder = 3;
        }

        float currentTime = 0;

        //effects to clone
        private Effect billboardEffect;
        private Effect slidingBillboardEffect;
        private Effect animatedBillboardEffect;

        //effects for specific billboards (might make this dictionary if we make many more)
        public Effect AdrenalineRushEffect { get; private set; }
        public Effect LevelUpBeamEffect { get; private set; }
        public Effect LevelUpCircleEffect { get; private set; }
        public Effect PillarBeamEffect { get; private set; }
        public Effect CircleBlueEffect { get; private set; }
        public Effect CircleEffect { get; private set; }
        public Effect ArrowVEffect { get; private set; }
        public Effect ShadowEffect { get; private set; }
        public Effect GroundTargetEffect { get; private set; }
        public Effect ChainEffect { get; private set; }
        public Effect RopeEffect { get; private set; }
        public Effect HealthBarEffect { get; private set; }
        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            billboardEffect = Game.Content.Load<Effect>("Shaders\\BillboardEffect");
            slidingBillboardEffect = Game.Content.Load<Effect>("Shaders\\SlidingBillboardEffect");
            animatedBillboardEffect = Game.Content.Load<Effect>("Shaders\\AnimatedBillboardEffect");

            AdrenalineRushEffect = animatedBillboardEffect.Clone();
            AdrenalineRushEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.Saiyan));
            AdrenalineRushEffect.Parameters["World"].SetValue(Matrix.Identity);
            AdrenalineRushEffect.Parameters["rows"].SetValue(2);
            AdrenalineRushEffect.Parameters["cols"].SetValue(2);
            AdrenalineRushEffect.Parameters["totalFrames"].SetValue(4);
            AdrenalineRushEffect.Parameters["framesPerSecond"].SetValue(14);

            LevelUpBeamEffect = billboardEffect.Clone();
            LevelUpBeamEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.LevelUpBeam));
            LevelUpBeamEffect.Parameters["World"].SetValue(Matrix.Identity);
            LevelUpBeamEffect.Parameters["colorTint"].SetValue(Color.Gold.ToVector3());
            LevelUpBeamEffect.Parameters["alpha"].SetValue(.75f);

            PillarBeamEffect = slidingBillboardEffect.Clone();
            PillarBeamEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.Beam));
            PillarBeamEffect.Parameters["World"].SetValue(Matrix.Identity);

            LevelUpCircleEffect = billboardEffect.Clone();
            LevelUpCircleEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.Rings));
            LevelUpCircleEffect.Parameters["World"].SetValue(Matrix.Identity);
            LevelUpCircleEffect.Parameters["colorTint"].SetValue(Color.Gold.ToVector3());

            CircleBlueEffect = billboardEffect.Clone();
            CircleBlueEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.CIRCLE));
            CircleBlueEffect.Parameters["World"].SetValue(Matrix.Identity);
            CircleBlueEffect.Parameters["colorTint"].SetValue(Color.Blue.ToVector3());

            CircleEffect = billboardEffect.Clone();
            CircleEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.CIRCLE));
            CircleEffect.Parameters["World"].SetValue(Matrix.Identity);

            ArrowVEffect = billboardEffect.Clone();
            ArrowVEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.ARROWV));
            ArrowVEffect.Parameters["World"].SetValue(Matrix.Identity);

            ShadowEffect = billboardEffect.Clone();
            ShadowEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.BLOB));
            ShadowEffect.Parameters["World"].SetValue(Matrix.Identity);

            GroundTargetEffect = billboardEffect.Clone();
            GroundTargetEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.GRND_IND));
            GroundTargetEffect.Parameters["World"].SetValue(Matrix.Identity);
            
            ChainEffect = billboardEffect.Clone();
            ChainEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.CHAIN));
            ChainEffect.Parameters["World"].SetValue(Matrix.Identity);
            
            RopeEffect = billboardEffect.Clone();
            RopeEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.ROPE));
            RopeEffect.Parameters["World"].SetValue(Matrix.Identity);
            
            HealthBarEffect = billboardEffect.Clone();
            HealthBarEffect.Parameters["Texture"].SetValue(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.HEALTH_BAR));
            HealthBarEffect.Parameters["World"].SetValue(Matrix.Identity);
        }

        public override void Update(GameTime gameTime)
        {
            currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = components.Count - 1; i >= 0; --i)
            {
                components[i].Update(gameTime);
                if (components[i].Remove)
                {
                    components[i].End();
                    components.RemoveAt(i);
                }
            }
        }

        public void AddComponent(DrawableComponentBillboard d)
        {
            d.Start();
            components.Add(d);
        }

        CameraComponent camera;
        public void Draw()
        {
            Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            AdrenalineRushEffect.Parameters["CurrentTime"].SetValue(currentTime);

            for (int i = 0; i < components.Count; ++i)
            {
                components[i].Draw(camera);
            }
        }
    }
}
