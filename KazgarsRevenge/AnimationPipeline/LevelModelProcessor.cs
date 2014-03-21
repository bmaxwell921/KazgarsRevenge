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
            
            AddPointsTo(input, tag, "soul", tag.soulLocations);
            AddPointsTo(input, tag, "ewdoors", tag.ewdoorLocations);
            AddPointsTo(input, tag, "nsdoors", tag.nsdoorLocations);
            AddPointsTo(input, tag, "hanginglights", tag.hangingLightLocations);
            AddPointsTo(input, tag, "lights", tag.lightLocations);
            AddPointsTo(input, tag, "mobspawn", tag.mobSpawnLocations);
            AddPointsTo(input, tag, "playerspawn", tag.playerSpawnLocations);
            AddPointsTo(input, tag, "smallobjs", tag.smallObjLocations);
            AddPointsTo(input, tag, "mediumobjs", tag.mediumObjLocations);
            AddPointsTo(input, tag, "largeobjs", tag.largeObjLocations);

            ModelContent retModel = base.Process(input, context);
            retModel.Tag = tag;

            return retModel;
        }

        void AddPointsTo(NodeContent input, LevelTagData tag, string nodeName, List<Vector3> list)
        {
            //get the object that has the positions we want as children
            NodeContent dataHolder = GetDataHolder(input, nodeName);

            //iterate through and get positions
            if (dataHolder != null)
            {
                for (int i = dataHolder.Children.Count - 1; i >= 0; --i)
                {
                    MeshContent spherePoint = dataHolder.Children[i] as MeshContent;
                    if (spherePoint != null)
                    {
                        BoundingSphere sphere = BoundingSphere.CreateFromPoints(spherePoint.Positions);
                        list.Add(Vector3.Transform(sphere.Center, spherePoint.AbsoluteTransform));

                    }
                }
            }
        }

        NodeContent GetDataHolder(NodeContent input, string name)
        {
            NodeContentCollection children = input.Children;
            for (int i = children.Count - 1; i >= 0; --i)
            {
                NodeContent n = children[i];

                if (n.Name.ToLower() == name.ToLower())
                {
                    n.Parent.Children.Remove(n);
                    return n;
                }
            }
            return null;
        }

        /*
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

                    if (n.Name.ToLower() == name.ToLower())
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
        }*/
    }
}
