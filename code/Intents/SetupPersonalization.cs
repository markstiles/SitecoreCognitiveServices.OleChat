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
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class SetupPersonalizationIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - setup";

        public override string DisplayName => Translator.Text("Chat.Intents.SetupPersonalization.Name");

        public override bool RequiresConfirmation => false;
        
        public SetupPersonalizationIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var itemList = new List<ListItem>
            {
                new ListItem("Create a goal"),
                new ListItem("Assign a goal"),
                new ListItem("Create a persona")
            };
            var intentList = IntentInputFactory.Create(IntentInputType.LinkList, itemList);

            return ConversationResponseFactory.Create(KeyName, "To setup personalization, you'll need to do the following:", true, intentList);
        }
    }
}