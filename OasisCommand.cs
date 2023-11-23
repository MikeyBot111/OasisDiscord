using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;


namespace OasisDiscord
{
    public class OasisCommand : IRocketCommand
    {
        #region Declarations
        private Dictionary<CSteamID, DateTime> Cooldown = new Dictionary<CSteamID, DateTime>();
        public List<string> Permissions => new List<String>() { "OasisDiscord.report" };
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "report";
        public string Syntax => "report (zpráva)";
        public string Help => "Příkaz pro report něčeho.";
        public List<string> Aliases => new List<string>();
        #endregion

        public void Execute(IRocketPlayer caller, string[] command)
        {
            #region Help
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, OasisPlugin.Instance.Translate("report_help"), Color.yellow);
                return;
            }
            #endregion

            if (command[0].ToString().ToLower() != "")
            {
                if (!player.HasPermission("report") && !player.IsAdmin)
                {
                    UnturnedChat.Say(player, OasisPlugin.Instance.Translate("report_permission"), Color.red);
                    return;
                }

                if (!player.HasPermission("report") && !player.IsAdmin)
                {
                    UnturnedChat.Say(player, OasisPlugin.Instance.Translate("report_permission"), Color.red);
                    return;
                }

                if (OasisPlugin.Instance.Configuration.Instance.ReportCooldown)
                {
                    if (Cooldown.ContainsKey(player.CSteamID))
                    {
                        int timeLeft = Convert.ToInt32(Math.Abs((DateTime.Now - Cooldown[player.CSteamID]).TotalSeconds));
                        if (timeLeft < OasisPlugin.Instance.Configuration.Instance.ReportCooldownSeconds)
                        {
                            UnturnedChat.Say(caller, OasisPlugin.Instance.Translate("report_cooldown", OasisPlugin.Instance.Configuration.Instance.ReportCooldownSeconds - timeLeft, OasisPlugin.Instance.Configuration.Instance.ReportCooldownSeconds), Color.red);
                            return;
                        }
                        Cooldown.Remove(player.CSteamID);
                    }
                }
                try
                { 
                    string Message = string.Join(" ", command); 
                    SendMsgToWebhook(player, Message, OasisPlugin.Instance.Configuration.Instance.ReportWebhook);
                    if (Cooldown.ContainsKey(player.CSteamID))
                    {
                        Cooldown[player.CSteamID] = DateTime.Now;
                    }
                    else
                    {
                        Cooldown.Add(player.CSteamID, DateTime.Now);
                    }
                    UnturnedChat.Say(caller, OasisPlugin.Instance.Translate("report_sent"), Color.green);
                    return;
                }
                catch (Exception e)
                {
                    Rocket.Core.Logging.Logger.LogError("Error when sending to webhook : " + e);
                    UnturnedChat.Say(caller, OasisPlugin.Instance.Translate("report_failed"), Color.red);
                    return;
                }
            }


        }

        private static void SendMsgToWebhook(UnturnedPlayer player, string message, string webhook)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{ \"content\": \"\", \"tts\": false, \"embeds\": [ { \"id\": 652627557, \"title\": \"⚠️ Report\", \"description\": \"🧑🏿 **Uživatel : ** `" + player.DisplayName + "`\\n\\n🪪 **SteamID: ** `" + player.CSteamID.ToString() + "`\\n\\n📨 **Zpráva: ** `" + message + "`\", \"color\": 16725250, \"fields\": [], \"footer\": { \"text\": \"Odesláno v " + DateTime.Now.ToString() + "\"} } ], \"components\": [], \"actions\": { } }";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }
    }
}