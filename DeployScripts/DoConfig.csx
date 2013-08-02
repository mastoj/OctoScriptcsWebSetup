#load "ConfigurationClasses.csx"

var configFileName = Octopus.Parameters["ConfigFile"];
var relativeFilePath = ".\\configs\\" + configFileName;

var config = IISSetup.SetupFromFile(relativeFilePath);
var octopusWebSiteName = config.SiteName;
var applicationConfig = config as ApplicationBaseConfiguration;
while(applicationConfig.Applications != null && 
	applicationConfig.Applications.Count > 0)
{
	SiteName += "/" + applicationConfig.Applications[0].ApplicationName;		
	applicationConfig = applicationConfig.Applications[0];
}

Octopus.Parameters["OctopusWebSiteName"] = octopusWebSiteName;
Console.WriteLine("Configured IIS for site: " + octopusWebSiteName);