using Microsoft.AspNetCore.SignalR;

namespace ProjectFlow.API.Hubs
{
    public class SimpleHub:Hub
    {
        public SimpleHub()
        {
            Console.WriteLine("SimpleHub created!");
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"SimpleHub connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public string Test()
        {
            Console.WriteLine("Test method called!");
            return "It works!";
        }
    }
}
