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

    public class BlobShadowDecal : DrawableComponentDecal
    {
        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
        short[] indices = new short[6];

        float width;
        public BlobShadowDecal(KazgarsRevengeGame game, GameEntity entity, float width)
            : base(game, entity)
        {
            this.width = width;

            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;

            FillVertices();
        }

        Entity physicalData;
        public override void Update(GameTime gameTime)
        {
            UpdateVerts(new Vector3(physicalData.Position.X, -18.4f, physicalData.Position.Z));
        }

        private void UpdateVerts(Vector3 origin)
        {
            Vector3 uppercenter =  origin + (Vector3.Forward * width / 2);

            // Calculate the quad corners (flat, horizontal quad)
            Vector3 UpperLeft = uppercenter + (Vector3.Left * width / 2);
            Vector3 UpperRight = uppercenter - (Vector3.Left * width / 2);
            Vector3 LowerLeft = UpperLeft - (Vector3.Forward * width);
            Vector3 LowerRight = UpperRight - (Vector3.Forward * width);

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
                vertices[i].Normal = Vector3.Up;
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
            Game.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                vertices, 0, 4,
                indices, 0, 2);
        }
    }
}
