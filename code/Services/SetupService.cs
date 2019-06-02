using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Foundation.MSSDK;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;

namespace SitecoreCognitiveServices.Feature.OleChat.Services
{
    public class SetupService : ISetupService
    {
        #region Constructor

        protected readonly IMicrosoftCognitiveServicesApiKeys MSCSApiKeys;
        protected readonly ILuisService LuisService;
        protected readonly IOleSettings OleSettings;
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IContentSearchWrapper ContentSearch;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly HttpContextBase Context;
        protected readonly ISCSDKSettings SCSDKSettings;

        public SetupService(
            IMicrosoftCognitiveServicesApiKeys mscsApiKeys,
            ILuisService luisService,
            IOleSettings oleSettings,
            ISitecoreDataWrapper dataWrapper,
            IContentSearchWrapper contentSearch,
            IPublishWrapper publishWrapper,
            HttpContextBase context,
            ISCSDKSettings scsdkSettings)
        {
            MSCSApiKeys = mscsApiKeys;
            LuisService = luisService;
            OleSettings = oleSettings;
            Context = context;
            DataWrapper = dataWrapper;
            ContentSearch = contentSearch;
            PublishWrapper = publishWrapper;
            SCSDKSettings = scsdkSettings;
        }

        #endregion

        public void SaveKeys(string luisApi, string luisApiEndpoint)
        {
            //save items to fields
            if (MSCSApiKeys.Luis != luisApi)
                UpdateKey(SCSDKSettings.MSSDK_LuisFieldId, luisApi);
            if (MSCSApiKeys.LuisEndpoint != luisApiEndpoint)
                UpdateKey(SCSDKSettings.MSSDK_LuisEndpointFieldId, luisApiEndpoint);
        }

        public void UpdateKey(ID fieldId, string value)
        {
            var keyItem = DataWrapper?
                .GetDatabase(SCSDKSettings.MasterDatabase)
                .GetItem(SCSDKSettings.MSSDKId);
            DataWrapper.UpdateFields(keyItem, new Dictionary<ID, string>
            {
                { fieldId, value }
            });
        }

        public bool BackupOle()
        {
            var apps = LuisService.GetApplicationVersions(OleSettings.OleApplicationId).OrderByDescending(a => a.Version).ToList();
            if (!apps.Any())
                return false;
            
            var export = LuisService.ExportApplicationVersion(OleSettings.OleApplicationId, apps.First().Version);
            if (export == null)
                return false;

            Item folderItem = DataWrapper.GetDatabase(OleSettings.MasterDatabase).GetItem(OleSettings.OleSettingsFolderId);
            if (folderItem == null)
                return false;

            DataWrapper.UpdateFields(folderItem, new Dictionary<ID, string>
            {
                { OleSettings.ApplicationBackupFieldId, JsonConvert.SerializeObject(export) }        
            });

            return true;
        }

        public bool RestoreOle(bool overwrite)
        {
            var jsonText = OleSettings.ApplicationBackup;
            var appDefinition = JsonConvert.DeserializeObject<ApplicationDefinition>(jsonText);

            var infoResponse = LuisService.GetUserApplications().FirstOrDefault(a => a.Name.Equals(appDefinition.Name));
            bool shouldOverwrite = infoResponse != null && overwrite;
            bool isNoApp = infoResponse == null;
            bool hasAppId = !string.IsNullOrWhiteSpace(infoResponse?.Id);
            if (shouldOverwrite)
                LuisService.DeleteApplication(new Guid(infoResponse.Id));

            Guid appId;
            if (shouldOverwrite || isNoApp)
            {
                var importResponse = LuisService.ImportApplication(appDefinition, appDefinition.Name);
                if (!Guid.TryParse(importResponse, out appId))
                    return false;

                OleSettings.OleApplicationId = appId;
            }
            else if (OleSettings.OleApplicationId == Guid.Empty && hasAppId)
            {
                appId = Guid.Parse(infoResponse.Id);
                OleSettings.OleApplicationId = appId;
            }
            else
            {
                appId = OleSettings.OleApplicationId;
            }
                    
            LuisService.TrainApplicationVersion(appId, appDefinition.VersionId);
            int trainCount = 1;
            int loopCount = 0;
            var hasResponse = false;
            do
            {
                System.Threading.Thread.Sleep(1000);
                    
                var trainResponse = LuisService.GetApplicationVersionTrainingStatus(appId, appDefinition.VersionId);
                var statusList = trainResponse.Select(a => a.Details.Status).ToList();
                var anyFailed = statusList.Any(a => a.Equals("Fail"));
                var anyInProgress = statusList.Any(b => b.Equals("InProgress"));
                if (anyFailed)
                {
                    if (trainCount > 3)
                        return false;

                    LuisService.TrainApplicationVersion(appId, appDefinition.VersionId);
                    trainCount++;
                }
                else if (!anyInProgress) { 
                    hasResponse = true;
                }

                if (loopCount > 100)
                    return false;

                loopCount++;
            }
            while (!hasResponse);

            PublishRequest pr = new PublishRequest()
            {
                VersionId = appDefinition.VersionId,
                IsStaging = false,
                EndpointRegion = OleSettings.LuisPublishResource
            };
            var publishResponse = LuisService.PublishApplication(appId, pr);
            
            return true;
        }

        public bool QueryOle()
        {
            var response = LuisService.Query(OleSettings.OleApplicationId, OleSettings.TestMessage);
            if (response == null)
                return false;
            
            return true;
        }

        public void PublishOleContent()
        {
            //start at templates folder for yourself and core, and publish scs root in modules
            List<ID> itemGuids = new List<ID>() {
                OleSettings.SCSDKTemplatesFolderId,
                OleSettings.OleTemplatesFolderId,
                OleSettings.SCSModulesFolderId
            };
            
            Database fromDb = DataWrapper.GetDatabase(OleSettings.MasterDatabase);
            Database toDb = DataWrapper.GetDatabase(OleSettings.WebDatabase);
            foreach (var g in itemGuids)
            {
                var folder = fromDb.GetItem(g);

                PublishWrapper.PublishItem(folder, new[] { toDb }, new[] { folder.Language }, true, false, false);
            }

            Sitecore.Globalization.Translate.ResetCache(true);
        }
    }
}