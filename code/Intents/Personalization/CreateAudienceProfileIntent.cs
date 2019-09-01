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
    public class CreateAudienceProfileIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - create audience profile";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateAudienceProfile.Name");

        public override bool RequiresConfirmation => true;

        #region Local Properties

        protected string NameKey = "Target Audience Name";
        protected string ItemKey = "Item ";
        
        #endregion

        public CreateAudienceProfileIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new StringParameter(NameKey, "What is the name of this target audience?", inputFactory, resultFactory));
            var contentParameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath }
            };
            ConversationParameters.Add(new ItemParameter(ItemKey, "What demographic feature do you want to create and audience profile for?", contentParameters, dataWrapper, inputFactory, resultFactory));
            //ask for the numeric value for each key in the profile 
        }

        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey].Value;
            var profileItem = (Item) conversation.Data[ItemKey].Value;

            // profile card value field
            var profileCardValue = "";
            //<tracking>  
            //<profile id="{24DFF2CF-B30A-4B75-8967-2FE3DED82271}" name="Focus">    
            //<key name="Background" value="2" />    
            //<key name="Practical" value="1" />    
            //<key name="Process" value="5" />    
            //<key name="Scope" value="7" />  
            //</profile>
            //</tracking>

            var fields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.ProfileCard.NameFieldId, name },
                { Constants.FieldIds.ProfileCard.ProfileCardValueFieldId, profileCardValue }
            };
            
            //create profile card
            var fromDb = "master";
            var profileCardFolder = profileItem.Axes.GetChild("Profile Cards");
            var newProfileItem = DataWrapper.CreateItem(profileCardFolder.ID, Constants.TemplateIds.ProfileCardTemplateId, fromDb, name, fields);

            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.CreateAudienceProfile.Response"),
                profileItem.DisplayName));
        }
    }
}