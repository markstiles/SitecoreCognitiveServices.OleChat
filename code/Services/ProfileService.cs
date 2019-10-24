using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public class ProfileService : IProfileService
    {
        protected readonly ISitecoreDataWrapper DataWrapper;

        public ProfileService(ISitecoreDataWrapper dataWrapper)
        {
            DataWrapper = dataWrapper;
        }

        public Item GetProfileItem(Item profileDescendant)
        {
            var profile = profileDescendant
                .Axes
                .GetAncestors()
                .FirstOrDefault(a => a.TemplateID.Guid.Equals(Constants.TemplateIds.ProfileTemplateId.Guid));

            return profile;
        }
    
        public List<Item> GetProfileKeys(Item profileItem)
        {
            if (!profileItem.TemplateID.Guid.Equals(Constants.TemplateIds.ProfileTemplateId.Guid))
                return new List<Item>();

            var keys = profileItem
                .GetChildren()
                .Where(a => a.TemplateID.Guid.Equals(Constants.TemplateIds.ProfileKeyTemplateId.Guid))
                .ToList();

            return keys;
        }
        
        public string GetProfileName(Item profileItem)
        {
            List<Guid> validIds = new List<Guid>
            {
                Constants.TemplateIds.ProfileKeyTemplateId.Guid,
                Constants.TemplateIds.ProfileCardTemplateId.Guid,
                Constants.TemplateIds.ProfileTemplateId.Guid,
                Constants.TemplateIds.PatternCardTemplateId.Guid
            };

            if (profileItem == null)
                return string.Empty;

            if(!validIds.Contains(profileItem.TemplateID.Guid))
                return string.Empty;

            var name = profileItem?.Fields["Name"]?.Value ?? string.Empty;

            return name;
        }

        public string GetTrackingFieldValue(Item profileItem, Dictionary<string, string> keyValues)
        {
            var tracking = new StringBuilder();
            tracking.Append("<tracking>");
            tracking.AppendFormat("<profile id=\"{0}\" name=\"{1}\">", profileItem.ID, GetProfileName(profileItem));
            foreach (var k in keyValues)
            {
                tracking.AppendFormat("<key name=\"{0}\" value=\"{1}\" />", k.Key, k.Value);
            }
            tracking.Append("</profile>");
            tracking.Append("</tracking>");

            return tracking.ToString();
        }

        public XElement GetProfileNode(string trackingFieldValue, ID profileId)
        {
            if (string.IsNullOrWhiteSpace(trackingFieldValue))
                return null;

            var profileCardDoc = XDocument.Parse(trackingFieldValue);
            var profileNode = profileCardDoc
                .Root
                .Descendants("profile")
                .FirstOrDefault(a => a.Attribute("id").Value == profileId.ToString());

            return profileNode;
        }

        public string UpdateTrackingProfile(Item pageItem, Item profileCardItem)
        {
            var profileItem = GetProfileItem(profileCardItem);

            var profileCardValueField = profileCardItem.Fields[Constants.FieldIds.ProfileCard.ProfileCardValueFieldId];
            var cardProfile = GetProfileNode(profileCardValueField.Value, profileItem.ID);
            var keys = cardProfile.Descendants("key")
                .Select(a => new KeyValuePair<string, string>(a.Attribute("name").Value, a.Attribute("value").Value))
                .ToDictionary(a => a.Key, b => b.Value);
            
            var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];
            var trackingValue = string.IsNullOrWhiteSpace(trackingField.Value) ? "<tracking></tracking>" : trackingField.Value;
            var profileCardDoc = XDocument.Parse(trackingValue);
            var profileNode = profileCardDoc
                .Root
                .Descendants("profile")
                .FirstOrDefault(a => a.Attribute("id").Value == profileItem.ID.ToString());
            if (profileNode == null)
            {
                profileNode = new XElement("profile",
                    new XAttribute("id", profileItem.ID.ToString()),
                    new XAttribute("name", profileItem.DisplayName),
                    new XAttribute("presets", $"{profileCardItem.DisplayName.ToLower()}|100||"));
                profileCardDoc.Root.Add(profileNode);
            }
            else if (profileNode.Attribute("presets") == null)
            {
                profileNode.Add(new XAttribute("presets", $"{profileCardItem.DisplayName.ToLower()}|100||"));
            }
            else 
            {
                profileNode.Attribute("presets").Value = $"{profileCardItem.DisplayName.ToLower()}|100||";
            }

            profileNode.RemoveNodes();
            foreach (var k in keys)
            {   
                profileNode.Add(new XElement("key", 
                    new XAttribute("name", k.Key), 
                    new XAttribute("value", k.Value)));
            }
                 
            return profileCardDoc.Root.ToString();
        }

        public string UpdateTrackingGoal(Item pageItem, Item goalItem)
        {
            //get the item's tracking field and append the new goal to it
            var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];

            XDocument xdoc = XDocument.Parse(trackingField.Value);
            if (!string.IsNullOrWhiteSpace(trackingField?.Value))
            {
                var events = xdoc.Root.Descendants("event");
                XElement eventNode = events.FirstOrDefault(a => a.Attribute("id").Value == goalItem.ID.ToString());
                if (eventNode == null)
                {
                    eventNode = new XElement("event",
                        new XAttribute("id", goalItem.ID.ToString()),
                        new XAttribute("name", goalItem.DisplayName));
                    xdoc.Root.Add(eventNode);
                }
            }

            return xdoc.Root.ToString();
        }

        public string GetMinValue(Item profileKeyItem)
        {
            var minValue = profileKeyItem.Fields["MinValue"].Value;

            return string.IsNullOrWhiteSpace(minValue) ? "0" : minValue;
        }

        public string GetMaxValue(Item profileKeyItem)
        {
            var maxValue = profileKeyItem.Fields["MaxValue"].Value;

            return maxValue;
        }

        public List<Item> GetGoals()
        {
            var goals = Sitecore.Context.Database.GetItem(Constants.ItemIds.GoalNodeId)
                .Axes
                .GetDescendants()
                .Where(a => a.TemplateID.Guid == Constants.TemplateIds.GoalTemplateId.Guid)
                .ToList();
            
            return goals;
        }

        public List<Item> GetProfiles()
        {
            var profiles = Sitecore.Context.Database.GetItem(Constants.ItemIds.ProfileNodeId)
                .Axes
                .GetDescendants()
                .Where(a => a.TemplateID.Guid == Constants.TemplateIds.ProfileTemplateId.Guid)
                .ToList();
            
            return profiles;
        }

        public List<Item> GetProfileCards(Item profileItem)
        {
            var profileCards = profileItem
                .Axes
                .GetDescendants()
                .Where(a => a.TemplateID.Guid.Equals(Constants.TemplateIds.ProfileCardTemplateId.Guid))
                .ToList();

            return profileCards;
        }

        public List<Item> GetPatternCards(Item profileItem)
        {
            var profiles = profileItem
                .Axes
                .GetDescendants()
                .Where(a => a.TemplateID.Guid.Equals(Constants.TemplateIds.PatternCardTemplateId.Guid))
                .ToList();

            return profiles;
        }

        public List<Item> GetAllProfileCards()
        {
            var profileCards = Sitecore.Context.Database.GetItem(Constants.ItemIds.ProfileNodeId)
                .Axes
                .GetDescendants()
                .Where(a => a.TemplateID.Guid == Constants.TemplateIds.ProfileCardTemplateId.Guid)
                .ToList();
            
            return profileCards;
        }

        public List<Item> GetAllPatternCards()
        {
            var patternCards = Sitecore.Context.Database.GetItem(Constants.ItemIds.ProfileNodeId)
                .Axes
                .GetDescendants()
                .Where(a => a.TemplateID.Guid == Constants.TemplateIds.PatternCardTemplateId.Guid)
                .ToList();
            
            return patternCards;
        }
    }
}