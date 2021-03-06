﻿using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class DateParameter : IFieldParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string ParamMessage { get; set; }
        public bool IsOptional { get; set; }

        public IIntentInputFactory IntentInputFactory { get; set; }
        public ISitecoreDataWrapper DataWrapper { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public DateParameter(
            string paramName,
            string paramMessage,
            IIntentInputFactory inputFactory,
            ISitecoreDataWrapper dataWrapper,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = paramMessage;
            IntentInputFactory = inputFactory;
            DataWrapper = dataWrapper;
            ResultFactory = resultFactory;
            IsOptional = false;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            if (string.IsNullOrWhiteSpace(paramValue))
                return ResultFactory.GetFailure(ParamMessage);

            var at = Translator.Text("Chat.Parameters.At");
            var on = Translator.Text("Chat.Parameters.On");
            DateTime result;
            var cleanParamValue = paramValue.ToLower()
                .Replace($" {at} ", " ")
                .Replace($" {on} ", " ");
            if (DateTime.TryParse(cleanParamValue, out result))
                return ResultFactory.GetSuccess(paramValue, DataWrapper.GetDateFieldValue(result));

            return ResultFactory.GetFailure(Translator.Text("Chat.Parameters.DateParameterValidationError"));
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            return IntentInputFactory.Create(IntentInputType.None);
        }
    }
}