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
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;
using System.Text;
using System.Xml.Linq;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class AssignGoalIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;


        public override string KeyName => "personalization - assign goal";

        public override string DisplayName => Translator.Text("Chat.Intents.AssignGoal.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties
        
        protected string GoalItemKey = "Goal Item";
        protected string PageItemKey = "Page Item";
        
        #endregion

        public AssignGoalIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper,
            IParameterResultFactory resultFactory) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new ItemParameter(GoalItemKey, "What goal do you want to assign?", dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new ItemParameter(PageItemKey, "What page do you want to assign to?", dataWrapper, inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var goalItem = (Item)conversation.Data[GoalItemKey];
            var pageItem = (Item)conversation.Data[PageItemKey];
            
            //todo make sure this doesn't lose any tracking field data

            //get the item's tracking field and append the new goal to it
            var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];
            var newFieldValue = new StringBuilder("<tracking>");
            if (!string.IsNullOrWhiteSpace(trackingField?.Value))
            {
                XDocument xdoc = XDocument.Parse(trackingField.Value);
                var events = xdoc.Descendants("event");
                foreach(XElement e in events)
                {
                    if(e.Attribute("id").Value != goalItem.ID.ToString())
                        newFieldValue.Append(e.ToString());
                }
            }
            newFieldValue.Append($"<event id=\"{goalItem.ID}\" name=\"{goalItem.DisplayName}\" />");
            newFieldValue.Append("</tracking>");

            var pageFields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.StandardFields.TrackingFieldId, newFieldValue.ToString() }
            };
            
            DataWrapper.UpdateFields(pageItem, pageFields);
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.AssignGoal.Response"), goalItem.DisplayName, pageItem.DisplayName));
        }
    }
}