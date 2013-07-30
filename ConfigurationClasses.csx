#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"
#r "System.Web.Extensions"

using System.Web.Script.Serialization;
using Microsoft.Web.Administration;
using System.Collections.Generic;

public class AppPoolConfiguration
{
	public string AppPoolName { get; set; }

//	public static AppPoolConfiguration Create(string appPoolName)
//	{
//		return new AppPoolConfiguration(appPoolName);
//	}

//	private AppPoolConfiguration(string appPoolName)
//	{
//		_appPoolName = appPoolName;
//	}

	public void DoSetup(ServerManager serverManager)
	{
		if(serverManager.ApplicationPools[AppPoolName] == null)
		{
			Console.WriteLine("Adding application pool: " + AppPoolName); 
			serverManager.ApplicationPools.Add(AppPoolName);
		}
		else
		{
			Console.WriteLine("Application pool exists: " + AppPoolName);
		}	
	}
}

public abstract class ApplicationBaseConfiguration
{
	public string ServerPath { get; set; }
	public AppPoolConfiguration AppPoolConfig { get; set; }
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
		if(AppPoolConfig != null)
		{
			AppPoolConfig.DoSetup(serverManager);
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

//	public static ApplicationConfiguration Create(string applicationName, string applicationPath, AppPoolConfiguration appPoolConfig, IParentConfiguration parentConfiguration)
//	{ 
//		return new ApplicationConfiguration(applicationName, applicationPath, appPoolConfig, parentConfiguration);
//	}

//	public ApplicationConfiguration(string applicationName, string applicationPath, AppPoolConfiguration appPoolConfig, IParentConfiguration parentConfiguration)
//	{
//		_applicationName = applicationName;
//		_applicationPath = applicationPath;
//		_appPoolConfig = appPoolConfig;
//		_parentConfiguration = parentConfiguration;
//	}

//	public ApplicationConfiguration WithApplication(string applicationName, string applicationPath)
//	{
//		_child = ApplicationConfiguration.Create(applicationName, applicationPath, _appPoolConfig, this);
//		return _child;
//	}

	public override void DoSetup(ServerManager serverManager, Site site, ApplicationBaseConfiguration parentConfig)
	{
		var appCollection = site.Applications;
		ServerPath = parentConfig.ServerPath + "/" + ApplicationName; 
		if(appCollection[ServerPath] == null)
		{
			Console.WriteLine("Adding application: " + ApplicationName);
			var add = appCollection.Add(ServerPath, FilePath);
		}
		base.DoSetup(serverManager, site, this);
	}
}

public abstract class SiteBindingConfiguration
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

//	public static SiteBindingConfiguration CreateHttpBinding(string ip, int port, string hostHeader)
//	{
//		return new HttpBindingConfiguration(ip, port, hostHeader);
//	}

//	public SiteBindingConfiguration(string ip, int port, string hostHeader)
//	{
//		_ip = ip;
//		_port = port;
//		_hostHeader = hostHeader;
//	}

	public void DoSetup(ServerManager serverManager, Site site)
	{
		site.Bindings.Add(BindingInformation, Protocol);		
	}

//	private class HttpBindingConfiguration : SiteBindingConfiguration
//	{
//		public HttpBindingConfiguration(string ip, int port, string hostHeader) : base(ip, port, hostHeader)
//		{}

//		public override void DoSetup(ServerManager serverManager, Site site)
//		{
//			site.Bindings.Add(BindingInformation, "http");
//		}
//	}
}

public class SiteConfiguration : ApplicationBaseConfiguration
{
	public SiteConfiguration()
	{
		ServerPath = "";
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


//	public static SiteConfiguration Create(string siteName, string filePath, int port)
//	{
//		return new SiteConfiguration(siteName, filePath, port);
//	}

//	public SiteConfiguration(string siteName, string filePath, int port)
//	{
//		_siteName = siteName;
//		_filePath = filePath;
//		_port = port;
//	}

//	public AppPoolConfiguration WithApplicationPool(string name)
//	{
//		_appPoolConfig = AppPoolConfiguration.Create(name);
//		return _appPoolConfig;
//	}

//	public ApplicationConfiguration WithApplication(string applicationName, string applicationPath)
//	{
//		_application = ApplicationConfiguration.Create(applicationName, applicationPath, _appPoolConfig, this);
//		return _application;
//	}

//	public SiteConfiguration WithHttpBinding(string ip, int port, string hostHeader)
//	{
//		var bindingConfiguration = SiteBindingConfiguration.CreateHttpBinding(ip, port, hostHeader);
//		_bindings.Add(bindingConfiguration);
//		return this;
//	}

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