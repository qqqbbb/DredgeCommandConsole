using BepInEx.Configuration;
using CommandTerminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommandConsole
{
    internal class Config
    {
        public static ConfigEntry<bool> showGUIButtons;
        public static ConfigEntry<bool> logUnityDebug;
        public static ConfigEntry<bool> logUnityWarning;
        public static ConfigEntry<bool> logUnityError;

        public static ConfigEntry<Color> fontColor;
        public static ConfigEntry<Color> errorFontColor;
        public static ConfigEntry<Color> backgroundColor;

        public static AcceptableValueRange<float> zeroOneRange = new(0f, 1f);

        public static void Bind()
        {
            errorFontColor = Main.config.Bind("", "Error message font color", Color.red);
            fontColor = Main.config.Bind("", "Font color", Color.cyan);
            backgroundColor = Main.config.Bind("", "Background color", Color.black);
            showGUIButtons = Main.config.Bind("", "Show console GUI Buttons", false);
            logUnityDebug = Main.config.Bind("", "Log Unity debug messages", true, "Unity debug messages will be logged to %userprofile%\\appdata\\LocalLow\\Black Salt Games\\DREDGE\\Player.log");
            logUnityWarning = Main.config.Bind("", "Log Unity warning messages", true, "Unity warning messages will be logged to %userprofile%\\appdata\\LocalLow\\Black Salt Games\\DREDGE\\Player.log");
            logUnityError = Main.config.Bind("", "Log Unity error messages", true, "Unity error messages will be logged to %userprofile%\\appdata\\LocalLow\\Black Salt Games\\DREDGE\\Player.log");

            showGUIButtons.SettingChanged += showGUIButtons_SettingChanged;
            errorFontColor.SettingChanged += errorColor_SettingChanged;
            fontColor.SettingChanged += fontColor_SettingChanged;
            backgroundColor.SettingChanged += errorColor_SettingChanged;

        }


        private static void showGUIButtons_SettingChanged(object sender, EventArgs e)
        {
            if (Main.terminal == null)
                return;

            Main.terminal.ShowGUIButtons = showGUIButtons.Value;
        }

        private static void BackgroundColor_SettingChanged(object sender, EventArgs e)
        {
            Util.SetConsoleBackground();
        }

        private static void errorColor_SettingChanged(object sender, EventArgs e)
        {
            if (Main.terminal == null)
                return;

            Main.terminal.ErrorColor = errorFontColor.Value;
        }
        private static void fontColor_SettingChanged(object sender, EventArgs e)
        {
            if (Main.terminal == null)
                return;

            Main.terminal.input_style.normal.textColor = fontColor.Value;
            Main.terminal.InputColor = fontColor.Value;
        }

    }
}
