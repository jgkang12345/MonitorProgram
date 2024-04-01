using System.Data;
using System.Net;
using System.Net.Sockets;
using static System.Windows.Forms.AxHost;

namespace MonitorProgram
{
    public class RecvBuffer
    {
        // [w/r][][][]
        private int _readPos;
        private int _writePos;
        private int _bufferSize;
        private byte[] _buffer;
        public byte[] GetRecvBuffer { get { return _buffer; } }
        public int ReadPos { get { return _readPos; } }
        public int WritePos { get { return _writePos; } }
        public int FreeSize { get { return _bufferSize - _writePos; } }
        public int DataSize { get { return _writePos - _readPos; } }

        public RecvBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
            _readPos = 0;
            _writePos = 0;
            _bufferSize = bufferSize;
        }

        public void AddRecvPos(int numOfBytes)
        {
            _readPos += numOfBytes;
            if (_writePos == _readPos)
            {
                _writePos = 0;
                _readPos = 0;
            }
            else if (_writePos >= 4096 * 4)
            {
                int dataSize = DataSize;
                Buffer.BlockCopy(GetRecvBuffer, ReadPos, GetRecvBuffer, 0, dataSize);
                _writePos = dataSize;
            }
        }

        public void AddWritePos(int numOfBytes)
        {
            _writePos += numOfBytes;
        }

    }

    public class StateObject
    {
        public bool Latency = false;

        public Socket workSocket = null;

        public RecvBuffer recvBuffer = new RecvBuffer(4096 * 5);
        public int Port { get; set; } = -1;
        public int Row { get; set; } = -1;
        public ServerInfo serverInfo { get; set; }
    }

    public partial class MainForm : Form
    {
        public Socket Listener { get; set; }
        public Dictionary<int, ServerInfo> ServerInfoHash = new Dictionary<int, ServerInfo>();
        public List<StateObject> sessionList = new List<StateObject>();
        private readonly object lockObject = new object();

        public MainForm()
        {
            InitializeComponent();
            dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
            InitDataGridView();
            NetworkInit();
        }

        public void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) 
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "ConnectionState") 
            {
                string content = e.Value.ToString();

                if (content == "Disconnect")
                {
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.ForeColor = Color.Yellow;    
                }
                else
                {
                    e.CellStyle.BackColor = Color.Green;
                    e.CellStyle.ForeColor = Color.Black;
                }
            }

            if (dataGridView1.Columns[e.ColumnIndex].Name == "ConnectionCnt")
            {
                e.CellStyle.ForeColor = Color.Red;
            }
        }

        public void InitDataGridView()
        {
            string ip = "58.236.130.58";
            List<ServerInfo> dataList = new List<ServerInfo>();
            dataList.Add(new ServerInfo(0,"Disconnect", ip, 30004, "���� 1ä��", 0, 0));
            dataList.Add(new ServerInfo(1,"Disconnect", ip, 30005, "���� 2ä��", 0, 0));
            dataList.Add(new ServerInfo(2,"Disconnect", ip, 30006, "���� 3ä��", 0, 0));
            dataList.Add(new ServerInfo(3,"Disconnect", ip, 30007, "�ʺ� 1ä��", 0, 0));
            dataList.Add(new ServerInfo(4,"Disconnect", ip, 30008, "�ʺ� 2ä��", 0, 0));
            dataList.Add(new ServerInfo(5,"Disconnect", ip, 30009, "�߼� 1ä��", 0, 0));
            dataList.Add(new ServerInfo(6,"Disconnect", ip, 30010, "��� 1ä��", 0, 0));
            dataList.Add(new ServerInfo(7,"Disconnect", ip, 30011, "��� 2ä��", 0, 0));
            for (int i = 0; i < dataList.Count; i++) 
            {
                int port = dataList[i].Port;
                ServerInfoHash.Add(port, dataList[i]);
            }
            dataGridView1.DataSource = dataList;
        }

        public void NetworkInit() 
        {
            IPAddress ipAddress = IPAddress.Parse("58.236.130.58");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 7777);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(localEndPoint);
            Listener.Listen(100);
            
            Thread acceptThread = new Thread(AcceptProc);
            acceptThread.IsBackground = true;
            acceptThread.Start();
            
            Thread isConnectThread = new Thread(IsConnect);
            isConnectThread.IsBackground = true;
            isConnectThread.Start();

        }

        bool IsConnectionClosed(Socket socket)
        {
            try
            {
                // microSeconds �Ķ���Ϳ� 0�� �ָ� ���� ��� ���� ���� ���¸� üũ�մϴ�.
                // SelectRead ��忡�� ������ �����ְų�, Listen ���°� �ƴϰ�, ���� �����Ͱ� ���ٸ� true�� ��ȯ�մϴ�.
                return socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0;
            }
            catch (SocketException)
            {
                // ���� ������ �߻��� ���, ������ ���� ������ ������ �� �ֽ��ϴ�.
                return true;
            }
        }

        void IsConnect() 
        {
            while (true) 
            {
                lock (lockObject) 
                {
                    List<StateObject> tempList = new List<StateObject>();
                    for (int i = 0; i < sessionList.Count; i++)
                    {
                        // bool isConnCheck = ((sessionList[i].workSocket.Poll(1000, SelectMode.SelectRead) && (sessionList[i].workSocket.Available == 0)) || !sessionList[i].workSocket.Connected);
                        if (IsConnectionClosed(sessionList[i].workSocket))
                        {
                            if (dataGridView1.InvokeRequired)
                            {
                                dataGridView1.Invoke(new Action(() =>
                                {
                                    dataGridView1.Rows[sessionList[i].Row].Cells["ConnectionState"].Value = "Disconnect";
                                }));
                            }
                            else
                            {
                                dataGridView1.Rows[sessionList[i].Row].Cells["ConnectionState"].Value = "Disconnect";
                            }

                            tempList.Add(sessionList[i]);
                        }
                    }
                    tempList.ForEach((it) => sessionList.Remove(it));
                }

                Thread.Sleep(1000);
            }
        }

        void AcceptProc() 
        {
            while (true)
            {
                Socket handler = Listener.Accept();
                // Ŭ���̾�Ʈ�� ����� ���� ��ü�� �����Ͽ� �񵿱� ���� �۾� ����
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.recvBuffer.GetRecvBuffer, 0, state.recvBuffer.FreeSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // StateObject���� ����� ������
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Ŭ���̾�Ʈ�κ��� �����͸� �а� ó��
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.recvBuffer.AddWritePos(bytesRead);
                    if (state.recvBuffer.DataSize < 4)
                    {
                        handler.BeginReceive(state.recvBuffer.GetRecvBuffer, state.recvBuffer.WritePos, state.recvBuffer.FreeSize, 0, new AsyncCallback(ReceiveCallback), state);
                        return;
                    }

                    ArraySegment<byte> pktCodeByte = new ArraySegment<byte>(state.recvBuffer.GetRecvBuffer, state.recvBuffer.ReadPos, 2);
                    ArraySegment<byte> pktSizeByte = new ArraySegment<byte>(state.recvBuffer.GetRecvBuffer, state.recvBuffer.ReadPos + 2, 2);
                    Int16 pktCode = BitConverter.ToInt16(pktCodeByte);
                    Int16 pktSize = BitConverter.ToInt16(pktSizeByte);


                    if (state.recvBuffer.DataSize < pktSize)
                    {
                        handler.BeginReceive(state.recvBuffer.GetRecvBuffer, state.recvBuffer.WritePos, state.recvBuffer.FreeSize, 0, new AsyncCallback(ReceiveCallback), state);
                        return;
                    }

                    byte[] packet = new byte[pktSize];
                    ArraySegment<byte> source = new ArraySegment<byte>(state.recvBuffer.GetRecvBuffer, state.recvBuffer.ReadPos, pktSize);
                    Array.Copy(source.ToArray(), packet, pktSize);
                    PacketHandle(state, packet);


                    state.recvBuffer.AddRecvPos(bytesRead);
                    handler.BeginReceive(state.recvBuffer.GetRecvBuffer, state.recvBuffer.WritePos, state.recvBuffer.FreeSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    if (state.Port != -1) 
                    {
                        // ������ ����� ���
                        if (dataGridView1.InvokeRequired)
                        {
                            dataGridView1.Invoke(new Action(() =>
                            {
                                dataGridView1.Rows[state.Row].Cells["ConnectionState"].Value = "Disconnect";
                            }));
                        }
                        else
                        {
                            dataGridView1.Rows[state.Row].Cells["ConnectionState"].Value = "Disconnect";
                        }
                    }

                    lock (lockObject)
                    {
                        sessionList.Remove(state);
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        void PacketHandle(StateObject state, byte[] packet) 
        {
            ArraySegment<byte> pktCodeByte = new ArraySegment<byte>(packet, 0, 2);
            ArraySegment<byte> pktSizeByte = new ArraySegment<byte>(packet, 2, 2);
            Int16 pktCode = BitConverter.ToInt16(pktCodeByte);
            Int16 pktSize = BitConverter.ToInt16(pktSizeByte);
            switch (pktCode)
            {
                case (short)Type.PacketProtocol.S2C_MONITORINIT:
                    {
                        MemoryStream ms = new MemoryStream(packet);
                        ms.Position = 0;
                        BinaryReader br = new BinaryReader(ms);
                        br.ReadInt32();
                        Int32 port = br.ReadInt32();
                        state.Port = port;
                        ServerInfoHash.TryGetValue(port, out var serverInfo);   
                        if (serverInfo != null) 
                        {
                            state.Row = serverInfo.Row;
                            state.serverInfo = serverInfo;
                            lock (lockObject)
                            {
                                sessionList.Add(state);
                            }
                            // dataGridView1�� DataGridView ��Ʈ���� �ν��Ͻ��Դϴ�.
                            if (dataGridView1.InvokeRequired)
                            {
                                dataGridView1.Invoke(new Action(() =>
                                {
                                    dataGridView1.Rows[serverInfo.Row].Cells["ConnectionState"].Value = "Connect";
                                }));
                            }
                            else
                            {
                                dataGridView1.Rows[serverInfo.Row].Cells["ConnectionState"].Value = "Connect";
                            }
                        }
                        break;
                    }
                case (short)Type.PacketProtocol.S2C_CONNECTIONLIST:
                    {
                        MemoryStream ms = new MemoryStream(packet);
                        ms.Position = 0;
                        BinaryReader br = new BinaryReader(ms);
                        br.ReadInt32();
                        int totalConnectionCnt = br.ReadInt32();
                        ServerInfoHash.TryGetValue(state.Port, out var serverInfo);
                        if (serverInfo != null)
                        {
                            if (dataGridView1.InvokeRequired)
                            {
                                dataGridView1.Invoke(new Action(() =>
                                {
                                    dataGridView1.Rows[serverInfo.Row].Cells["ConnectionCnt"].Value = $"{totalConnectionCnt}";
                                }));
                            }
                            else
                            {
                                dataGridView1.Rows[serverInfo.Row].Cells["ConnectionCnt"].Value = $"{totalConnectionCnt}";
                            }
                        }
                        break;
                    }
            }
        }
    }
}