using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kraken.Tests
{
    public class KrakenApiResponseHandler : DelegatingHandler
    {
        private readonly Dictionary<KeyValuePair<Uri, String>, HttpResponseMessage> _responses;

        public KrakenApiResponseHandler()
        {
            _responses = new Dictionary<KeyValuePair<Uri, String>, HttpResponseMessage>();
        }

        public void AddResponse(Uri uri, String parameters, HttpResponseMessage responseMessage)
        {
            _responses.Add(new KeyValuePair<Uri, String>(uri, parameters), responseMessage);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string parameters = String.Empty;
            if (request.Content != null)
                parameters = await request.Content.ReadAsStringAsync();
            
            KeyValuePair<Uri, String> key = new KeyValuePair<Uri, String>(request.RequestUri, parameters);

            if (_responses.ContainsKey(key))
                return _responses[key];
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
            }
        }
    }
}