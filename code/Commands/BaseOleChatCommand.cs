using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SitecoreCognitiveServices.Feature.OleChat;
using SitecoreCognitiveServices.Foundation.SCSDK.Commands;

namespace SitecoreCognitiveServices.Feature.OleChat.Commands
{
    public class BaseOleChatCommand : BaseCommand
    {
        protected readonly IOleSettings Settings;

        public BaseOleChatCommand()
        {
            Settings = DependencyResolver.Current.GetService<IOleSettings>();
        }
    }
}