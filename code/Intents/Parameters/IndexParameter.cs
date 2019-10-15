using Sitecore.ContentSearch;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Foundation.SCSDK.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class IndexParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        protected string ParamMessage { get; set; }

        public IIntentInputFactory IntentInputFactory { get; set; }
        public IContentSearchWrapper SearchWrapper { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public IndexParameter(
            string paramName,
            IIntentInputFactory inputFactory,
            IContentSearchWrapper searchWrapper,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = Translator.Text("Chat.Parameters.IndexParameterRequest");
            IntentInputFactory = inputFactory;
            SearchWrapper = searchWrapper;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context)
        {
            if (string.IsNullOrWhiteSpace(paramValue))
                return ResultFactory.GetFailure(ParamMessage);

            var error = Translator.Text("Chat.Parameters.IndexParameterValidationError");
            try
            {
                var searchIndex = ContentSearchManager.GetIndex(paramValue);
                if (searchIndex == null)
                    return ResultFactory.GetFailure(error);
            }
            catch
            {
                return ResultFactory.GetFailure(error);
            }

            return ResultFactory.GetSuccess(paramValue, paramValue);
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            var indexes = SearchWrapper.GetIndexNames().Concat(new[] { Translator.Text("Chat.Parameters.All") });

            return IntentInputFactory.Create(IntentInputType.ListSearch, indexes.Select(a => new ListItem(a, a)).ToList());
        }
    }
}