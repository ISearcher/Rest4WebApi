using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WebApiClient.Controllers;

namespace WebApiClient
{
	public static class WebApiConnection
	{
		public static void Initialize(string url)
		{
			try
			{
				if (string.IsNullOrEmpty(url))
				{
					return;
				}

				ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

				_apiList = new List<object>()
				{
					new ClientVersionApi(url),
					new TaskApi(url),
					new UpdateApi(url)
				};
			}
			catch (Exception e)
			{
				//Logger.Log(Logger.LogLevel.Fatal, e);
			}
		}

		public static event Action ConnectionStateChanged = delegate { };

		public static T GetApi<T>() where T : class
		{
			var api = _apiList.FirstOrDefault(x => x.GetType() == typeof(T));

			if (api == null)
			{
				Console.WriteLine("Can't find API for {0}", typeof(T));
				return null;
			}

			return (T)Convert.ChangeType(api, typeof(T));
		}

		private static List<object> _apiList;
	}
}
