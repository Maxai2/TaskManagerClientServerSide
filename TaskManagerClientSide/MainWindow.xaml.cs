using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        public ObservableCollection<string> ProcessList { get; set; }

        Socket socket;
        EndPoint ep;

        //var answer = new byte[8192];

        private string conDisConIp = "";
        public string ConDisConIp
        {
            get { return conDisConIp; }
            set { conDisConIp = value; OnChanged(); }
        }

        private string runTask;
        public string RunTask
        {
            get { return runTask; }
            set { runTask = value; OnChanged(); }
        }

        string ClientString;

        //----------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            ProcessList = new ObservableCollection<string>();
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
                            //ClientString = string.Empty;
                            //ClientString = "Connect: " + ConDisConIp;

                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            ep = new IPEndPoint(IPAddress.Parse(ConDisConIp), 7534);

                            socket.Connect(ep);

                            //var data = Encoding.Default.GetBytes(ClientString);
                            var data = Encoding.Default.GetBytes("Connect:");
                            socket.Send(data);

                            var answer = new byte[8192];

                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    var length = socket.Receive(answer);

                                    if (length != 0)
                                    {
                                        var mStream = new MemoryStream();
                                        var binFormatter = new BinaryFormatter();

                                        mStream.Write(answer, 0, length);
                                        mStream.Position = 0;

                                        var myObject = binFormatter.Deserialize(mStream) as ObservableCollection<string>;

                                        /*var mStream = new MemoryStream();
                var binFormatter = new BinaryFormatter();

                // Where 'objectBytes' is your byte array.
                mStream.Write (objectBytes, 0, objectBytes.Length);
                mStream.Position = 0;

                var myObject = binFormatter.Deserialize(mStream) as YourObjectType;*/
                                    }
                                }
                            });
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //----------------------------------------------------------------------

    }
}
//----------------------------------------------------------------------