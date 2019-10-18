using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public interface IProfileService
    {
        Item GetProfileItem(Item profileDescendant);
        List<Item> GetProfileKeys(Item profileItem);
        string GetProfileName(Item profileItem);
        string GetTrackingFieldValue(Item profileItem, Dictionary<string, string> keyValues);
        XElement GetProfileNode(string trackingFieldValue, ID profileId);
        string UpdateTrackingProfile(Item pageItem, Item profileCardItem);
        string UpdateTrackingGoal(Item pageItem, Item goalItem);
        string GetMinValue(Item profileKeyItem);
        string GetMaxValue(Item profileKeyItem);
        List<Item> GetGoals();
        List<Item> GetProfiles();
        List<Item> GetProfileCards(Item profileItem);
        List<Item> GetPatternCards(Item profileItem);
        List<Item> GetAllProfileCards();
        List<Item> GetAllPatternCards();
    }
}