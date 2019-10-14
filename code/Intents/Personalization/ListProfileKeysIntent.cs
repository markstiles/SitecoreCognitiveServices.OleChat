using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;
using System.Text;
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class ListProfileKeysIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;

        public override string KeyName => "personalization - list profile keys";

        public override string DisplayName => Translator.Text("Chat.Intents.ListProfileKeys.Name");

        public override bool RequiresConfirmation => false;
        
        #region Local Properties

        protected string ItemKey = "Profile";
        
        #endregion

        public ListProfileKeysIntent(
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
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() },
                { Constants.SearchParameters.AutoStart, "true" }
            };
            ConversationParameters.Add(new ItemParameter(ItemKey, Translator.Text("Chat.Intents.ListProfileKeys.WhichProfile"), parameters, dataWrapper, inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profileItem = (Item) conversation.Data[ItemKey].Value;
            var profileKeys = ProfileService.GetProfileKeys(profileItem);

            var response = new StringBuilder();
            var profileKeyList = string.Join("", profileKeys.Select(a => $"<li>{a.DisplayName}</li>"));
            response.AppendFormat(Translator.Text("Chat.Intents.ListProfileKeys.Response"), profileKeys.Count(), profileItem.DisplayName, $"<ul>{profileKeyList}</ul>");
            
            return ConversationResponseFactory.Create(KeyName, response.ToString());
        }
    }
}