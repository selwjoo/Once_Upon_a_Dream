using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;

public class LiveSocket : MonoBehaviour
{
    private WebSocket websocket;

    async void Start()
    {
        websocket = new WebSocket("ws://192.168.0.5:8000/ws/game/");

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            Debug.Log("¹ÞÀ½: " + message);
        };

        await websocket.Connect();
    }

    void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
