using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class BillBoardManager : GameComponent
    {
        List<DrawableComponentBillboard> components = new List<DrawableComponentBillboard>();
        public BillBoardManager(KazgarsRevengeGame game)
            : base(game)
        {

            this.UpdateOrder = 3;
        }

        public BasicEffect ArrowVEffect { get; private set; }
        public BasicEffect ShadowEffect { get; private set; }
        public BasicEffect GroundTargetEffect { get; private set; }
        public BasicEffect ChainEffect { get; private set; }
        public BasicEffect RopeEffect { get; private set; }
        public BasicEffect HealthBarEffect { get; private set; }
        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            ShadowEffect = new BasicEffect(Game.GraphicsDevice);
            ShadowEffect.EnableDefaultLighting();
            ShadowEffect.World = Matrix.Identity;
            ShadowEffect.TextureEnabled = true;
            ShadowEffect.Texture = Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.BLOB);

            GroundTargetEffect = new BasicEffect(Game.GraphicsDevice);
            GroundTargetEffect.EnableDefaultLighting();
            GroundTargetEffect.World = Matrix.Identity;
            GroundTargetEffect.TextureEnabled = true;
            GroundTargetEffect.Texture = Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.GRND_IND);

            ChainEffect = new BasicEffect(Game.GraphicsDevice);
            
            ChainEffect.EnableDefaultLighting();
            ChainEffect.World = Matrix.Identity;
            ChainEffect.TextureEnabled = true;
            ChainEffect.Texture = Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.CHAIN);

            RopeEffect = new BasicEffect(Game.GraphicsDevice);
            RopeEffect.EnableDefaultLighting();
            RopeEffect.World = Matrix.Identity;
            RopeEffect.TextureEnabled = true;
            RopeEffect.Texture = Texture2DUtil.Instance.GetTexture(TextureStrings.BillBoards.ROPE);

            HealthBarEffect = new BasicEffect(Game.GraphicsDevice);
            HealthBarEffect.EnableDefaultLighting();
            HealthBarEffect.World = Matrix.Identity;
            HealthBarEffect.TextureEnabled = true;
            HealthBarEffect.Texture = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.HEALTH_BAR);
        }

        public override void Update(GameTime gameTime)
        {
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


            for (int i = 0; i < components.Count; ++i)
            {
                components[i].Draw(camera);
            }
        }
    }
}
