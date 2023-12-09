using Bark.GUI;
using GorillaNetworking;

namespace Bark.Modules.Misc
{
    public class Lobby : BarkModule
    {

        public static readonly string DisplayName = "Join Bark Code";

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            Plugin.Instance.JoinLobby("BARK_MOD", "MODDED_MODDED_CASUALCASUAL");
            this.enabled = false;
        }
        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Joins the official Bark Mod code";
        }

        protected override void Cleanup() { }   
    }
}
