using System;
using System.Net;

namespace WebApiClient.Controllers.Exceptions
{
	public class WebApiException: ApplicationException
	{
		public WebApiException()
		{ }

		protected WebApiException(Exception e) 
			:base("", e)
		{ }

		public string Request { get; set; }
		public HttpStatusCode Code { get; set; }
		public string Reason { get; set; }
	}

	public class UnauthorizedException : WebApiException
	{
		public UnauthorizedException(string request)
		{
			Request = request;
		}
	}

	public class ForbiddenException : WebApiException
	{
		public ForbiddenException(string request)
		{
			Request = request;
		}
	}

	public class ConnectionException : WebApiException
	{
		public ConnectionException(Exception e)
			: base(e) {}
	}

	public class LongOperationException : WebApiException
	{
		public LongOperationException(string request)
		{
			Request = request;
		}
	}

	public class InternalServerErrorException : WebApiException
	{
		public InternalServerErrorException(string request)
		{
			Request = request;
		}
	}
}
