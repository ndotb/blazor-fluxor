﻿using FullStackSample.Api.Requests;
using FullStackSample.Client.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Json = Newtonsoft.Json.JsonConvert;

namespace FullStackSample.Client.Services
{
	public class ApiService : IApiService
	{
		private readonly HttpClient HttpClient;
		private readonly NavigationManager NavigationManager;
		private readonly ReadOnlyDictionary<Type, Uri> UriByRequestType;
		private readonly JsonSerializerSettings JsonOptions;

		public ApiService(HttpClient httpClient, NavigationManager navigationManager)
		{
			HttpClient = httpClient;
			NavigationManager = navigationManager;
			UriByRequestType = CreateUrlsByRequestTypeLookup();
			JsonOptions = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};
		}

		public async Task<TResponse> Execute<TRequest, TResponse>(TRequest request)
			where TRequest : IRequest<TResponse>
			where TResponse : ApiResponse, new()
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			Type requestType = request.GetType();
			if (!UriByRequestType.TryGetValue(requestType, out Uri uri))
				throw new ApiEndpointNotFoundException(requestType);

			try
			{
				string jsonResponse = await ExecuteHttpRequest(request, uri);
				return Json.DeserializeObject<TResponse>(jsonResponse, JsonOptions);
			}
#if DEBUG
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
				throw;
			}
#else
			catch
			{
				throw;
			}
#endif
		}

		private async Task<string> ExecuteHttpRequest(object request, Uri uri)
		{
			var httpContent = new StringContent(Json.SerializeObject(request, JsonOptions), Encoding.UTF8, "application/json");
			var httpMessage = new HttpRequestMessage(HttpMethod.Post, uri)
			{
				Content = httpContent
			};
			using (httpMessage)
			{
				HttpResponseMessage httpResponse = await HttpClient.SendAsync(httpMessage);
				using (httpResponse)
				{
					string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
					return jsonResponse;
				}
			}
		}

		private const string ServerApiVersion = ""; //None yet
		private class ClientUrls
		{
			const string Base = ServerApiVersion + "Client/";
			public const string Create = Base + "Create/";
			public const string IsNameAvailable = Base + "IsNameAvailable/";
			public const string IsRegistrationNumberAvailable = Base + "IsRegistrationNumberAvailable/";
			public const string Search = Base + "Search/";
		}

		private ReadOnlyDictionary<Type, Uri> CreateUrlsByRequestTypeLookup()
		{
			string baseUrl = NavigationManager.BaseUri;
			var lookup = new Dictionary<Type, Uri>
			{
				[typeof(ClientCreateCommand)] = new Uri(baseUrl + ClientUrls.Create),
				[typeof(ClientIsNameAvailableQuery)] = new Uri(baseUrl + ClientUrls.IsNameAvailable),
				[typeof(ClientIsRegistrationNumberAvailableQuery)] = new Uri(baseUrl + ClientUrls.IsRegistrationNumberAvailable),
				[typeof(ClientsSearchQuery)] = new Uri(baseUrl + ClientUrls.Search)
			};
			return new ReadOnlyDictionary<Type, Uri>(lookup);
		}
	}
}