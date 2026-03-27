using Newtonsoft.Json;

namespace BeatSaberPlus_HTTPHook.Models.Events
{
    public class HTTPHookEvent : ChatPlexMod_ChatIntegrations.Models.Event
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string HookName = "";
    }
}
