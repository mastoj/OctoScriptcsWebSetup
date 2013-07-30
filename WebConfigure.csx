#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"

#load "ConfigurationClasses.csx"
using Microsoft.Web.Administration;

var json = @"{
		SiteName: ""Test2"",
		Port: 9999,
		FilePath: ""c:\\tmp"",
		Applications: [
			{
				ApplicationName: ""app1"",
				FilePath: ""c:\\tmp""
			}
		]
	}";

var config = Parser.ParseJson(json);

Console.WriteLine(config.FilePath);
Console.WriteLine(config.Applications[0].FilePath);

//var siteConfiguration = SiteConfiguration.Create("Test2", @"c:\tmp", 8989).WithHttpBinding("*", 9999, "*");
//siteConfiguration.WithApplicationPool("Dummy");
//var appConfig = siteConfiguration.WithApplication("Yo", @"c:\tmp");
//var appConfig2 = appConfig.WithApplication("Ohoy", @"c:\tmp");
var serverManager = new ServerManager();
config.DoSetup(serverManager);
serverManager.CommitChanges();


//var site = serverManager.Sites["Test2"];
//foreach(var app in site.Applications)
//{
//	Console.WriteLine("AppPool name: " + app.ApplicationPoolName);
//	foreach(var attribute in app.Attributes)
//	{
//		Console.WriteLine("Attribute name: " + attribute.Name);		
//		Console.WriteLine("Attribute value: " + attribute.Value);
//	}
//}

//#r "System.Web.Extensions"
//> using System.Web.Script.Serialization;
