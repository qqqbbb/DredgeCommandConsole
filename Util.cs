using CommandTerminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommandConsole
{
    internal class Util
    {
        public static void Message(string s)
        {
            //Main.log.LogMessage("Message s " + s);
            if (s == null || s.Length == 0)
                return;

            GameEvents.Instance.TriggerNotification(NotificationType.ERROR, s);

        }

        public static void Log(string s)
        {
            if (s == null || s.Length == 0)
                return;

            Main.log.LogDebug(s);
        }

        public static void LogWarning(string s)
        {
            if (s == null || s.Length == 0)
                return;

            Main.log.LogWarning(s);
        }

        public static void LogError(string s)
        {
            if (s == null || s.Length == 0)
                return;

            Main.log.LogError(s);
        }

        public static void SetConsoleBackground()
        {
            if (Main.terminal == null)
                return;
            //Util.Log("SetConsoleBackground");
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.SetPixel(0, 0, Config.backgroundColor.Value);
            texture2D.Apply();
            Main.terminal.input_style.normal.background = texture2D;
            Main.terminal.window_style.normal.background = texture2D;
        }


    }
}
