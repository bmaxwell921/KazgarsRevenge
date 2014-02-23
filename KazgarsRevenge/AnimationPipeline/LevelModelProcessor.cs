using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModelLib;

namespace AnimationPipeline
{
    [ContentProcessor(DisplayName = "Level Model Processor")]
    public class LevelModelProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            //uncomment this to debug the content processor
            //System.Diagnostics.Debugger.Launch();

            LevelTagData tag = new LevelTagData();
            
            AddLights(input, tag);
            AddPhysics(input, tag);

            ModelContent retModel = base.Process(input, context);
            retModel.Tag = tag;

            return retModel;
        }

        void AddLights(NodeContent input, LevelTagData tag)
        {
            //get the object that has lights as children (should be named "lights")
            MeshContent lightHolder = FindMeshNameBFS(input, "lights");

            List<Vector3> lightLocations = new List<Vector3>();
            //iterate through and get positions
            if (lightHolder != null)
            {
                BoundingSphere holderSphere = BoundingSphere.CreateFromPoints(lightHolder.Positions);
                lightLocations.Add(Vector3.Transform(holderSphere.Center, lightHolder.AbsoluteTransform));

                for (int i = lightHolder.Children.Count - 1; i >= 0; --i)
                {
                    MeshContent light = lightHolder.Children[i] as MeshContent;
                    if (light != null)
                    {
                        BoundingSphere sphere = BoundingSphere.CreateFromPoints(light.Positions);
                        lightLocations.Add(Vector3.Transform(sphere.Center, light.AbsoluteTransform));

                    }
                    //remove from scene so that it isn't rendered
                    light.Parent.Children.Remove(light);
                }

                lightHolder.Parent.Children.Remove(lightHolder);
            }
            tag.lightLocations = lightLocations;
        }
        
        void AddPhysics(NodeContent input, LevelTagData tag)
        {
            List<Vector3[]> allVerts = new List<Vector3[]>();
            List<int[]> allInds = new List<int[]>();
            List<Matrix> allTransforms = new List<Matrix>();

            MeshContent physics = FindMeshNameBFS(input, "physics");

            if (physics != null)
            {
                allVerts.Add(physics.Geometry[0].Vertices.Positions.ToArray());
                allInds.Add(physics.Geometry[0].Indices.ToArray());
                allTransforms.Add(physics.AbsoluteTransform);

                for (int i = physics.Children.Count - 1; i >= 0; --i)
                {
                    MeshContent box = physics.Children[i] as MeshContent;
                    if (box != null)
                    {
                        //assuming the collidables are all cubes and there is only one entry in Geometry
                        allVerts.Add(box.Geometry[0].Vertices.Positions.ToArray());
                        allInds.Add(box.Geometry[0].Indices.ToArray());
                        allTransforms.Add(box.AbsoluteTransform);
                    }
                    box.Parent.Children.Remove(box);
                }
                physics.Parent.Children.Remove(physics);
            }

            tag.physicsVertices = allVerts;
            tag.physicsIndices = allInds;
            tag.physicsTransforms = allTransforms;
        }

        MeshContent FindMeshNameBFS(NodeContent node, string name)
        {
            List<NodeContent> toProcess = new List<NodeContent>();
            toProcess.Add(node);

            while (toProcess.Count > 0)
            {
                for (int i = toProcess.Count - 1; i >= 0; --i)
                {
                    NodeContent n = toProcess[i];
                    toProcess.RemoveAt(i);

                    if (n.Name == name)
                    {
                        return n as MeshContent;
                    }

                    foreach (NodeContent childnode in n.Children)
                    {
                        toProcess.Add(childnode);
                    }
                }
            }

            return null;
        }
    }
}
