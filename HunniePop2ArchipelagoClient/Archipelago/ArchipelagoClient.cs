using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
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

                alist = new ArchipelageItemList();

                foreach (ItemInfo item in session.Items.AllItemsReceived)
                {
                    alist.add(item);
                }

                buildshoplocations(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]));
                totalitem = Convert.ToInt32(ServerData.slotData["total_slots"]);
                totalloc = Convert.ToInt32(ServerData.slotData["total_slots"]);

                slotstate = session.DataStorage[Scope.Slot, "slotsetup"];

                outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

                string alists = session.DataStorage[Scope.Slot, "archdata"];
                ArchipelageItemList alist2 = JsonConvert.DeserializeObject<ArchipelageItemList>(alists);
                Plugin.BepinLogger.LogMessage("SERVER ARCHDATA:");
                Plugin.BepinLogger.LogMessage(alists);
                if (alist2.seed != "")
                {
                    //ArchipelagoConsole.LogMessage("ARCHDATA FOUND ON SERVER");
                    alist = alist2;
                }
                else
                {
                    alist.seed = session.RoomState.Seed;
                    //ArchipelagoConsole.LogMessage("ARCHDATA NOT FOUND CREATING NEW");
                }


                //if (File.Exists(Application.persistentDataPath + "/archdata"))
                //{
                //    using (StreamReader file = File.OpenText(Application.persistentDataPath + "/archdata"))
                //    {
                //        JsonSerializer serializer = new JsonSerializer();
                //        ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                //        if (alist.seed == savedlist.seed)
                //        {
                //            ArchipelagoConsole.LogMessage("archdata file found restoring session");
                //            if (alist.merge(savedlist.list))
                //            {
                //                ArchipelagoConsole.LogMessage("ERROR LOADING SAVED ITEM LIST RESETING ITEM LIST");
                //                resetlist();
                //            }
                //        }
                //        else
                //        {
                //            ArchipelagoConsole.LogMessage("archdata file found but dosent match server seed creating new session");
                //            ArchipelagoConsole.LogMessage(session.RoomState.Seed + ": does not equal :" + savedlist.seed);
                //        }
                //    }
                //}
                //else
                //{
                //    ArchipelagoConsole.LogMessage("archdata file not found creating new session");
                //}

                //ArchipelagoConsole.LogMessage(outText);
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
            //itemstoprocess.Enqueue(receivedItem.ItemId);
            alist.add(receivedItem);
            /*
            ArchipelagoConsole.LogMessage("------ITEM-RECEIVED--------");
            ArchipelagoConsole.LogMessage("ItemID: " + receivedItem.ItemId.ToString());
            ArchipelagoConsole.LogMessage("LocationID: " + receivedItem.LocationId.ToString());
            ArchipelagoConsole.LogMessage("Player: " + receivedItem.Player.ToString());
            ArchipelagoConsole.LogMessage("Flags: " + receivedItem.Flags.ToString());
            ArchipelagoConsole.LogMessage("ItemName: " + receivedItem.ItemName);
            ArchipelagoConsole.LogMessage("ItemDisplayName: " + receivedItem.ItemDisplayName);
            ArchipelagoConsole.LogMessage("locationName: " + receivedItem.LocationName);
            ArchipelagoConsole.LogMessage("LocationDisplayName: " + receivedItem.LocationDisplayName);
            ArchipelagoConsole.LogMessage("ItemGame: " + receivedItem.ItemGame);
            ArchipelagoConsole.LogMessage("LocationGame: " + receivedItem.LocationGame);
            ArchipelagoConsole.LogMessage("-------------------");
            */
            if (helper.Index < ServerData.Index) return;

            ServerData.Index++;

            // TODO reward the item here
            // if items can be received while in an invalid state for actually handling them, they can be placed in a local
            // queue to be handled later
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
                case "$resetitems":
                    ArchipelagoConsole.LogMessage("RESETING SENT ITEM LIST ALL ITEMS WILL BE REPROCESSED");
                    session.DataStorage[Scope.Slot, "archdata"] = "";
                    break;
                case "$voidfiller":
                    ArchipelagoConsole.LogMessage("REMOVING UNPORCESS FILLER ITEMS FROM BEING PROCESSED");
                    foreach (ArchipelagoItem i in alist.list)
                    {
                        if (i.Id > 69420344 && i.Id < 69420422)
                        {
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
                case "$debugarchdata":
                    string adata = ArchipelagoClient.session.DataStorage[Scope.Slot, $"archdata"];

                    ArchipelagoConsole.LogMessage($"------------ARCHDATA START -----------");
                    ArchipelagoConsole.LogMessage($"{adata}");
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