using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// A texture billboarded into 3D space
    /// </summary>
    public abstract class DrawableComponentBillboard: Component
    {
        public bool Visible { get; protected set; }

        public BlendState blend { get; protected set; }
        protected Effect effect;

        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
        short[] indices = new short[6];
        protected Vector3 up;
        protected Vector3 normal;
        protected Vector3 left;

        protected Vector2 originalSize { get; private set; }
        protected Vector2 size;
        protected Vector2 source = new Vector2(1, 1);
        public DrawableComponentBillboard(KazgarsRevengeGame game, GameEntity entity, Vector3 normal, Vector3 up, Vector2 size)
            : base(game, entity)
        {
            this.Visible = true;
            this.normal = normal;
            this.up = up;
            this.left = Vector3.Cross(normal, up);

            this.size = size;
            this.originalSize = size;

            vertices = new VertexPositionNormalTexture[4];
            indices = new short[6];

            InitVertsIndices(vertices, indices);

            UpdateVerts(vertices, origin);

            this.blend = BlendState.NonPremultiplied;
        }

        protected Vector3 origin = Vector3.Zero;
        public override void Update(GameTime gameTime)
        {
            UpdateVerts(vertices, origin);
        }

        protected void InitVertsIndices(VertexPositionNormalTexture[] verts, short[] ind)
        {
            // normal
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].Normal = normal;
            }

            ind[0] = 0;
            ind[1] = 1;
            ind[2] = 2;
            ind[3] = 2;
            ind[4] = 1;
            ind[5] = 3;
        }

        protected void UpdateVerts(VertexPositionNormalTexture[] verts, Vector3 vertorigin)
        {
            this.left = Vector3.Cross(normal, up);

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].Normal = normal;
            }

            Vector3 uppercenter = vertorigin + (up * size.Y / 2);
            
            Vector3 UpperLeft = uppercenter + (left * size.X / 2);
            Vector3 UpperRight = uppercenter - (left * size.X / 2);
            Vector3 LowerLeft = UpperLeft - (up * size.Y);
            Vector3 LowerRight = UpperRight - (up * size.Y);

            // position
            verts[0].Position = LowerLeft;
            verts[1].Position = UpperLeft;
            verts[2].Position = LowerRight;
            verts[3].Position = UpperRight;

            // texture coords
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(source.X, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, source.Y);
            Vector2 textureLowerRight = new Vector2(source.X, source.Y);
            verts[0].TextureCoordinate = textureLowerLeft;
            verts[1].TextureCoordinate = textureUpperLeft;
            verts[2].TextureCoordinate = textureLowerRight;
            verts[3].TextureCoordinate = textureUpperRight;
        }

        public virtual void Draw(CameraComponent camera)
        {
            if (Visible)
            {
                if (effect == null)
                {
                    throw new Exception("the BasicEffect was not set for this billboard component.");
                }
                Draw(camera.View, camera.Projection, vertices, indices, effect);
            }
        }

        protected void Draw(Matrix view, Matrix projection, VertexPositionNormalTexture[] verts, short[] ind, Effect effect)
        {
            Game.GraphicsDevice.BlendState = blend;

            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.CurrentTechnique.Passes[0].Apply();

            Game.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                verts, 0, 4,
                ind, 0, 2);
        }
    }
}
