using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HunniePop2ArchipelagoClient.Archipelago
{
    public class ArchipelagoClient
    {
        public const string APVersion = "0.5.0";
        private const string game = "Hunie Pop 2";

        public static bool Authenticated;
        private bool attemptingConnection;

        public static ArchipelagoData ServerData = new();
        public static ArchipelagoSession session;

        public static Dictionary<long, ScoutedItemInfo> shopdict = null;
        public static ArchipelageItemList alist = new ArchipelageItemList();
        public static int totalloc = 0;
        public static int totalitem = 0;
        public bool slotstate = false;

        /// <summary>
        /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            if (Authenticated || attemptingConnection) return;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri.Trim());
                SetupSession();
            }
            catch (Exception e)
            {
                Plugin.BepinLogger.LogError(e);
            }

            TryConnect();
        }

        /// <summary>
        /// add handlers for Archipelago events
        /// </summary>
        private void SetupSession()
        {
            session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogArchMessage(message);
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.ErrorReceived += OnSessionErrorReceived;
            session.Socket.SocketClosed += OnSessionSocketClosed;
        }

        /// <summary>
        /// attempt to connect to the server with our connection info
        /// </summary>
        private void TryConnect()
        {
            try
            {
                // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
                ThreadPool.QueueUserWorkItem(
                    _ => HandleConnectResult(
                        session.TryConnectAndLogin(
                            game,
                            ServerData.SlotName,
                            ItemsHandlingFlags.AllItems, // TODO make sure to change this line
                            new Version(APVersion),
                            password: ServerData.Password,
                            requestSlotData: true // ServerData.NeedSlotData
                        )));
            }
            catch (Exception e)
            {
                Plugin.BepinLogger.LogError(e);
                HandleConnectResult(new LoginFailure(e.ToString()));
                attemptingConnection = false;
            }
        }

        /// <summary>
        /// handle the connection result and do things
        /// </summary>
        /// <param name="result"></param>
        private void HandleConnectResult(LoginResult result)
        {
            string outText;
            if (result.Successful)
            {
                var success = (LoginSuccessful)result;

                ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
                Authenticated = true;

                Plugin.BepinLogger.LogMessage($"CONNECTED TO SERVER | CLIENT V{Plugin.PluginVersion}, SERVER V{ServerData.slotData["world_version"]}");

                alist = new ArchipelageItemList();

                foreach (ItemInfo item in session.Items.AllItemsReceived)
                {
                    alist.add(item);
                }

                buildshoplocations(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]));
                totalitem = Convert.ToInt32(ServerData.slotData["total_items"]);
                totalloc = Convert.ToInt32(ServerData.slotData["total_locations"]);

                slotstate = session.DataStorage[Scope.Slot, "slotsetup"];

                outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

                string alists = session.DataStorage[Scope.Slot, "archdata"];
                if (alists.IsNullOrWhiteSpace())
                {
                    Plugin.BepinLogger.LogMessage("SERVER ARCHDATA = NULL");
                    alist.seed = session.RoomState.Seed;
                }
                else
                {
                    ArchipelageItemList alist2 = JsonConvert.DeserializeObject<ArchipelageItemList>(alists);
                    Plugin.BepinLogger.LogMessage("SERVER ARCHDATA:");
                    Plugin.BepinLogger.LogMessage(alists.ToString());
                    if (alist2.seed != "")
                    {
                        alist.merge(alist2.list);
                    }
                    else
                    {
                        alist.seed = session.RoomState.Seed;
                    }
                }
            }
            else
            {
                var failure = (LoginFailure)result;
                outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
                outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

                Plugin.BepinLogger.LogError(outText);

                Authenticated = false;
                Disconnect();
            }

            ArchipelagoConsole.LogMessage(outText);
            attemptingConnection = false;
        }

        /// <summary>
        /// something we wrong or we need to properly disconnect from the server. cleanup and re null our session
        /// </summary>
        private void Disconnect()
        {
            Plugin.BepinLogger.LogDebug("disconnecting from server...");
            session?.Socket.DisconnectAsync();
            session = null;
            Authenticated = false;
        }

        public void SendMessage(string message)
        {
            if ( message.StartsWith("$"))
            {
                processcode(message);
                return;
            }
            session.Socket.SendPacketAsync(new SayPacket { Text = message });
        }

        /// <summary>
        /// we received an item so reward it here
        /// </summary>
        /// <param name="helper">item helper which we can grab our item from</param>
        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var receivedItem = helper.DequeueItem();

            alist.add(receivedItem);

            if (helper.Index < ServerData.Index) return;

            ServerData.Index++;
        }

        public static void sendloc(int loc)
        {
            session.Locations.CompleteLocationChecks(loc);
        }

        public static ScoutedItemInfo getshopitem(int loc)
        {
            if (!Authenticated) { return null; }
            long key = 69420505 + loc;
            if (shopdict.ContainsKey(key))
            {
                return shopdict[key];
            }
            return null;
        }

        public void buildshoplocations(int num)
        {
            long[] shopids = new long[num];
            for (int i = 0; i < shopids.Length; i++) { shopids[i] = 69420506 + i; }


            Task<Dictionary<long, ScoutedItemInfo>> scoutedInfoTask = Task.Run(async () => await session.Locations.ScoutLocationsAsync(shopids));
            if (scoutedInfoTask.IsFaulted)
            {
                ArchipelagoConsole.LogMessage("ERROR:"+scoutedInfoTask.Exception.GetBaseException().Message);
                return;
            }
            shopdict = scoutedInfoTask.Result;

        }

        public static string seed()
        {
            return session.RoomState.Seed;
        }

        public static List<long> completeloc()
        {
            return session.Locations.AllLocationsChecked.ToList();
        }

        public static bool locdone(long flag)
        {
            return session.Locations.AllLocationsChecked.Contains(flag);
        }

        public static string itemidtoname(long flag)
        {
            return session.Items.GetItemName(flag);
        }

        public static void resetlist()
        {
            ArchipelagoConsole.LogMessage("RESETING RECIEVED ITEMS");
            ArchipelageItemList newlist = new ArchipelageItemList();
            int i = 0;

            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                ArchipelagoConsole.LogMessage($"NAME:{item.ItemName} ID:{item.ItemId} RECIEVED");
                newlist.add(item);
                i++;
            }
            alist = newlist;
            ArchipelagoConsole.LogMessage("ITEM RESET COMPLETE, RESET "+i.ToString()+" ITEMS");
        }

        public static void complete()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            session.Socket.SendPacket(statusUpdatePacket);
        }

        /// <summary>
        /// something went wrong with our socket connection
        /// </summary>
        /// <param name="e">thrown exception from our socket</param>
        /// <param name="message">message received from the server</param>
        private void OnSessionErrorReceived(Exception e, string message)
        {
            Plugin.BepinLogger.LogError(e);
            ArchipelagoConsole.LogMessage(message);
        }

        /// <summary>
        /// something went wrong closing our connection. disconnect and clean up
        /// </summary>
        /// <param name="reason"></param>
        private void OnSessionSocketClosed(string reason)
        {
            Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
            Disconnect();
        }

        public static void processcode(string code)
        {
            switch (code)
            {
                case "$resync":
                    ArchipelagoConsole.LogMessage("Resyncing Items");
                    session.Socket.SendPacket(new SyncPacket());
                    break;
                case "$resetitems":
                    ArchipelagoConsole.LogMessage("RESETING SENT ITEM LIST ALL ITEMS WILL BE REPROCESSED");
                    session.DataStorage[Scope.Slot, "archdata"] = "";
                    resetlist();
                    break;
                case "$voidfiller":
                    ArchipelagoConsole.LogMessage("REMOVING UNPROCESSED FILLER ITEMS FROM BEING PROCESSED");
                    foreach (ArchipelagoItem i in alist.list)
                    {
                        if (i.Id > 69420344 && i.Id < 69420422)
                        {
                            Plugin.BepinLogger.LogMessage($"setting item({i.ToString()}) to be processed");
                            i.processed = true;
                        }
                    }
                    ArchipelagoClient.session.DataStorage[Scope.Slot, "archdata"] = JsonConvert.SerializeObject(ArchipelagoClient.alist);
                    break;
                case "$debugsave":
                    string playerfile = ArchipelagoClient.session.DataStorage[Scope.Slot, $"savefile"];

                    ArchipelagoConsole.LogMessage($"------------PLAYER FILE START -----------");
                    ArchipelagoConsole.LogMessage($"{playerfile}");
                    ArchipelagoConsole.LogMessage($"------------PLAYER FILE END -----------");
                    break;
                case "$debugserverarchdata":
                    string adata = ArchipelagoClient.session.DataStorage[Scope.Slot, $"archdata"];

                    ArchipelagoConsole.LogMessage($"------------SERVER ARCHDATA START -----------");
                    ArchipelagoConsole.LogMessage($"{adata}");
                    ArchipelagoConsole.LogMessage($"------------SERVER ARCHDATA END -----------");
                    break;                
                case "$debugarchdata":
                    ArchipelagoConsole.LogMessage($"------------ARCHDATA START -----------");
                    ArchipelagoConsole.LogMessage($"{alist.ToString()}");
                    ArchipelagoConsole.LogMessage($"------------ARCHDATA END -----------");
                    break;
                case "$verifygame":
                    PlayerFile playerFilea = Game.Persistence.playerData.files[4];

                    ArchipelagoConsole.LogMessage("RESENDING PLAYER LOCATIONS");
                    foreach (PlayerFileGirl girl in playerFilea.girls)
                    {
                        ArchipelagoConsole.LogMessage($"RESENDING {girl.girlDefinition.name} LOCATIONS");
                        foreach (int q in girl.learnedFavs)
                        {
                            sendloc(69420144 + (girl.girlDefinition.id - 1) * 20 + q);
                        }
                        foreach (int s in girl.receivedShoes)
                        {
                            sendloc(IDs.idtoflag(girl.girlDefinition.shoesItemDefs[s].id) - 44);
                        }
                        foreach (int u in girl.receivedUniques)
                        {
                            sendloc(IDs.idtoflag(girl.girlDefinition.uniqueItemDefs[u].id) - 44);
                        }
                    }
                    foreach (PlayerFileGirlPair pair in playerFilea.girlPairs)
                    {
                        ArchipelagoConsole.LogMessage($"RESENDING {pair.girlPairDefinition.name} LOCATIONS");
                        if (pair.relationshipType == GirlPairRelationshipType.ATTRACTED)
                        {
                            sendloc(69420000 + pair.girlPairDefinition.id);
                        }
                        if (pair.relationshipType == GirlPairRelationshipType.LOVERS)
                        {
                            sendloc(69420024 + pair.girlPairDefinition.id);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}