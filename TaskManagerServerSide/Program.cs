using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ProcessorItemDLL;

namespace TaskManagerServerSide
{
    class Program
    {
        static List<ProcessorItemDLL.ProcItem> ProcessList = new List<ProcessorItemDLL.ProcItem>();
        static Socket socket;
        //--------------------------------------------------------------------

        static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //var ep = new IPEndPoint(IPAddress.Parse("172.16.1.60"), 7534);
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7534);

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

                    //socket.Connect(ep);
                    client.Send(sendBytes);

                    msg = string.Empty;
                }
            }
        }

        //--------------------------------------------------------------------

        static void ParseMsg(string msg)
        {
            var mode = msg.Substring(0, msg.IndexOf(':'));

            var variable = msg.Substring(msg.IndexOf(':') + 1);

            switch (mode)
            {
                case "Connect":
                    {

                    }
                    break;
                case "Refresh":
                    break;
                case "Run":
                    {
                        Process.Start(variable);
                    }
                    break;
                case "Kill":
                    {
                        var proc = Process.GetProcessById(Convert.ToInt32(variable));

                        proc.Kill();
                    }
                    break;
                case "Disconnect":
                    socket.Close();
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
                    var proc = new ProcessorItemDLL.ProcItem();
                    proc.Id = item.Id;
                    proc.Name = item.ProcessName;

                    ProcessList.Add(proc);
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
