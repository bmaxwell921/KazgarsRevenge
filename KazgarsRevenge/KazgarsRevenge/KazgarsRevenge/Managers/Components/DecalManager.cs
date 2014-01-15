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
        Dictionary<Type, List<DrawableComponentDecal>> components = new Dictionary<Type, List<DrawableComponentDecal>>();
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
            shadowEffect.Texture = Game.Content.Load<Texture2D>("Textures\\blob");
            effects.Add(typeof(BlobShadowDecal), shadowEffect);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, List<DrawableComponentDecal>> k in components)
            {
                List<DrawableComponentDecal> componentList = k.Value;
                for (int i = componentList.Count - 1; i >= 0; --i)
                {
                    componentList[i].Update(gameTime);
                    if (componentList[i].Remove)
                    {
                        componentList[i].End();
                        componentList.RemoveAt(i);
                    }
                }
            }
        }

        public void AddBlobShadow(BlobShadowDecal b)
        {
            AddComponent(typeof(BlobShadowDecal), b);
        }

        public void AddComponent(Type t, DrawableComponentDecal c)
        {
            c.Start();
            if (!components.ContainsKey(t))
            {
                components.Add(t, new List<DrawableComponentDecal>());
            }
            components[t].Add(c);
        }

        CameraComponent camera;
        public void Draw()
        {

            Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (KeyValuePair<Type, List<DrawableComponentDecal>> k in components)
            {
                List<DrawableComponentDecal> componentList = k.Value;

                //apply corresponding effect before rendering
                effects[k.Key].View = camera.View;
                effects[k.Key].Projection = camera.Projection;
                effects[k.Key].CurrentTechnique.Passes[0].Apply();

                for (int i = componentList.Count - 1; i >= 0; --i)
                {
                    componentList[i].Draw(camera.View, camera.Projection);
                }
            }



        }
    }
}
