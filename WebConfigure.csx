#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"

using Microsoft.Web.Administration;

public class AppPool
{
	private string _appPoolName;
	public AppPool(string appPoolName)
	{
		_appPoolName = appPoolName;
	} 

	public AppPool WithApplicationPoolIdentity()
	{
		return this;
	}
}

public class Application
{
	public Application(string applicationName, string applicationPath, AppPool appPool)
	{

	}

	public Application WithApplicationPool(AppPool appPool)
	{
		return this;
	}
}

public class Site
{
	private AppPool _appPool;

	public Site(string siteName, string filePath)
	{

	}

	public Site WithApplicationPool(AppPool appPool)
	{
		return this;
	}

	public Application CreateApplication(string applicationName, string filePath)
	{
		return new Application(applicationName, filePath, _appPool);
	}
}

public class IISConfig
{
	public IISConfig WithSite(Site site)
	{
		return this;
	}
	public IISConfig WithApplicationPool(AppPool appPool)
	{
		return this;
	}
	public IISConfig WithApplication(Application application)
	{
		return this;
	}

	public void DoSetup()
	{
		var serverManager = new ServerManager();
		var sites = serverManager.Sites;
		sites.Add("Test", "C:\\tmp", 9999);
		serverManager.CommitChanges();
	}
}


var appPool = new AppPool("MySiteAppPool")
					.WithApplicationPoolIdentity();
var site = new Site("MySite", "c:\\tmp")
					.WithApplicationPool(appPool);
var application = site.CreateApplication("MyApplication", "c:\\tmp");


var setup = new IISConfig()
	.WithApplicationPool(appPool)
	.WithSite(site)
	.WithApplication(application);
setup.DoSetup();

Console.WriteLine("Site added");