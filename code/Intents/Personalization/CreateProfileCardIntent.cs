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
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class CreateProfileCardIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;

        public override string KeyName => "personalization - create profile card";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateProfileCard.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string NameKey = "Content Profile Name";
        protected string ProfileItemKey = "Profile";
        protected string ProfileKeyValuesKey = "Profile Key Values";

        #endregion

        public CreateProfileCardIntent(
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

            var parameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() }
            };
            ConversationParameters.Add(new ItemParameter(ProfileItemKey, "What profile do you want to create this profile card for", parameters, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new StringParameter(NameKey, "What is the name of this profile card", inputFactory, resultFactory));
            ConversationParameters.Add(new ProfileKeysParameter(ProfileItemKey, ProfileKeyValuesKey, "What value do you want for {0} ({1} - {2})", inputFactory, resultFactory, profileService));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string)conversation.Data[NameKey].Value;
            var profileItem = (Item)conversation.Data[ProfileItemKey].Value;
            var profileKeyData = (Dictionary<string, string>)conversation.Data[ProfileKeyValuesKey].Value;
            var keys = ProfileService.GetProfileKeys(profileItem);

            // profile card value field
            var profileCardValue = ProfileService.GetTrackingFieldValue(profileItem, profileKeyData);

            var fields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.PatternCard.NameFieldId, name },
                { Constants.FieldIds.ProfileCard.ProfileCardValueFieldId, profileCardValue }
            };
            
            //create pattern card
            var fromDb = "master";
            var patternCardFolder = profileItem.Axes.GetChild("Pattern Cards");
            var newProfileItem = DataWrapper.CreateItem(patternCardFolder.ID, Constants.TemplateIds.PatternCardTemplateId, fromDb, name, fields);

            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.CreateProfileCard.Response"),
                profileItem.DisplayName));
        }
    }
}