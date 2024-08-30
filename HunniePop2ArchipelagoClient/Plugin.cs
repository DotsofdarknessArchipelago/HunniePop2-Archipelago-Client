using BepInEx;
using BepInEx.Logging;
using HunniePop2ArchipelagoClient.Archipelago;
using HunniePop2ArchipelagoClient.Utils;
using UnityEngine;

namespace HunniePop2ArchipelagoClient
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.dots.hunniepop2";
        public const string PluginName = "HunniePop2Archielago";
        public const string PluginVersion = "0.1.0";

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;

        public bool enablecon = false;
        public bool startmenu = false;



        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            ArchipelagoClient = new ArchipelagoClient();
            Patches.patch(ArchipelagoClient);
            ArchipelagoConsole.Awake();

            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                enablecon = !enablecon;
            }
            if (Input.GetMouseButtonDown(0))
            {
                startmenu = true;
            }
        }

        private void OnGUI()
        {
            // show the mod is currently loaded in the corner
            //GUI.Label(new Rect(16, 16, 300, 20), ModDisplayInfo);
            GUI.depth = 0;
            if (enablecon)
            {
                ArchipelagoConsole.OnGUI();
            }
            if (ArchipelagoClient.Authenticated)
            {
                GUI.Window(69, new Rect(Screen.width - 300, 10, 300, 50), window, "Archipelago");
            }
            else
            {
                GUI.Window(69, new Rect(Screen.width - 300, 10, 300, 130), window, "Archipelago");
            }
            
            // this is a good place to create and add a bunch of debug buttons
        }

        public void window(int id)
        {
            GUI.backgroundColor = Color.black;
            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;

                statusMessage = " Status: Connected";
                GUI.Label(new Rect(5, 20, 300, 20), APDisplayInfo + statusMessage);
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(5, 20, 300, 20), APDisplayInfo + statusMessage);
                GUI.Label(new Rect(5, 40, 150, 20), "Host: ");
                GUI.Label(new Rect(5, 60, 150, 20), "Player Name: ");
                GUI.Label(new Rect(5, 80, 150, 20), "Password: ");

                ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 40, 140, 20),
                    ArchipelagoClient.ServerData.Uri);
                ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 60, 140, 20),
                    ArchipelagoClient.ServerData.SlotName);
                ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 80, 140, 20),
                    ArchipelagoClient.ServerData.Password);

                // requires that the player at least puts *something* in the slot name
                if (GUI.Button(new Rect(100, 105, 100, 20), "Connect") &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    ArchipelagoClient.Connect();
                }

            }
        }

    }
}