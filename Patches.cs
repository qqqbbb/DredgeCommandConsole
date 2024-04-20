using CommandTerminal;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;


namespace CommandConsole
{
    internal class Patches
    {
        //[HarmonyPatch(typeof(CommandShell), "RunCommand", new Type[] { typeof(string), typeof(CommandArg[]) })]
        class CommandShell_SetState_Patch
        {
            public static void Postfix(CommandShell __instance, string command_name, CommandArg[] arguments)
            {
                Util.Log("CommandShell RunCommand " + command_name);
                foreach (var a in arguments)
                {
                    Util.Log("CommandShell RunCommand " + command_name + " arg " + a.String);
                }
            }
        }

        //[HarmonyPatch(typeof(WorldEventManager), "AddTerminalCommands")]
        class WorldEventManager_AddTerminalCommands_Patch
        {
            public static bool AddTerminalCommandsPrefix(WorldEventManager __instance)
            { // fix: world.event.list command requires args
                Terminal.Shell.AddCommand("world.event.list", new Action<CommandArg[]>(__instance.ListWorldEvents), 0, 0, "Lists all world events.");
                Terminal.Shell.AddCommand("world.event", new Action<CommandArg[]>(__instance.SpawnWorldEvent), 1, 2, "world.event <id> <delay>. Starts world.event. Ignores all conditions. Optional delay.");
                Terminal.Shell.AddCommand("world.event.test", new Action<CommandArg[]>(__instance.TestWorldEvent), 1, 1, "world.event.test <id>. Tests conditions for an event. Does not spawn it.");
                return false;
            }
        }

