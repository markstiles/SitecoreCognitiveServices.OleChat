using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
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
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.CreateProfile")), // profile
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.CreatePatternCard")), // pattern card
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.CreateProfileCard")), // profile card
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.AssignProfileCard")), // add profile card to page or search action
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.CreateGoal")),
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.AssignGoal"))
            };
            var intentList = IntentInputFactory.Create(IntentInputType.LinkList, itemList);

            return ConversationResponseFactory.Create(KeyName, Translator.Text("Chat.Intents.SetupPersonalization.ToSetup"), true, intentList);
        }
    }
}