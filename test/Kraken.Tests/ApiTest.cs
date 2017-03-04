using System;
using System.IO;
using System.Net;
using System.Net.Http;

using Xunit;

using Kraken.Models;

namespace Kraken.Tests
{
    public class ApiTest
    {
        private readonly Api _api;

        public ApiTest()
        {
            KrakenApiResponseHandler handler = new KrakenApiResponseHandler();

            _api = new Api(null, null, Api.Url, Api.Version, handler);

            handler.AddResponse(
                new Uri(String.Format("{0}/{1}/public/{2}", Api.Url, Api.Version, "Assets")), 
                String.Empty, 
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("Responses/Assets.json"))
                });
        }

        [Fact]
        public void TestGetAssets()
        {
            var assets = _api.GetAssets(null);
            
            Assert.True(assets.Count > 0);

            Assert.Contains(assets, a => a.Name.Equals("EUR"));
        }
    }
}