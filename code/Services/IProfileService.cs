﻿using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public interface IProfileService
    {
        Item GetProfileItem(Item profileDescendant);
        List<Item> GetProfileKeys(Item profileItem);
        string GetProfileName(Item profileItem);
        string GetTrackingFieldValue(Item profileItem, Dictionary<string, string> keyValues);
        string GetMinValue(Item profileKeyItem);
        string GetMaxValue(Item profileKeyItem);
        List<Item> GetGoals(string db);
        List<Item> GetProfiles(string db);
        List<Item> GetProfileCards(Item profileItem);
        List<Item> GetPatternCards(Item profileItem);
        List<Item> GetAllProfileCards(string db);
        List<Item> GetAllPatternCards(string db);
    }
}