using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models
{
    public class ProfileKeyData
    {
        public ProfileKeyData(Guid profileId)
        {
            ProfileId = profileId;
            KeyValuePairs = new Dictionary<string, string>();
        }

        public Guid ProfileId { get; set; }
        public Dictionary<string, string> KeyValuePairs { get; set; } 
    }
}