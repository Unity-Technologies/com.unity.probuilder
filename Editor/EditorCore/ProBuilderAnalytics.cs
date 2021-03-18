// Will print event registration in the console as well as in editor.log
//#define PB_ANALYTICS_LOGGING

// Will allow data to be send if using a dev build
//#define PB_ANALYTICS_ALLOW_DEVBUILD

// Will allow data to be send from automated tests or scripts
//#define PB_ANALYTICS_ALLOW_AUTOMATION

// Data will stay locally and never sent to the database, the editor preference can also be used to prevent sending data
//#define PB_ANALYTICS_DONTSEND

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderAnalytics
    {
        static bool s_EventRegistered = false;
        const int k_MaxEventsPerHour = 1000;
        const int k_MaxNumberOfElements = 1000;
        const string k_VendorKey = "unity.probuilder";
        static string packageName = $"com.{k_VendorKey}";


        // Data structure for Triggered Actions
        [Serializable]
        struct ProBuilderActionData
        {
            public string actionName;
            public string actionType;
            public string subLevel;
            public int subLevelId;
            public string triggeredFrom;
        }

        // Holds the type of data we want to send to the database
        enum EventName
        {
            ProbuilderAction
        }

        // Triggered type is from where the action was performed
        public enum TriggerType
        {
            MenuOrShortcut,
            ProBuilderUI
        }

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
            if (allNames.Any(eventName => !RegisterEvent(eventName)))
            {
                return false;
            }

            return true;
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

            #if !PB_ANALYTICS_DONTSEND
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
