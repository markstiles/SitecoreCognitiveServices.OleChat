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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class AssignProfileCardIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - assign profile card";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateProfile.Name");

        public override bool RequiresConfirmation => true;

        #region Local Properties

        protected string ContentProfileItemKey = "Content Profile Item";
        protected string PageItemKey = "Page Item";
        
        #endregion

        public AssignProfileCardIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            var profileParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() }
            };
            ConversationParameters.Add(new ItemParameter(ContentProfileItemKey, "What content profile do you want to assign?", profileParameters, dataWrapper, inputFactory, resultFactory));
            var contentParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath }
            };
            ConversationParameters.Add(new ItemParameter(PageItemKey, "What page do you want to assign this profile to?", contentParameters, dataWrapper, inputFactory, resultFactory));
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var contentProfile = (Item) conversation.Data[ContentProfileItemKey].Value;
            var pageItem = (Item) conversation.Data[PageItemKey].Value;

            //<tracking>  
            //<profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus" presets="profile card|100||">    
            //<key name="Background" value="2" />    
            //<key name="Practical" value="1" />    
            //<key name="Process" value="5" />    
            //<key name="Scope" value="7" />  
            //</profile>
            //</tracking>

            //< tracking >
            //    < profile id = "{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name = "Focus" presets = "profile card|100||" >
            //        < key name = "Background" value = "2" />
            //        < key name = "Practical" value = "1" />
            //        < key name = "Process" value = "5" />
            //        < key name = "Scope" value = "7" />
            //    </ profile >
            //    <event id="{CC2CC47C-8CC8-469D-B8C6-C11F202FD20A}" name="Google Plus One" />
            //    <event id="{28A7C944-B8B6-45AD-A635-6F72E8F81F69}" name="Instant Demo" />
            //</tracking>

            //get the item's tracking field and append the new goal to it
            var trackingField = pageItem.Fields[Constants.FieldIds.StandardFields.TrackingFieldId];
            var newFieldValue = new StringBuilder("<tracking>");
            if (!string.IsNullOrWhiteSpace(trackingField?.Value))
            {
                XDocument xdoc = XDocument.Parse(trackingField.Value);
                //var events = xdoc.Descendants("event");
                //foreach (XElement e in events)
                //{
                    
                //}
            }
            newFieldValue.Append("</tracking>");

            var pageFields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.StandardFields.TrackingFieldId, newFieldValue.ToString() }
            };

            DataWrapper.UpdateFields(pageItem, pageFields);
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(pageItem, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);


            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.AssignContentProfile.Response"),
                contentProfile.DisplayName, pageItem.DisplayName));
        }
    }
}