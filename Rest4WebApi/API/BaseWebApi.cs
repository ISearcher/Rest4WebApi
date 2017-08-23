using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApiClient.Controllers.Exceptions;

namespace WebApiClient.Controllers
{
	public interface IBaseWebApi
	{
		void SetAuthHeader();
	}

	public abstract class BaseWebApi<T> : IBaseWebApi where T : class
	{
		protected BaseWebApi(string baseUri, string route)
		{
			try
			{
				Url = baseUri + "api/" + route;

				_messageHandler = new WebRequestHandler { ClientCertificateOptions = ClientCertificateOption.Manual };

				if (baseUri.Contains("https"))
				{
					_messageHandler.ClientCertificates.Add(GetCertificate());
					_messageHandler.UseProxy = false;
				}

				_client = new HttpClient(_messageHandler) { BaseAddress = new Uri(baseUri) };
				_client.DefaultRequestHeaders.Accept.Clear();
				_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				_jsonSerializerSettings = new JsonSerializerSettings
				{
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					TypeNameHandling = TypeNameHandling.Auto,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize
				};
			}
			catch (Exception e)
			{
				//Logger.Log(Logger.LogLevel.Error, e);
			}
		}

		private X509Certificate GetCertificate()
		{
			var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
			
			store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
			var certificates = store.Certificates.Find(X509FindType.FindBySerialNumber, "", false);

			if (certificates.Count > 0)
			{
				//Logger.Log(Logger.LogLevel.Debug, "Cert was found");
				store.Close();
				return certificates[0];
			}
			//Logger.Log(Logger.LogLevel.Warning, "Client cert not found");

			store.Close();
			return null;
		}

		protected HttpResponseMessage Create(T entity, string method = "")
		{
			CheckMethod(ref method);
			return DoCreate(entity, $"{Url}/{method}");
		}

		protected HttpResponseMessage Create<U>(U entity, string method = "")
		{
			CheckMethod(ref method);
			return DoCreate(entity, $"{Url}/{method}");
		}

		protected T GetByParam(string paramValue, string method = "")
		{
			if (method == "")
			{
				return DoGet<T>($"{Url}/{paramValue}");
			}

			CheckMethod(ref method);
			return DoGet<T>($"{Url}/{method}/{paramValue}");
		}

		protected U GetByParam<U>(string paramValue, string method = "") where U : class
		{
			if (method == "")
			{
				return DoGet<U>($"{Url}/{paramValue}");
			}

			CheckMethod(ref method);
			return DoGet<U>($"{Url}/{method}/{paramValue}");
		}

		protected U PutByParam<U>(U paramValue, string method = "") where U : class 
		{
			if (method == "")
			{
				return DoUpdate($"{Url}", paramValue);
			}

			CheckMethod(ref method);
			return DoUpdate($"{Url}/{method}", paramValue);
		}

		protected T Get(string method = "")
		{
			CheckMethod(ref method);
			return DoGet<T>($"{Url}/{method}");
		}

		protected U Get<U>(string method = "") where U : class
		{
			CheckMethod(ref method);
			return DoGet<U>($"{Url}/{method}");
		}

		protected HttpResponseMessage Update(T entity, string method = "")
		{
			if (method == "")
			{
				return DoUpdate(entity, $"{Url}");
			}

			return DoUpdate(entity, $"{Url}/{method}");
		}

		protected HttpResponseMessage Update<U>(U entity, string method = "")
		{
			CheckMethod(ref method);
			return DoUpdate(entity, $"{Url}/{method}");
		}

		protected byte[] GetContentByParam(string paramValue, string method = "")
		{
			if (method == "")
			{
				return DoGetContent($"{Url}/{paramValue}");
			}

			CheckMethod(ref method);
			return DoGetContent($"{Url}/{method}/{paramValue}");
		}

		protected HttpResponseMessage SendRequest(string method = "")
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri($"{Url}/{method}"),
				Method = HttpMethod.Get
			};

