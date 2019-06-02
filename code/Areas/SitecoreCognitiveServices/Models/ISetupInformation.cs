using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models
{
    public interface ISetupInformation
    {
        string LuisApiKey { get; set; }
        string LuisApiEndpoint { get; set; }
        string TextAnalyticsApiKey { get; set; }
        string TextAnalyticsApiEndpoint { get; set; }
    }
}