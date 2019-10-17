using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using Sitecore.Data.Managers;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;
using System.Text;
using System.Xml.Linq;
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class AssignProfileCardIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;
        protected readonly ISearchService SearchService;

        public override string KeyName => "personalization - assign profile card";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateProfile.Name");

        public override bool RequiresConfirmation => true;

        #region Local Properties

        protected string ProfileCardItemKey = "Profile Card";
        protected string ItemNameKey = "Item Name";
        protected string TemplateItemKey = "Template Item";
        protected string FolderItemKey = "Folder Item";
        protected string FieldItemKey = "Field Item";
        
        #endregion

        public AssignProfileCardIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper,
            IProfileService profileService,
            ISearchService searchService) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
            ProfileService = profileService;
            SearchService = searchService;

            var profileParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileCardTemplateId.ToString() },
                { Constants.SearchParameters.AutoStart, "true" }
            };
            ConversationParameters.Add(new ItemParameter(ProfileCardItemKey, "What profile card do you want to assign?", profileParameters, dataWrapper, inputFactory, resultFactory));

            var nameParameter = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath },
            };
            var nameParam = new ItemParameter(ItemNameKey, "What item do you want to filter by?", nameParameter, dataWrapper, inputFactory, resultFactory);
            nameParam.IsOptional = true;
            ConversationParameters.Add(nameParam);

            var templateParameter = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.TemplatePath },
            };
            var templateParam = new ItemParameter(TemplateItemKey, "What template do you want to filter by?", templateParameter, dataWrapper, inputFactory, resultFactory);
            templateParam.IsOptional = true;
            ConversationParameters.Add(templateParam);

            var folderParameter = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath },
            };
            var folderParam = new ItemParameter(FolderItemKey, "What folder do you want to filter by?", folderParameter, dataWrapper, inputFactory, resultFactory);
            folderParam.IsOptional = true;
            ConversationParameters.Add(folderParam);

            var fieldParam = new KeyValueParameter(FieldItemKey, "What field do you want to filter by?", "What Value in that field?", inputFactory, resultFactory);
            fieldParam.IsOptional = true;
            ConversationParameters.Add(fieldParam);
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profileCardItem = (Item)conversation.Data[ProfileCardItemKey].Value;
            var profileItem = ProfileService.GetProfileItem(profileCardItem);
            
            //do search
            var searchParameters = new Dictionary<string, string>();

            var namedItem = conversation.Data[ItemNameKey].Value;
            if (namedItem != null && namedItem is Item)
                searchParameters.Add(Constants.SearchParameters.ItemName, ((Item)namedItem).DisplayName);

            var templateItem = conversation.Data[TemplateItemKey].Value;
            if (templateItem != null && templateItem is Item)
                searchParameters.Add(Constants.SearchParameters.TemplateId, ((Item)templateItem).ID.ToString());

            var folderItem = conversation.Data[FolderItemKey].Value;
            var folderId = (folderItem != null && folderItem is Item)
                ? ((Item)templateItem).ID.ToString()
                : Constants.Paths.ContentPath;
            searchParameters.Add(Constants.SearchParameters.TemplateId, folderId);

            var fieldSet = conversation.Data[FieldItemKey].Value;
            if (fieldSet != null && fieldSet is Item)
            {
                var kvp = (KeyValuePair<string, string>)fieldSet;
                searchParameters.Add(Constants.SearchParameters.FieldName, kvp.Key);
                searchParameters.Add(Constants.SearchParameters.FieldValue, kvp.Value);
            }

            var results = SearchService.GetResults(parameters.Database, parameters.Language, "", searchParameters, 0, -1);
            if (results.Count < 1)
            {
                conversation.IsEnded = true;
                ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.AssignContentProfile.FailedResponse"),
                profileCardItem.DisplayName, results.Count));
            }

            //get the item's tracking field and append the new goal to it
            foreach (var r in results)
            {
                var pageItem = r.GetItem();
                var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];
                var parseValue = string.IsNullOrWhiteSpace(trackingField.Value) ? "<tracking></tracking>" : trackingField.Value;
                XDocument xdoc = XDocument.Parse(parseValue);
                if (!string.IsNullOrWhiteSpace(trackingField?.Value))
                {
                    var profile = xdoc.Root.Descendants("profile");
                    XElement profileNode = profile.FirstOrDefault(a => a.Attribute("id").Value == profileItem.ID.ToString());
                    if (profileNode == null)
                    {
                        profileNode = new XElement("profile",
                            new XAttribute("id", profileCardItem.ID.ToString()),
                            new XAttribute("name", profileCardItem.DisplayName),
                            new XAttribute("presets", $"{profileCardItem.DisplayName}|100||"));
                        xdoc.Root.Add(profileNode);
                    }

                    var profileCardValueField = profileCardItem.Fields[Constants.FieldIds.ProfileCard.ProfileCardValueFieldId];
                    XDocument.Parse(profileCardValueField.Value);
                    if (!string.IsNullOrWhiteSpace(profileCardValueField?.Value))
                    {
                        var cardProfile = xdoc.Root.Descendants("profile");
                        XElement cardProfileNode = profile.FirstOrDefault(a => a.Attribute("id").Value == profileItem.ID.ToString());
                        var keys = cardProfileNode.Descendants("key");
                        foreach (var k in keys)
                        {
                            profileNode.Add(k);
                        }
                    }
                }


                /* added 2 profiles
                 <tracking>  
                     <event id="{4B518240-1A88-4A9D-B71A-1C21BE173060}" name="Download brochure" />  
                     <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />  
                     <profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus" presets="profile card 2|100||">    
                         <key name="Background" value="6" />    
                         <key name="Practical" value="5" />    
                         <key name="Process" value="0" />    
                         <key name="Scope" value="6" />  
                     </profile>  
                     <profile id="{BA06B827-C6F2-4748-BD75-AA178B770E83}" name="Function" presets="profile card 3|100||">    
                         <key name="Building Trust" value="4" />    
                         <key name="Call to Action" value="0" />    
                         <key name="Create Desire" value="0" />    
                         <key name="Define Concept" value="7" />  
                     </profile>
                 </tracking>
                 */

                    /* add 2 profiles and customized function
                    <tracking>  <event id="{4B518240-1A88-4A9D-B71A-1C21BE173060}" name="Download brochure" />  
                        <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />  
                        <profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus" presets="profile card 2|100||">    
                            <key name="Background" value="6" />    
                            <key name="Practical" value="5" />    
                            <key name="Process" value="0" />    
                            <key name="Scope" value="6" />  
                        </profile>  
                        <profile id="{BA06B827-C6F2-4748-BD75-AA178B770E83}" name="Function">    
                            <key name="Building Trust" value="4" />    
                            <key name="Call to Action" value="1" />    
                            <key name="Create Desire" value="0" />    
                            <key name="Define Concept" value="7" />  
                        </profile>
                    </tracking>
                    */

                    // added 1 profile
                    //< tracking >
                    //    < profile id = "{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name = "Focus" presets = "profile card|100||" >
                    //        < key name = "Background" value = "2" />
                    //        < key name = "Practical" value = "1" />
                    //        < key name = "Process" value = "5" />
                    //        < key name = "Scope" value = "7" />
                    //    </ profile >
                    //    <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />
                    //    <event id="{28A7C944-B8B6-45AD-A635-6F72E8F81F69}" name="Instant Demo" />
                    //</tracking>

                var pageFields = new Dictionary<ID, string>
                {
                    { Constants.FieldIds.StandardFields.TrackingFieldId, xdoc.Root.ToString() }
                };

                DataWrapper.UpdateFields(pageItem, pageFields);
                var toDb = DataWrapper.GetDatabase("web");
                PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            }
            
            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.AssignContentProfile.Response"),
                profileCardItem.DisplayName, results.Count));
        }
    }
}