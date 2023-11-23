using Rocket.API;
using Rocket.Core.Assets;

namespace OasisDiscord
{
    public class OasisConfiguration : IRocketPluginConfiguration
    {
        public int ReportCooldownSeconds;
        public bool ReportCooldown, ChatLogEnabled;
        public string ReportWebhook, ChatLogWebhook;

        public void LoadDefaults()
        {
            this.ReportWebhook = "";
            this.ReportCooldown = false;
            this.ReportCooldownSeconds = 30;
            this.ChatLogWebhook = "";
            this.ChatLogEnabled = false;
        }
    }
}
