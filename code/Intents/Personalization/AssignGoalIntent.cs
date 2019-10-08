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
        
        protected string GoalItemKey = "Goal";
        protected string PageItemKey = "Page";
        
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

            var goalParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.GoalPath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.GoalTemplateId.ToString() }
            };
            ConversationParameters.Add(new ItemParameter(GoalItemKey, "Chat.Intents.AssignGoal.GoalParameterRequest", goalParameters, dataWrapper, inputFactory, resultFactory));
            var contentParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath }
            };
            ConversationParameters.Add(new ItemParameter(PageItemKey, "Chat.Intents.AssignGoal.PageParameterRequest", contentParameters, dataWrapper, inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var goalItem = (Item)conversation.Data[GoalItemKey].Value;
            var pageItem = (Item)conversation.Data[PageItemKey].Value;

            //get the item's tracking field and append the new goal to it
            var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];
            XDocument xdoc = XDocument.Parse(trackingField.Value);
            if (!string.IsNullOrWhiteSpace(trackingField?.Value))
            {
                var events = xdoc.Root.Descendants("event");
                XElement eventNode = events.FirstOrDefault(a => a.Attribute("id").Value != goalItem.ID.ToString());
                if(eventNode == null) { 
                    eventNode = new XElement("event", 
                        new XAttribute("id", goalItem.ID.ToString()), 
                        new XAttribute("name", goalItem.DisplayName));
                }
                xdoc.Root.Add(eventNode);
            }

            var pageFields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.StandardFields.TrackingFieldId, xdoc.Root.ToString() }
            };
            
            DataWrapper.UpdateFields(pageItem, pageFields);
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);
            
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.AssignGoal.Response"), goalItem.DisplayName, pageItem.DisplayName));
        }
    }
}