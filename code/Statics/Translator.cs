using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Globalization;

namespace SitecoreCognitiveServices.Feature.OleChat.Statics
{
    public static class Translator
    {
        public static string Text(string key)
        {
            var settings = new OleSettings(null, null);
            var db = Sitecore.Configuration.Factory.GetDatabase(settings.MasterDatabase);

            using (new DatabaseSwitcher(db))
            {
                return Translate.TextByDomain(settings.DictionaryDomain, key) ?? string.Empty;
            }
        }
    }
}