using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// holds the data for an attached model
    /// </summary>
    public class AttachableModel
    {
        public bool Draw { get; set; }
        public Model model { get; private set; }
        public string otherBoneName { get; private set; }
        public float xRotation { get; private set; }
        public float yRotation { get; private set; }
        public AttachableModel(Model model, string otherBoneName)
        {
            this.model = model;
            this.otherBoneName = otherBoneName;
            this.xRotation = 0;
            this.Draw = true;
        }

        public AttachableModel(Model model, string otherBoneName, float xRotation, float yRotation)
        {
            this.model = model;
            this.otherBoneName = otherBoneName;
            this.xRotation = xRotation;
            this.yRotation = yRotation;
            this.Draw = true;
        }
    }
}
