using System;
using System.Collections;
using System.Collections.Generic;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using Sitecore.Data.Managers;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents
{
    public class PublishIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string Name => "publish";

        public override string Description => Translator.Text("Chat.Intents.Publish.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string DBKey = "Database Name";
        protected string ItemKey = "Item Path";
        protected string LangKey = "Language";
        protected string RecursionKey = "Descendants";
        protected string RelatedKey = "Related Items";

        #endregion

        public PublishIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IPublishWrapper publishWrapper,
            IParameterResultFactory resultFactory) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
            
            ConversationParameters.Add(new ItemParameter(ItemKey, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new DatabaseParameter(DBKey, settings, dataWrapper, inputFactory, publishWrapper, resultFactory));
            ConversationParameters.Add(new LanguageParameter(LangKey, settings, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new YesOrNoParameter(RecursionKey, Translator.Text("Chat.Intents.Publish.RecursionParameterRequest"), inputFactory, resultFactory));
            ConversationParameters.Add(new YesOrNoParameter(RelatedKey, Translator.Text("Chat.Intents.Publish.RelatedParameterRequest"), inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var toDb = (Database) conversation.Data[DBKey];
            var rootItem = (Item) conversation.Data[ItemKey];
            var langItem = (Language) conversation.Data[LangKey];
            var recursion = (bool) conversation.Data[RecursionKey];
            var related = (bool) conversation.Data[RelatedKey];
            PublishWrapper.PublishItem(rootItem, new[] { toDb }, new[] { langItem }, recursion, false, related);

            var recursionMessage = recursion
                ? Translator.Text("Chat.Intents.Publish.ResponseRecursion") 
                : string.Empty;

            var relatedMessage = related
                ? Translator.Text("Chat.Intents.Publish.ResponseRelated")
                : string.Empty;

            return ConversationResponseFactory.Create(Name, string.Format(
                Translator.Text("Chat.Intents.Publish.Response"), 
                rootItem.DisplayName, 
                toDb.Name, 
                LanguageManager.GetLanguageItem(langItem, toDb).DisplayName, 
                recursionMessage, 
                relatedMessage));
        }
    }
}