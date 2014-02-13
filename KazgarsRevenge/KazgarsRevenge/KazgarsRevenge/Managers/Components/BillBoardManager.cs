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

        }

        public BasicEffect ShadowEffect { get; private set; }
        public BasicEffect GroundTargetEffect { get; private set; }
        public BasicEffect ChainEffect { get; private set; }
        public BasicEffect HealthBarEffect { get; private set; }
        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            ShadowEffect = new BasicEffect(Game.GraphicsDevice);
            ShadowEffect.EnableDefaultLighting();
            ShadowEffect.World = Matrix.Identity;
            ShadowEffect.TextureEnabled = true;
            ShadowEffect.Texture = Game.Content.Load<Texture2D>("Textures\\blob");

            GroundTargetEffect = new BasicEffect(Game.GraphicsDevice);
            GroundTargetEffect.EnableDefaultLighting();
            GroundTargetEffect.World = Matrix.Identity;
            GroundTargetEffect.TextureEnabled = true;
            GroundTargetEffect.Texture = Game.Content.Load<Texture2D>("Textures\\groundIndicator");

            ChainEffect = new BasicEffect(Game.GraphicsDevice);
            ChainEffect.EnableDefaultLighting();
            ChainEffect.World = Matrix.Identity;
            ChainEffect.TextureEnabled = true;
            ChainEffect.Texture = Game.Content.Load<Texture2D>("Textures\\rope");

            HealthBarEffect = new BasicEffect(Game.GraphicsDevice);
            HealthBarEffect.EnableDefaultLighting();
            HealthBarEffect.World = Matrix.Identity;
            HealthBarEffect.TextureEnabled = true;
            HealthBarEffect.Texture = Game.Content.Load<Texture2D>("white");
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
                    components[i].Draw(camera.View, camera.Projection, camera.Position);
                }
        }
    }
}
