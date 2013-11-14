using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Represents the unique identifier for an entity.
    /// Equals and HashCode implemented so they can be used in Dictionaries
    /// 
    /// TODO needs to be added to Entities
    /// </summary>
    public class Identification
    {
        public byte id
        {
            get;
            protected set;
        }

        public Identification(byte id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            return obj != null && (obj as Identification).id == this.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
