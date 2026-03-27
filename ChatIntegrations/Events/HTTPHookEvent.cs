using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CI = ChatPlexMod_ChatIntegrations.ChatIntegrations;

namespace BeatSaberPlus_HTTPHook.ChatIntegrations.Events
{
    public class HTTPHookEvent : IEvent<HTTPHookEvent, Models.Events.HTTPHookEvent>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public HTTPHookEvent()
        {
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.String, "HookName")
            }.AsReadOnly();

            AvailableConditions = new List<string>()
                .Union(CI.RegisteredGlobalConditionsTypes)
                .Union(GetCustomConditionTypes())
                .Distinct().ToList().AsReadOnly();

            AvailableActions = new List<string>()
                .Union(CI.RegisteredGlobalActionsTypes)
                .Union(GetCustomActionTypes())
                .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUITextInput m_HookNameInput = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIVLayout.Make(
                    XUIText.Make("This event triggers when an HTTP POST is received at /hook/{HookName}")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                )
                .SetBackground(true),

                Templates.SettingsHGroup("Hook Name",
                    XUITextInput.Make("Enter hook name...")
                        .SetValue(Model.HookName)
                        .OnValueChanged((_) => OnHookNameChanged())
                        .Bind(ref m_HookNameInput)
                )
            };

            BuildUIAuto(p_Parent);
        }

        private void OnHookNameChanged()
        {
            Model.HookName = m_HookNameInput.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override sealed bool CanBeExecuted(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (p_Context.Type != HTTPHookTriggerType.TriggerType || p_Context.CustomData == null)
                return false;

            return string.Equals(
                (string)p_Context.CustomData,
                Model.HookName,
                StringComparison.OrdinalIgnoreCase
            );
        }

        protected override sealed void BuildProvidedValues(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            p_Context.AddValue(EValueType.String, "HookName", (string)p_Context.CustomData);
        }
    }
}
