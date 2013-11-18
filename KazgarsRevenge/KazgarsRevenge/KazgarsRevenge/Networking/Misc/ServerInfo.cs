using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace KazgarsRevenge
{
    public class ServerInfo
    {
        public string ServerName
        {
            get;
            protected set;
        }
        public IPEndPoint ServerEndpoint
        {
            get;
            protected set;
        }

        public ServerInfo(string ServerName, IPEndPoint ServerEndpoint)
        {
            this.ServerName = ServerName;
            this.ServerEndpoint = ServerEndpoint;
        }
    }
}
