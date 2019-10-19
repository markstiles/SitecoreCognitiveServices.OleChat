using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;
using SitecoreCognitiveServices.Feature.OleChat.Services;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class CreatePatternCardIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;
        
        public override string KeyName => "personalization - create pattern card";

        public override string DisplayName => Translator.Text("Chat.Intents.CreatePatternCard.Name");

        public override bool RequiresConfirmation => true;

        #region Local Properties

        protected string NameKey = "Target Audience Name";
        protected string ProfileItemKey = "Profile";
        protected string ProfileKeyValuesKey = "Profile Key Values";

        #endregion

        public CreatePatternCardIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper,
            IProfileService profileService) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
            ProfileService = profileService;

            var contentParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() },
                { Constants.SearchParameters.AutoStart, "true" }
            };
            ConversationParameters.Add(new ItemParameter(ProfileItemKey, Translator.Text("Chat.Intents.CreatePatternCard.ProfileItemParameterRequest"), contentParameters, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new StringParameter(NameKey, Translator.Text("Chat.Intents.CreatePatternCard.ProfileNameParameterRequest"), inputFactory, resultFactory));
            ConversationParameters.Add(new ProfileKeysParameter(ProfileItemKey, ProfileKeyValuesKey, Translator.Text("Chat.Intents.CreatePatternCard.ProfileKeyParameterRequest"), inputFactory, resultFactory, profileService));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey].Value;
            var profileItem = (Item) conversation.Data[ProfileItemKey].Value;
            var profileKeyData = (Dictionary<string, string>)conversation.Data[ProfileKeyValuesKey].Value;
            var keys = ProfileService.GetProfileKeys(profileItem);

            // pattern card value field
            var patternCardValue = ProfileService.GetTrackingFieldValue(profileItem, profileKeyData);
            
            var fields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.PatternCard.NameFieldId, name },
                { Constants.FieldIds.PatternCard.PatternFieldId, patternCardValue }
            };
            
            //create profile card
            var fromDb = "master";
            var patternCardFolder = profileItem.Axes.GetChild("Pattern Cards");
            var newPatternCardItem = DataWrapper.CreateItem(patternCardFolder.ID, Constants.TemplateIds.PatternCardTemplateId, fromDb, name, fields);

            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.CreatePatternCard.Response"),
                profileItem.DisplayName));
        }
    }
}