using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace ProjectFlow.API.Hubs
{
    public class TaskUpdatesHub : Hub
    {
        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
            Log.Information("Connection {ConnectionId} joined project group {ProjectId}",
                Context.ConnectionId, projectId);
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
            Log.Information("Connection {ConnectionId} left project group {ProjectId}",
                Context.ConnectionId, projectId);
        }

        public override async Task OnConnectedAsync()
        {
            Log.Information("Connection {ConnectionId} connected to TaskUpdatesHub", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Log.Information("Connection {ConnectionId} disconnected from TaskUpdatesHub", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}