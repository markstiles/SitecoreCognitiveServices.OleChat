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
using System.Text;
using System.Xml.Linq;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class CreateGoalIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;


        public override string Name => "create goal";

        public override string Description => Translator.Text("Chat.Intents.CreateGoal.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string NameKey = "Goal Name";
        protected string IsFailureKey = "Is Failure";
        protected string PointsKey = "Points";
        protected string PageItemKey = "Page Item";

        protected ID GoalNodeId = new ID("{0CB97A9F-CAFB-42A0-8BE1-89AB9AE32BD9}");
        protected ID GoalCategoryId = new ID("{DB6E13B8-786C-4DD6-ACF2-3E5E6A959905}");
        protected ID GoalId = new ID("{475E9026-333F-432D-A4DC-52E03B75CB6B}");
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
            ConversationParameters.Add(new YesOrNoParameter(IsFailureKey, Translator.Text("Chat.Intents.CreateGoal.IsFailureParameterRequest"), inputFactory, resultFactory));
            // rule parameter - ConversationParameters.Add(new YesOrNoParameter(RecursionKey, Translator.Text("Chat.Intents.CreateGoal.IsFailure"), inputFactory, resultFactory));
            ConversationParameters.Add(new ItemParameter(PageItemKey, dataWrapper, inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey];
            var points = (int) conversation.Data[PointsKey];
            var isFailure = (bool) conversation.Data[IsFailureKey];
            var pageItem = (Item)conversation.Data[PageItemKey];

            var fields = new Dictionary<ID, string>
            {
                { new ID("{AC3BC9B6-46A2-4EAD-AF5E-6BDB532EB832}"), "1" }, // IsGoal
                { new ID("{BD5D2A52-027F-4CC8-9606-C5CE6CBBF437}"), isFailure ? "1" : "" }, // IsFailure
                // { new ID("{71EBDEBD-9560-48C6-A66F-E17FC018232C}"), "" }, // Rule
                { new ID("{AC6BA888-4213-43BD-B787-D8DA2B6B881F}"), name },
                { new ID("{33AE0E84-74A0-437F-AB2B-859DFA96F6C9}"), points.ToString() },
                { Sitecore.FieldIDs.WorkflowState, "{EDCBB550-BED3-490F-82B8-7B2F14CCD26E}" } // workflow state
            };

            //create goal and folder if needed
            var fromDb = "master";
            var toDb = DataWrapper.GetDatabase("web");
            var goalItem = DataWrapper.GetItemById(GoalNodeId, fromDb);
            var folderName = "Custom Goals";
            var goalFolder = goalItem.Axes.GetChild(folderName);
            if(goalFolder == null)
                goalFolder = DataWrapper.CreateItem(GoalNodeId, GoalCategoryId, fromDb, folderName, new Dictionary<ID, string>());
            var newGoalItem = DataWrapper.CreateItem(goalFolder.ID, GoalId, fromDb, name, fields);

            PublishWrapper.PublishItem(goalFolder, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);

            //get the item's tracking field and append the new goal to it
            var trackingFieldId = new ID("{B0A67B2A-8B07-4E0B-8809-69F751709806}");
            var trackingField = pageItem.Fields[trackingFieldId];
            var newFieldValue = new StringBuilder("<tracking>");
            if (!string.IsNullOrWhiteSpace(trackingField?.Value))
            {
                XDocument xdoc = XDocument.Parse(trackingField.Value);
                var events = xdoc.Descendants("event");
                foreach(XElement e in events)
                {
                    newFieldValue.Append(e.ToString());
                }
            }
            newFieldValue.Append($"<event id=\"{newGoalItem.ID}\" name=\"{name}\" />");
            newFieldValue.Append("</tracking>");

            var pageFields = new Dictionary<ID, string>
            {
                { trackingFieldId, newFieldValue.ToString() } // tracking
            };
            
            DataWrapper.UpdateFields(pageItem, pageFields);
            PublishWrapper.PublishItem(goalFolder, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            
            return ConversationResponseFactory.Create(Name, string.Format(Translator.Text("Chat.Intents.CreateGoal.Response"), name));
        }
    }
}