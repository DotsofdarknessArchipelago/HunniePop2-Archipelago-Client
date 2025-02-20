using BepInEx;
using BepInEx.Logging;
using HunniePop2ArchipelagoClient.Archipelago;
using HunniePop2ArchipelagoClient.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace HunniePop2ArchipelagoClient
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.dots.hunniepop2";
        public const string PluginName = "HunniePop2Archielago";
        public const string PluginVersion = "1.1.1";

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Client V({PluginVersion})";
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;

        private static Texture2D SolidBoxTex;

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
                ArchipelagoConsole.toggle();
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
            if (true)
            {
                ArchipelagoConsole.OnGUI();
            }
            if (ArchipelagoClient.Authenticated)
            {
                GUI.Window(69, new Rect(Screen.width - 300, 10, 300, 50), window, "");
            }
            else
            {
                DrawSolidBox(new Rect(Screen.width - 300-90, 10, 300, 130));
                GUI.color = new Color(0, 0, 0, 0);
                GUI.Window(69, new Rect(Screen.width - 300-90, 10, 300, 130), window, "");
            }
            
            // this is a good place to create and add a bunch of debug buttons
        }

        public static void DrawSolidBox(Rect boxRect)
        {
            if (SolidBoxTex == null)
            {
                var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                windowBackground.SetPixel(0, 0, new Color(0, 0, 0));
                windowBackground.Apply();
                SolidBoxTex = windowBackground;
            }

            // It's necessary to make a new GUIStyle here or the texture doesn't show up
            GUI.Box(boxRect, "", new GUIStyle { normal = new GUIStyleState { background = SolidBoxTex } });
        }

        public void window(int id)
        {

            GUI.depth = 0;
            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;

                if (PluginVersion == ArchipelagoClient.ServerData.slotData["world_version"])
                {
                    GUI.Label(new Rect(5, 20, 300, 20), "Client/World V(" + PluginVersion + ") : Status: Connected");
                }
                else
                {
                    GUI.Label(new Rect(5, 20, 300, 20), "Client V(" + PluginVersion + "), World V(" + ArchipelagoClient.ServerData.slotData["world_version"] + "): Status: Connected");
                }
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
                GUI.backgroundColor = Color.white;
                if (GUI.Button(new Rect(100, 105, 100, 20), "Connect") &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    ArchipelagoClient.Connect();
                }

            }
        }

    }
}