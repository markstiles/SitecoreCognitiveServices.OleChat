using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public interface ISetupService
    {
        void SaveKeys(string luisApi, string luisApiEndpoint);
        void UpdateKey(ID fieldId, string value);
        bool BackupOle();
        bool RestoreOle(bool overwrite);
        bool QueryOle();
        void PublishOleContent();
    }
}