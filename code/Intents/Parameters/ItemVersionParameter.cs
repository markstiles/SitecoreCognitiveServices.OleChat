using Sitecore.Data.Items;
using SitecoreCognitiveServices.Feature.OleChat.Statics;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Enums;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Factories;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace SitecoreCognitiveServices.Feature.OleChat.Intents.Parameters
{
    public class ItemVersionParameter : IRequiredParameter
    {
        #region Constructor

        public string ParamName { get; set; }
        public string ParamMessage { get; set; }
        public string ItemParamName { get; set; }

        public IIntentInputFactory IntentInputFactory { get; set; }
        public IParameterResultFactory ResultFactory { get; set; }

        public ItemVersionParameter(
            string paramName,
            string paramMessage,
            string itemParamName,
            IIntentInputFactory inputFactory,
            IParameterResultFactory resultFactory)
        {
            ParamName = paramName;
            ParamMessage = paramMessage;
            ItemParamName = itemParamName;
            IntentInputFactory = inputFactory;
            ResultFactory = resultFactory;
        }

        #endregion

        public IParameterResult GetParameter(string paramValue, IConversationContext context, ItemContextParameters parameters, IConversation conversation)
        {
            if (paramValue.ToLower() == Translator.Text("Chat.Parameters.All"))
                return ResultFactory.GetSuccess(paramValue, "0");

            int result;
            if (int.TryParse(paramValue, out result))
                return ResultFactory.GetSuccess(paramValue, paramValue);

            return ResultFactory.GetFailure(Translator.Text("Chat.Parameters.VersionParameterValidationError"));
        }

        public IntentInput GetInput(ItemContextParameters parameters, IConversation conversation)
        {
            var all = Translator.Text("Chat.Parameters.All");
            var item = (Item)conversation.Data[ItemParamName].Value;
            var versions = item
                .Versions
                .GetVersionNumbers()
                .Select(a => new ListItem(a.Number.ToString()))
                .ToList();
            versions.Insert(0, new ListItem(all, all));

            return IntentInputFactory.Create(IntentInputType.LinkList, versions);
        }
    }
}