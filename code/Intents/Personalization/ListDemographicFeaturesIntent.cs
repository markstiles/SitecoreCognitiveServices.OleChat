﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SitecoreCognitiveServices.Foundation.MSSDK.Language.Models.Luis;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using System.Text;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization
{
    public class ListDemographicFeaturesIntent : BaseOleIntent
    {
        protected readonly ISitecoreDataWrapper DataWrapper;
        protected readonly IPublishWrapper PublishWrapper;
        
        public override string KeyName => "personalization - list demographic features";

        public override string DisplayName => Translator.Text("Chat.Intents.CreateProfile.Name");

        public override bool RequiresConfirmation => true;
        
        #region Local Properties

        protected string ItemKey = "Demographic Feature";
        
        #endregion

        public ListDemographicFeaturesIntent(
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IConversationResponseFactory responseFactory,
            IParameterResultFactory resultFactory,
            IPublishWrapper publishWrapper) : base(inputFactory, responseFactory, settings)
        {
            DataWrapper = dataWrapper;
            PublishWrapper = publishWrapper;
        }
        
        public override ConversationResponse Respond(LuisResult result, ItemContextParameters parameters, IConversation conversation)
        {
            var profiles = Sitecore.Context.Database.GetItem(Constants.ItemIds.ProfileNodeId)
                .Axes.GetDescendants()
                .Where(a => a.TemplateID == Constants.TemplateIds.ProfileTemplateId);

            var response = new StringBuilder();
            response.Append(Translator.Text("Chat.Intents.ListDemographicTraits.Response"));
            foreach(var p in profiles)
            {
                response.Append($", {p.DisplayName}");
            }

            return ConversationResponseFactory.Create(KeyName, response.ToString());
        }
    }
}