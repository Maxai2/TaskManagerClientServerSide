using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TaskManagerServerSide
{
    class Program
    {
        static List<string> ProcessList = new List<string>();

        //--------------------------------------------------------------------

        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ep = new IPEndPoint(IPAddress.Parse("172.16.1.60"), 7534);

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

                    var sendBytes = fillProcList();

                    socket.Send(sendBytes);

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
                    break;
                case "Run":
                    {
                        var variable = msg.Substring(msg.IndexOf(':'));

                        Process.Start(variable);
                    }
                    break;
                case "Kill":
                    {
                        var variable = Convert.ToInt32(msg.Substring(msg.IndexOf(':')));

                        var proc = Process.GetProcessById(variable);

                        proc.Kill();
                    }
                    break;
            }
        }

        //--------------------------------------------------------------------

        static byte[] fillProcList()
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

            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            binFormatter.Serialize(mStream, ProcessList);

            return mStream.ToArray();
        }

        //--------------------------------------------------------------------

    }
}