        [HarmonyPatch(typeof(Terminal))]
        class Terminal_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SetState")]
            public static bool SetStatePatch(Terminal __instance, TerminalState new_state)
            {
                //Util.Log("Terminal SetState " + __instance.state + " new state " + new_state);
                Util.SetConsoleBackground();
                __instance.input_style.normal.textColor = Config.fontColor.Value;
                __instance.InputColor = Config.fontColor.Value;
                __instance.input_fix = true;
                __instance.cached_command_text = __instance.command_text;
                __instance.command_text = "";
                if (new_state == TerminalState.Close)
                {
                    __instance.open_target = 0f;
                    //__instance.inputField.gameObject.SetActive(false);
                    //EventSystem.current.SetSelectedGameObject(__instance.selectedObjectBeforeShowingTerminal);
                }
                else if (new_state == TerminalState.OpenSmall)
                {
                    __instance.open_target = Screen.height * __instance.MaxHeight * __instance.SmallTerminalRatio;
                    if (__instance.current_open_t > __instance.open_target)
                    {
                        __instance.open_target = 0f;
                        __instance.state = TerminalState.Close;
                        return false;
                    }
                    __instance.real_window_size = __instance.open_target;
                    __instance.scroll_position.y = int.MaxValue;
                }
                else if (new_state == TerminalState.OpenFull)
                {
                    __instance.real_window_size = Screen.height * __instance.MaxHeight;
                    __instance.open_target = __instance.real_window_size;
                }
                __instance.state = new_state;
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldEventManager))]
        class WorldEventManager_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("AddTerminalCommands")]
            public static bool AddTerminalCommandsPrefix(WorldEventManager __instance)
            { // fix: world.event.list command requires args
                Terminal.Shell.AddCommand("world.event.list", new Action<CommandArg[]>(__instance.ListWorldEvents), 0, 0, "Lists all world events.");
                Terminal.Shell.AddCommand("world.event", new Action<CommandArg[]>(__instance.SpawnWorldEvent), 1, 2, "world.event <id> <delay>. Starts world.event. Ignores all conditions. Optional delay.");
                Terminal.Shell.AddCommand("world.event.test", new Action<CommandArg[]>(__instance.TestWorldEvent), 1, 1, "world.event.test <id>. Tests conditions for an event. Does not spawn it.");
                return false;
            }
            [HarmonyPostfix]
            [HarmonyPatch("ListWorldEvents")]
            public static void ListWorldEventsPostfix(WorldEventManager __instance)
            {
                string events = "";
                GameManager.Instance.DataLoader.allWorldEvents.ForEach(i => events = events + i.name + ", ");
                Terminal.Log(events);
            }
            [HarmonyPostfix]
            [HarmonyPatch("TestWorldEvent", new Type[] { typeof(CommandArg[]) })]
            public static void TestWorldEventPostfix(WorldEventManager __instance, CommandArg[] args)
            {
                string name = args[0].String.ToLower();
                WorldEventData e = GameManager.Instance.DataLoader.allWorldEvents.Find(x => x.name.ToLower() == name);
                if (e)
                    Terminal.Log(string.Format("[WorldEventManager] TestWorldEvent() result: {0}", __instance.TestWorldEvent(e, false)));
            }
        }

        [HarmonyPatch(typeof(AchievementManager), "ListAchievements")]
        class AchievementManager_ListAchievements_Patch
        {
            public static void Postfix(AchievementManager __instance)
            {
                string ids = "";
                __instance.allAchievements.ForEach(a => ids = ids + a.steamId + ", ");
                Terminal.Log(ids);
            }
        }

        [HarmonyPatch(typeof(EntitlementManager), "TestDLCOwned")]
        class EntitlementManager_ListAchievements_Patch
        {
            public static void Postfix(EntitlementManager __instance)
            {
                string s = "";
                __instance.entitlements.Keys.ToList().ForEach(e => s += (string.Format("Has DLC {0}: {1}", e, __instance.GetHasEntitlement(e))));
                Terminal.Log(s);
            }
        }

        [HarmonyPatch(typeof(ItemManager))]
        class ItemManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ListItems")]
            public static void ListItemsPostfix(ItemManager __instance)
            {
                string items = "";
                __instance.allItems.ForEach(i => items = items + i.id + ", ");
                Terminal.Log(items);
            }
            [HarmonyPostfix]
            [HarmonyPatch("ViewItemHistory")]
            public static void ViewItemHistoryPostfix(ItemManager __instance)
            {
                Terminal.Log("====== ITEM HISTORY ======");
                GameManager.Instance.SaveData.itemTransactions.ForEach(t => Terminal.Log(string.Format("{0} [Bought: {1}] [Sold: {2}]", t.itemId, t.bought, t.sold)));
                Terminal.Log("========== END ==========");
            }
            [HarmonyPostfix]
            [HarmonyPatch("ViewShopHistory")]
            public static void ViewShopHistoryPostfix(ItemManager __instance)
            {
                Terminal.Log("====== SHOP HISTORY ======");
                GameManager.Instance.SaveData.shopHistories.ForEach((h =>
                {
                    string visitDays = "[";
                    h.visitDays.ForEach(d => visitDays += string.Format("{0}, ", d));
                    visitDays += "]";
                    string transactionDays = "[";
                    h.transactionDays.ForEach(d => transactionDays += string.Format("{0}, ", d));
                    transactionDays += "]";
                    Terminal.Log(string.Format("{0} [VisitCount: {1}] [VisitDays: {2}] [TransactionDays: {3}] [TotalValue: {4}]", h.id, h.visits, visitDays, transactionDays, h.totalTransactionValue));
                }));
                Terminal.Log("========== END ==========");
            }
        }

        [HarmonyPatch(typeof(Player), "ListDestinations")]
        class Player_ListDestinations_Patch
        {
            public static void Postfix(Player __instance)
            {
                foreach (Dock dock in UnityEngine.Object.FindObjectsOfType<Dock>())
                    Terminal.Log(dock.Data.Id);
            }
        }

        [HarmonyPatch(typeof(PlayerAbilityManager), "DebugListAbilities")]
        class PlayerAbilityManager_DebugListAbilities_Patch
        {
            public static void Postfix(PlayerAbilityManager __instance)
            {
                string abilities = "";
                __instance.abilityMap.Keys.ToList().ForEach(i => abilities = abilities + i + ", ");
                Terminal.Log(abilities);
            }
        }

        [HarmonyPatch(typeof(PlayerSanity), "SetSanity")]
        class PlayerSanity_SetSanity_Patch
        {
            public static void Postfix(PlayerSanity __instance)
            {
                Terminal.Log(string.Format("Current sanity is: {0} with a rate of change of {1}.", __instance.CurrentSanity, __instance.RateOfChange));
            }
        }

        [HarmonyPatch(typeof(QuestManager))]
        class QuestManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ListQuests")]
            public static void ListQuestsPostfix(QuestManager __instance)
            {
                string quests = "";
                __instance.allQuests.Values.ToList().ForEach(i => quests = quests + i.name + ", ");
                Terminal.Log(quests);
            }
            [HarmonyPostfix]
            [HarmonyPatch("ListQuestSteps")]
            public static void ListQuestStepsPostfix(QuestManager __instance)
            {
                string steps = "";
                __instance.allQuestSteps.Values.ToList().ForEach(i => steps = steps + i.name + ", ");
                Terminal.Log(steps);
            }
            [HarmonyPostfix]
            [HarmonyPatch("ListQuestsDebug")]
            public static void ListQuestsDebugPostfix(QuestManager __instance)
            {
                List<SerializedQuestEntry> questsByState1 = __instance.GetQuestsByState(QuestState.OFFERED);
                List<SerializedQuestEntry> questsByState2 = __instance.GetQuestsByState(QuestState.STARTED);
                List<SerializedQuestEntry> questsByState3 = __instance.GetQuestsByState(QuestState.COMPLETED);
                Terminal.Log("======== START ========");
                Terminal.Log("OFFERED:");
                questsByState1.ForEach(q => Terminal.Log(q.id + " @ " + q.activeStepId));
                Terminal.Log("STARTED:");
                questsByState2.ForEach(q => Terminal.Log(q.id + " @ " + q.activeStepId));
                Terminal.Log("COMPLETED:");
                questsByState3.ForEach(q => Terminal.Log(q.id ?? ""));
                Terminal.Log("========= END =========");
            }
        }

        [HarmonyPatch(typeof(UpgradeManager), "ListUpgrades")]
        class UpgradeManager_ListUpgrades_Patch
        {
            public static void Postfix(UpgradeManager __instance)
            {
                string upgrades = "";
                __instance.allUpgradeData.ForEach(i => upgrades = upgrades + i.id + ", ");
                Terminal.Log(upgrades);
            }
        }

        [HarmonyPatch(typeof(WeatherController), "ListWeather")]
        class WeatherController_ListWeather_Patch
        {
            public static void Postfix(WeatherController __instance)
            {
                string weathers = "";
                __instance.allWeather.ForEach(w => weathers = weathers + w.name + ", ");
                Terminal.Log(weathers);
            }
        }


        [HarmonyPatch(typeof(Debug))]
        class Debug_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.Log), new Type[] { typeof(object) })]
            public static void LogPatch(Debug __instance, object message)
            {
                if (Config.logUnityDebug.Value)
                    Util.Log(message.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.Log), new Type[] { typeof(object), typeof(UnityEngine.Object)})]
            public static void LogPatch(Debug __instance, object message, UnityEngine.Object context)
            {
                if (Config.logUnityDebug.Value)
                    Util.Log(context.name + ": " + message.ToString());
            }
            [HarmonyPostfix] 
            [HarmonyPatch(nameof(Debug.LogFormat), new Type[] { typeof(string), typeof(object[]) })]
            public static void LogFormatPatch(Debug __instance, string format, object[] args)
            {
                if (!Config.logUnityDebug.Value)
                    return;

                StringBuilder sb = new(". args: ");
                foreach (object o in args)
                {
                    sb.Append(o.ToString());
                    sb.Append(", ");
                }
                Util.Log(format + ". args:  " + sb.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.LogError), new Type[] { typeof(object) })]
            public static void LogErrorPatch(Debug __instance, object message)
            {
                if (Config.logUnityError.Value)
                    Util.LogError(message.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.LogError), new Type[] { typeof(object), typeof(UnityEngine.Object) })]
            public static void LogErrorPatchx(Debug __instance, object message, UnityEngine.Object context)
            {
                if (Config.logUnityError.Value)
                    Util.LogError(message.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.LogException), new Type[] { typeof(Exception) })]
            public static void LogExceptionPatch(Debug __instance, Exception exception)
            {
                if (Config.logUnityError.Value)
                    Util.LogError(exception.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.LogException), new Type[] { typeof(Exception), typeof(UnityEngine.Object) })]
            public static void LogExceptionPatch(Debug __instance, Exception exception, UnityEngine.Object context)
            {
                if (Config.logUnityError.Value)
                    Util.LogError(exception.ToString());
            }
            [HarmonyPostfix] 
            [HarmonyPatch(nameof(Debug.LogWarning), new Type[] { typeof(object) })]
            public static void LogWarningPatch(Debug __instance, object message)
            {
                if (Config.logUnityWarning.Value)
                    Util.LogWarning(message.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.LogWarning), new Type[] { typeof(object), typeof(UnityEngine.Object) })]
            public static void LogWarningPatch(Debug __instance, object message, UnityEngine.Object context)
            {
                if (Config.logUnityWarning.Value)
                    Util.LogWarning(message.ToString());
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Debug.LogWarningFormat), new Type[] { typeof(string), typeof(object[]) })]
            public static void LogWarningFormatPatch(Debug __instance, string format, params object[] args)
            {
                if (!Config.logUnityWarning.Value)
                    return;

                StringBuilder sb = new(". args: ");
                foreach (object o in args)
                {
                    sb.Append(o.ToString());
                    sb.Append(", ");
                }
                Util.LogWarning(format + ". args:  " + sb.ToString());
            }

        }



    }
}
