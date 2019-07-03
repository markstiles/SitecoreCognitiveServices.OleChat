﻿using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Controllers;
using SitecoreCognitiveServices.Feature.OleChat.Areas.SitecoreCognitiveServices.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents;
using SitecoreCognitiveServices.Feature.OleChat.Factories;
using SitecoreCognitiveServices.Feature.OleChat.Services;
using SitecoreCognitiveServices.Foundation.SCSDK.Services.MSSDK.Language.Models;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Personalization;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Self;
using SitecoreCognitiveServices.Feature.OleChat.Intents.Publishing;
using SitecoreCognitiveServices.Feature.OleChat.Intents.User;
using SitecoreCognitiveServices.Feature.OleChat.Intents.ProfileUser;
using SitecoreCognitiveServices.Feature.OleChat.Intents.System;

namespace SitecoreCognitiveServices.Feature.OleChat.App_Start
{
    public class IocConfig : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            //system
            serviceCollection.AddTransient<IOleSettings, OleSettings>();

            //models
            serviceCollection.AddTransient<ISetupInformation, SetupInformation>();
            
            //intents
            serviceCollection.AddTransient<IIntent, DefaultIntent>();
            serviceCollection.AddTransient<IIntent, SetupPersonalizationIntent>();
            serviceCollection.AddTransient<IIntent, CreateGoalIntent>();
            serviceCollection.AddTransient<IIntent, AssignGoalIntent>();
            serviceCollection.AddTransient<IIntent, CreateProfileIntent>();
            serviceCollection.AddTransient<IIntent, GreetIntent>();
            serviceCollection.AddTransient<IIntent, KickUserIntent>();
            serviceCollection.AddTransient<IIntent, LoggedInUsersIntent>();
            serviceCollection.AddTransient<IIntent, PublishIntent>();
            serviceCollection.AddTransient<IIntent, SchedulePublishIntent>();
            serviceCollection.AddTransient<IIntent, UnpublishIntent>();
            serviceCollection.AddTransient<IIntent, VersionIntent>();
            serviceCollection.AddTransient<IIntent, AboutIntent>();
            serviceCollection.AddTransient<IIntent, UnlockItemsIntent>();
            serviceCollection.AddTransient<IIntent, LockedItemCountIntent>();
            serviceCollection.AddTransient<IIntent, QuitIntent>();
            serviceCollection.AddTransient<IIntent, ThanksIntent>();
            serviceCollection.AddTransient<IIntent, LogoutIntent>();
            serviceCollection.AddTransient<IIntent, RebuildIndexIntent>();
            serviceCollection.AddTransient<IIntent, FrustratedIntent>();

            //factories
            serviceCollection.AddTransient<ISetupInformationFactory, SetupInformationFactory>();
            
            //setup
            serviceCollection.AddTransient<ISetupService, SetupService>();
            serviceCollection.AddTransient<ISearchService, SearchService>();

            serviceCollection.AddTransient(typeof(OleChatController));
        }
    }
}