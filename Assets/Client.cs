using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine.Events;

public class TextClient : MonoBehaviour
{

    
  
    
    [SerializeField] private IncomingServerCommunicator incomingServerCommunicator;
    [SerializeField]
    TextMeshProUGUI textField;


    const int maxLinesDisplay = 22;
    List<string> lines = new List<string>();
    TcpClient client;
    StreamReader reader;
    StreamWriter writer;


    /// <summary>
    /// Adds new text to the text display, while ensuring the total number of lines 
    /// doesn't exceed the maximum.
    /// </summary>
    void DisplayMessage(string text) {
        textField.text = text;
    }
    
    void Start()
    {
        TryServerConnection();
    }
    private bool connected = false;
    async void TryServerConnection()
    {
        Debug.Log("running");

        try
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 50001);

            NetworkStream stream = client.GetStream();

            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            DisplayMessage("Connected to server");
            writer.WriteLine("wawawaw");
            connected = true;
            return;
        }
        catch
        {
            DisplayMessage("Connection failed... retrying for ");
        }

        connected = false;

        if (!connected)
            //try again after 1 second
            Invoke(nameof(TryServerConnection), 1f);
    }
    
    public void SendMessageToClient(string message)
    {
        if(writer == null || client == null) return;
        writer.WriteLine(message);
    }
    void Update()
    {
        if (client == null || writer == null) return;

         if (client.Available > 0)
        {
            incomingServerCommunicator.HandleIncomingMessage(reader.ReadLine().Trim());
        }
    }
}