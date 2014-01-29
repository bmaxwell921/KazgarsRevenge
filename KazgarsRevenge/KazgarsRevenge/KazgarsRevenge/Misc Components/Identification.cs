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
    /// TODO ISSUE #11
    /// </summary>
    public class Identification
    {
        public int id
        {
            get;
            protected set;
        }

        public Identification(int id)
        {
            this.id = id;
        }

        public override string ToString()
        {
            return id.ToString();
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
