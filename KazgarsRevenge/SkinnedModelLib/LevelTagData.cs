using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SkinnedModelLib
{
    public class LevelTagData
    {
        public List<Vector3> lightLocations = null;
        public List<Vector3[]> physicsVertices = null;
        public List<int[]> physicsIndices = null;
        public List<Matrix> physicsTransforms = null;

        public LevelTagData()
        {
        }
    }
}
