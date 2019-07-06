using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
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
    public class ListDemographicTraitsIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - list demographic traits";

        public override string DisplayName => Translator.Text("Chat.Intents.ListDemographicTraits.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string ItemKey = "Demographic Feature";
        
        #endregion

        public ListDemographicTraitsIntent(
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
            ConversationParameters.Add(new ItemParameter(ItemKey, "What demographic feature do you want to know about?", parameters, dataWrapper, inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profileItem = (Item) conversation.Data[ItemKey];
            var profileKeys = profileItem.GetChildren().Where(a => a.TemplateID == Constants.TemplateIds.ProfileKeyTemplateId);

            var response = new StringBuilder();
            response.AppendFormat(Translator.Text("Chat.Intents.ListDemographicTraits.Response"), profileItem.DisplayName);
            foreach (var p in profileKeys)
            {
                response.Append($", {p.DisplayName}");
            }

            return ConversationResponseFactory.Create(KeyName, response.ToString());
        }
    }
}