using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HunniePop2ArchipelagoClient.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HunniePop2ArchipelagoClient.Archipelago
{
    public class ArchipelagoClient
    {
        public const string APVersion = "0.5.0";
        private const string Game = "Hunie Pop 2";

        public static bool Authenticated;
        private bool attemptingConnection;

        public static ArchipelagoData ServerData = new();
        private static ArchipelagoSession session;

        public static Dictionary<long, ScoutedItemInfo> shopdict = null;
        public static ArchipelageItemList alist = new ArchipelageItemList();

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
            session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
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
                            Game,
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

                buildshoplocations(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]));

                outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";
                alist.seed = session.RoomState.Seed;

                if (File.Exists(Application.persistentDataPath + "/archdata"))
                {
                    using (StreamReader file = File.OpenText(Application.persistentDataPath + "/archdata"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                        if (session.RoomState.Seed == savedlist.seed)
                        {
                            ArchipelagoConsole.LogMessage("archdata file found restoring session");
                            if (savedlist.list.Count >= alist.list.Count)
                            {
                                alist.merge(savedlist.list);
                            }
                            alist = savedlist;
                        }
                        else
                        {
                            ArchipelagoConsole.LogMessage("archdata file found but dosent match server seed creating new session");
                        }
                    }
                }
                else
                {
                    ArchipelagoConsole.LogMessage("archdata file not found creating new session");
                }



                ArchipelagoConsole.LogMessage(outText);
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

        public static string itemidtoname(long flag)
        {
            return session.Items.GetItemName(flag);
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
    }
}