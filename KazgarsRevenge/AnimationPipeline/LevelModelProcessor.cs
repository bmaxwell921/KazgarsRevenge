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

            AddPointsTo(GetDataHolder(input, "key"), tag.keyLocations);
            AddPointsTo(GetDataHolder(input, "bossSpawn"), tag.bossSpawnLocations);
            AddPointsTo(GetDataHolder(input, "soul"), tag.soulLocations);
            AddPointsTo(GetDataHolder(input, "ewclosedDoors"), tag.ewdoorLocations);
            AddPointsTo(GetDataHolder(input, "nscloseddDoors"), tag.nsdoorLocations);
            AddPointsTo(GetDataHolder(input, "hanginglights"), tag.hangingLightLocations);

            NodeContent lightHolder = GetDataHolder(input, "lights");
            AddPointsTo(lightHolder, tag.lightLocations);
            GetColorsFromNames(lightHolder, tag.lightColors);

            AddPointsTo(GetDataHolder(input, "mobspawn"), tag.mobSpawnLocations);
            AddPointsTo(GetDataHolder(input, "playerspawn"), tag.playerSpawnLocations);
            AddPointsTo(GetDataHolder(input, "groundobjs"), tag.groundPropLocations);

            ModelContent retModel = base.Process(input, context);
            retModel.Tag = tag;

            return retModel;
        }

        void AddPointsTo(NodeContent dataHolder, List<Vector3> list)
        {
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

        void GetColorsFromNames(NodeContent dataHolder, List<Color> list)
        {
            if (dataHolder != null)
            {
                for (int i = dataHolder.Children.Count - 1; i >= 0; --i)
                {
                    //interpreting name as color
                    string[] name = dataHolder.Children[i].Name.Split(new char[] { '-' });
                    if (name.Length >= 3)
                    {
                        int r;
                        if (!Int32.TryParse(name[0], out r))
                        {
                            list.Add(Color.White);
                            continue;
                        }
                        int g;
                        if (!Int32.TryParse(name[1], out g))
                        {
                            list.Add(Color.White);
                            continue;
                        }
                        int b;
                        if (!Int32.TryParse(name[2], out b))
                        {
                            list.Add(Color.White);
                            continue;
                        }
                        list.Add(new Color(Int32.Parse(name[0]), Int32.Parse(name[1]), Int32.Parse(name[2])));
                    }
                    else
                    {
                        list.Add(Color.White);
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
