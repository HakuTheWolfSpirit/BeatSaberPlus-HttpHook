using BeatSaberPlus_HTTPHook.ChatIntegrations.Actions;
using IPA;

using CI = ChatPlexMod_ChatIntegrations.ChatIntegrations;

namespace BeatSaberPlus_HTTPHook
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class BSIPA
    {
        [Init]
        public BSIPA(IPA.Logging.Logger p_Logger)
        {
            Logger.Instance = new CP_SDK.Logging.IPALogger(p_Logger);
        }

        [OnEnable]
        public void OnEnable()
        {
            ChatPlexMod_ChatIntegrations.ChatIntegrations.RegisterEventType(
                "HTTPHookEvent",
                () => new ChatIntegrations.Events.HTTPHookEvent(),
                true
            );
            CI.RegisterActionType("WebSocket_SendMessage", () => new WebSocket_SendMessage());
        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}
