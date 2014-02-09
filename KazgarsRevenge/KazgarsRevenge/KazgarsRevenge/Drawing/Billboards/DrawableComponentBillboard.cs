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
        Vector3 up;
        Vector3 normal;
        Vector3 left;

        protected Vector2 size;
        public DrawableComponentBillboard(KazgarsRevengeGame game, GameEntity entity, Vector3 normal, Vector3 up, Vector2 size)
            : base(game, entity)
        {
            this.Visible = true;
            this.normal = normal;
            this.up = up;
            this.left = Vector3.Cross(normal, up);

            this.size = size;

            FillVertices();
        }

        protected Vector3 origin = Vector3.Zero;
        public override void Update(GameTime gameTime)
        {
            UpdateVerts();
        }

        private void UpdateVerts()
        {
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
        }

        private void FillVertices()
        {

            vertices = new VertexPositionNormalTexture[4];
            indices = new short[6];

            // normal
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = normal;
            }

            // texture coords
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);
            vertices[0].TextureCoordinate = textureLowerLeft;
            vertices[1].TextureCoordinate = textureUpperLeft;
            vertices[2].TextureCoordinate = textureLowerRight;
            vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;

            UpdateVerts();
        }

        public void Draw(Matrix view, Matrix projection)
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
