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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class CreateContentProfileIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - create content profile";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateContentProfile.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string NameKey = "Content Profile Name";
        protected string ItemKey = "Item";

        #endregion

        public CreateContentProfileIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new StringParameter(NameKey, "What is the name of this content profile", inputFactory, resultFactory));
            var parameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ProfilePath },
                { Constants.SearchParameters.TemplateId, Constants.TemplateIds.ProfileTemplateId.ToString() }
            };
            ConversationParameters.Add(new ItemParameter(ItemKey, "what demographic feature do you want to create a content profile for?", parameters, dataWrapper, inputFactory, resultFactory));
            //ask for the numeric value for each key in the profile 
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey].Value;
            var profileItem = (Item) conversation.Data[ItemKey].Value;

            var patternFieldValue = "";
            //<tracking>  
            //<profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus">    
            //<key name="Background" value="3" />    
            //<key name="Practical" value="5" />    
            //<key name="Process" value="5" />    
            //<key name="Scope" value="5" />  
            //</profile>
            //</tracking>

            var fields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.PatternCard.NameFieldId, name },    
                { Constants.FieldIds.PatternCard.PatternFieldId, patternFieldValue }    
            };

            //create pattern card
            var fromDb = "master";
            var patternCardFolder = profileItem.Axes.GetChild("Pattern Cards");
            var newProfileItem = DataWrapper.CreateItem(patternCardFolder.ID, Constants.TemplateIds.PatternCardTemplateId, fromDb, name, fields);

            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.CreateContentProfile.Response"),
                profileItem.DisplayName));
        }
    }
}