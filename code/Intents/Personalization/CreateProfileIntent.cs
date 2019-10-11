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
    public class CreateProfileIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - create profile";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateProfile.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string NameKey = "Profile Name";
        protected string ProfileKeysKey = "Profile Keys";
        
        #endregion

        public CreateProfileIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;

            ConversationParameters.Add(new StringParameter(NameKey, "Chat.Intents.CreateProfile.NameParameterRequest", inputFactory, resultFactory));
            ConversationParameters.Add(new StringParameter(ProfileKeysKey, "Chat.Intents.CreateProfile.KeysParameterRequest", inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var name = (string) conversation.Data[NameKey].Value;
            var keyString = (string) conversation.Data[ProfileKeysKey].Value;
            var keys = keyString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();

            var fields = new Dictionary<ID, string>
            {
                { Constants.FieldIds.Profile.NameFieldId, name },
                { Constants.FieldIds.Profile.TypeFieldId, "Sum" },
                { Constants.FieldIds.Profile.DecayFieldId, "0" },
            };
            
            //create goal and folder if needed
            var fromDb = "master";
            var profileNode = DataWrapper.GetItemById(Constants.ItemIds.ProfileNodeId, fromDb);

            //create the profile
            var newProfileItem = DataWrapper.CreateItemFromBranch(profileNode.ID, Constants.TemplateIds.ProfileTemplateBranchId, fromDb, name, fields);

            //create the profile keys
            foreach (var k in keys)
            {
                var keyFields = new Dictionary<ID, string>
                {
                    { Constants.FieldIds.ProfileKey.NameFieldId, k },     
                    { Constants.FieldIds.ProfileKey.MinValueFieldId, "0" },  
                    { Constants.FieldIds.ProfileKey.MaxValueFieldId, "10" },
                };

                var keyItem = DataWrapper.CreateItem(newProfileItem.ID, Constants.TemplateIds.ProfileKeyTemplateId, fromDb, k, keyFields);
            }

            //publish
            var toDb = DataWrapper.GetDatabase("web");
            PublishWrapper.PublishItem(profileNode, new[] { toDb }, new[] { DataWrapper.ContentLanguage }, true, false, false);

            
            return ConversationResponseFactory.Create(KeyName, string.Format(Translator.Text("Chat.Intents.CreateProfile.Response"), newProfileItem.DisplayName));
        }
    }
}