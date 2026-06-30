using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

/// <summary>
/// Drives a small in-VR console for a websocket-sharp connection:
///   - Connect() -> opens a connection to the configured server URL.
///   - Cancel()  -> closes / cancels the connection.
///   - Log text  -> shows connection events ("Connection successful.", errors, etc.).
///
/// Wire the Connect button's OnClick() to Connect() and the Cancel button's OnClick() to Cancel()
/// in the Inspector. websocket-sharp raises its events on background threads, so handlers only
/// enqueue text; the queue is drained onto the UI in Update() (the Unity main thread). Assign the
/// TMP text field in the Inspector.
/// </summary>
public class WebSocketConsole : MonoBehaviour
{
    [Header("Server")]
    [Tooltip("WebSocket URL, e.g. ws://echo.websocket.events or ws://192.168.1.50:8080")]
    [SerializeField] string url = "ws://echo.websocket.events";

    [Header("UI")]
    [SerializeField] TMP_Text logText;
    [Tooltip("Scroll View containing the log text. New lines auto-scroll to the bottom; scroll up to read history.")]
    [SerializeField] ScrollRect scrollRect;
    [Tooltip("Safety cap on stored lines to bound memory. 0 = keep the entire log.")]
    [SerializeField] int maxLogLines = 0;

    WebSocket ws;

    readonly Queue<string> pending = new Queue<string>();
    readonly List<string> lines = new List<string>();
    readonly object gate = new object();

    void Update()
    {
        // Move any messages queued by background threads onto the UI on the main thread.
        List<string> batch = null;
        lock (gate)
        {
            if (pending.Count > 0)
            {
                batch = new List<string>(pending);
                pending.Clear();
            }
        }

        if (batch == null)
            return;

        foreach (var entry in batch)
        {
            // A single message may contain embedded newlines (e.g. multi-line received data).
            // Split it so each visual row is its own list entry.
            foreach (var line in entry.Split('\n'))
                lines.Add(line.TrimEnd('\r'));
        }

        // Optional safety cap so the log can't grow without bound; 0 keeps everything.
        if (maxLogLines > 0 && lines.Count > maxLogLines)
            lines.RemoveRange(0, lines.Count - maxLogLines);

        // Stick to the bottom only when the user is already there, so scrolling up to read
        // history isn't yanked back down by new messages.
        bool stickToBottom = IsAtBottom();

        if (logText != null)
            logText.text = string.Join("\n", lines);

        if (scrollRect != null && stickToBottom)
        {
            // The content's height changes when text is added; rebuild before snapping to bottom.
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // True when there's nothing to scroll yet, or the view is already at (or near) the bottom.
    bool IsAtBottom()
    {
        if (scrollRect == null || scrollRect.content == null || scrollRect.viewport == null)
            return true;

        bool scrollable = scrollRect.content.rect.height > scrollRect.viewport.rect.height + 1f;
        return !scrollable || scrollRect.verticalNormalizedPosition <= 0.05f;
    }

    public void Connect()
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            Log("Already connected.");
            return;
        }

        Log($"Connecting to {url} ...");

        try
        {
            ws = new WebSocket(url);
            ws.OnOpen    += (s, e) => Log("Connection successful.");
            ws.OnMessage += (s, e) => Log($"Received: {e.Data}");
            ws.OnError   += (s, e) => Log($"Error: {e.Message}");
            ws.OnClose   += (s, e) => Log($"Connection closed ({e.Code}) {e.Reason}");
            ws.ConnectAsync();
        }
        catch (Exception ex)
        {
            Log($"Failed to start connection: {ex.Message}");
        }
    }

    public void Cancel()
    {
        if (ws == null || ws.ReadyState == WebSocketState.Closed)
        {
            Log("Not connected.");
            return;
        }

        Log("Cancelling connection ...");
        ws.CloseAsync();
    }

    /// <summary>Thread-safe — may be called from websocket-sharp's background threads.</summary>
    void Log(string message)
    {
        lock (gate)
            pending.Enqueue($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }
}
