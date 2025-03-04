using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Logging;
using Microsoft.Extensions.Logging;

namespace AiUo.Net;

/// <summary>
/// WebSocket客户端
/// </summary>
public class WebSocketClientEx : IDisposable
{
    private ILogger _logger;
    private readonly Uri _uri;
    private WebSocketMessageType MessageType => WebSocketMessageType.Binary;
    private readonly ClientWebSocket _webSocket;

    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;

    public event Action<byte[]> OnReceived;
    public event Action OnClosed;
    public event Action<Exception> OnError;

    public bool IsConnected => _webSocket.State == WebSocketState.Open;

    public WebSocketClientEx(string url) : this(new Uri(url)) { }
    public WebSocketClientEx(Uri uri)
    {
        _logger = LogUtil.CreateLogger<WebSocketClientEx>();
        _webSocket = new ClientWebSocket();
        _uri = uri;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
    }

    public Task SendAsync(ArraySegment<byte> data)
    {
        return _webSocket.SendAsync(data, MessageType, true, _cancellationToken);
    }
    public void Close() => Dispose();
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _webSocket.ConnectAsync(_uri, _cancellationToken)
                .ConfigureAwait(false);

            Task task = Task.Run(this.RunAsync, _cancellationToken);
        }
        catch (Exception ex)
        {
            RaiseConnectionError(ex);
            RaiseConnectionClosed();
        }
    }
    public void Connect()
    {
        ConnectAsync().Wait(5000);
    }

    private async Task RunAsync()
    {
        try
        {
            /*We define a certain constant which will represent
              size of received data. It is established by us and
              we can set any value. We know that in this case the size of the sent
              data is very small.
            */
            const int maxMessageSize = 2048;

            // Buffer for received bits.
            ArraySegment<byte> receivedDataBuffer = new ArraySegment<byte>(new byte[maxMessageSize]);

            MemoryStream memoryStream = new MemoryStream();

            // Checks WebSocket state.
            while (IsConnected && !_cancellationToken.IsCancellationRequested)
            {
                // Reads data.
                WebSocketReceiveResult webSocketReceiveResult =
                    await ReadMessage(receivedDataBuffer, memoryStream).ConfigureAwait(false);

                if (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    memoryStream.Position = 0;
                    OnNewMessage(memoryStream);
                }

                memoryStream.Position = 0;
                memoryStream.SetLength(0);
            }
        }
        catch (Exception ex)
        {
            if (!(ex is OperationCanceledException) ||
                !_cancellationToken.IsCancellationRequested)
            {
                RaiseConnectionError(ex);
            }
        }

        if (_webSocket.State != WebSocketState.CloseReceived && _webSocket.State != WebSocketState.Closed)
        {
            await CloseWebSocket().ConfigureAwait(false);
        }

        RaiseConnectionClosed();
    }

    private async Task CloseWebSocket()
    {
        try
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,"Failed sending a close message to client");
        }
    }

    private async Task<WebSocketReceiveResult> ReadMessage(ArraySegment<byte> receivedDataBuffer, MemoryStream memoryStream)
    {
        WebSocketReceiveResult webSocketReceiveResult;

        do
        {
            webSocketReceiveResult = await _webSocket.ReceiveAsync(receivedDataBuffer, _cancellationToken).ConfigureAwait(false);

            await memoryStream.WriteAsync(receivedDataBuffer.Array,
                    receivedDataBuffer.Offset,
                    webSocketReceiveResult.Count,
                    _cancellationToken)
                .ConfigureAwait(false);
        }
        while (!webSocketReceiveResult.EndOfMessage);

        return webSocketReceiveResult;
    }

    private void OnNewMessage(MemoryStream payloadData)
    {
        var data = payloadData.ToArray();
        OnReceived?.Invoke(data);
    }

    protected virtual void RaiseConnectionClosed()
    {
        _logger?.LogDebug("Connection has been closed");
        OnClosed?.Invoke();
    }

    protected virtual void RaiseConnectionError(Exception ex)
    {
        _logger?.LogError(ex,"A connection error occured");
        OnError?.Invoke(ex);
    }
}