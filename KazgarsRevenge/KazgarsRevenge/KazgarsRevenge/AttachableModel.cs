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
        Model model;
        string otherBoneName;
        public AttachableModel(Model model, string otherBoneName)
        {
            this.model = model;
            this.otherBoneName = otherBoneName;
        }
    }
}
