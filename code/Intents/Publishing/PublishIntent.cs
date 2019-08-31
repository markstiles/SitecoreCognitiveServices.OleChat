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

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Publishing
{
    public class PublishIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "publishing - publish";

        public override string DisplayName => Translator.Text("Chat.Intents.Publish.Name");

        public override bool RequiresConfirmation => true;

        #region Local Properties

        protected string WorkflowKey = "Workflow Role";
        protected string RoleKey = "User Role";
        protected string DBKey = "Database Name";
        protected string ItemKey = "Item";
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

            ConversationParameters.Add(new RoleParameter(RoleKey, Translator.Text("Chat.Intents.Publish.RoleParameterWarning"), new List<string> { @"sitecore\Sitecore Client Publishing", @"sitecore\Sitecore Client Advanced Publishing" }, dataWrapper, inputFactory, resultFactory));
            var parameters = new Dictionary<string, string>
            {
                { Constants.SearchParameters.FilterPath, Constants.Paths.ContentPath }
            };
            ConversationParameters.Add(new ItemParameter(ItemKey, Translator.Text("Chat.Intents.Publish.ItemParameterRequest"), parameters, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new LanguageParameter(LangKey, Translator.Text("Chat.Parameters.LangParameterRequest"), settings, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new WorkflowParameter(WorkflowKey, Translator.Text("Chat.Intents.Publish.WorkflowParameterWarning"),parameters, dataWrapper, inputFactory, resultFactory));
            ConversationParameters.Add(new DatabaseParameter(DBKey, Translator.Text("Chat.Parameters.DBParameterRequest"), settings, dataWrapper, inputFactory, publishWrapper, resultFactory));
            ConversationParameters.Add(new YesOrNoParameter(RecursionKey, Translator.Text("Chat.Intents.Publish.RecursionParameterRequest"), inputFactory, resultFactory));
            ConversationParameters.Add(new YesOrNoParameter(RelatedKey, Translator.Text("Chat.Intents.Publish.RelatedParameterRequest"), inputFactory, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var toDb = (Database) conversation.Data[DBKey].Data;
            var rootItem = (Item) conversation.Data[ItemKey].Data;
            var langItem = (Language) conversation.Data[LangKey].Data;
            var recursion = (bool) conversation.Data[RecursionKey].Data;
            var related = (bool) conversation.Data[RelatedKey].Data;
            PublishWrapper.PublishItem(rootItem, new[] { toDb }, new[] { langItem }, recursion, false, related);

            var recursionMessage = recursion
                ? Translator.Text("Chat.Intents.Publish.ResponseRecursion") 
                : string.Empty;

            var relatedMessage = related
                ? Translator.Text("Chat.Intents.Publish.ResponseRelated")
                : string.Empty;

            return ConversationResponseFactory.Create(KeyName, string.Format(
                Translator.Text("Chat.Intents.Publish.Response"), 
                rootItem.DisplayName, 
                toDb.Name, 
                LanguageManager.GetLanguageItem(langItem, toDb).DisplayName, 
                recursionMessage, 
                relatedMessage));
        }
    }
}