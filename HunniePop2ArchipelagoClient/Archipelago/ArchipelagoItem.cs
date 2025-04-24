using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;

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

    }

    public class ArchipelageItemList
    {
        public List<ArchipelagoItem> list = new List<ArchipelagoItem>();
        public string seed = "";
        public int listversion = 1;

        public void add(ItemInfo netitem)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == netitem.ItemId && list[i].PlayerName == netitem.Player.Name && list[i].LocationId == netitem.LocationId)
                {
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem(netitem);
            list.Add(item);
        }

        public bool merge(List<ArchipelagoItem> oldlist)
        {
            for (int i = 0; i < oldlist.Count; i++)
            {
                if (list[i].Id != oldlist[i].Id && list[i].PlayerName != oldlist[i].PlayerName && list[i].LocationId != oldlist[i].LocationId)
                {
                    return true;
                }
                if (i>= list.Count) 
                { 
                    ArchipelagoItem tmp = new ArchipelagoItem();
                    tmp.Id = oldlist[i].Id;
                    tmp.ItemName = oldlist[i].ItemName;
                    tmp.PlayerName = oldlist[i].PlayerName;
                    tmp.LocationId = oldlist[i].LocationId;
                    tmp.processed = oldlist[i].processed;
                    tmp.putinshop = oldlist[i].putinshop;
                    list.Add(tmp);
                }
                list[i].Id = oldlist[i].Id;
                list[i].ItemName = oldlist[i].ItemName;
                list[i].PlayerName = oldlist[i].PlayerName;
                list[i].LocationId = oldlist[i].LocationId;
                list[i].processed = oldlist[i].processed;
                list[i].putinshop = oldlist[i].putinshop;
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

        public string listprint()
        {
            string output = "";
            output += "-------------\n";
            for (int i = 0; i < list.Count; i++)
            {
                output += $"I:{i}";
                output += $"ID:{list[i].Id} PLAYER:{list[i].PlayerName} LOC:{list[i].LocationId}\n";
                output += $"PROCESSED:{list[i].processed} PUTINSHOP:{list[i].putinshop}\n";
            }
            return output;
        }

        public bool needprocessing()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].processed) { return true; }
            }
            return false;
        }
    }
}
