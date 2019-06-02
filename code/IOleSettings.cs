using System;
using Sitecore.Data;

namespace SitecoreCognitiveServices.Feature.OleChat {
    public interface IOleSettings {
        Guid OleApplicationId { get; set; }
        string ApplicationBackup { get; }
        string CoreDatabase { get; }
        string MasterDatabase { get; }
        string WebDatabase { get; }
        ID SCSDKTemplatesFolderId { get; }
        ID SCSModulesFolderId { get; }
        string DictionaryDomain { get; }
        ID OleSettingsFolderId { get; }
        ID OleTemplatesFolderId { get; }
        ID OleAppIdFieldId { get; }
        ID ApplicationBackupFieldId { get; }
        string TestMessage { get; }
        string LuisPublishResource { get; }
        string SpeechRegion { get; }
        bool HasNoValue(string str);
        bool MissingKeys();
    }
}