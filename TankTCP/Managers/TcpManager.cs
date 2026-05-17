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
        private const string GameStartMarker = "GAMESTART";
        private string Ip;
        private const int Port = 8080;
        public bool GameEnded = false;

        private IPEndPoint _tcpEndpoint;
        private Socket _socket;
        private Socket _clientSocket;
        private NetworkStream _stream;
        private StreamReader _streamReader;

        public event Action<string> OnPlayerConnected;
        public event Action OnGameStart;
        public event Action<SendedDto> OnClientReceived;
        public event Action<string[]> OnHostReceived;
        public event Action<SendedDto> OnTankDestroyed;

        public TcpManager()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool TryIP(string ip)
        {
            if (IPEndPoint.TryParse(ip, out var temp))
            {
                Ip = ip;
                return true;
            }
            return false;
        }
        public async Task Connect(bool IsClient)
        {
            if (IsClient)
            {
                _tcpEndpoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
                await _socket.ConnectAsync(_tcpEndpoint);
                _stream = new NetworkStream(_socket);
                _streamReader = new StreamReader(_stream, Encoding.UTF8);
                ClientReceiveLoopAsync();
            }
            else
            {
                _tcpEndpoint = new IPEndPoint(IPAddress.Any, Port);
                _socket.Bind(_tcpEndpoint);
                _socket.Listen();
                ServerWaitingAsync();
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

        public async void ServerWaitingAsync()
        {
            _clientSocket = await _socket.AcceptAsync();
            _stream = new NetworkStream(_clientSocket);
            _streamReader = new StreamReader(_stream, Encoding.UTF8);
            string json = await _streamReader.ReadLineAsync();
            if (json != null)
            {
                string name = JsonSerializer.Deserialize<string>(json);
                OnPlayerConnected?.Invoke(name);
                ServerAsync();
            }
        }

        public async void ClientReceiveLoopAsync()
        {
            while (true)
            {
                string line = await _streamReader.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                if (line == GameStartMarker)
                {
                    GameEnded = false;
                    OnGameStart?.Invoke();
                    continue;
                }

                if (GameEnded)
                {
                    continue;
                }

                SendedDto dto = JsonSerializer.Deserialize<SendedDto>(line);
                if (dto?.gameObjects[0].Id == -67)
                {
                    OnTankDestroyed?.Invoke(dto);
                }
                else
                {
                    OnClientReceived?.Invoke(dto);
                }
            }
        }

        public async Task StartGameAsync()
        {
            byte[] data = Encoding.UTF8.GetBytes(GameStartMarker + "\n");
            await _clientSocket.SendAsync(data);
        }

        public async void SendRemoteKey(string[] arr)
        {
            string keys = JsonSerializer.Serialize(arr);
            byte[] data = Encoding.UTF8.GetBytes(keys + "\n");
            await _socket.SendAsync(data);
        }

        public async void SendTankDestroyMessage(SendedDto attachType)
        {
            string type = JsonSerializer.Serialize(attachType);
            byte[] data = Encoding.UTF8.GetBytes(type + "\n");
            await _clientSocket.SendAsync(data);
        }

        public async void SendName(string name)
        {
            string json = JsonSerializer.Serialize(name);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");
            await _socket.SendAsync(data);
        }
    }

}
