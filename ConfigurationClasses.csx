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

public class SiteConfiguration
{
	private AppPoolConfiguration _appPoolConfig;
	private string _siteName;
	private string _filePath;
	private int _port;

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
	}
}
