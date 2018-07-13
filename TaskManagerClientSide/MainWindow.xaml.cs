using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

//----------------------------------------------------------------------

namespace TaskManagerClientSide
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [Serializable]
        public struct Proc
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public ObservableCollection<Proc> ProcessList { get; set; }

        Socket socket;
        EndPoint ep;

        private string conDisConIp = "";
        public string ConDisConIp
        {
            get { return conDisConIp; }
            set { conDisConIp = value; OnChanged(); }
        }

        private string runTask = "";
        public string RunTask
        {
            get { return runTask; }
            set { runTask = value; OnChanged(); }
        }

        private Proc procIndex;
        public Proc ProcIndex
        {
            get { return procIndex; }
            set { procIndex = value; OnChanged(); }
        }

        private Visibility conButVis;
        public Visibility ConButVis
        {
            get { return conButVis; }
            set { conButVis = value; OnChanged(); }
        }

        private Visibility disconButVis = Visibility.Collapsed;
        public Visibility DisconButVis
        {
            get { return disconButVis; }
            set { disconButVis = value; OnChanged(); }
        }

        private bool listIsEnable = false;
        public bool ListIsEnable
        {
            get { return listIsEnable; }
            set { listIsEnable = value; OnChanged(); }
        }

        private bool runIsEnable = false;
        public bool RunIsEnable
        {
            get { return runIsEnable; }
            set { runIsEnable = value; OnChanged(); }
        }


        //----------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            ProcessList = new ObservableCollection<Proc>();
        }

        //----------------------------------------------------------------------

        private ICommand connectCom;
        public ICommand ConnectCom
        {
            get
            {
                if (connectCom is null)
                {
                    connectCom = new RelayCommand(
                        (param) =>
                        {
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            ep = new IPEndPoint(IPAddress.Parse(ConDisConIp), 7534);

                            socket.Connect(ep);

                            RefreshProcListByCom("Connect");

                            ConButVis = Visibility.Collapsed;
                            DisconButVis = Visibility.Visible;

                            ListIsEnable = true;
                            RunIsEnable = true;
                        },
                        (param) =>
                        {
                            if (ConDisConIp != "")
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                }

                return connectCom;
            }
        }

        //----------------------------------------------------------------------

        private ICommand disconnectCom;
        public ICommand DisconnectCom
        {
            get
            {
                if (disconnectCom is null)
                {
                    disconnectCom = new RelayCommand(
                        (param) =>
                        {
                            RefreshProcListByCom("Disconnect");

                            ConButVis = Visibility.Visible;
                            DisconButVis = Visibility.Collapsed;

                            ListIsEnable = false;
                            RunIsEnable = false;
                        });
                }

                return disconnectCom;
            }
        }


        //----------------------------------------------------------------------

        private ICommand killCom;
        public ICommand KillCom
        {
            get
            {
                if (killCom is null)
                {
                    killCom = new RelayCommand(
                        (param) =>
                        {
                            RefreshProcListByCom("Kill");
                        });
                }

                return killCom;
            }
        }

        //----------------------------------------------------------------------

        private ICommand refreshCom;
        public ICommand RefreshCom
        {
            get
            {
                if (refreshCom is null)
                {
                    refreshCom = new RelayCommand(
                        (param) =>
                        {
                            RefreshProcListByCom("Refresh");
                        });
                }

                return refreshCom;
            }
        }
        //----------------------------------------------------------------------

        private ICommand runCom;
        public ICommand RunCom
        {
            get
            {
                if (runCom is null)
                {
                    runCom = new RelayCommand(
                        (param) =>
                        {
                            RefreshProcListByCom("Run");

                            RunTask = "";
                        },
                        (param) =>
                        {
                            if (RunTask != "")
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                }

                return runCom;
            }
        }

        //----------------------------------------------------------------------

        void RefreshProcListByCom(string com)
        {
            byte[] data = null;

            switch (com)
            {
                case "Disconnect":
                    data = Encoding.Default.GetBytes("Disconnect:");
                    break;
                case "Connect":
                    data = Encoding.Default.GetBytes("Connect:");
                    break;
                case "Kill":
                    data = Encoding.Default.GetBytes($"Kill:{ProcIndex.Id}");
                    break;
                case "Run":
                    data = Encoding.Default.GetBytes($"Run:{RunTask}");
                    break;
                case "Refresh":
                    data = Encoding.Default.GetBytes($"Refresh:");
                    break;
            }

            socket.Send(data);

            var answer = new byte[8192];

            var length = socket.Receive(answer);

            if (length != 0)
            {
                var mStream = new MemoryStream();
                var binFormatter = new BinaryFormatter();

                mStream.Write(answer, 0, length);
                mStream.Position = 0;

                var tempCol = binFormatter.Deserialize(mStream) as List<Proc>;

                ProcessList.Clear();

                foreach (var item in tempCol)
                {
                    var proc = new Proc();
                    proc.Id = item.Id;
                    proc.Name = item.Name;

                    ProcessList.Add(proc);
                }
            }
        }

        //----------------------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //----------------------------------------------------------------------

    }
}
//----------------------------------------------------------------------