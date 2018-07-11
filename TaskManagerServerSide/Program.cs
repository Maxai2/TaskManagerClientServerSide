using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TaskManagerServerSide
{
    class Program
    {
        static List<string> ProcessList = new List<string>();
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //--------------------------------------------------------------------

        static void Main(string[] args)
        {
            var ep = new IPEndPoint(IPAddress.Parse("10.2.14.3"), 7534);

            socket.Bind(ep);
            socket.Listen(10);

            var bytes = new byte[8192];

            var client = socket.Accept();

            while (true)
            {
                var length = client.Receive(bytes);

                var msg = Encoding.Default.GetString(bytes, 0, length);

                //Console.WriteLine(msg);

                if (msg != "")
                {
                    ParseMsg(msg);
                    msg = string.Empty;
                }
            }
        }

        //--------------------------------------------------------------------

        static void ParseMsg(string msg)
        {
            var mode = msg.Substring(0, msg.IndexOf(':'));

            switch (mode)
            {
                case "Connect":
                    {
                        ProcessList.Clear();
                        foreach (var item in Process.GetProcesses().OrderBy(f => f.ProcessName))
                        {
                            try
                            {
                                ProcessList.Add(item.ProcessName);
                            }
                            catch (Exception)
                            { }
                        }

                        /*var binFormatter = new BinaryFormatter();
var mStream = new MemoryStream();
binFormatter.Serialize(mStream, myObjToSerialize);

//This gives you the byte array.
mStream.ToArray();*/

                        /*var mStream = new MemoryStream();
var binFormatter = new BinaryFormatter();

// Where 'objectBytes' is your byte array.
mStream.Write (objectBytes, 0, objectBytes.Length);
mStream.Position = 0;

var myObject = binFormatter.Deserialize(mStream) as YourObjectType;*/
                    }
                    break;
                case "Run":
                    break;
                case "Kill":
                    break;
            }
        }

        //--------------------------------------------------------------------

    }
}
