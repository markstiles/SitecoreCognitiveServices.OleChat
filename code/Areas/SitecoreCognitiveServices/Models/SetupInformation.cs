﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models
{
    public class SetupInformation : ISetupInformation
    {
        public string LuisApiKey { get; set; }
        public string LuisAuthoringApiKey { get; set; }
        public string LuisApiEndpoint { get; set; }
    }
}