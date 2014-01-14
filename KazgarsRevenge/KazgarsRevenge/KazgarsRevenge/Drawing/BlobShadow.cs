using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{

    class BlobShadow : DrawableComponentDecal
    {
        BasicEffect shadowEffect;

        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
        short[] indices = new short[6];

        float width;
        public BlobShadow(KazgarsRevengeGame game, GameEntity entity, float width, BasicEffect shadowEffect)
            : base(game, entity)
        {
            this.shadowEffect = shadowEffect;
            this.width = width;

            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        Entity physicalData;
        public override void Update(GameTime gameTime)
        {
            UpdateVerts(new Vector3(physicalData.Position.X, 0, physicalData.Position.Z));
        }

        Vector3 normal = Vector3.Up;
        private void UpdateVerts(Vector3 origin)
        {
            // Calculate the quad corners
            Vector3 Left = Vector3.Cross(normal, Vector3.Up);
            Vector3 uppercenter = (Vector3.Up * width / 2) + origin;
            Vector3 UpperLeft = uppercenter + (Left * width / 2);
            Vector3 UpperRight = uppercenter - (Left * width / 2);
            Vector3 LowerLeft = UpperLeft - (Vector3.Up * width);
            Vector3 LowerRight = UpperRight - (Vector3.Up * width);

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

            UpdateVerts(Vector3.Zero);
        }

        public override void Draw(Matrix view, Matrix projection)
        {
            shadowEffect.CurrentTechnique.Passes[0].Apply();

            Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                PrimitiveType.TriangleList,
                vertices, 0, 4,
                indices, 0, 2);

        }
    }
}