			var response = MakeRequest(() => _client.SendAsync(request).Result);
			CheckIsResponseOk(response, request.RequestUri.ToString());
			return response;
		}

		protected HttpResponseMessage SendFile(T entity, string filePath, string method = "")
		{
			var uri = new Uri($"{Url}/{method}");

			using (var content = new MultipartFormDataContent())
			{
				content.Add(
					new StreamContent(
						new FileStream(filePath, FileMode.Open)), "entity");

				content.Add(Serialize(entity), "dto");

				var response = MakeRequest(() => _client.PostAsync(uri, content).Result);
				CheckIsResponseOk(response, uri.ToString());

				return response;
			}
		}

		protected HttpResponseMessage DoDelete<T1>(T1 entity, string method = "")
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri($"{Url}/{method}"),
				Method = HttpMethod.Delete,
				Content = Serialize(entity)
			};

			var response = MakeRequest(() => _client.SendAsync(request).Result);
			CheckIsResponseOk(response, request.RequestUri.ToString());

			return response;
		}

		protected HttpResponseMessage DoDelete(string method)
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri($"{Url}/{method}"),
				Method = HttpMethod.Delete
			};

			var response = MakeRequest(() => _client.SendAsync(request).Result);
			CheckIsResponseOk(response, request.RequestUri.ToString());

			return response;
		}

		private U DoUpdate<U>(string uri, U dto) where U: class
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri(uri),
				Content = Serialize(dto),
				Method = HttpMethod.Put
			};

			var response = MakeRequest(() => _client.SendAsync(request).Result);
			if (CheckIsResponseOk(response, uri))
			{
				return Deserialize<U>(response.Content.ReadAsStringAsync().Result);
			}
			return default(U);
		}

		private U DoGet<U>(string uri) where U : class 
		{
			var response = MakeRequest(() => _client.GetAsync(uri).Result);
			if (CheckIsResponseOk(response, uri))
			{
				return Deserialize<U>(response.Content.ReadAsStringAsync().Result);
			}
			return default(U);
		}

		private byte[] DoGetContent(string uri)
		{
			var response = MakeRequest(() => _client.GetAsync(uri).Result);
			if (CheckIsResponseOk(response, uri))
			{
				Task<byte[]> data = response.Content.ReadAsByteArrayAsync();
				return data.Result;
			}
			return null;
		}

		private HttpResponseMessage DoUpdate<U>(U entity, string uri)
		{
			var content = Serialize(entity);
			var response = MakeRequest(() => _client.PutAsync(uri, content).Result);
			CheckIsResponseOk(response, uri);

			return response;
		}

		private HttpResponseMessage DoCreate<T1>(T1 entity, string uri)
		{
			var content = Serialize(entity);
			var response = MakeRequest(()=>_client.PostAsync(uri, content).Result);
			CheckIsResponseOk(response, uri);

			return response;
		}

		HttpResponseMessage MakeRequest(Func<HttpResponseMessage> func)
		{
			try
			{
				return func();
			}
			catch (AggregateException e)
			{
				if (e.InnerExceptions.OfType<HttpRequestException>().Any())
				{
					throw new ConnectionException(e);
				}
				throw;
			}
		}

		private static void CheckMethod(ref string method)
		{
			if (string.IsNullOrEmpty(method))
				return;

			if (method.LastIndexOf("/", StringComparison.Ordinal) != method.Length - 1)
			{
				method += "/";
			}
		}

		protected static bool CheckIsResponseOk(HttpResponseMessage response, string requestStr)
		{
			if (response.IsSuccessStatusCode)
			{
				return true;
			}

			switch (response.StatusCode)
			{
				case HttpStatusCode.Unauthorized:
				{
					throw new UnauthorizedException(requestStr) {Code = response.StatusCode, Reason = response.ReasonPhrase};
				}
				case HttpStatusCode.Forbidden:
				{
					throw new ForbiddenException(requestStr) { Code = response.StatusCode, Reason = response.ReasonPhrase };
				}
				case HttpStatusCode.InternalServerError:
				{
					throw new InternalServerErrorException(requestStr) { Code = response.StatusCode, Reason = response.ReasonPhrase };
				}
				default:
				{
					/*Logger.Log(Logger.LogLevel.Warning, "Response to {0} failed: {1}. Reason {3}", 
						requestStr, 
						response.StatusCode,
						response.ReasonPhrase);*/

					return false;
				}
			}
		}

		protected static void HandleWebApiException(WebApiException e)
		{
		}

		public void SetAuthHeader()
		{
		}

		protected static U Deserialize<U>(string content)
		{
			return JsonConvert.DeserializeObject<U>(content, _jsonSerializerSettings);
		}

		protected static ObjectContent<T1> Serialize<T1>(T1 entity)
		{
			return new ObjectContent<T1>(entity, new JsonMediaTypeFormatter {SerializerSettings = _jsonSerializerSettings});
		}

		private readonly HttpClient _client;
		private readonly WebRequestHandler _messageHandler;

		public readonly string Url;

		private static JsonSerializerSettings _jsonSerializerSettings;
	}
}