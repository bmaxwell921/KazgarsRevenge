﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class ModelManager : DrawableGameComponent
    {
        List<DrawableComponent3D> components = new List<DrawableComponent3D>();
        CameraComponent camera;

        public ModelManager(MainGame game)
            : base(game)
        {
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
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

        public void AddComponent(DrawableComponent3D toAdd)
        {
            toAdd.Start();
            components.Add(toAdd);
        }

        public void Draw(GameTime gameTime, bool edgeDetection)
        {
            foreach (DrawableComponent3D d in components)
            {
                d.Draw(gameTime, camera.View, camera.Projection, edgeDetection);
            }
        }
    }
}
