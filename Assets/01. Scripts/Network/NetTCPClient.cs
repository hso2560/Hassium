using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class NetTCPClient : MonoBehaviour
{
    private readonly string ip = "127.0.0.1";
    private readonly int port = 5400;

    private TcpClient client;
    private NetworkStream stream;

    private bool connected = false;

    public InputField idInput;
    public InputField pwInput;

    public void OnConnectToServer()
    {
        if(!connected)
        {
            try
            {
                client = new TcpClient();
                client.Connect(IPAddress.Parse(ip), port);
                stream = client.GetStream();

                connected = true;
            }
            catch
            {
                Debug.Log("Connection Error");
            }
        }
    }

    public void Login()
    {
        if(connected)
        {

        }
        else
        {

        }
    }
}
