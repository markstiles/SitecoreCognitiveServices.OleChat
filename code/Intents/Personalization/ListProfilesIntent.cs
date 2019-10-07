using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using System.Text;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class ListProfilesIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - list profiles";

        public override string DisplayName => Translator.Text("Chat.Intents.ListSegments.Name");

        public override bool RequiresConfirmation => false;
       
        public ListProfilesIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profiles = Sitecore.Context.Database.GetItem(Constants.ItemIds.ProfileNodeId)
                .Axes.GetDescendants()
                .Where(a => a.TemplateID == Constants.TemplateIds.ProfileTemplateId);

            var response = new StringBuilder();
            var profileList = string.Join(", ", profiles.Select(a => a.DisplayName));
            response.AppendFormat(Translator.Text("Chat.Intents.ListProfiles.Response"), profiles.Count(), profileList);

            return ConversationResponseFactory.Create(KeyName, response.ToString());
        }
    }
}