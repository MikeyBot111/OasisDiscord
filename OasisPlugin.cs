using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using System.Net;
using System.Text;
using System;
using Rocket.Unturned.Events;
using SDG.Unturned;
using Steamworks;
using Newtonsoft.Json;

namespace OasisDiscord
{
    public class OasisPlugin : RocketPlugin<OasisConfiguration>
    {
        public static string version = "1.0.0.3";
        public static OasisPlugin Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList
                {
                    { "report_permission", "[Oasis] Nemáš oprávnění na tento příkaz." },
                    { "report_cooldown", "[Oasis] Report můžeš odeslat každých {1} sekund. Zbývá ti {0} sekund." },
                    { "report_failed", "[Oasis] Něco se pokazilo při posílaní reportu, zkus to znovu." },
                    { "report_sent", "[Oasis] Report byl odeslán admin teamu!" },
                    { "report_help", "[Oasis] Špatné použití příkazu, příkaz vypadá takto /report (zpráva)!" },
                };
            }
        }

        protected override void Load()
        {
            OasisPlugin.Instance = this;

            Logger.Log("OasisDiscord, Version " + OasisPlugin.version);
            Logger.Log("Checking errors... ");
            if (Configuration.Instance.ReportWebhook == "")
            {
                Logger.LogWarning("Report webhook isn't set, please set webhook.");
            }
            if (Configuration.Instance.ChatLogWebhook == "")
            {
                Logger.LogWarning("ChatLog webhook isn't set, please set webhook.");
            }
            else
            {
                Logger.Log("No errors have been found!");
            }

            U.Events.OnPlayerConnected += PlayerJoined;
            U.Events.OnPlayerDisconnected += PlayerDisconnect;
            UnturnedPlayerEvents.OnPlayerChatted += PlayerChatted;
            UnturnedPlayerEvents.OnPlayerDeath += PlayerDied;
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= PlayerJoined;
            U.Events.OnPlayerDisconnected -= PlayerDisconnect;
            UnturnedPlayerEvents.OnPlayerChatted -= PlayerChatted;
            UnturnedPlayerEvents.OnPlayerDeath -= PlayerDied;
        }

        private string GetIp(Steamworks.CSteamID cSteamID)
        {
            var ip = SteamGameServerNetworking.GetP2PSessionState(cSteamID, out var p2p)? Parser.getIPFromUInt32(p2p.m_nRemoteIP): "???";
            return ip;
        }

        private RootObject GetInfo(string Ip)
        {
            string response = new WebClient().DownloadString("http://ip-api.com/json/" + Ip);

            RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(response);

            if (Ip == "???")
            {
                rootObject.country = "???";
            }

            return rootObject;
        }


        private void PlayerJoined(UnturnedPlayer player)
        {
            if (Configuration.Instance.ChatLogEnabled) 
            {
                string ip = GetIp(player.CSteamID);
                RootObject info = GetInfo(ip);

                SendJoinToWebhook(player, Configuration.Instance.ChatLogWebhook, ip, info);
            }
        }

        private void PlayerDisconnect(UnturnedPlayer player)
        {
            if (Configuration.Instance.ChatLogEnabled)
            {
                SendDiscToWebhook(player, Configuration.Instance.ChatLogWebhook);
            }
        }

        private void PlayerDied(UnturnedPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            if (Configuration.Instance.ChatLogEnabled)
            {
                SendDiedToWebhook(player, Configuration.Instance.ChatLogWebhook, cause, limb, murderer);
            }
        }

        private void PlayerChatted(UnturnedPlayer player, ref UnityEngine.Color color, string message, SDG.Unturned.EChatMode chatMode, ref bool cancel)
        {
            if (Configuration.Instance.ChatLogEnabled)
            {
                message = message.Replace("\\", "");
                SendChatToWebhook(player, message, Configuration.Instance.ChatLogWebhook);
            }
        }

        private static void SendJoinToWebhook(UnturnedPlayer player, string webhook, string ip, RootObject info)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{ \"content\": \"\", \"tts\": false, \"embeds\": [ { \"id\": 652627557, \"title\": \"✅ Hráč se připojil\", \"description\": \"🧑🏿 **Uživatel : ** `" + player.DisplayName + "`\\n\\n🪪 **SteamID: ** `" + player.CSteamID.ToString() + "`\\n\\n🪪 **IP: ** `" + ip + "`\\n\\n🪪 **Country: ** `" + info.country + "`\", \"color\": 4325123, \"fields\": [], \"footer\": { \"text\": \"Odesláno v " + DateTime.Now.ToString() + "\"} } ], \"components\": [], \"actions\": { } }";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        private static void SendDiscToWebhook(UnturnedPlayer player, string webhook)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{ \"content\": \"\", \"tts\": false, \"embeds\": [ { \"id\": 652627557, \"title\": \"🚪 Hráč se odpojil\", \"description\": \"🧑🏿 **Uživatel : ** `" + player.DisplayName + "`\\n\\n🪪 **SteamID: ** `" + player.CSteamID.ToString() + "`\", \"color\": 16712451, \"fields\": [], \"footer\": { \"text\": \"Odesláno v " + DateTime.Now.ToString() + "\"} } ], \"components\": [], \"actions\": { } }";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        private static void SendDiedToWebhook(UnturnedPlayer player, string webhook, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            try
            {
                string payload = "{ \"content\": \"\", \"tts\": false, \"embeds\": [ { \"id\": 652627557, \"title\": \"💀 Hráč umřel\", \"description\": \"🧑🏿 **Uživatel : ** `" + player.DisplayName + "`\\n\\n🪪 **SteamID: ** `" + player.CSteamID.ToString() + "`\\n\\n🪪 **Vrah: ** `" + murderer + "`\\n\\n💀 **Příčina: ** `" + cause + "`\\n\\n💀 **Část: ** `" + limb + "`\", \"color\": 16712451, \"fields\": [], \"footer\": { \"text\": \"Odesláno v " + DateTime.Now.ToString() + "\"} } ], \"components\": [], \"actions\": { } }";
                client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
            }
            catch
            {
                string payload = "{ \"content\": \"\", \"tts\": false, \"embeds\": [ { \"id\": 652627557, \"title\": \"💀 Hráč umřel\", \"description\": \"🧑🏿 **Uživatel : ** `" + player.DisplayName + "`\\n\\n🪪 **SteamID: ** `" + player.CSteamID.ToString() + "`\\n\\n💀 **Příčina: ** `" + cause + "`\\n\\n💀 **Část: ** `" + limb + "`\", \"color\": 16712451, \"fields\": [], \"footer\": { \"text\": \"Odesláno v " + DateTime.Now.ToString() + "\"} } ], \"components\": [], \"actions\": { } }";
                client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
            }
                
        }

        private static void SendChatToWebhook(UnturnedPlayer player, string message, string webhook)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{ \"content\": \"\", \"tts\": false, \"embeds\": [ { \"id\": 652627557, \"title\": \"📨 Zpráva\", \"description\": \"🧑🏿 **Uživatel : ** `" + player.DisplayName + "`\\n\\n🪪 **SteamID: ** `" + player.CSteamID.ToString() + "`\\n\\n📨 **Zpráva: ** `" + message + "`\", \"color\": 257535, \"fields\": [], \"footer\": { \"text\": \"Odesláno v " + DateTime.Now.ToString() + "\"} } ], \"components\": [], \"actions\": { } }";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }


        public class RootObject
        {
            public string _as { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
            public string isp { get; set; }
            public float lat { get; set; }
            public float lon { get; set; }
            public string org { get; set; }
            public string query { get; set; }
            public string region { get; set; }
            public string regionName { get; set; }
            public string status { get; set; }
            public string timezone { get; set; }
            public string zip { get; set; }
        }
    }
}

