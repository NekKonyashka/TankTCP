using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TankTCP
{
    public class TcpManager
    {
        private string Ip;
        private const int Port = 8080;

        private IPEndPoint _tcpEndpoint;
        private Socket _socket;
        private Socket _clientSocket;
        private NetworkStream _stream;
        private StreamReader _streamReader;

        public event Action OnPlayerConnected;
        public event Action OnGameStart;
        public event Action<SendedDto> OnClientReceived;
        public event Action<string[]> OnHostReceived;

        public TcpManager()
        {
            _socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        }

        public bool TryIP(string ip)
        {
            if(IPEndPoint.TryParse(ip, out var temp))
            {
                Ip = ip;
                return true;
            }
            return false;
        }
        public async void Connect(bool IsClient)
        {
            if (IsClient)
            {
                _tcpEndpoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
                await _socket.ConnectAsync(_tcpEndpoint);
                _stream = new NetworkStream(_socket);
                _streamReader = new StreamReader(_stream, Encoding.UTF8);
                ClientWaitingAsync();
            }
            else
            {
                _tcpEndpoint = new IPEndPoint(IPAddress.Any, Port);
                _socket.Bind(_tcpEndpoint);
                _socket.Listen();
                ServerAsync();
            }
        }

        public async Task SendWorldStateAsync(SendedDto dto)
        {
            string world = JsonSerializer.Serialize(dto);
            byte[] data = Encoding.UTF8.GetBytes(world + "\n");
            await _clientSocket.SendAsync(data);
        }

        public async void ServerAsync()
        {
            _clientSocket = await _socket.AcceptAsync();
            OnPlayerConnected?.Invoke();
            _stream = new NetworkStream(_clientSocket);
            _streamReader = new StreamReader(_stream, Encoding.UTF8);
            while (true)
            {
                string json = await _streamReader.ReadLineAsync();
                if (json != null)
                {
                    string[] keys = JsonSerializer.Deserialize<string[]>(json);
                    OnHostReceived?.Invoke(keys);
                }

            }
        }

        public async void ClientAsync()
        {
            while (true)
            {
                string json = await _streamReader.ReadLineAsync();

                if (json != null)
                {
                    SendedDto dto = JsonSerializer.Deserialize<SendedDto>(json);
                    OnClientReceived?.Invoke(dto);
                }

            }
        }

        public async void StartGameAsync()
        {
            await _clientSocket.SendAsync(new byte[1]);
        }

        public async void ClientWaitingAsync()
        {
            byte[] data = new byte[1024];
            int size = await _socket.ReceiveAsync(data);

            if (size > 0)
            {
                OnGameStart?.Invoke();
                ClientAsync();
            }
        }

        public async void SendRemoteKey(string[] arr)
        {
            string keys = JsonSerializer.Serialize(arr);
            byte[] data = Encoding.UTF8.GetBytes(keys + "\n");
            await _socket.SendAsync(data);
        }

    }

}
