using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static app.MainWindow;

namespace app.modules
{
    public class ServicesModule
    {
        public bool xcloud = false, yt = false, gnow = false;
        DiscordModule discord = new DiscordModule();
        private bool YTdisplay_playlist = false, YTdisplay_channel = false;
        public void xCloudHandle(data_bundle bundle)
        {
            string state = "Xbox Cloud Gaming";
            string details = "Loading";
            string title = bundle.title;
            string url = bundle.url;

            if (title == "Xbox Cloud Gaming")
                details = "Browsing the game library";
            else if (url.Contains("search"))
                details = "Searching for a game";
            else if (title == "Xbox Cloud Gaming (Beta) on Xbox.com")
                details = "Signing in";
            else if (title.Contains("|"))
            {
                int index = title.IndexOf('|');
                string game = title.Substring(0, index);
                game = game.Replace("&amp;", "&").Replace("&#39;", "'");

                if (url.Contains("/gallery/"))
                {
                    if (url.Contains("gallery/all-games"))
                        details = "Browsing the game library";
                    else
                        details = $"Browsing the {game} gallery";
                }
                else if (url.Contains("/games/"))
                    details = $"Viewing {game}";
                else if (url.Contains("/launch/"))
                    details = $"Playing {game}";
            }
            if (details == "Loading")
                return;
            discord.SetPresence(state, details, "xboxlogo");
        }

        public void YTHandle(data_bundle bundle)
        {
            string state = "Youtube";
            string details = "Loading";
            string title = bundle.title;
            string url = bundle.url;

            if (title == "YouTube")
            {
                if(url == "https://www.youtube.com/")
                    details = "Browsing the main page";
                else if(url == "https://www.youtube.com/feed/history")
                    details = "Browsing their history";
            }
            else if (title.Contains("Library - YouTube"))
                details = "Browsing their library";
            else if (url.Contains("playlist?list="))
            {
                if(YTdisplay_playlist)
                {
                    int index = title.IndexOf("- ");
                    string name = title.Substring(0, index);
                    details = "Browsing playlist: " + name;
                }
                else
                    details = "Browsing a playlist";
            }
            else if(url.Contains("/watch?v="))
            {
                int index = title.IndexOf("- ");
                string name = title.Substring(0, index);
                details = "Watching " + name;
            }
            else if(url.Contains("/channel/"))
            {
                if(YTdisplay_channel)
                {
                    int index = title.IndexOf("- ");
                    string name = title.Substring(0, index);
                    details = "Viewing " + name;
                }
                else
                    details = "Viewing a channel";
            }
            if (details == "Loading")
                return;
            discord.SetPresence(state, details, "yt");
        }

        public void GNowHandle(data_bundle bundle)
        {
            string state = "Geforce Now";
            string details = "Loading";
            string title = bundle.title;
            string url = bundle.url;

            if (title == "GeForce NOW")
                details = "Browsing games";
            else if (url.Contains("/games?game-id="))
            {
                int index = title.IndexOf("on GeForce NOW");
                string name = title.Substring(0, index);
                details = "Playing " + name;
            }
            else
                return;
            discord.SetPresence(state, details, "gnow");
        }

        public void UnsupportedHandle() => discord.ClearPresence();
    }
}
