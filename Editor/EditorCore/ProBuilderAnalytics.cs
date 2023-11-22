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

namespace UnityEditor.ProBuilder
{
    [AnalyticInfo(
        eventName: k_ProbuilderEventName,
        vendorKey: k_VendorKey,
        maxEventsPerHour: k_MaxEventsPerHour,
        maxNumberOfElements: k_MaxNumberOfElements)]
    class ProBuilderAnalytics : IAnalytic
    {
        const int k_MaxEventsPerHour = 1000;
        const int k_MaxNumberOfElements = 1000;
        const string k_VendorKey = "unity.probuilder";

        const string k_ProbuilderEventName = "ProbuilderAction";
        const string k_PackageName = "com.unity.probuilder";

        string m_ActionName;
        string m_ActionType;
        string m_SelectMode;
        int m_SelectModeId;
        string m_TriggerType;

        // Data structure for Triggered Actions
        [Serializable]
        struct ProBuilderActionData : IAnalytic.IData
        {
            public string actionName;
            public string actionType;
            public string subLevel;
            public int subLevelId;
            public string triggeredFrom;
        }

        internal ProBuilderAnalytics(string actionName, string actionType, SelectMode mode)
        {
            m_ActionName = actionName;
            m_ActionType = actionType;
            m_SelectMode = mode.ToString();
            m_SelectModeId = (int)mode;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new ProBuilderActionData
            {
                actionName = m_ActionName,
                actionType = m_ActionType,
                subLevel = m_SelectMode,
                subLevelId = m_SelectModeId,
                triggeredFrom = m_TriggerType
            };
            data = parameters;
            return data != null;
        }

        // This is the main call to register an action event
        public static void SendActionEvent(MenuAction mAction)
        {
            SendActionEvent(mAction.menuTitle, mAction.GetType().Name);
        }

        public static void SendActionEvent(string actionName, string actionType)
            {
                var data = new ProBuilderAnalytics(actionName, actionType, ProBuilderEditor.selectMode);

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
