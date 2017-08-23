using WebApiClient.Controllers.Exceptions;

namespace WebApiClient.Controllers
{
	public class UpdateApi : BaseWebApi<object>
	{
		public UpdateApi(string uri)
			: base(uri, "updates")
		{ }

		public byte[] GetUpdate(string version)
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
	}
}
