using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using HunniePop2ArchipelagoClient.HuniePop2.Gameplay;
using HunniePop2ArchipelagoClient.HuniePop2.UI;
using System.Linq;
using UnityEngine;

namespace HunniePop2ArchipelagoClient
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.dots.hunniepop2";
        public const string PluginName = "HunniePop2Archielago";
        public const string PluginVersion = "2.0.3";

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Client V({PluginVersion})";
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;

        private static Texture2D SolidBoxTex;
        public static GUIStyle mlabel;
        public static GUIStyle mlabel2;
        public static GUIStyle mbutton;
        public static GUIStyle mtext;

        public bool enablecon = false;
        public bool startmenu = false;
        public static bool reserror = false;

        public GameData data=> Game.Data;
        public static ArchipelageItemList alistt => ArchipelagoClient.alist;



        public static bool test = false;
        public GameObject test2;
        public Rect main;



        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            ArchipelagoClient = new ArchipelagoClient();

            new Harmony(PluginGUID).PatchAll();

            ArchipelagoConsole.Awake();
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchipelagoConsole.toggle();
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Game.Manager.gameCamera.Refresh();
                reserror = false;
            }
            if (Input.GetMouseButtonDown(0))
            {
                startmenu = true;
            }
        }

        private void OnGUI()
        {
            if (!MainMenuStuff.SCREEN_RES_WIDTHS.Contains(Screen.width) || !MainMenuStuff.SCREEN_RES_HEIGHTS.Contains(Screen.height))
            {
                if (!reserror)
                {
                    ArchipelagoConsole.LogMessage("SCREEN RESOLUTION BUGGED PRESS F5 TO RESET");
                    reserror = true;
                }
            }

            if (mlabel == null)
            {
                mlabel = new GUIStyle(GUI.skin.label.name);
                mlabel2 = new GUIStyle(GUI.skin.label.name);
                mbutton = new GUIStyle(GUI.skin.button.name);
                mtext = new GUIStyle(GUI.skin.textField.name);
                mlabel.fontSize = 34;
                mlabel2.fontSize = 28;
                mtext.fontSize = 20;
                mbutton.fontSize = 20;
                mtext.alignment = TextAnchor.MiddleLeft;
                mlabel.alignment = TextAnchor.MiddleRight;
                mlabel2.alignment = TextAnchor.MiddleCenter;
            }

            // show the mod is currently loaded in the corner
            //GUI.Label(new Rect(16, 16, 300, 20), ModDisplayInfo);
            if (true)
            {
                ArchipelagoConsole.OnGUI();
            }
            if (ArchipelagoClient.Authenticated)
            {
                GUI.Window(69, new Rect(Screen.width - 300, 10, 300, 50), statswindow, "");
            }
            if (test && (GameObject.Find("Canvas").GetComponent<UiTitleCanvas>() !=null))
            {

                GUI.color = new Color(0, 0, 0, 0);
                main = GUI.Window(420, new Rect((int)(Screen.width / 4.45), (int)(Screen.height / 4.9), (int)(Screen.width / 1.81), (int)(Screen.height / 1.9)), gamewindow, "");

            }
            else
            {
                DrawSolidBox(new Rect(Screen.width - 300, 10, 300, 50));
                GUI.color = new Color(0, 0, 0, 0);
                GUI.Window(69, new Rect(Screen.width - 300, 10, 300, 50), statswindow, "");
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

        public void statswindow(int id)
        {

            GUI.depth = 0;
            GUI.backgroundColor = Color.clear;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;

                if (PluginVersion == (string)ArchipelagoClient.ServerData.slotData["world_version"])
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
                GUI.Label(new Rect(5, 20, 300, 20), "Client V(" + PluginVersion + "): Status: NOT Connected");
            }
        }

        public void gamewindow(int id)
        {

            GUI.depth = 0;
            string statusMessage;
            if (ArchipelagoClient.Authenticated)
            {
                string state = "";
                string button = "";
                if (ArchipelagoClient.slotstate)
                {
                    state = "GAME STARTED";
                    button = "CONTINUE";
                }
                else
                {
                    state = "NEW GAME";
                    button = "START GAME";
                }
                GUI.Label(new Rect(20, (main.height / 2) - 100, main.width - 40, 40), $"SERVER STATE: {state}", mlabel2);
                GUI.Label(new Rect(20, (main.height / 2) - 50, main.width - 40, 40), $"LOCATIONS CHECKED: {ArchipelagoClient.session.Locations.AllLocationsChecked.Count()} OF {ArchipelagoClient.totalloc} ({((ArchipelagoClient.session.Locations.AllLocationsChecked.Count()/ ArchipelagoClient.totalloc)*100):G4}%)", mlabel2);
                GUI.Label(new Rect(20, (main.height / 2), main.width - 40, 40), $"ITEMS RECIEVED: {ArchipelagoClient.alist.list.Count()} OF {ArchipelagoClient.totalitem} ({((ArchipelagoClient.alist.list.Count() / ArchipelagoClient.totalitem) * 100):G4}%)", mlabel2);
                if (GUI.Button(new Rect(main.width/2-100, (main.height / 2) + 50, 200, 60), button, mbutton))
                {
                    StartGame.startarchipelago(GameObject.Find("Canvas").GetComponent<UiTitleCanvas>());
                }
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(20, (main.height/2)-100, main.width-40, 40), APDisplayInfo + statusMessage, mlabel2);
                GUI.Label(new Rect((main.width/2) - 300, (main.height / 2) - 60, 300, 40), "Host: ", mlabel);
                GUI.Label(new Rect((main.width/2) - 300, (main.height / 2) - 20, 300, 40), "Player Name: ", mlabel);
                GUI.Label(new Rect((main.width/2) - 300, (main.height / 2) + 20, 300, 40), "Password: ", mlabel);

                ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(main.width / 2, (main.height / 2) - 60, 260, 40),
                    ArchipelagoClient.ServerData.Uri, mtext);
                ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(main.width / 2, (main.height / 2) - 20, 260, 40),
                    ArchipelagoClient.ServerData.SlotName, mtext);
                ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(main.width / 2, (main.height / 2) + 20, 260, 40),
                    ArchipelagoClient.ServerData.Password, mtext);

                // requires that the player at least puts *something* in the slot name
                GUI.backgroundColor = Color.white;
                if (GUI.Button(new Rect((main.width / 2)-50, (main.height / 2) +80, 100, 40), "Connect", mbutton) &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    ArchipelagoClient.Connect();
                }
            }
        }

    }
}