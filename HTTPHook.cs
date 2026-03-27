namespace BeatSaberPlus_HTTPHook
{
    internal class HTTPHook : CP_SDK.ModuleBase<HTTPHook>
    {
        public override CP_SDK.EIModuleBaseType             Type            => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name            => "HTTP Hook";
        public override string                              Description     => "Receive HTTP webhooks as ChatIntegrations triggers";
        public override bool                                UseChatFeatures => false;
        public override bool                                IsEnabled       { get => HTTPHookConfig.Instance.Enabled; set { HTTPHookConfig.Instance.Enabled = value; HTTPHookConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType  => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Network.HTTPServer m_Server = null;
        private UI.SettingsMainView m_SettingsMainView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override void OnEnable()
        {
            m_Server = new Network.HTTPServer();
            m_Server.OnHookReceived += Server_OnHookReceived;
            m_Server.Start(HTTPHookConfig.Instance.Port);
        }

        protected override void OnDisable()
        {
            if (m_Server != null)
            {
                m_Server.OnHookReceived -= Server_OnHookReceived;
                m_Server.Stop();
                m_Server = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsMainView == null)
                m_SettingsMainView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();

            return (m_SettingsMainView, null, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Server_OnHookReceived(string p_HookName)
        {
            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                if (ChatPlexMod_ChatIntegrations.ChatIntegrations.Instance != null)
                {
                    ChatPlexMod_ChatIntegrations.ChatIntegrations.Instance.HandleEvents(
                        new ChatPlexMod_ChatIntegrations.Models.EventContext()
                        {
                            Type = ChatIntegrations.HTTPHookTriggerType.TriggerType,
                            CustomData = p_HookName
                        }
                    );
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal void RestartServer()
        {
            if (m_Server != null)
            {
                m_Server.Stop();
                m_Server.Start(HTTPHookConfig.Instance.Port);
            }
        }
    }
}
