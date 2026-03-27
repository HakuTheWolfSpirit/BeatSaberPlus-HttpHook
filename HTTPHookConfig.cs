using Newtonsoft.Json;

namespace BeatSaberPlus_HTTPHook
{
    internal class HTTPHookConfig : CP_SDK.Config.JsonConfig<HTTPHookConfig>
    {
        [JsonProperty] internal bool Enabled = false;
        [JsonProperty] internal int Port = 2948;

        public override string GetRelativePath()
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/HTTPHook/Config";

        protected override void OnInit(bool p_OnCreation)
        {

        }
    }
}
