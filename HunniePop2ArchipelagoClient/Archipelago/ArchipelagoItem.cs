using Archipelago.MultiClient.Net.Models;
using HunniePop2ArchipelagoClient.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace HunniePop2ArchipelagoClient.Archipelago
{
    public class ArchipelagoItem
    {
        //public NetworkItem item;
        public ItemInfo item;
        public bool processed = false;
        public bool putinshop = false;

    }

    public class ArchipelageItemList
    {
        public List<ArchipelagoItem> list = new List<ArchipelagoItem>();
        public string seed = "";

        public void add(ItemInfo netitem)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].item == netitem)
                {
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem();
            item.item = netitem;
            list.Add(item);
        }

        public void merge(List<ArchipelagoItem> oldlist)
        {
            for (int i = 0; i < oldlist.Count; i++)
            {
                if (i>= list.Count) { return; }
                list[i].processed = oldlist[i].processed;
                list[i].putinshop = oldlist[i].putinshop;
            }
        }

        public bool hasitem(int flag)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].item.ItemId == flag)
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
                output += $"ID:{list[i].item.ItemId} PLAYER:{list[i].item.Player} LOC:{list[i].item.LocationId}\n";
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

        public void removedupes()
        {
            for (int i = list.Count-1; i >=0 ; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (list[i].item == list[j].item)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            
        }
    }
}
