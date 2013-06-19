﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Everest.Redirection;

namespace Everest.SystemNetHttp
{
    public class SystemNetHttpClientAdapter : HttpClientAdapter
    {
        private readonly AutoRedirect _autoRedirect;
        private readonly HttpClient _client;

        public SystemNetHttpClientAdapter(AutoRedirect autoRedirect)
        {
            _autoRedirect = autoRedirect;
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = !(AutoRedirect.AutoRedirectAndForwardAuthorizationHeader.Equals(autoRedirect) ||
                                      AutoRedirect.DoNotAutoRedirect.Equals(autoRedirect)),
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseCookies = false
            };
            _client = new HttpClient(handler);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var task = _client.SendAsync(request);
            
            return Task.Factory.StartNew(() =>
            {
                var response = task.Result;
                if (ShouldManuallyRedirect(response))
                {
                    Task<HttpResponseMessage> sendAsync = _client.SendAsync(CreateRedirectRequest(request, response));

                    return sendAsync.Result;
                }
                return response;
            });
        }

        private bool ShouldManuallyRedirect(HttpResponseMessage response)
        {
            return (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently)
                   && AutoRedirect.AutoRedirectAndForwardAuthorizationHeader.Equals(_autoRedirect);
        }

        private static HttpRequestMessage CreateRedirectRequest(HttpRequestMessage request, HttpResponseMessage response)
        {
            var newRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(request.RequestUri, response.Headers.Location));
            foreach (var header in request.Headers)
            {
                newRequest.Headers.Add(header.Key, header.Value);
            }
            return newRequest;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public HttpClient Client
        {
            get { return _client; }
        }
    }
}
