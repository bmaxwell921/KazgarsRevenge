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

namespace AnimationPipeline
{
    [ContentProcessor(DisplayName = "Level Model Processor")]
    public class LevelModelProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            //uncomment this to debug the content processor
            System.Diagnostics.Debugger.Launch();

            List<Vector3> lightLocations = GetLightLocations(input);

            ModelContent retModel = base.Process(input, context);

            retModel.Tag = lightLocations;

            return retModel;
        }

        List<Vector3> GetLightLocations(NodeContent input)
        {
            //get the object that has lights as children (should be named "lights")
            NodeContent lightHolder = FindNodeNameBFS(input, "lights");

            List<Vector3> retLocations = new List<Vector3>();
            //iterate through and get positions
            if (lightHolder != null)
            {
                for (int i = lightHolder.Children.Count - 1; i >= 0; --i)
                {
                    MeshContent light = lightHolder.Children[i] as MeshContent;
                    if (light != null)
                    {
                        BoundingSphere sphere = BoundingSphere.CreateFromPoints(light.Positions);
                        retLocations.Add(Vector3.Transform(sphere.Center, light.AbsoluteTransform));

                    }
                    //remove from scene so that it isn't rendered
                    light.Parent.Children.Remove(light);
                }
            }

            return retLocations;
        }

        NodeContent FindNodeNameBFS(NodeContent node, string name)
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
                        return n;
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
