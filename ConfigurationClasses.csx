#r "c:\windows\System32\inetsrv\Microsoft.Web.Administration.dll"

using Microsoft.Web.Administration;

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
}

public class ApplicationConfiguration
{
	private string _applicationName;
	private string _applicationPath;
	private IParentConfiguration _parentConfiguration;
	private AppPoolConfiguration _appPoolConfig;

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

	public void DoSetup(ServerManager serverManager)
	{
		if(_appPoolConfig != null)
		{
			_appPoolConfig.DoSetup(serverManager);
		}

		var appCollection = serverManager.Sites[_parentConfiguration.RootName].Applications;
		if(appCollection[_applicationName] == null)
		{
			Console.WriteLine("Adding application: " + _applicationName);
			var add = appCollection.Add(_applicationName, _applicationPath);
		}
	}
}

public class SiteConfiguration : IParentConfiguration
{
	private AppPoolConfiguration _appPoolConfig;
	private ApplicationConfiguration _application;
	private string _siteName;
	private string _filePath;
	private int _port;

	public string RootName { get { return _siteName; } }

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

	public void DoSetup(ServerManager serverManager)
	{
		var sites = serverManager.Sites;
		if(_appPoolConfig != null)
		{
			_appPoolConfig.DoSetup(serverManager);
		}

		if(sites[_siteName] == null)
		{
			Console.WriteLine("Adding site: " + _siteName);
			sites.Add(_siteName, _filePath, _port);
		}
		else
		{
			Console.WriteLine("Site exists: " + _siteName);
		}

		if(_application != null)
		{
			_application.DoSetup(serverManager);
		}
	}
}
