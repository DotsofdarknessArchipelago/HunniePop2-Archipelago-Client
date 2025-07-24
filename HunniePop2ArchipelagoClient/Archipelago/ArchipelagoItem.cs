using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace HunniePop2ArchipelagoClient.Archipelago
{
    public class ArchipelagoItem
    {
        public long Id;
        public string ItemName;
        public string PlayerName;
        public long LocationId;
        public bool processed = false;
        public bool putinshop = false;

        public ArchipelagoItem(ItemInfo item) 
        {
            this.Id = item.ItemId;
            this.ItemName = item.ItemName;
            this.PlayerName = item.Player.Name;
            this.LocationId = item.LocationId;
        }

        public ArchipelagoItem()
        {
            this.Id = -1;
            this.ItemName = "";
            this.PlayerName = "";
            this.LocationId = -1;
        }

        public override string ToString()
        {
            return $"{{ItemName:{ItemName},Id:{Id},PlayerName:{PlayerName},LocationId:{LocationId},processed:{processed},putinshop:{putinshop}}}";
        }
    }

    public class ArchipelageItemList
    {
        public List<ArchipelagoItem> list = new List<ArchipelagoItem>();
        public string seed = "";
        public int listversion = 1;

        public void add(ItemInfo netitem)
        {
            Plugin.BepinLogger.LogMessage($"trying to add new netitem{{{netitem.ItemName} from {netitem.Player.Slot}:{netitem.Player.Name} AT {netitem.LocationId}:{netitem.LocationName}}}");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == netitem.ItemId && list[i].PlayerName == netitem.Player.Name && list[i].LocationId == netitem.LocationId)
                {
                    Plugin.BepinLogger.LogMessage($"item skipped already in list");
                    return;
                }
            }
            Plugin.BepinLogger.LogMessage($"item not in list adding");
            ArchipelagoItem item = new ArchipelagoItem(netitem);
            list.Add(item);
        }

        public void add(ArchipelagoItem ai)
        {
            Plugin.BepinLogger.LogMessage($"trying to add new architem {ai.ToString()}");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == ai.Id && list[i].PlayerName == ai.PlayerName && list[i].LocationId == ai.LocationId)
                {
                    Plugin.BepinLogger.LogMessage($"item already in list setting processed to {ai.processed} and putinshop to {ai.putinshop}");
                    list[i].processed = ai.processed;
                    list[i].putinshop = ai.putinshop;
                    return;
                }
            }
            Plugin.BepinLogger.LogMessage($"item not in list adding");
            list.Add(ai);
        }

        public bool merge(List<ArchipelagoItem> oldlist)
        {
            if (oldlist == null || oldlist.Count == 0) return false;
            for (int i = 0; i < oldlist.Count; i++)
            {
                add(oldlist[i]);
            }
            return false;
        }

        public bool hasitem(int flag)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == flag)
                {
                    return true;
                }
            }
            return false;
        }

        public bool needprocessing()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].processed) { return true; }
            }
            return false;
        }

        public override string ToString()
        {
            string o = $"{{seed:{seed},listversion:{listversion},items:{{";
            for (int i = 0;i < list.Count;i++)
            {
                o += $"{i}:{list[i].ToString()},";
            }
            return $"{o}}}}}";
        }
    }
}
