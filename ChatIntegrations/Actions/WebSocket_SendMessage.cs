using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus_HTTPHook.ChatIntegrations.Actions
{
    /// <summary>
    /// Opens a WebSocket connection to the configured URL, sends one text message and closes it again.
    /// Every trigger makes its own short-lived connection, nothing stays open between triggers.
    /// </summary>
    public class WebSocket_SendMessage : IAction<WebSocket_SendMessage, Models.Actions.WebSocket_SendMessage>
    {
        private const int c_TimeoutMs = 5000;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUITextInput m_UrlInput = null;
        private XUIText      m_Message  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Send a message over a WebSocket connection";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("WebSocket URL",
                    XUITextInput.Make("ws://host:port/")
                        .SetValue(Model.Url)
                        .OnValueChanged((_) => OnUrlChanged())
                        .Bind(ref m_UrlInput)
                ),

                Templates.SettingsHGroup("Message",
                    XUIText.Make(Model.Message)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetWrapping(true)
                        .Bind(ref m_Message)
                ),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Set message", OnSetMessageButton),
                    XUISecondaryButton.Make("Test", OnTestButton)
                )
                .SetPadding(0)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnUrlChanged()
            => Model.Url = m_UrlInput.Element.GetValue().Trim();

        private void OnSetMessageButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == EValueType.String || x.Item1 == EValueType.Integer || x.Item1 == EValueType.Floating).ToArray();
            var l_Keys      = new List<(string, System.Action, string)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => View.KeyboardModal_Append("$" + l_Var.Item2), null));

            View.ShowKeyboardModal(Model.Message, (p_Result) =>
            {
                Model.Message = p_Result;
                m_Message.SetText(Model.Message);
            }, null, l_Keys);
        }

        private void OnTestButton()
        {
            if (!TryParseUrl(Model.Url, out var l_Uri, out var l_Error))
            {
                View.ShowMessageModal(l_Error);
                return;
            }

            View.ShowLoadingModal("Sending...", false);

            SendAsync(l_Uri, Model.Message).ContinueWith(p_Task =>
            {
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    View.CloseLoadingModal();
                    View.ShowMessageModal(p_Task.Result == null ? "Message sent!" : "Failed: " + p_Task.Result);
                });
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            p_Context.HasActionFailed = false;

            if (!TryParseUrl(Model.Url, out var l_Uri, out var l_Error))
            {
                p_Context.HasActionFailed = true;
                Logger.Instance.Error("[WebSocket_SendMessage] Event:" + Event.GenericModel.Name + " " + l_Error);
                yield break;
            }

            var l_Message = ReplaceVariables(Model.Message, p_Context);
            var l_Task    = SendAsync(l_Uri, l_Message);

            while (!l_Task.IsCompleted)
                yield return null;

            if (l_Task.Result != null)
            {
                p_Context.HasActionFailed = true;
                Logger.Instance.Error("[WebSocket_SendMessage] Event:" + Event.GenericModel.Name + " " + l_Task.Result);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static bool TryParseUrl(string p_Url, out Uri p_Uri, out string p_Error)
        {
            p_Error = null;

            if (!Uri.TryCreate(p_Url, UriKind.Absolute, out p_Uri)
                || (p_Uri.Scheme != "ws" && p_Uri.Scheme != "wss"))
            {
                p_Error = "Invalid WebSocket URL \"" + p_Url + "\" (expected ws:// or wss://)";
                return false;
            }

            return true;
        }

        private static string ReplaceVariables(string p_Input, ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            var l_Result    = p_Input ?? "";
            var l_Variables = p_Context.GetValues(EValueType.String, EValueType.Integer, EValueType.Floating);

            foreach (var l_Var in l_Variables)
            {
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == EValueType.String ? "" : "0";

                if (l_Var.Item1 == EValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == EValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == EValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = l_StringVal;

                l_Result = l_Result.Replace(l_Key, l_ReplaceValue);
            }

            return l_Result;
        }

        /// <summary>
        /// Connect, send one text frame, close. Returns null on success or an error description.
        /// Runs entirely on the thread pool so the game never blocks.
        /// </summary>
        private static Task<string> SendAsync(Uri p_Uri, string p_Message)
        {
            return Task.Run(async () =>
            {
                using (var l_Cancel = new CancellationTokenSource(c_TimeoutMs))
                using (var l_Client = new ClientWebSocket())
                {
                    try
                    {
                        await l_Client.ConnectAsync(p_Uri, l_Cancel.Token).ConfigureAwait(false);

                        var l_Bytes = Encoding.UTF8.GetBytes(p_Message ?? "");
                        await l_Client.SendAsync(new ArraySegment<byte>(l_Bytes), WebSocketMessageType.Text, true, l_Cancel.Token).ConfigureAwait(false);

                        try
                        {
                            await l_Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", l_Cancel.Token).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                            /// Message is already delivered, a sloppy close handshake on the server side is not a failure
                        }

                        return null;
                    }
                    catch (OperationCanceledException)
                    {
                        return "Timed out after " + c_TimeoutMs + "ms talking to " + p_Uri;
                    }
                    catch (Exception l_Exception)
                    {
                        return l_Exception.GetType().Name + " talking to " + p_Uri + ": " + l_Exception.Message;
                    }
                }
            });
        }
    }
}
