﻿using Archipelago.MultiClient.Net.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
            ArchipelagoItem item = new ArchipelagoItem();
            item.item = netitem;
            list.Add(item);
        }

        public void merge(List<ArchipelagoItem> oldlist)
        {
            for (int i = 0; i < oldlist.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (oldlist[i].item == list[j].item)
                    {
                        list[j].processed = oldlist[i].processed;
                        list[j].putinshop = oldlist[i].putinshop;
                        break;
                    }
                }

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