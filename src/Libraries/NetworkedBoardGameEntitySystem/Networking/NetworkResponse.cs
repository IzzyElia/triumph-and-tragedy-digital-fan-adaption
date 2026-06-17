using System.Text.Json;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

public struct NetworkResponse
{
    public NetworkResponse(int callbackId, string message, NetworkResponseError err = NetworkResponseError.None)
    {
        this.CallbackId = callbackId;
        this.Error = err;
        this.Message = message;
    }
    public NetworkResponse(int callbackId, object responseObj, NetworkResponseError err = NetworkResponseError.None)
    {
        this.CallbackId = callbackId;
        this.Error = err;
        this.Message = JsonSerializer.Serialize(responseObj);
    }
    public int CallbackId { get; set; }
    public NetworkResponseError Error { get; set; }
    public string Message { get; set; }
}

public enum NetworkResponseError
{
    None = 0,
    Error = 1,
}