using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorProgram
{
    public class ServerInfo
    {

        public ServerInfo(int row, string connectionState, string ip, int port, string serverName, int latency, int connectionCnt) 
        {
            _row = row;
            _connectionState = connectionState;
            _ip = ip;
            _port = port;
            _serverName = serverName;   
            _latency = latency; 
            _connectionCnt = connectionCnt; 
        }
        public string ConnectionState { get { return _connectionState; } set { _connectionState = value; } }
        public string ServerName { get { return _serverName; } set { _serverName = value; } }
        public string IP { get { return _ip; } set { _serverName = value; } }
        public int Port { get { return _port; } set { _port = value; } }
        public long Latency { get { return _latency; } set { _latency = value; } }
        public int ConnectionCnt { get { return _connectionCnt; } set { _connectionCnt = value; } }
        public int Row { get { return _row; } set { _row = value; } }
        public long LastTick { get; set; } = 0;

        private string _connectionState;
        private string _ip;
        private int _port;
        private string _serverName;
        private long _latency;
        private int _connectionCnt;
        private int _row;
    }
}
