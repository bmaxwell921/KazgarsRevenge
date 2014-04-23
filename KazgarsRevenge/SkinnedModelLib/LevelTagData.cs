using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SkinnedModelLib
{
    public class LevelTagData
    {
        public List<Vector3> emitterLocations = new List<Vector3>();
        public List<Vector3> keyLocations = new List<Vector3>();
        public List<Vector3> bossSpawnLocations = new List<Vector3>();
        public List<Vector3> soulLocations = new List<Vector3>();
        public List<Vector3> ewLockedDoorLocations = new List<Vector3>();
        public List<Vector3> nsLockedDoorLocations = new List<Vector3>();
        public List<Vector3> ewOpenDoorLocations = new List<Vector3>();
        public List<Vector3> nsOpenDoorLocations = new List<Vector3>();
        public List<Vector3> hangingLightLocations = new List<Vector3>();
        public List<Vector3> lightLocations = new List<Vector3>();
        public List<Color> lightColors = new List<Color>();
        public List<Vector3> mobSpawnLocations = new List<Vector3>();
        public List<Vector3> playerSpawnLocations = new List<Vector3>();
        public List<Vector3> groundPropLocations = new List<Vector3>();
        public List<Vector3> shopKeeperLocations = new List<Vector3>();


        public LevelTagData()
        {
        }
    }
}
