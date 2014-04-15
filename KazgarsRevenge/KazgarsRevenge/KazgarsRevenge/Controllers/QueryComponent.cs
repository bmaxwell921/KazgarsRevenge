using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.ResourceManagement;


namespace KazgarsRevenge
{
    /// <summary>
    /// component with ability to query for nearby entities
    /// </summary>
    public class QueryComponent : Component
    {
        public QueryComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {

        }

        /// <summary>
        /// grabs the entity of an entityName within radius of position (not necessarily the nearest, just the first one it sees)
        /// </summary>
        protected GameEntity QueryNearEntityName(string entityName, Vector3 position, float outsideOfRadius, float insideOfRadius)
        {
            Vector3 min = new Vector3(position.X - insideOfRadius, 0, position.Z - insideOfRadius);
            Vector3 max = new Vector3(position.X + insideOfRadius, 20, position.Z + insideOfRadius);
            BoundingBox b = new BoundingBox(min, max);

            var entries = Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(b, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == entityName)
                {
                    if (outsideOfRadius != 0)
                    {
                        BoundingBox entryBox = entry.BoundingBox;
                        Vector3 entrymid = (entryBox.Max + entryBox.Min) / 2;
                        Vector3 selfmid = (b.Max + b.Min) / 2;
                        if (Math.Abs(entrymid.X - selfmid.X) > outsideOfRadius && Math.Abs(entrymid.Z - selfmid.Z) > outsideOfRadius)
                        {
                            return other;
                        }
                    }
                    else
                    {
                        return other;
                    }
                }
            }
            return null;
        }

        protected GameEntity QueryNearEntityFaction(FactionType entityFaction, Vector3 position, float outsideOfRadius, float insideOfRadius, bool findNearest)
        {
            Vector3 min = new Vector3(position.X - insideOfRadius, 0, position.Z - insideOfRadius);
            Vector3 max = new Vector3(position.X + insideOfRadius, 20, position.Z + insideOfRadius);
            BoundingBox b = new BoundingBox(min, max);

            var entries = Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(b, entries);

            float[] entityDistances = new float[entries.Count];
            for (int i = 0; i < entityDistances.Length; ++i)
            {
                entityDistances[i] = float.MaxValue;
            }

            for (int i = 0; i < entries.Count; ++i)
            {
                GameEntity other = entries[i].Tag as GameEntity;
                if (other != null && other.Faction == entityFaction)
                {
                    if (outsideOfRadius != 0 || !findNearest)
                    {
                        BoundingBox entryBox = entries[i].BoundingBox;
                        Vector3 entrymid = (entryBox.Max + entryBox.Min) / 2;
                        Vector3 selfmid = (b.Max + b.Min) / 2;
                        float x = Math.Abs(entrymid.X - selfmid.X);
                        float y = Math.Abs(entrymid.Z - selfmid.Z);

                        if (x > outsideOfRadius || y > outsideOfRadius)
                        {
                            if (findNearest)
                            {
                                entityDistances[i] = x + y;
                            }
                            else
                            {
                                return other;
                            }
                        }
                    }
                    else if (!findNearest)
                    {
                        return other;
                    }
                }
            }
            if (!findNearest)
            {
                return null;
            }

            int foundIndex = -1;
            float smallest = float.MaxValue;
            for (int i = 0; i < entityDistances.Length; ++i)
            {
                if (entityDistances[i] < smallest)
                {
                    foundIndex = i;
                    smallest = entityDistances[i];
                }
            }

            if (foundIndex == -1)
            {
                return null;
            }
            else
            {
                return entries[foundIndex].Tag as GameEntity;
            }
        }

    }
}
