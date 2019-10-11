using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class CreateGoalIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;


        public override string KeyName => "personalization - create goal";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateGoal.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string NameKey = "Goal Name";
        //protected string IsFailureKey = "Is Failure";
        protected string PointsKey = "Points";
        protected string PageItemKey = "Page Item";
        
        protected List<string> SystemGoalIds = new List<string>
        {
            "{968897F1-328A-489D-88E8-BE78F4370958}", //create brochure
            "{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}", //google+ goal
            "{28A7C944-B8B6-45AD-A635-6F72E8F81F69}", //instant demo
            "{FE65DCC7-4A2A-4C31-BCA9-3BC9F1630373}", //like
            "{E971E2BF-06BE-4DC7-BFBE-5464A5E68326}", //linkedin
            "{66722F52-2D13-4DCC-90FC-EA7117CF2298}", //login
            "{1779CC42-EF7A-4C58-BF19-FA85D30755C9}", //newsletter signup
            "{8FFB183B-DA1A-4C74-8F3A-9729E9FCFF6A}", //register
            "{3B6F2891-22A2-4AD6-8033-3E3504F481B4}", //tweet
        };

        #endregion

        public CreateGoalIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper,
            IParameterResultFactory resultFactory) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new StringParameter(NameKey, Translator.Text("Chat.Intents.CreateGoal.NameParameterRequest"), inputFactory, resultFactory));
            ConversationParameters.Add(new IntegerParameter(PointsKey, Translator.Text("Chat.Intents.CreateGoal.PointsParameterRequest"), inputFactory, resultFactory));
            //ConversationParameters.Add(new YesOrNoParameter(IsFailureKey, Translator.Text("Chat.Intents.CreateGoal.IsFailureParameterRequest"), inputFactory, resultFactory));
            // rule parameter - ConversationParameters.Add(new YesOrNoParameter(RecursionKey, Translator.Text("Chat.Intents.CreateGoal.IsFailure"), inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey].Value;
            var points = (int) conversation.Data[PointsKey].Value;
            //var isFailure = (bool) conversation.Data[IsFailureKey];

            var fields = new Dictionary<ID, string>
            {
                { new ID("{AC3BC9B6-46A2-4EAD-AF5E-6BDB532EB832}"), "1" },                      // IsGoal
                // { new ID("{BD5D2A52-027F-4CC8-9606-C5CE6CBBF437}"), isFailure ? "1" : "" },  // IsFailure
                // { new ID("{71EBDEBD-9560-48C6-A66F-E17FC018232C}"), "" },                    // Rule
                { new ID("{AC6BA888-4213-43BD-B787-D8DA2B6B881F}"), name },                     // Name
                { new ID("{33AE0E84-74A0-437F-AB2B-859DFA96F6C9}"), points.ToString() },        // Points
                { Sitecore.FieldIDs.WorkflowState, "{EDCBB550-BED3-490F-82B8-7B2F14CCD26E}" }   // workflow state
            };

            //create goal and folder if needed
            var fromDb = "master";
            var toDb = DataWrapper.GetDatabase("web");
            var goalItem = DataWrapper.GetItemById(Constants.ItemIds.GoalNodeId, fromDb);
            var newGoalItem = DataWrapper.CreateItem(goalItem.ID, Constants.TemplateIds.GoalTemplateId, fromDb, name, fields);

            PublishWrapper.PublishItem(goalItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
                        
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.CreateGoal.Response"), name));
        }
    }
}