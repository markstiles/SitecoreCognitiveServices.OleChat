using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;
using System.Xml.Linq;
using SitecoreCognitiveServices.Feature.OleChat.Services;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class AssignGoalIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        protected readonly IProfileService ProfileService;

        public override string KeyName => "personalization - assign goal";

        public override string DisplayName => Translator.Text("Chat.Intents.AssignGoal.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties
        
        protected string GoalItemKey = "Goal";
        protected string PageItemKey = "Page";
        
        #endregion

        public AssignGoalIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper,
            IParameterResultFactory resultFactory,
            IProfileService profileService) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
            ProfileService = profileService;

            var goalParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.GoalPath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.GoalTemplateId.ToString() },
                { Constants.SearchParameters.AutoStart, "true" }
            };
            ConversationParameters.Add(new ItemParameter(GoalItemKey, Translator.Text("Chat.Intents.AssignGoal.GoalParameterRequest"), goalParameters, dataWrapper, inputFactory, resultFactory));
            var contentParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath }
            };
            ConversationParameters.Add(new ItemParameter(PageItemKey, Translator.Text("Chat.Intents.AssignGoal.PageParameterRequest"), contentParameters, dataWrapper, inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var goalItem = (Item)conversation.Data[GoalItemKey].Value;
            var pageItem = (Item)conversation.Data[PageItemKey].Value;

            var newTrackingValue = ProfileService.UpdateTrackingGoal(pageItem, goalItem);

            var pageFields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.StandardFields.TrackingFieldId, newTrackingValue }
            };
            
            DataWrapper.UpdateFields(pageItem, pageFields);
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.AssignGoal.Response"), goalItem.DisplayName, pageItem.DisplayName));
        }
    }
}