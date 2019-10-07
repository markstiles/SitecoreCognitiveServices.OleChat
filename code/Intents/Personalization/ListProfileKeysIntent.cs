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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class ListProfileKeysIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
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
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            var parameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() }
            };
            ConversationParameters.Add(new ItemParameter(ItemKey, Translator.Text("Chat.Intents.ListProfileKeys.WhichProfile"), parameters, dataWrapper, inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profileItem = (Item) conversation.Data[ItemKey].Value;
            var profileKeys = profileItem.GetChildren().Where(a => a.TemplateID == Constants.TemplateIds.ProfileKeyTemplateId);

            var response = new StringBuilder();
            var profileKeyList = string.Join(", ", profileKeys.Select(a => a.DisplayName));
            response.AppendFormat(Translator.Text("Chat.Intents.ListProfileKeys.Response"), profileKeys.Count(), profileItem.DisplayName, profileKeyList);

            return ConversationResponseFactory.Create(KeyName, response.ToString());
        }
    }
}