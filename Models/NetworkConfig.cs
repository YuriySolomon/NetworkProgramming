using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgramming.Models
{
    internal class NetworkConfig
    {
        public String Ip { get; set; }
        public int Port { get; set; }
        public Encoding Encoding { get; set; }

        private IPEndPoint endPoint;

        public IPEndPoint EndPoint
        {
            get {
                endPoint ??= new IPEndPoint(IPAddress.Parse(Ip), Port);
                return endPoint;
            }
        }
    }
}
