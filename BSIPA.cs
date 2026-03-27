using IPA;

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
        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}
