using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TextIt.Startup))]

namespace TextIt
{
    #pragma warning disable 1591
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(10);
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromDays(1);
        }
    }
}
