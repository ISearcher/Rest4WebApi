using System.Collections.Generic;
using Common.DTO;
using WebApiClient.Controllers.Exceptions;

namespace WebApiClient.Controllers
{
	public class ClientVersionApi : BaseWebApi<ClientVersionDto>
	{
		public ClientVersionApi(string uri)
			: base(uri, "version")
		{ }

		public List<ClientVersionDto> GetClientVersions()
		{
			try
			{
				var versions = Get<List<ClientVersionDto>>("clients");
				return versions ?? new List<ClientVersionDto>();
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return new List<ClientVersionDto>();
		}

		public byte[] DownloadVersion(string version)
		{
			try
			{
				return GetContentByParam(version);
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return null;
		}

		public bool CreateNewVersion(ClientVersionDto verInfo, string filePath)
		{
			try
			{
				var response = SendFile(verInfo, filePath);
				return response.IsSuccessStatusCode;
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}

			return false;
		}

		public void DeleteVersion(string name)
		{
			try
			{
				DoDelete(name);
			}
			catch (WebApiException e)
			{
				HandleWebApiException(e);
			}
		}
	}
}
