using Newtonsoft.Json;

namespace BeatSaberPlus_HTTPHook.Models.Actions
{
    public class WebSocket_SendMessage : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string Url = "ws://127.0.0.1:8080/";

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string Message = "";
    }
}
