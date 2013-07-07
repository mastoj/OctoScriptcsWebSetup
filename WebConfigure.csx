#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"
#load "ConfigurationClasses.csx"
using Microsoft.Web.Administration;

var siteConfiguration = SiteConfiguration.Create("Test2", @"c:\tmp", 8989);
siteConfiguration.WithApplicationPool("Dummy");
var serverManager = new ServerManager();
siteConfiguration.DoSetup(serverManager);
serverManager.CommitChanges();

//var appPool = AppPool.Create("MySiteAppPool")
//					.WithApplicationPoolIdentity();
//var site = new Site("MySite", "c:\\tmp")
//					.WithApplicationPool(appPool);
//var application = site.CreateApplication("MyApplication", "c:\\tmp");

var site = serverManager.Sites["Test2"];
foreach(var app in site.Applications)
{
	Console.WriteLine("AppPool: " + app.Name);
	Console.WriteLine("AppPool: " + app.ApplicationPoolName);
}