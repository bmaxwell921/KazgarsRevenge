using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class DecalManager : GameComponent
    {
        List<DrawableComponentDecal> components = new List<DrawableComponentDecal>();
        public DecalManager(KazgarsRevengeGame game)
            : base(game)
        {

        }

        Dictionary<Type, BasicEffect> effects = new Dictionary<Type, BasicEffect>();
        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            BasicEffect shadowEffect = new BasicEffect(Game.GraphicsDevice);
            shadowEffect.EnableDefaultLighting();
            shadowEffect.World = Matrix.Identity;
            shadowEffect.TextureEnabled = true;
            shadowEffect.Texture = Game.Content.Load<Texture2D>("Textures\\Particles\\blob");
        }

        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, BasicEffect> k in effects)
            {
                k.Value.View = camera.View;
                k.Value.Projection = camera.Projection;
            }
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

        public void AddComponent(DrawableComponentDecal c)
        {
            c.Start();
            components.Add(c);
        }

        CameraComponent camera;
        public void Draw()
        {
            
            for (int i = components.Count - 1; i >= 0; --i)
            {
                components[i].Draw(camera.View, camera.Projection);
            }
        }
    }
}
