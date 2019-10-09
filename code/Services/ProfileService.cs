using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public class ProfileService : IProfileService
    {

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

        public string GetMinValue(Item profileKeyItem)
        {
            var minValue = profileKeyItem.Fields["MinValue"].Value;

            return minValue;
        }

        public string GetMaxValue(Item profileKeyItem)
        {
            var maxValue = profileKeyItem.Fields["MaxValue"].Value;

            return maxValue;
        }
    }
}