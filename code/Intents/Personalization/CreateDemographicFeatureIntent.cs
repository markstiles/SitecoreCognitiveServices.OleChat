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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class CreateDemographicFeatureIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - create demographic feature";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateDemographicFeature.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string NameKey = "Demographic Feature Name";
        protected string TraitsKey = "Feature Traits";
        
        #endregion

        public CreateDemographicFeatureIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new StringParameter(NameKey, "What do you want to name this demographic feature?", inputFactory, resultFactory));
            ConversationParameters.Add(new StringParameter(TraitsKey, "What are the traits of this feature? (comma separated)", inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey];
            var traits = (List<string>) conversation.Data[TraitsKey];

            var fields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.Profile.NameFieldId, name },
                { Constants.FieldIds.Profile.TypeFieldId, "Sum" },
                { Constants.FieldIds.Profile.DecayFieldId, "0" },
            };
            
            //create goal and folder if needed
            var fromDb = "master";
            var profileNode = DataWrapper.GetItemById(Constants.ItemIds.ProfileNodeId, fromDb);
            var folderName = "User Defined";
            var profileFolder = profileNode.Axes.GetChild(folderName);
            if (profileFolder == null)
                profileFolder = DataWrapper.CreateItem(Constants.ItemIds.ProfileNodeId, Constants.TemplateIds.FolderTemplateId, fromDb, folderName, new Dictionary<ID, string>());

            //create the profile
            var newProfileItem = DataWrapper.CreateItem(profileFolder.ID, Constants.TemplateIds.ProfileTemplateId, fromDb, name, fields);

            //create the profile keys
            foreach (var t in traits)
            {
                var traitFields = new Dictionary<ID, string>
                {
                    { Constants.FieldIds.ProfileKey.NameFieldId, name },     
                    { Constants.FieldIds.ProfileKey.MinValueFieldId, "0" },  
                    { Constants.FieldIds.ProfileKey.MaxValueFieldId, "110" },
                };

                var traitItem = DataWrapper.CreateItem(newProfileItem.ID, Constants.TemplateIds.ProfileKeyTemplateId, fromDb, t, traitFields);
            }

            //publish
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(profileFolder, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);

            
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.CreateDemographicFeature.Response"), newProfileItem.DisplayName));
        }
    }
}