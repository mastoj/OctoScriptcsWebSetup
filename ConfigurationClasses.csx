#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"

using Microsoft.Web.Administration;
using System.Collections.Generic;

public class AppPoolConfiguration
{
	private string _appPoolName;
	public static AppPoolConfiguration Create(string appPoolName)
	{
		return new AppPoolConfiguration(appPoolName);
	}

	private AppPoolConfiguration(string appPoolName)
	{
		_appPoolName = appPoolName;
	}

	public void DoSetup(ServerManager serverManager)
	{
		if(serverManager.ApplicationPools[_appPoolName] == null)
		{
			Console.WriteLine("Adding application pool: " + _appPoolName); 
			serverManager.ApplicationPools.Add(_appPoolName);
		}
		else
		{
			Console.WriteLine("Application pool exists: " + _appPoolName);
		}	
	}
}

public interface IParentConfiguration
{
	string RootName { get; }
	string ServerPath { get; }
}

public class ApplicationConfiguration : IParentConfiguration
{
	private string _applicationName;
	private string _applicationPath;
	private IParentConfiguration _parentConfiguration;
	private AppPoolConfiguration _appPoolConfig;
	private ApplicationConfiguration _child;

	public string RootName { get { return _parentConfiguration.RootName; }}

	public string ServerPath { get { return _parentConfiguration.ServerPath + "/" + _applicationName; }}

	public static ApplicationConfiguration Create(string applicationName, string applicationPath, AppPoolConfiguration appPoolConfig, IParentConfiguration parentConfiguration)
	{ 
		return new ApplicationConfiguration(applicationName, applicationPath, appPoolConfig, parentConfiguration);
	}

	public ApplicationConfiguration(string applicationName, string applicationPath, AppPoolConfiguration appPoolConfig, IParentConfiguration parentConfiguration)
	{
		_applicationName = applicationName;
		_applicationPath = applicationPath;
		_appPoolConfig = appPoolConfig;
		_parentConfiguration = parentConfiguration;
	}

	public ApplicationConfiguration WithApplication(string applicationName, string applicationPath)
	{
		_child = ApplicationConfiguration.Create(applicationName, applicationPath, _appPoolConfig, this);
		return _child;
	}

	public void DoSetup(ServerManager serverManager, Site site)
	{
		if(_appPoolConfig != null)
		{
			_appPoolConfig.DoSetup(serverManager);
		}

		var appCollection = site.Applications;
		if(appCollection[ServerPath] == null)
		{
			Console.WriteLine("Adding application: " + _applicationName);
			var add = appCollection.Add(ServerPath, _applicationPath);
		}

		if(_child != null)
		{
			_child.DoSetup(serverManager, site);
		}
	}
}

public abstract class SiteBindingConfiguration
{
	private string _ip;
	private string _hostHeader;
	private int _port;

	protected string BindingInformation
	{
		get
		{
			return string.Format("{0}:{1}:{2}", _ip, _port, _hostHeader);
		}
	}

	public static SiteBindingConfiguration CreateHttpBinding(string ip, int port, string hostHeader)
	{
		return new HttpBindingConfiguration(ip, port, hostHeader);
	}

	public SiteBindingConfiguration(string ip, int port, string hostHeader)
	{
		_ip = ip;
		_port = port;
		_hostHeader = hostHeader;
	}

	public abstract void DoSetup(ServerManager serverManager, Site site);

	private class HttpBindingConfiguration : SiteBindingConfiguration
	{
		public HttpBindingConfiguration(string ip, int port, string hostHeader) : base(ip, port, hostHeader)
		{}

		public override void DoSetup(ServerManager serverManager, Site site)
		{
			site.Bindings.Add(BindingInformation, "http");
		}
	}
}

public class SiteConfiguration : IParentConfiguration
{
	private AppPoolConfiguration _appPoolConfig;
	private ApplicationConfiguration _application;
	private List<SiteBindingConfiguration> _bindings = new List<SiteBindingConfiguration>();
	private string _siteName;
	private string _filePath;
	private int _port;

	public string RootName { get { return _siteName; } }

	public string ServerPath { get { return ""; } }

	public static SiteConfiguration Create(string siteName, string filePath, int port)
	{
		return new SiteConfiguration(siteName, filePath, port);
	}

	public SiteConfiguration(string siteName, string filePath, int port)
	{
		_siteName = siteName;
		_filePath = filePath;
		_port = port;
	}

	public AppPoolConfiguration WithApplicationPool(string name)
	{
		_appPoolConfig = AppPoolConfiguration.Create(name);
		return _appPoolConfig;
	}

	public ApplicationConfiguration WithApplication(string applicationName, string applicationPath)
	{
		_application = ApplicationConfiguration.Create(applicationName, applicationPath, _appPoolConfig, this);
		return _application;
	}

	public SiteConfiguration WithHttpBinding(string ip, int port, string hostHeader)
	{
		var bindingConfiguration = SiteBindingConfiguration.CreateHttpBinding(ip, port, hostHeader);
		_bindings.Add(bindingConfiguration);
		return this;
	}

	public void DoSetup(ServerManager serverManager)
	{
		var sites = serverManager.Sites;
		Site site;
		if(_appPoolConfig != null)
		{
			_appPoolConfig.DoSetup(serverManager);
		}

		site = sites[_siteName];
		if(site == null)
		{
			Console.WriteLine("Adding site: " + _siteName);
			site = sites.Add(_siteName, _filePath, _port);
		}
		else
		{
			Console.WriteLine("Site exists: " + _siteName);
		}

		if(_application != null)
		{
			_application.DoSetup(serverManager, site);
		}

		if(_bindings.Count > 0)
		{
			site.Bindings.Clear();
			foreach(var bindingConfiguration in _bindings)
			{
				bindingConfiguration.DoSetup(serverManager, site);
			}
		}
	}
}
