using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class TextClient : MonoBehaviour
{
    [SerializeField] private IncomingServerCommunicator incomingServerCommunicator;

    private TcpClient client;
    private StreamReader reader;
    public StreamWriter writer { get; private set; }

    public string serverConnectionStatus;

    private bool connected = false;

    private CancellationTokenSource cancellationTokenSource;
    
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        _ = ConnectionLoop(cancellationTokenSource.Token);
    }

    async Task ConnectionLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!connected)
            {
                await TryConnect(token);
            }

            await Task.Delay(1000, token);
        }
    }

    async Task TryConnect(CancellationToken token)
    {
        try
        {
            serverConnectionStatus = "Connecting...";

            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 50001);

            NetworkStream stream = client.GetStream();

            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            connected = true;
            serverConnectionStatus = "Connected";
            Debug.Log("Connected to server");

           
            _ = Task.Run(() => ReceiveLoop(token), token);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Connection failed: " + e.Message);

            Cleanup();

            connected = false;
            serverConnectionStatus = "Retrying...";
        }
    }

    void ReceiveLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && client != null && client.Connected)
            {
                string message = reader.ReadLine(); 

                if (!string.IsNullOrEmpty(message))
                {
                    messageQueue.Enqueue(message.Trim());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Disconnected: " + e.Message);
        }


        connected = false;
        Cleanup();
        serverConnectionStatus = "Disconnected, retrying...";
    }

    void Update()
    {
     
        while (messageQueue.TryDequeue(out string message))
        {
            incomingServerCommunicator.HandleIncomingMessage(message);
        }
    }

    void Cleanup()
    {
        try { reader?.Close(); } catch { }
        try { writer?.Close(); } catch { }
        try { client?.Close(); } catch { }

        reader = null;
        writer = null;
        client = null;
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        Cleanup();
    }
}