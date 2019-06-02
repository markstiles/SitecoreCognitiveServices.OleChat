using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Foundation.MSSDK;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;

namespace SitecoreCognitiveServices.Feature.OleChat {
    public class OleSettings : IOleSettings
    {
        protected readonly IMicrosoftCognitiveServicesApiKeys MSApiKeys;
        protected ISitecoreDataWrapper DataWrapper;

        public OleSettings(IMicrosoftCognitiveServicesApiKeys msApiKeys, ISitecoreDataWrapper dataWrapper)
        {
            MSApiKeys = msApiKeys;
            DataWrapper = dataWrapper;
        }

        public virtual Guid OleApplicationId
        {
            get
            {
                Item folderItem = DataWrapper.GetDatabase(MasterDatabase).GetItem(OleSettingsFolderId);
                if (folderItem == null)
                    return Guid.Empty;

                Field f = folderItem.Fields[OleAppIdFieldId];
                if (f == null)
                    return Guid.Empty;

                Guid returnObj;
                return (Guid.TryParse(f.Value, out returnObj))
                    ? returnObj
                    : Guid.Empty;
            }
            set
            {
                var settingsItem = DataWrapper.GetDatabase(MasterDatabase).GetItem(OleSettingsFolderId);
                if (settingsItem == null)
                    return;

                using (new EditContext(settingsItem, true, false))
                {
                    settingsItem.Fields.ReadAll();
                    settingsItem.Fields[OleAppIdFieldId].Value = value.ToString();
                }
            }
        }
        
        public virtual string ApplicationBackup
        {
            get
            {
                Item folderItem = DataWrapper.GetDatabase(MasterDatabase).GetItem(OleSettingsFolderId);
                Field f = folderItem?.Fields[ApplicationBackupFieldId];
                return (f == null)
                    ? string.Empty
                    : f.Value;
            }
        }

        #region Globally Shared Settings

        public virtual string CoreDatabase => Settings.GetSetting("CognitiveService.CoreDatabase");
        public virtual string MasterDatabase => Settings.GetSetting("CognitiveService.MasterDatabase");
        public virtual string WebDatabase => Settings.GetSetting("CognitiveService.WebDatabase");
        public virtual ID SCSDKTemplatesFolderId => new ID(Settings.GetSetting("CognitiveService.SCSDKTemplatesFolder"));
        public virtual ID SCSModulesFolderId => new ID(Settings.GetSetting("CognitiveService.SCSModulesFolder"));

        #endregion

        public virtual string DictionaryDomain => Settings.GetSetting("CognitiveService.OleChat.DictionaryDomain");
        public virtual ID OleSettingsFolderId => new ID(Settings.GetSetting("CognitiveService.OleChat.OleSettingsFolder"));
        public virtual ID OleTemplatesFolderId => new ID(Settings.GetSetting("CognitiveService.OleChat.OleTemplatesFolder"));
        public virtual ID OleAppIdFieldId => new ID(Settings.GetSetting("CognitiveService.OleChat.OleAppIdFieldId"));
        public virtual ID ApplicationBackupFieldId => new ID(Settings.GetSetting("CognitiveService.OleChat.ApplicationBackupFieldId"));
        public virtual string TestMessage => Settings.GetSetting("CognitiveService.OleChat.TestMessage");

        public virtual string LuisPublishResource
        {
            get
            {
                var r = new Regex("https://([a-zA-Z]+).api.cognitive.microsoft.com/luis/");
                var m = r.Match(MSApiKeys.LuisEndpoint);

                return (m.Groups.Count > 0) ? m.Groups[1].Value : "";
            }
        }

        public virtual string SpeechRegion
        {
            get
            {
                var r = new Regex("https://([a-zA-Z]+).tts.speech.microsoft.com/cognitiveservices/v1/");
                var m = r.Match(MSApiKeys.SpeechEndpoint);

                return (m.Groups.Count > 0) ? m.Groups[1].Value : "";
            }
        }

        public bool HasNoValue(string str)
        {
            return string.IsNullOrWhiteSpace(str.Trim());
        }
        public bool MissingKeys()
        {
            if (MSApiKeys == null)
                return true;

            return HasNoValue(MSApiKeys.LuisEndpoint)
                   || HasNoValue(MSApiKeys.Luis)
                   || HasNoValue(MSApiKeys.TextAnalyticsEndpoint)
                   || HasNoValue(MSApiKeys.TextAnalytics);
        }
    }
}