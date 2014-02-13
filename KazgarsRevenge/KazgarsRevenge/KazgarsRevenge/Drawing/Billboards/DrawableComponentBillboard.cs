using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public abstract class DrawableComponentBillboard: Component
    {
        public bool Visible { get; protected set; }


        protected BasicEffect effect;

        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
        short[] indices = new short[6];
        protected Vector3 up;
        protected Vector3 normal;
        protected Vector3 left;

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

            vertices = new VertexPositionNormalTexture[4];
            indices = new short[6];

            // normal
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = normal;
            }

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;

            UpdateVerts();
        }

        protected Vector3 origin = Vector3.Zero;
        public override void Update(GameTime gameTime)
        {
            UpdateVerts();
        }

        private void UpdateVerts()
        {
            this.left = Vector3.Cross(normal, up);

            Vector3 uppercenter = origin + (up * size.Y / 2);
            
            Vector3 UpperLeft = uppercenter + (left * size.X / 2);
            Vector3 UpperRight = uppercenter - (left * size.X / 2);
            Vector3 LowerLeft = UpperLeft - (up * size.Y);
            Vector3 LowerRight = UpperRight - (up * size.Y);

            // position
            vertices[0].Position = LowerLeft;
            vertices[1].Position = UpperLeft;
            vertices[2].Position = LowerRight;
            vertices[3].Position = UpperRight;

            // texture coords
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(source.X, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, source.Y);
            Vector2 textureLowerRight = new Vector2(source.X, source.Y);
            vertices[0].TextureCoordinate = textureLowerLeft;
            vertices[1].TextureCoordinate = textureUpperLeft;
            vertices[2].TextureCoordinate = textureLowerRight;
            vertices[3].TextureCoordinate = textureUpperRight;
        }

        public virtual void Draw(Matrix view, Matrix projection, Vector3 cameraPos)
        {
            if (Visible)
            {
                if (effect == null)
                {
                    throw new Exception("the BasicEffect was not set for this billboard component.");
                }
                effect.View = view;
                effect.Projection = projection;
                effect.CurrentTechnique.Passes[0].Apply();

                Game.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices, 0, 4,
                    indices, 0, 2);
            }
        }
    }
}
