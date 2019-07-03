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

        protected string DBKey = "Database Name";
        protected string ItemKey = "Item Path";
        protected string LangKey = "Language";
        protected string RecursionKey = "Descendants";
        protected string RelatedKey = "Related";

        #endregion

        public CreateProfileIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var toDb = (Database) conversation.Data[DBKey];
            var rootItem = (Item) conversation.Data[ItemKey];
            var langItem = (Language) conversation.Data[LangKey];
            var recursion = (string) conversation.Data[RecursionKey];
            var related = (string) conversation.Data[RelatedKey];
            PublishWrapper.PublishItem(rootItem, new[] { toDb }, new[] { langItem }, recursion.Equals("y"), false, related.Equals("y"));

            var recursionMessage = recursion.Equals("y") 
                ? Translator.Text("Chat.Intents.CreateProfile.ResponseRecursion") 
                : string.Empty;

            var relatedMessage = related.Equals("y")
                ? Translator.Text("Chat.Intents.CreateProfile.ResponseRelated")
                : string.Empty;

            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.CreateProfile.Response"), 
                rootItem.DisplayName, 
                toDb.Name, 
                LanguageManager.GetLanguageItem(langItem, toDb).DisplayName, 
                recursionMessage, 
                relatedMessage));
        }
    }
}