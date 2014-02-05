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
        public static readonly int SERVER_ID = 42;
        public static readonly int NO_CLIENT = -1;
        public int id
        {
            get;
            protected set;
        }

        /*
         * Field used to differentiate ids over the network. Since clients are responsible for
         * creating attacks and stuff it's possible for two client ids to collide.
         */ 
        public int client
        {
            get;
            set;
        }

        public Identification(int id, int client)
        {
            this.id = id;
            this.client = client;
        }

        public override string ToString()
        {
            return String.Format("Id: {0}, Client: {1}", id, client);
        }

        public override bool Equals(object obj)
        {
            return obj != null && (obj as Identification).id == this.id && (obj as Identification).client == this.client;
        }

        public override int GetHashCode()
        {
            int PRIME = 31;
            int hash = 1;

            hash = hash * PRIME + id;
            hash = hash * PRIME + client;
            return hash;
        }
    }
}
