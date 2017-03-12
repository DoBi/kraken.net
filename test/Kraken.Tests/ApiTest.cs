using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                }
            );

            handler.AddResponse(
                new Uri(String.Format("{0}/{1}/public/{2}", Api.Url, Api.Version, "Time")), 
                String.Empty, 
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("Responses/ServerTime.json"))
                }
            );
        }

        [Fact]
        public void TestGetAssets()
        {
            var assets = _api.GetAssets(null);
            
            Assert.True(assets.Count > 0);

            Assert.Contains(assets, a => a.Name.Equals("EUR"));
        }

        [Fact]
        public void TestGetAssetsCache()
        {
            var assets1 = _api.GetAssets(null);
            var assets2 = _api.GetAssets(null);

            Assert.Same(assets1, assets2);
        }

        [Fact]
        // TODO: Currently not enabled because there are no json response for asset pairs
        public void TestGetAssetPairsCache()
        {
            var assetPairs1 = _api.GetAssetPairs();
            var assetPairs2 = _api.GetAssetPairs();

            Assert.Same(assetPairs1, assetPairs2);
        }

        [Fact]
        public void TestGetServerTime()
        {
            var time = _api.GetServerTime();
            var utc = time.ToUniversalTime();

            Assert.Equal(12, utc.Day);
            Assert.Equal(3, utc.Month);
            Assert.Equal(2017, utc.Year);
            Assert.Equal(14, utc.Hour);
            Assert.Equal(48, utc.Minute);
            Assert.Equal(43, utc.Second);
        }

        [Fact]
        public void TestGeneralError()
        {
            IList<Error> errors = new List<Error>();

            try 
            {
                var assets = _api.QueryPublicAsync("Tim", null).Result;
            } 
            catch (AggregateException ex)
            {
                Assert.IsType(typeof(KrakenException), ex.InnerException);

                errors = ((KrakenException) ex.InnerException).Errors;
            }

            Assert.Equal(1, errors.Count());
            Error error = errors.FirstOrDefault();
            Assert.NotNull(error);
            Assert.Equal("General", error.Category);
            Assert.Equal(Error.Severity.Error, error.SeverityCode);
            Assert.Equal("Unknown method", error.ErrorType);
            Assert.Equal(null, error.ExtraInfo);
        }
    }
}