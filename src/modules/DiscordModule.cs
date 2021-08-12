using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.modules
{
    public class DiscordModule
    {
        private DiscordRpcClient discord_client;
        private string current_details = "";
        public DiscordModule()
        {
            discord_client = new DiscordRpcClient("750690488809422933");
            discord_client.Initialize();
        }

        public void Close()
        {
            discord_client.ClearPresence();
            discord_client.Dispose();
        }

        public void ClearPresence() => discord_client.ClearPresence();
        public void SetPresence(string state, string details, string service)
        {
            
            if (!discord_client.IsInitialized || current_details == details)
                return;
            discord_client.SetPresence(new RichPresence()
            {
                Details = state,
                State = details,
                Assets = new Assets()
                {
                    LargeImageKey = service,
                    LargeImageText = "OBC'S RP"
                }
            });
            discord_client.UpdateStartTime();
            current_details = details;
        }
    }
}
