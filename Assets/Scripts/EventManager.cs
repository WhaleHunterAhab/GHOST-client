using UnityEngine;
using WebSocketSharp;

public class EventManager : MonoBehaviour
{
    private WebSocket ws;
    void Start()
    {
        // Connect to your WebSocket server
        ws = new WebSocket("ws://echo.websocket.events");

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message received: " + e.Data);
        };

        ws.Connect();
        ws.Send("Hello Server!");
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
