using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class AttachableModel
    {
        public bool Draw { get; set; }
        public Model model { get; private set; }
        public string otherBoneName { get; private set; }
        public float xRotation { get; private set; }
        public AttachableModel(Model model, string otherBoneName, float xRotation)
        {
            this.model = model;
            this.otherBoneName = otherBoneName;
            this.xRotation = xRotation;
            this.Draw = true;
        }
    }
}
