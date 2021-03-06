﻿using Sitecore.Data.Managers;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System.Linq;
using System.Web.UI.WebControls;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class LanguageParameter : IFieldParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string ParamMessage { get; set; }
        public bool IsOptional { get; set; }

        public IOleSettings Settings { get; set; }
        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public LanguageParameter(
            string paramName,
            string paramMessage,
            IOleSettings settings,
            ISitecoreDataWrapper dataWrapper,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = paramMessage;
            Settings = settings;
            DataWrapper = dataWrapper;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
            IsOptional = false;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            if (string.IsNullOrWhiteSpace(paramValue))
                return ResultFactory.GetFailure(ParamMessage);

            var dbName = (!string.IsNullOrEmpty(context.Parameters.Database)) ? context.Parameters.Database : Settings.MasterDatabase;
            var db = DataWrapper.GetDatabase(dbName);
            var lang = DataWrapper.GetLanguages(db)
                .FirstOrDefault(l => LanguageManager
                    .GetLanguageItem(l, db)
                    .DisplayName.ToLower().Contains(paramValue.ToLower()));

            return lang == null
                ? ResultFactory.GetFailure(Translator.Text("Chat.Parameters.LangParameterValidationError"))
                : ResultFactory.GetSuccess(lang.GetDisplayName(), lang);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            var dbName = (!string.IsNullOrEmpty(parameters.Database)) ? parameters.Database : Settings.MasterDatabase;
            var db = DataWrapper.GetDatabase(dbName);

            var options = DataWrapper
                .GetLanguages(db)
                .Select(l => LanguageManager.GetLanguageItem(l, db))
                .Select(a => new ListItem(a.DisplayName, a.DisplayName))
                .ToList();

            return IntentInputFactory.Create(IntentInputType.LinkList, options);
        }
    }
}