#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"
#r "System.Web.Extensions"

using System.Web.Script.Serialization;
using Microsoft.Web.Administration;
using System.Collections.Generic;
using System.IO;

public class AppPoolConfiguration
{
	public string AppPoolName { get; set; }
	private string _identityMode;
	public string IdentityMode { 
		get
		{
			_identityMode = _identityMode ?? "";
			return _identityMode;
		} 
		set
		{
			_identityMode = value;
		} 
	}
	public string UserName { get; set; }
	public string Password { get; set; }

	private Dictionary<string, Action<ApplicationPool >> IdentityModelConfigurations = 
		new Dictionary<string, Action<ApplicationPool >>() {
			{"SpecificUser", SetupSpecificUser},
			{"LocalSystem", (applicationPool) => {SetupSystemIdentity(applicationPool, ProcessModelIdentityType.LocalSystem);}},
			{"LocalService", (applicationPool) => {SetupSystemIdentity(applicationPool, ProcessModelIdentityType.LocalService);}},
			{"NetworkService", (applicationPool) => {SetupSystemIdentity(applicationPool, ProcessModelIdentityType.NetworkService);}},
			{"ApplicationPoolIdentity", (applicationPool) => {SetupSystemIdentity(applicationPool, ProcessModelIdentityType.ApplicationPoolIdentity);}},
			{"", _ => {}}
		};

	private static void SetupSystemIdentity(ApplicationPool appPool, ProcessModelIdentityType identityScope)
	{
		appPool.ProcessModel.IdentityType = identityScope;
		Console.WriteLine("Configuring {0} identity", identityScope);
	}

	private void SetupSpecificUser(ApplicationPool appPool)
	{
		appPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
		Console.WriteLine("Setting {0} as the user of the application pool", UserName);
		appPool.ProcessModel.Password = Password;
		appPool.ProcessModel.UserName = UserName;
	}	

	public void DoSetup(ServerManager serverManager)
	{
		var appPool = serverManager.ApplicationPools[AppPoolName];
		if(appPool == null)
		{
			Console.WriteLine("Adding application pool: " + AppPoolName + " " + IdentityMode + UserName + Password); 
			appPool = serverManager.ApplicationPools.Add(AppPoolName);
		}
		else
		{
			Console.WriteLine("Application pool exists: " + AppPoolName);
		}	
		IdentityModelConfigurations[IdentityMode](appPool);
	}
}

public abstract class ApplicationBaseConfiguration
{
	public string ServerPath { get; set; }
	public AppPoolConfiguration AppPool { get; set; }
	public string FilePath { get; set; }

	private List<ApplicationConfiguration> _applications;
	public List<ApplicationConfiguration> Applications 
	{ 
		get
		{
			_applications = _applications ?? new List<ApplicationConfiguration>();
			return _applications;
		} 
		set
		{
			_applications = value;
		} 
	}

	public virtual void DoSetup(ServerManager serverManager, Site site, ApplicationBaseConfiguration parentConfig)
	{
		if(AppPool != null)
		{
			Console.WriteLine("Configuring app pool");
			AppPool.DoSetup(serverManager);
			var application = site.Applications[ServerPath];
			application.ApplicationPoolName = AppPool.AppPoolName;
		}
		foreach(var child in Applications)
		{
			child.DoSetup(serverManager, site, this);
		}
	}
}

public class ApplicationConfiguration : ApplicationBaseConfiguration
{
	public string ApplicationName { get; set; }

	public override void DoSetup(ServerManager serverManager, Site site, ApplicationBaseConfiguration parentConfig)
	{
		var appCollection = site.Applications;
		ServerPath = parentConfig.ServerPath + (parentConfig.ServerPath.EndsWith("/") ?  "" : "/") + ApplicationName; 
		if(appCollection[ServerPath] == null)
		{
			Console.WriteLine("Adding application: " + ApplicationName);
			var add = appCollection.Add(ServerPath, FilePath);
		}
		base.DoSetup(serverManager, site, this);
	}
}

public class SiteBindingConfiguration
{
	public string Ip { get; set; }
	public int Port { get; set; }
	public string HostHeader { get; set; }
	public string Protocol { get; set; }

	protected string BindingInformation
	{
		get
		{
			return string.Format("{0}:{1}:{2}", Ip, Port, HostHeader);
		}
	}

	public void DoSetup(ServerManager serverManager, Site site)
	{
		Console.WriteLine("Adding binding: " + BindingInformation);
		site.Bindings.Add(BindingInformation, Protocol);
	}
}

public class SiteConfiguration : ApplicationBaseConfiguration
{
	public SiteConfiguration()
	{
		ServerPath = "/";
	}

	private List<SiteBindingConfiguration> _bindings;
	public List<SiteBindingConfiguration> Bindings 
	{ 
		get
		{
			_bindings = _bindings ?? new List<SiteBindingConfiguration>();
			return _bindings;
		} 
		set
		{
			_bindings = value;
		} 
	}

	public string SiteName { get; set; }
	public int Port { get; set; }

	public void DoSetup(ServerManager serverManager)
	{
		var sites = serverManager.Sites;
		Site site;

		site = sites[SiteName];
		if(site == null)
		{
			Console.WriteLine("Adding site: " + SiteName);
			site = sites.Add(SiteName, FilePath, Port);
		}
		else
		{
			Console.WriteLine("Site exists: " + SiteName);
		}
		if(Bindings.Count > 0)
		{
			site.Bindings.Clear();
			foreach(var bindingConfiguration in Bindings)
			{
				bindingConfiguration.DoSetup(serverManager, site);
			}
		}
		base.DoSetup(serverManager, site, this);
	}
}

public static class Parser
{
	public static SiteConfiguration ParseJson(string json)
	{
		var jSerialize = new JavaScriptSerializer();
		return jSerialize.Deserialize<SiteConfiguration>(json);
	}
}

public static class IISSetup
{
	public static SiteConfiguration SetupFromFile(string filePath)
	{
		var json = ReadFromFile(filePath);
		Console.WriteLine("Config read: ");
		Console.WriteLine(json);
		return SetupFromJson(json);
	}

	private static string ReadFromFile(string filePath)
	{
		return File.ReadAllText(filePath);
	}

	public static SiteConfiguration SetupFromJson(string json)
	{
		var siteConfiguration = Parser.ParseJson(json);
		var serverManager = new ServerManager();
		siteConfiguration.DoSetup(serverManager);
		serverManager.CommitChanges();
		return siteConfiguration;
	}
}