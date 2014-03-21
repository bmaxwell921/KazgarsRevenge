using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SkinnedModelLib
{
    public class LevelTagData
    {
        public List<Vector3> soulLocations = new List<Vector3>();
        public List<Vector3> ewdoorLocations = new List<Vector3>();
        public List<Vector3> nsdoorLocations = new List<Vector3>();
        public List<Vector3> hangingLightLocations = new List<Vector3>();
        public List<Vector3> lightLocations = new List<Vector3>();
        public List<Vector3> mobSpawnLocations = new List<Vector3>();
        public List<Vector3> playerSpawnLocations = new List<Vector3>();
        public List<Vector3> smallObjLocations = new List<Vector3>();
        public List<Vector3> mediumObjLocations = new List<Vector3>();
        public List<Vector3> largeObjLocations = new List<Vector3>();

        public LevelTagData()
        {
        }
    }
}
