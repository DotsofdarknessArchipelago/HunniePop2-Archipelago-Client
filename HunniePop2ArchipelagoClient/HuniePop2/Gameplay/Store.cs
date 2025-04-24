using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System.Collections.Generic;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Store
    {
        /// <summary>
        /// patch the store so that only the stuff we want is shown in there
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "PopulateStoreProducts")]
        [HarmonyPrefix]
        public static bool shoppopulate(PlayerFile __instance)
        {
            //possible deletion
            List<int> pairints = new List<int>();
            for (int i = 0; i < __instance.metGirlPairs.Count; i++) { pairints.Add(__instance.metGirlPairs[i].id); }

            //call mathond to generate store list might be able to clapse
            __instance.storeProducts = genStore(__instance);

            return false;
        }

        [HarmonyPatch(typeof(ItemData), "Get")]
        [HarmonyPrefix]
        public static bool itemid(int id, ref ItemDefinition __result)
        {
            if (id > 400)
            {
                __result = genarchitem(id - 400, 1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// make sure that if an item is thrown away that is needed for logic be placed into the shop to be baught
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneTrashZone), "OnDrop")]
        [HarmonyPrefix]
        public static void uniquetrash(Draggable draggable, UiCellphoneTrashZone __instance)
        {
            //only care about unique/shoe gifts
            if (draggable.type == DraggableType.INVENTORY_SLOT && (draggable.GetItemDefinition().itemType == ItemType.UNIQUE_GIFT || draggable.GetItemDefinition().itemType == ItemType.SHOES))
            {
                //get the item in the recieved items listthat matched out item and toggle the putinshop value
                int flag = IDs.idtoflag(draggable.GetItemDefinition().id);
                for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    if (ArchipelagoClient.alist.list[i].Id == flag && ArchipelagoClient.alist.list[i].processed)
                    {
                        ArchipelagoClient.alist.list[i].putinshop = true;
                    }
                }

            }
        }

        /// <summary>
        /// make sure that if an item was put into the shop and baught it no longer appears in the shop list
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneAppStore), "OnProductPurchased")]
        [HarmonyPrefix]
        public static void giftpurchase(ItemSlotBehavior itemSlotBehavior)
        {
            //if the item is a unique/shoe gift get the item in the recieved items list and set the putinshop value to false
            if (itemSlotBehavior.itemDefinition.itemType == ItemType.UNIQUE_GIFT || itemSlotBehavior.itemDefinition.itemType == ItemType.SHOES)
            {
                int flag = IDs.idtoflag(itemSlotBehavior.itemDefinition.id);
                for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    if (ArchipelagoClient.alist.list[i].Id == flag && ArchipelagoClient.alist.list[i].putinshop)
                    {
                        ArchipelagoClient.alist.list[i].putinshop = false;
                    }
                }
                //Game.Persistence.playerFile.SetFlagValue(Util.idtoflag(itemSlotBehavior.itemDefinition.id).ToString(), 1);
            }

            //if the item type is a fruit send the relevent location and set the flag that its been baught
            if (itemSlotBehavior.itemDefinition.itemType == ItemType.FRUIT)
            {
                ArchipelagoClient.sendloc(69420505 + itemSlotBehavior.itemDefinition.id - 400);
                Game.Persistence.playerFile.SetFlagValue("shopslot" + (itemSlotBehavior.itemDefinition.id - 400).ToString(), 1);
            }
        }


        /// <summary>
        /// generates a PlayerFileStoreProduct based on whats given
        /// i: index of the store product in the store list
        /// item: ItemDefinition of the product
        /// c: cost of the product
        /// </summary>
        public static PlayerFileStoreProduct genproduct(int i, ItemDefinition item, int c)
        {
            PlayerFileStoreProduct product = new PlayerFileStoreProduct();
            product.productIndex = i;
            product.itemDefinition = item;
            product.itemCost = c;
            return product;

        }

        /// <summary>
        /// Generates a custom archipelago item to use for locations
        /// i: the number of the arch item to be generated
        /// hide: toggles weather the item name will show what type of item the locaion will contain
        /// </summary>
        public static ItemDefinition genarchitem(int i, int hide)
        {
            ScoutedItemInfo architem = ArchipelagoClient.getshopitem(i);

            if (architem == null) { return null; }

            ItemDefinition tmp = Game.Data.Items.Get(314);
            //will through a warning since your not supposed to initilize an item definition like this but it works
            ItemDefinition item = new ItemDefinition();

            if (hide != 1)
            {
                if ((int)architem.Flags == 1 || (int)architem.Flags == 3 || (int)architem.Flags == 5 || (int)architem.Flags == 4)
                {
                    item.itemName = architem.Player.Name + " Progression item";
                }
                else if ((int)architem.Flags == 2 || (int)architem.Flags == 6)
                {
                    item.itemName = architem.Player.Name + " Useful item";
                }
                else
                {
                    item.itemName = architem.Player.Name + " Filler item";
                }
            }
            else
            {
                item.itemName = architem.Player.Name + " item";
            }

            //make the id start at 400 so it dosent confict with vanilla huniepop2
            item.id = i + 400;
            if (hide == 1)
            {
                item.itemDescription = "Buy this item to send it to " + architem.Player.Name + ", Trash after buying";
            }
            else
            {
                item.itemDescription = "Buy this item to send it to " + architem.Player.Name + $", Trash after buying, shop slot #{i}";
            }
            item.itemType = ItemType.FRUIT;
            item.itemSprite = tmp.itemSprite;
            item.energyDefinition = tmp.energyDefinition;
            //ArchipelagoConsole.LogMessage((i + 1).ToString());
            return item;
            
        }

        /// <summary>
        /// method to generate store list
        /// </summary>
        public static List<PlayerFileStoreProduct> genStore(PlayerFile file)
        {

            List<PlayerFileStoreProduct> store = new List<PlayerFileStoreProduct>();
            List<ItemDefinition> girlgifts = new List<ItemDefinition>();
            List<ItemDefinition> food = Game.Data.Items.GetAllOfTypes(ItemType.FOOD);
            List<ItemDefinition> date = Game.Data.Items.GetAllOfTypes(ItemType.DATE_GIFT);

            //get list of store locations that havent been collected yet
            List<int> architems = new List<int>();
            for (int s = 0; s < file.GetFlagValue("shopslots"); s++)
            {
                if (!ArchipelagoClient.locdone(69420506 + s))
                {
                    architems.Add(s + 1);
                }
            }

            //get list of items(items that have been recieved and tossed) that need to be put in the shop
            for (int f = 0; f < ArchipelagoClient.alist.list.Count; f++)
            {
                if (ArchipelagoClient.alist.list[f].putinshop)
                {
                    ItemDefinition def = Game.Data.Items.Get(IDs.flagtoid((int)ArchipelagoClient.alist.list[f].Id));
                    girlgifts.Add(def);
                }
            }

            //loop through all shop slots
            for (int i = 0; i < 32; i++)
            {
                //get a random number to select what get populated in the slot
                int ran = UnityEngine.Random.Range(0, 100);
                if (ran % 4 == 0)
                {
                    //populate the slot with "date gifts"
                    int num = UnityEngine.Random.Range(0, date.Count - 1);
                    store.Add(genproduct(i, date[num], UnityEngine.Random.Range(1, 5)));
                    date.RemoveAt(num);
                }
                else if ((ran % 4 == 1) && girlgifts.Count > 0)
                {
                    //populate the slot with a unique/shoe gift
                    int num = UnityEngine.Random.Range(0, girlgifts.Count - 1);
                    store.Add(genproduct(i, girlgifts[num], UnityEngine.Random.Range(1, 5)));
                    girlgifts.RemoveAt(num);
                }
                else if ((ran % 4 == 2) && architems.Count > 0)
                {
                    //populate the slot with a custom archipelago item
                    int num = UnityEngine.Random.Range(0, architems.Count - 1);
                    store.Add(genproduct(i, genarchitem(architems[num], file.GetFlagValue("disableshopslots")), UnityEngine.Random.Range(10, 20)));
                    architems.RemoveAt(num);
                }
                else
                {
                    //DEFAULT populate with food item
                    int num = UnityEngine.Random.Range(0, food.Count - 1);
                    store.Add(genproduct(i, food[num], UnityEngine.Random.Range(1, 5)));
                    food.RemoveAt(num);
                }
            }

            return store;
        }
    }
}