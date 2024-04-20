using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using CommandTerminal;
using UnityEngine;
using UnityEngine.EventSystems;


namespace CommandConsole
{

    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Main : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "qqqbbb.dredge.CommandConsole";
        public const string PLUGIN_NAME = "Command Console";
        public const string PLUGIN_VERSION = "1.1.1";

        public static ConfigFile config;
        public static ManualLogSource log;
        public static Terminal terminal;

        private void Awake()
        {
            Harmony harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            config = this.Config;
            CommandConsole.Config.Bind();
            log = Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
        }

        private void Start()
        {
            //Util.Log("Command Console Start ");
            if (ApplicationEvents.Instance == null)
            {
                Util.Log("ApplicationEvents.Instance == null");
                return;
            }
            if (GameManager.Instance == null)
            {
                Util.Log("GameManager.Instance == null");
                return;
            }
            //ApplicationEvents.Instance.OnGameUnloaded += new Action(this.OnGameUnloaded);
            //ApplicationEvents.Instance.OnGameStartable += new Action(this.OnGameStarted);
            EnableConsole();
        }

        private static void EnableConsole()
        {
            terminal = GameManager.Instance.gameObject.AddComponent<Terminal>();
            terminal.ShowGUIButtons = CommandConsole.Config.showGUIButtons.Value;
            terminal.ErrorColor = CommandConsole.Config.errorFontColor.Value;
            //terminal.input_style.normal.textColor = CommandConsole.Config.fontColor.Value;
        }

        private void OnGameUnloaded()
        {

            //Util.Log("OnGameUnloaded");
        }

        private void OnGameStarted()
        {
            //Util.Log("OnGameStarted");
        }



        //private void Update()
        //{
            //if (Input.GetKeyDown(KeyCode.Z))
            //{
                //SetConsoleBackground(terminal);
            //}
            //if (Input.GetKeyDown(KeyCode.X))
            //{
                //Util.PrintPlayerPos();
            //}
            //if (Input.GetKeyDown(KeyCode.C))
            //{
                //Util.PrintPlayerTarget();
            //}
        //}


    }
}
