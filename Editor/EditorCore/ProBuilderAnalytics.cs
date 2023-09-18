// Will print event registration in the console as well as in editor.log
//#define PB_ANALYTICS_LOGGING

// Will allow data to be send if using a dev build
//#define PB_ANALYTICS_ALLOW_DEVBUILD

// Will allow data to be send from automated tests or scripts
//#define PB_ANALYTICS_ALLOW_AUTOMATION

// Data will stay locally and never sent to the database, the editor preference can also be used to prevent sending data
//#define PB_ANALYTICS_DONTSEND

using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine.Analytics;
using UnityEngine.ProBuilder;
#if !UNITY_2023_2_OR_NEWER
using System.Linq;
using UnityEngine;
#endif

namespace UnityEditor.ProBuilder
{
#if UNITY_2023_2_OR_NEWER
    [AnalyticInfo(
        eventName: k_ProbuilderEventName,
        vendorKey: k_VendorKey,
        maxEventsPerHour: k_MaxEventsPerHour,
        maxNumberOfElements: k_MaxNumberOfElements)]
    class ProBuilderAnalytics : IAnalytic
#else
    static class ProBuilderAnalytics
#endif
    {
        const int k_MaxEventsPerHour = 1000;
        const int k_MaxNumberOfElements = 1000;
        const string k_VendorKey = "unity.probuilder";

#if UNITY_2023_2_OR_NEWER
        const string k_ProbuilderEventName = "ProbuilderAction";
        const string k_PackageName = "com.unity.probuilder";

        MenuAction m_Action;
        string m_SelectMode;
        int m_SelectModeId;
        string m_TriggerType;
#else
        static bool s_EventRegistered = false;
        static string packageName = $"com.{k_VendorKey}";

        // Holds the type of data we want to send to the database
        enum EventName
        {
            ProbuilderAction
        }
#endif

        // Data structure for Triggered Actions
        [Serializable]
#if UNITY_2023_2_OR_NEWER
        struct ProBuilderActionData : IAnalytic.IData
#else
        struct ProBuilderActionData
#endif
        {
            public string actionName;
            public string actionType;
            public string subLevel;
            public int subLevelId;
            public string triggeredFrom;
        }

        // Triggered type is from where the action was performed
        public enum TriggerType
        {
            MenuOrShortcut,
            ProBuilderUI
        }

#if UNITY_2023_2_OR_NEWER
        internal ProBuilderAnalytics(MenuAction action, SelectMode mode, TriggerType triggerType)
        {
            m_Action = action;
            m_SelectMode = mode.ToString();
            m_SelectModeId = (int)mode;
            m_TriggerType = triggerType.ToString();
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new ProBuilderActionData
            {
                actionName = m_Action.menuTitle,
                actionType = m_Action.GetType().Name,
                subLevel = m_SelectMode,
                subLevelId = m_SelectModeId,
                triggeredFrom = m_TriggerType
            };
            data = parameters;
            return data != null;
        }

        // This is the main call to register an action event
        public static void SendActionEvent(MenuAction mAction, TriggerType triggerType)
        {
            var data = new ProBuilderAnalytics(mAction, SelectMode.Object, triggerType);

            // Don't send analytics when editor is used by an automated system
            #if !PB_ANALYTICS_ALLOW_AUTOMATION
            if (!InternalEditorUtility.isHumanControllingUs || InternalEditorUtility.inBatchMode)
            {
                DumpLogInfo($"[PB] Analytics deactivated, ProBuilder is currently used in Batch mode or run by automated system.");
                return;
            }
            #endif

            // Don't send analytics when using package repository
            #if !PB_ANALYTICS_ALLOW_DEVBUILD
            if (Directory.Exists($"Packages/{k_PackageName}/.git"))
            {
                DumpLogInfo($"[PB] Analytics deactivated, Dev build of ProBuilder is currently used.");
                return;
            }
            #endif

            #if PB_ANALYTICS_DONTSEND || DEBUG
                DumpLogInfo($"[PB] Analytics disabled: event='{k_ProbuilderEventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(data, true)}");
                return;
            #else

            try
            {
                // If DONTSEND is defined, skip sending stuff to the server
                #if !PB_ANALYTICS_DONTSEND
                var sendResult = EditorAnalytics.SendAnalytic(data);

                if (sendResult == AnalyticsResult.Ok)
                {
                    DumpLogInfo($"[PB] Event='{k_ProbuilderEventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(data, true)}");
                }
                else
                {
                    DumpLogInfo($"[PB] Failed to send event {k_ProbuilderEventName}. Result: {sendResult}");
                }
                #else
                DumpLogInfo($"[PB] Event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                #endif
            }
            catch(Exception e)
            {
                DumpLogInfo($"[PB] Exception --> {e}, Something went wrong while trying to send Event='{k_ProbuilderEventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(data, true)}");
            }
            #endif
        }
#else
        // This will register all the Event type at once
        static bool RegisterEvents()
        {
            if (!EditorAnalytics.enabled)
            {
                DumpLogInfo("[PB] Editor analytics are disabled");
                return false;
            }

            if (s_EventRegistered)
            {
                return true;
            }

            var allNames = Enum.GetNames(typeof(EventName));

            return !allNames.Any(eventName => !RegisterEvent(eventName));
        }
                static bool RegisterEvent(string eventName)
        {
            var result = EditorAnalytics.RegisterEventWithLimit(eventName, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey);
            switch (result)
            {
                case AnalyticsResult.Ok:
                {
                    DumpLogInfo($"ProBuilder: Registered event: {eventName}");
                    return true;
                }

                case AnalyticsResult.TooManyRequests:
                    // this is fine - event registration survives domain reload (native)
                    return true;

                default:
                {
                    DumpLogInfo($"[PB] Failed to register analytics event '{eventName}'. Result: '{result}'");
                    return false;
                }
            }
        }

        // This is the main call to register an action event
        public static void SendActionEvent(MenuAction mAction, TriggerType triggerType)
        {
            var data = new ProBuilderActionData();
            data.actionName = mAction.menuTitle;
            data.actionType = mAction.GetType().Name;
            data.subLevel = ProBuilderToolManager.selectMode.ToString();
            data.subLevelId = (int)ProBuilderToolManager.selectMode;
            data.triggeredFrom = triggerType.ToString();

            Send(EventName.ProbuilderAction, data);
        }

        static void Send(EventName eventName, object eventData)
        {
            // Don't send analytics when editor is used by an automated system
            #if !PB_ANALYTICS_ALLOW_AUTOMATION
            if (!InternalEditorUtility.isHumanControllingUs || InternalEditorUtility.inBatchMode)
            {
                DumpLogInfo($"[PB] Analytics deactivated, ProBuilder is currently used in Batch mode or run by automated system.");
                return;
            }
            #endif

            // Don't send analytics when using package repository
            #if !PB_ANALYTICS_ALLOW_DEVBUILD
            if (Directory.Exists($"Packages/{packageName}/.git"))
            {
                DumpLogInfo($"[PB] Analytics deactivated, Dev build of ProBuilder is currently used.");
                return;
            }
            #endif

            #if !PB_ANALYTICS_DONTSEND || DEBUG
            s_EventRegistered = RegisterEvents();
            #endif

            if (!s_EventRegistered)
            {
                DumpLogInfo($"[PB] Analytics disabled: event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                return;
            }

            try
            {
                // If DONTSEND is defined, skip sending stuff to the server
                #if !PB_ANALYTICS_DONTSEND
                var sendResult = EditorAnalytics.SendEventWithLimit(eventName.ToString(), eventData);
                if (sendResult == AnalyticsResult.Ok)
                {
                    DumpLogInfo($"[PB] Event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                }
                else
                {
                    DumpLogInfo($"[PB] Failed to send event {eventName}. Result: {sendResult}");
                }
                #else
                DumpLogInfo($"[PB] Event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                #endif
            }
            catch(Exception e)
            {
                DumpLogInfo($"[PB] Exception --> {e}, Something went wrong while trying to send Event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
            }
        }
#endif

        static void DumpLogInfo(string message)
        {
            #if PB_ANALYTICS_LOGGING
            Debug.Log(message);
            Console.WriteLine(message);
            #else
            if(Unsupported.IsSourceBuild())
                Console.WriteLine(message);
            #endif
        }
    }
}
