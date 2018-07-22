using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace ConferenceBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Conversation.UpdateContainer(
                builder =>
                {
                    // Using Azure Table Storage
                    // var store = new TableBotDataStore(ConfigurationManager.AppSettings["AzureWebJobsStorage"]); // requires Microsoft.BotBuilder.Azure Nuget package 

                    // To use CosmosDb or InMemory storage instead of the default table storage, uncomment the corresponding line below
                    // var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 
                    var store = new InMemoryDataStore(); // volatile in-memory store

                    builder.Register(c => store)
                        .AsSelf()
                        .SingleInstance();

                });
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
