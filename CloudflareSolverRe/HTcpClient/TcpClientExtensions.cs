using System.Net.Sockets;
using System.Text;

namespace HTcp.Extensions
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    public static class TcpClientExtensions
    {
        private static readonly int TimeOutMs = 100;

        public static string Read(this TcpClient tcpClient, bool wait = false)
        {
            if (!tcpClient.Connected) return null;

            if (wait)
            {
                var response = string.Empty;

                using (var stream = tcpClient.GetStream())
                {
                    int byteRead = 0;
                    byte[] buffer = new byte[1000];

                    do
                    {
                        byteRead = stream.Read(buffer, 0, 1000);
                        response += Encoding.ASCII.GetString(buffer, 0, byteRead);
                    }
                    while (byteRead > 0);
                }

                return response;
            }

            return tcpClient.Read();
        }

        public static string Read(this TcpClient tcpClient)
        {
            if (!tcpClient.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                tcpClient.ParseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (tcpClient.Available > 0);
            return sb.ToString();
        }
        
        public static void Write(this TcpClient tcpClient, string cmd)
        {
            if (!tcpClient.Connected) return;
            byte[] buf = Encoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
            tcpClient.GetStream().Write(buf, 0, buf.Length);
        }

        public static void WriteLine(this TcpClient tcpClient, string cmd)
        {
            tcpClient.Write(cmd + "\n");
        }


        private static void ParseTelnet(this TcpClient tcpClient, StringBuilder sb)
        {
            while (tcpClient.Available > 0)
            {
                int input = tcpClient.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpClient.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpClient.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpClient.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpClient.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpClient.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpClient.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }

    }    
}











//namespace Htcp
//{
//    using System;
//    using System.Linq;
//    using System.Net;
//    //using System.Net.Sockets;
//    using System.Threading.Tasks;

//    //using System.Net.Sockets;

//    /// <summary>
//    /// A TcpClient to connect to the specified socket.
//    /// </summary>
//    public class TcpClient : System.Net.Sockets.TcpClient, IDisposable
//    {
//        ///// <summary>
//        ///// Initializes a new instance of the <see cref="TcpClient"/> class.
//        ///// </summary>
//        ///// <param name="hostName">The host name.</param>
//        ///// <param name="port">The port.</param>
//        //public TcpClient() : base()
//        //{
//        //}

//        public async Task ConnectAsync(string host, int port)
//        {
//#if NETSTANDARD2_0
//            //ConnectAsync()
//#endif
//            await ConnectAsync(host, port);
//        }

//        public async Task ConnectAsync(IPAddress[] addresses, int port)
//        {
//            await client.ConnectAsync(addresses, port);
//        }

//        public async Task ConnectAsync(IPAddress address, int port)
//        {
//            await client.ConnectAsync(address, port);
//        }


//        /// <summary>
//        /// Gets or sets the receive timeout.
//        /// </summary>
//        /// <value>
//        /// The receive timeout.
//        /// </value>
//        public int ReceiveTimeout
//        {
//            get => client.ReceiveTimeout;
//            set => client.ReceiveTimeout = value;
//        }

//        /// <summary>
//        /// Gets a value indicating whether this <see cref="ISocket" /> is connected.
//        /// </summary>
//        /// <value>
//        ///   <c>true</c> if connected; otherwise, <c>false</c>.
//        /// </value>
//        public bool Connected => client.Connected;

//        /// <summary>
//        /// Gets the available bytes to be read.
//        /// </summary>
//        /// <value>
//        /// The available bytes to be read.
//        /// </value>
//        public int Available => client.Available;

//        public Socket Client
//        {
//            get => client.Client;
//            set => client.Client = value;
//        }

//        public int ReceiveBufferSize
//        {
//            get => client.ReceiveBufferSize;
//            set => client.ReceiveBufferSize = value;
//        }

//        public bool NoDelay
//        {
//            get => client.NoDelay;
//            set => client.NoDelay = value;
//        }

//        public LingerOption LingerState
//        {
//            get => client.LingerState;
//            set => client.LingerState = value;
//        }

//        public bool ExclusiveAddressUse
//        {
//            get => client.ExclusiveAddressUse;
//            set => client.ExclusiveAddressUse = value;
//        }

//        public int SendTimeout
//        {
//            get => client.SendTimeout;
//            set => client.SendTimeout = value;
//        }

//        public int SendBufferSize
//        {
//            get => client.SendBufferSize;
//            set => client.SendBufferSize = value;
//        }

//        /// <summary>
//        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        /// <summary>
//        /// Closes this instance.
//        /// </summary>
//        public void Close()
//        {
//#if NETSTANDARD1_3
//            client.Dispose();
//#else
//            this.client.Close();
//#endif
//        }

//        /// <summary>
//        /// Gets the stream.
//        /// </summary>
//        /// <returns>
//        /// Network stream socket connected to.
//        /// </returns>
//        public NetworkStream GetStream() => client.GetStream();

//        private void Dispose(bool isDisposing)
//        {
//            if (isDisposing)
//            {
//#if NETSTANDARD1_3
//                client.Dispose();
//#else
//                client.Close();
//#endif
//            }
//        }
//    }
//}






//namespace Htcp
//{
//    using System;
//    using System.Linq;
//    using System.Net;
//    using System.Net.Sockets;
//    using System.Threading.Tasks;

//    //using System.Net.Sockets;

//    /// <summary>
//    /// A TcpClient to connect to the specified socket.
//    /// </summary>
//    public class TcpClient : IDisposable
//    {
//        private readonly System.Net.Sockets.TcpClient client;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="TcpClient"/> class.
//        /// </summary>
//        /// <param name="hostName">The host name.</param>
//        /// <param name="port">The port.</param>
//        public TcpClient(string hostName, int port)
//        {
//#if NETSTANDARD1_3
//            client = new System.Net.Sockets.TcpClient();
//            client.ConnectAsync(hostName, port);
//#else
//            client = new System.Net.Sockets.TcpClient(hostName, port);       
//#endif
//        }

//        public async Task ConnectAsync(string host, int port)
//        {
//#if NETSTANDARD2_0

//#endif
//            await client.ConnectAsync(host, port);
//        }

//        public async Task ConnectAsync(IPAddress[] addresses, int port)
//        {
//            await client.ConnectAsync(addresses, port);
//        }

//        public async Task ConnectAsync(IPAddress address, int port)
//        {
//            await client.ConnectAsync(address, port);
//        }


//        /// <summary>
//        /// Gets or sets the receive timeout.
//        /// </summary>
//        /// <value>
//        /// The receive timeout.
//        /// </value>
//        public int ReceiveTimeout
//        {
//            get => client.ReceiveTimeout;
//            set => client.ReceiveTimeout = value;
//        }

//        /// <summary>
//        /// Gets a value indicating whether this <see cref="ISocket" /> is connected.
//        /// </summary>
//        /// <value>
//        ///   <c>true</c> if connected; otherwise, <c>false</c>.
//        /// </value>
//        public bool Connected => client.Connected;

//        /// <summary>
//        /// Gets the available bytes to be read.
//        /// </summary>
//        /// <value>
//        /// The available bytes to be read.
//        /// </value>
//        public int Available => client.Available;

//        public Socket Client
//        {
//            get => client.Client;
//            set => client.Client = value;
//        }

//        public int ReceiveBufferSize
//        {
//            get => client.ReceiveBufferSize;
//            set => client.ReceiveBufferSize = value;
//        }

//        public bool NoDelay
//        {
//            get => client.NoDelay;
//            set => client.NoDelay = value;
//        }

//        public LingerOption LingerState
//        {
//            get => client.LingerState;
//            set => client.LingerState = value;
//        }

//        public bool ExclusiveAddressUse
//        {
//            get => client.ExclusiveAddressUse;
//            set => client.ExclusiveAddressUse = value;
//        }

//        public int SendTimeout
//        {
//            get => client.SendTimeout;
//            set => client.SendTimeout = value;
//        }

//        public int SendBufferSize
//        {
//            get => client.SendBufferSize;
//            set => client.SendBufferSize = value;
//        }

//        /// <summary>
//        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        /// <summary>
//        /// Closes this instance.
//        /// </summary>
//        public void Close()
//        {
//#if NETSTANDARD1_3
//            client.Dispose();
//#else
//            this.client.Close();
//#endif
//        }

//        /// <summary>
//        /// Gets the stream.
//        /// </summary>
//        /// <returns>
//        /// Network stream socket connected to.
//        /// </returns>
//        public NetworkStream GetStream() => client.GetStream();

//        private void Dispose(bool isDisposing)
//        {
//            if (isDisposing)
//            {
//#if NETSTANDARD1_3
//                client.Dispose();
//#else
//                client.Close();
//#endif
//            }
//        }
//    }
//}