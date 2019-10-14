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
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class SetupPersonalizationIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;

        public override string KeyName => "personalization - setup";

        public override string DisplayName => Translator.Text("Chat.Intents.SetupPersonalization.Name");

        public override bool RequiresConfirmation => false;
        
        public SetupPersonalizationIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper,
            IProfileService profileService) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
            ProfileService = profileService;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profiles = ProfileService.GetProfiles(parameters.Database);
            var patternCards = ProfileService.GetAllPatternCards(parameters.Database);
            var profileCards = ProfileService.GetAllProfileCards(parameters.Database);

            var patternParents = patternCards.Select(a => a.Paths.ParentPath).Distinct().Count();
            var profileParents = profileCards.Select(a => a.Paths.ParentPath).Distinct().Count();

            var profilesWithoutPatterns = profiles.Count - patternParents;
            var profilesWithoutProfiles = profiles.Count - profileParents;

            var itemList = new List<ListItem>
            {
                new ListItem(string.Format(Translator.Text("Chat.Intents.SetupPersonalization.CreateProfile"), profiles.Count)), // profile
                new ListItem(string.Format(Translator.Text("Chat.Intents.SetupPersonalization.CreatePatternCard"), profilesWithoutPatterns)), // pattern card
                new ListItem(string.Format(Translator.Text("Chat.Intents.SetupPersonalization.CreateProfileCard"), profilesWithoutProfiles)), // profile card
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.AssignProfileCard")), // add profile card to page or search action
                new ListItem(string.Format(Translator.Text("Chat.Intents.SetupPersonalization.CreateGoal"), ProfileService.GetGoals(parameters.Database).Count)),
                new ListItem(Translator.Text("Chat.Intents.SetupPersonalization.AssignGoal"))
            };
            var intentList = IntentInputFactory.Create(IntentInputType.LinkList, itemList);

            return ConversationResponseFactory.Create(KeyName, Translator.Text("Chat.Intents.SetupPersonalization.ToSetup"), true, intentList);
        }
    }
}