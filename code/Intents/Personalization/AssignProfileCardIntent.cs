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

        public override string DisplayName => Translator.Text("Chat.Intents.AssignProfileCard.Name");

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
            ConversationParameters.Add(new ItemParameter(ProfileCardItemKey, Translator.Text("Chat.Intents.AssignProfileCard.ProfileCardItemParameterRequest"), profileParameters, dataWrapper, inputFactory, resultFactory));

            var nameParameter = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath },
            };
            var nameParam = new ItemParameter(ItemNameKey, Translator.Text("Chat.Intents.AssignProfileCard.ItemNameParameterRequest"), nameParameter, dataWrapper, inputFactory, resultFactory);
            nameParam.IsOptional = true;
            ConversationParameters.Add(nameParam);

            var templateParameter = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.TemplatePath },
            };
            var templateParam = new ItemParameter(TemplateItemKey, Translator.Text("Chat.Intents.AssignProfileCard.TemplateItemParameterRequest"), templateParameter, dataWrapper, inputFactory, resultFactory);
            templateParam.IsOptional = true;
            ConversationParameters.Add(templateParam);

            var folderParameter = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath },
            };
            var folderParam = new ItemParameter(FolderItemKey, Translator.Text("Chat.Intents.AssignProfileCard.FolderItemParameterRequest"), folderParameter, dataWrapper, inputFactory, resultFactory);
            folderParam.IsOptional = true;
            ConversationParameters.Add(folderParam);

            var fieldParam = new KeyValueParameter(FieldItemKey, Translator.Text("Chat.Intents.AssignProfileCard.FieldNameParameterRequest"), Translator.Text("Chat.Intents.AssignProfileCard.FieldValueParameterRequest"), inputFactory, resultFactory);
            fieldParam.IsOptional = true;
            ConversationParameters.Add(fieldParam);
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profileCardItem = (Item)conversation.Data[ProfileCardItemKey].Value;
            
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
            searchParameters.Add(Constants.SearchParameters.FilterPath, folderId);

            var fieldSet = conversation.Data[FieldItemKey].Value;
            if (fieldSet != null && fieldSet is ListItem)
            {
                var kvp = (ListItem)fieldSet;
                searchParameters.Add(Constants.SearchParameters.FieldName, kvp.Text);
                searchParameters.Add(Constants.SearchParameters.FieldValue, kvp.Value);
            }

            var results = SearchService.GetResults(parameters.Database, parameters.Language, "", searchParameters, 0, -1);
            if (results.Count < 1)
            {
                conversation.IsEnded = true;
                ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.AssignProfileCard.FailedResponse"),
                profileCardItem.DisplayName, results.Count));
            }

            //get the item's tracking field and append the new goal to it
            foreach (var r in results)
            {
                var pageItem = r.GetItem();
                var newTrackingValue = ProfileService.UpdateTrackingProfile(pageItem, profileCardItem);

                var pageFields = new Dictionary<ID, string>
                {
                    { Constants.FieldIds.StandardFields.TrackingFieldId, newTrackingValue }
                };

                DataWrapper.UpdateFields(pageItem, pageFields);
                var toDb = DataWrapper.GetDatabase("web");
                PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            }
            
            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.AssignProfileCard.Response"),
                profileCardItem.DisplayName, results.Count));
        }
    }
}