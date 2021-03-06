﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.System
{
    public class RebuildIndexIntent : BaseOleIntent
    {
        protected readonly IContentSearchWrapper ContentSearchWrapper;
        
        public override string KeyName => "system - rebuild index";

        public override string DisplayName => Translator.Text("Chat.Intents.RebuildIndex.Name");

        public override bool RequiresConfirmation => false;

        #region Local Properties

        protected string IndexKey = "Index Name";

        #endregion

        public RebuildIndexIntent(
            IContentSearchWrapper searchWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IOleSettings settings) : base(inputFactory, responseFactory, settings)
        {
            ContentSearchWrapper = searchWrapper;

            ConversationParameters.Add(new IndexParameter(IndexKey, inputFactory, searchWrapper, resultFactory));
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var message = "";
            var index = (string)conversation.Data[IndexKey].Value;
            if (index == Translator.Text("Chat.Intents.RebuildIndex.All"))
            {
                IndexCustodian.RebuildAll(new [] { IndexGroup.Experience });
                message = Translator.Text("Chat.Intents.RebuildIndex.AllRebuiltMessage");
            }
            else
            {
                var searchIndex = ContentSearchWrapper.GetIndex(index);
                IndexCustodian.FullRebuild(searchIndex);
                message = string.Format(Translator.Text("Chat.Intents.RebuildIndex.RebuildIndexMessage"), index);
            }

            return ConversationResponseFactory.Create(KeyName, message);
        }
    }
}