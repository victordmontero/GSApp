using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GPSApp
{
    public class GPSSender
    {
        public byte[] Buffer { get; set; }
        public Socket Socket { get; set; }

        public GPSSender()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.3.47"), 9000);

            Socket.Connect(localEndPoint);
        }

        public void Send(byte[] data)
        {
            Socket.Send(data);
        }

        public byte[] Serialize<T>(T something)
        {
            if (something == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, something);
                return ms.ToArray();
            }
        }

    }
}