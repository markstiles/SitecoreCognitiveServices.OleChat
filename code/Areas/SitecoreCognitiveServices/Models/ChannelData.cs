using System.Collections.Generic;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models
{
    public class ChannelData
    {
        public IntentInput Input { get; set; }
        public Dictionary<string, string> Selections { get; set; }
        public string Action { get; set; }
        public string AudioFile { get; set; }
    }
}