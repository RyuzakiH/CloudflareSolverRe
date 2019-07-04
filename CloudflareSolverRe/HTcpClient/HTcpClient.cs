using HTcp.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HTcp
{
    public class HTcpClient
    {
        public WebHeaderCollection DefaultHeaders { get; set; }

        private readonly Dictionary<string, TcpClient> tcpClients;


        public HTcpClient()
        {
            tcpClients = new Dictionary<string, TcpClient>();
            
            DefaultHeaders = new WebHeaderCollection();
        }
        
        public Response SendRequest(Request request)
        {
            //IPEndPoint endPoint = (IPEndPoint)tcpClients[0].Client.RemoteEndPoint;
            //IPAddress ipAddress = endPoint.Address;
            //// get the hostname
            //IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
            //string hostName = hostEntry.HostName;

            TcpClient tcpClient;

            request.Headers.Add(DefaultHeaders);


            if (tcpClients.ContainsKey(request.Host))
            {
                tcpClient = tcpClients[request.Host];
            }
            else
            {
                tcpClient = new TcpClient();
                tcpClient.ConnectAsync(request.Host, 80).Wait();

                tcpClients.Add(request.Host, tcpClient);
            }

            if (tcpClient.Connected)
            {
                var str = request.ToString();

                //send client input to server
                tcpClient.WriteLine(request.ToString());

                var response = "";
                while ((response = tcpClient.Read()) == "")
                    Thread.Sleep(200);

                //var response = tcpClient.Read(true);

                return Response.Parse(response);
            }

            return null;
        }

        //private StringBuilder BuildRequest(Request request)
        //{

        //}


    }
}
