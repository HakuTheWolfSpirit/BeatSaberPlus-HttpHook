using CP_SDK.XUI;

namespace BeatSaberPlus_HTTPHook.UI
{
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUITextInput m_PortInput = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("HTTP Hook | Settings"),

                XUIText.Make($"HTTP server is listening on port {HTTPHookConfig.Instance.Port}")
                    .SetAlign(TMPro.TextAlignmentOptions.Midline),

                XUIText.Make("Port"),
                XUITextInput.Make("Port")
                    .SetValue(HTTPHookConfig.Instance.Port.ToString())
                    .Bind(ref m_PortInput),

                XUIPrimaryButton.Make("Apply & Restart Server", OnApplyPressed),

                XUIText.Make("Usage: POST http://your-ip:{port}/hook/{hookName}")
                    .SetAlign(TMPro.TextAlignmentOptions.Midline)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        protected sealed override void OnViewDeactivation()
        {
            HTTPHookConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnApplyPressed()
        {
            if (int.TryParse(m_PortInput.Element.GetValue(), out var l_Port) && l_Port > 0 && l_Port <= 65535)
            {
                HTTPHookConfig.Instance.Port = l_Port;
                HTTPHookConfig.Instance.Save();
                HTTPHook.Instance.RestartServer();
                ShowMessageModal("Server restarted on port " + l_Port);
            }
            else
            {
                ShowMessageModal("Invalid port number");
            }
        }
    }
}
