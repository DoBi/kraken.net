using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kraken.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Kraken.Net
{
    /// <summary>
    /// Class for interaction with the Kraken API
    /// </summary>
    public class Api
    {
        /// <summary>
        /// The default Kraken API url
        /// </summary>
        public const String Url = "https://api.kraken.com";
        /// <summary>
        /// The default Kraken API version
        /// </summary>
        public const String Version = "0";
        
        /// <summary>
        /// The media type for requests
        /// </summary>
        protected const String _mediaType = "application/x-www-form-urlencoded";
        /// <summary>
        /// The API key
        /// </summary>
        protected readonly String _key;
        /// <summary>
        /// The API secret
        /// </summary>
        protected readonly String _secret;
        /// <summary>
        /// The URL to the API
        /// </summary>
        protected readonly String _url;
        /// <summary>
        /// The API version
        /// </summary>
        protected readonly String _version;
        /// <summary>
        /// The HttpMessageHandler for HttpRequests
        /// </summary>
        protected readonly HttpMessageHandler _clientHandler;

        private readonly Cache _cache;

        #region Constructors

        /// <summary>
        /// Create a new instance with the given key and secret
        /// </summary>
        /// <param name="key">The API key</param>
        /// <param name="secret">The API secret</param>
        public Api(String key, String secret) : this(key, secret, Url, Version)
        {}

        /// <summary>
        /// Create a new instance with the given key and secret, and a custom url
        /// </summary>
        /// <param name="key">The API key</param>
        /// <param name="secret">The API secret</param>
        /// <param name="url">A custom API url</param>
        public Api(String key, String secret, String url) : this(key, secret, url, Version)
        {}

        /// <summary>
        /// Create a new instance with the given key and secret, and a custom url and version
        /// </summary>
        /// <param name="key">The API key</param>
        /// <param name="secret">The API secret</param>
        /// <param name="url">A custom API url</param>
        /// <param name="version">A custom API version</param>
        public Api(String key, String secret, String url, String version) : this(key, secret, url, version, null)
        {}

        /// <summary>
        /// Create a new instance with the given key and secret, and a custom url and version
        /// </summary>
        /// <param name="key">The API key</param>
        /// <param name="secret">The API secret</param>
        /// <param name="url">A custom API url</param>
        /// <param name="version">A custom API version</param>
        /// <param name="clientHandler">a custom http client handler</param>
        public Api(String key, String secret, String url, String version, HttpMessageHandler clientHandler)
        {
            _key = key;
            _secret = secret;
            _url = url;
            _version = version;
            _clientHandler = clientHandler;

            _cache = new Cache(this, true);
        }

        #endregion Constructors

        #region Methods

        #region public
        
        #region async Methods

        /// <summary>
        /// Get asset info
        /// </summary>
        /// <returns>A list with all assets</returns>
        public async Task<IList<Asset>> GetAssetsAsync()
        {
            return await GetAssetsAsync(null);
        }

        /// <summary>
        /// Get asset info
        /// </summary>
        /// <param name="asset">comma delimited list of assets to get info on</param>
        /// <returns>A list with the requested assets</returns>
        public async Task<IList<Asset>> GetAssetsAsync(String asset)
        {
            IList<Asset> assets = await _cache.GetAssetsAsync();

            if (!String.IsNullOrWhiteSpace(asset))
            {
                string[] assetNames = asset.Split(',');

                assets = assets.Where(a => assetNames.Contains(a.Name)).ToList();
            }

            return assets;
        }

        /// <summary>
        /// Get a fresh, not cached version of all assets
        /// </summary>
        /// <returns>A list with all assets</returns>
        internal async Task<IList<Asset>> GetNoCacheAssetsAsync()
        {
            string json = await QueryPublicAsync("Assets", new Dictionary<String, String>());

            JObject jObj = JObject.Parse(json);
            JEnumerable<JToken> results = jObj["result"].Children();

            IList<Asset> assets = new List<Asset>();
            foreach (JToken token in results)
            {
                Asset a = JsonConvert.DeserializeObject<Asset>(token.First.ToString());
                assets.Add(a);
            }

            return assets;
        }

        /// <summary>
        /// Get all available asset pairs
        /// </summary>
        /// <returns>A list with all asset pairs</returns>
        public async Task<IList<AssetPair>> GetAssetPairsAsync()
        {
            return await GetAssetPairsAsync(InfoLevel.All);
        }

        /// <summary>
        /// Get the requested info for all available asset pairs
        /// </summary>
        /// <param name="level">The InfoLevel</param>
        /// <returns>A list with all asset pairs</returns>
        public async Task<IList<AssetPair>> GetAssetPairsAsync(InfoLevel level)
        {
            return await GetAssetPairsAsync(null, level);
        }

        /// <summary>
        /// Get a list of the requested asset pairs
        /// </summary>
        /// <param name="pairs">A comma separated list of asset pairs (e.g. XBTEUR for Bitcoin-Euro)</param>
        /// <returns>A list of the requested asset pairs</returns>
        public async Task<IList<AssetPair>> GetAssetPairsAsync(String pairs)
        {
            return await GetAssetPairsAsync(pairs, InfoLevel.All);
        }

        /// <summary>
        /// Get the requested info for the list of the requested asset pairs
        /// </summary>
        /// <param name="pairs">A comma separated list of asset pairs (e.g. XBTEUR for Bitcoin-Euro)</param>
        /// <param name="level">The InfoLevel</param>
        /// <returns>A list with the requested pairs</returns>
        public async Task<IList<AssetPair>> GetAssetPairsAsync(String pairs, InfoLevel level)
        {
            if (level != InfoLevel.All)
                return await GetNoCacheAssetPairsAsync(pairs, level);

            IList<AssetPair> assetPairs = await _cache.GetAssetPairsAsync();

            if (String.IsNullOrWhiteSpace(pairs)) 
                return assetPairs;
            
            string[] pairNames = pairs.Split(',');
                
            assetPairs = assetPairs.Where(a => pairNames.Contains(a.Name)).ToList();

            return assetPairs;
        }

        /// <summary>
        /// Get a fresh, not cached list of asset pairs
        /// </summary>
        /// <param name="pairs">A comma separated list of asset pairs (e.g. XBTEUR for Bitcoin-Euro)</param>
        /// <param name="level">The InfoLevel</param>
        /// <returns>A list with the requested pairs</returns>
        internal async Task<IList<AssetPair>> GetNoCacheAssetPairsAsync(String pairs, InfoLevel level)
        {
            // TODO: What is about the pairs parameter?
            
            var parameters = new Dictionary<String, String>();

            switch (level)
            {
                case InfoLevel.Fees:
                    parameters.Add("info", "fees");
                    break;
                case InfoLevel.Leverage:
                    parameters.Add("info", "leverage");
                    break;
                case InfoLevel.Margin:
                    parameters.Add("info", "margin");
                    break;
                case InfoLevel.All:
                default:
                    parameters.Add("info", "info");
                    break;
            }

            var json = await QueryPublicAsync("AssetPairs", parameters);

            JObject jObj = JObject.Parse(json);
            JEnumerable<JToken> results = jObj["result"].Children();

            IList<Asset> assets = await GetAssetsAsync();
            IList<AssetPair> assetPairs = new List<AssetPair>();
            foreach (JToken token in results)
            {
                AssetPair a = JsonConvert.DeserializeObject<AssetPair>(token.First.ToString());
                
                if (!String.IsNullOrWhiteSpace(a.BaseAlias) && a.BaseAlias.Length > 1)
                    a.Base = assets.FirstOrDefault(o => o.Name.Equals(a.BaseAlias.Substring(1)));

                if (!String.IsNullOrWhiteSpace(a.QuoteAlias) && a.QuoteAlias.Length > 1)
                    a.Quote = assets.FirstOrDefault(o => o.Name.Equals(a.QuoteAlias.Substring(1)));

                assetPairs.Add(a);
            }

            return assetPairs;
        }

        /// <summary>
        /// Get the current users account balance
        /// </summary>
        /// <returns>array of asset names and balance amount</returns>
        public async Task<String> GetBalanceAsync()
        {
            return await QueryPrivateAsync("Balance", new Dictionary<String, String>());
        }

        /// <summary>
        /// Gets all open orders from the current user
        /// </summary>
        /// <param name="includeTrades">Whether or not to include trades in output</param>
        /// <param name="userReferenceId">Restrict results to given user reference id</param>
        /// <returns></returns>
        public async Task<String> GetOpenOrdersAsync(Boolean includeTrades = false, String userReferenceId = null)
        {
            var parameters = new Dictionary<String, String> {{"trades", includeTrades.ToString()}};

            if (!String.IsNullOrWhiteSpace(userReferenceId))
            {
                parameters.Add("userref", userReferenceId);
            }

            return await QueryPrivateAsync("OpenOrders", parameters);
        }

        /// <summary>
        /// Get the time from the Kraken server
        /// </summary>
        /// <returns>The current time from the Kraken server</returns>
        public async Task<DateTime> GetServerTimeAsync()
        {
            string result = await QueryPublicAsync("Time", null);

            JObject jObj = JObject.Parse(result);

            string rfc = (string) jObj.SelectToken("result.rfc1123");

            return Convert.ToDateTime(rfc);
        }

        /// <summary>
        /// Gets the trade balance
        /// </summary>
        /// <param name="asset"></param>
        /// <returns>The raw result from the api</returns>
        public async Task<String> GetTradeBalanceAsync(String asset)
        {
            var parameters = new Dictionary<String, String>();
            
            if (!String.IsNullOrWhiteSpace(asset))
            {
                parameters.Add("asset", asset);
            }

            return await QueryPrivateAsync("TradeBalance", parameters);
        }

        /// <summary>
        /// Call the public Kraken API and returns the raw result
        /// </summary>
        /// <param name="method">The API method to call</param>
        /// <param name="parameters">All parameters as key value pairs</param>
        /// <returns>The raw result string (probably a json string)</returns> 
        /// <exception cref="KrakenException">If there was an error in the result json</exception>
        public async Task<String> QueryPublicAsync(String method, Dictionary<String, String> parameters)
        {
            string postData = String.Empty;

            if (parameters != null && parameters.Count > 0) 
            {
                foreach(KeyValuePair<String, String> pair in parameters)
                {
                    postData = String.Concat(postData, "&", pair.Key, "=", pair.Value);
                }

                postData = postData.Substring(1);
            }

            using (var client = _clientHandler == null ?  new HttpClient() : new HttpClient(_clientHandler))
            {
                string address = String.Format("{0}/{1}/public/{2}", _url, _version, method);
                var content = new StringContent(postData, Encoding.UTF8, _mediaType);

                var response = await client.PostAsync(address, content);

                string json = await response.Content.ReadAsStringAsync();
                var errors = GetErrorsFromJson(json);

                if (errors.Find(e => e.SeverityCode == Error.Severity.Error) != null)
                    throw new KrakenException(errors);

                return json;
            }
        }

        /// <summary>
        /// Call the private Kraken API and returns the raw result
        /// </summary>
        /// <param name="method">The API method to call</param>
        /// <param name="parameters">All parameters for this method</param>
        /// <returns>The raw result string (probably a json string)</returns> 
        /// <exception cref="KrakenException">If there was an error in the result json</exception>
        public async Task<String> QueryPrivateAsync(String method, Dictionary<String, String> parameters)
        {
            // If there are no parameters, create a new Dictionary
            if (parameters == null)
                parameters = new Dictionary<String, String>();

            // There have to be a nonce. Create it from time
            if (!parameters.ContainsKey("nonce"))
            {
                string nonce = Convert.ToString(DateTime.Now.Microtime() + 123).PadRight(16, '0');
                parameters.Add("nonce", nonce);
            }

            string postData = String.Empty;
            foreach(KeyValuePair<String, String> pair in parameters)
            {
                postData = String.Concat(postData, "&", pair.Key, "=", pair.Value);
            }

            postData = postData.Substring(1);

            string path = String.Format("/{0}/private/{1}", _version, method);
            string signature = CreateSignature(path, parameters["nonce"], postData);

            using (var client = _clientHandler == null ?  new HttpClient() : new HttpClient(_clientHandler))
            {
                client.DefaultRequestHeaders.Add("API-Key", _key);
                client.DefaultRequestHeaders.Add("API-Sign", signature);

                string address = String.Concat(_url, path);
                var content = new StringContent(postData, Encoding.UTF8, _mediaType);

                var response = await client.PostAsync(address, content);

                string json = await response.Content.ReadAsStringAsync();
                var errors = GetErrorsFromJson(json);

                if (errors.Find(e => e.SeverityCode == Error.Severity.Error) != null)
                    throw new KrakenException(errors);

                return json;
            }
        }

        #endregion async methods

        #region non-async methods

        /// <summary>
        /// Get all available assets
        /// </summary>
        /// <returns>A list of all assets</returns>
        public IList<Asset> GetAssets()
        {
            try 
            {
                return GetAssetsAsync().Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get asset info
        /// </summary>
        /// <param name="asset">comma delimited list of assets to get info on</param>
        /// <returns>A list with the requested assets</returns>
        public IList<Asset> GetAssets(String asset)
        {
            try
            {
                return GetAssetsAsync(asset).Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get all available asset pairs
        /// </summary>
        /// <returns>A list with all asset pairs</returns>
        public IList<AssetPair> GetAssetPairs()
        {
            try
            {
                return GetAssetPairsAsync().Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get the requested info for all available asset pairs
        /// </summary>
        /// <param name="level">The InfoLevel</param>
        /// <returns>A list with all asset pairs</returns>
        public IList<AssetPair> GetAssetPairs(InfoLevel level)
        {
            try 
            {
                return GetAssetPairsAsync(level).Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get a list of the requested asset pairs
        /// </summary>
        /// <param name="pairs">A comma separated list of asset pairs (e.g. XBTEUR for Bitcoin-Euro)</param>
        /// <returns>A list of the requested asset pairs</returns>
        public IList<AssetPair> GetAssetPairs(String pairs)
        {
            try
            {
                return GetAssetPairsAsync(pairs).Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get the requested info for the list of requested asset pairs
        /// </summary>
        /// <param name="pairs">A comma separated list of asset pairs (e.g. XBTEUR for Bitcoin-Euro)</param>
        /// <param name="level">The InfoLevel</param>
        /// <returns>A list of the requested asset pairs</returns>
        public IList<AssetPair> GetAssetPairs(String pairs, InfoLevel level)
        {
            try
            {
                return GetAssetPairsAsync(pairs, level).Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get the current users account balance
        /// </summary>
        /// <returns>array of asset names and balance amount</returns>
        public String GetBalance()
        {
            try 
            {
                return GetBalanceAsync().Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets all open orders from the current user
        /// </summary>
        /// <param name="includeTrades">Whether or not to include trades in output</param>
        /// <param name="userReferenceId">Restrict results to given user reference id</param>
        /// <returns></returns>
        public String GetOpenOrders(Boolean includeTrades = false, String userReferenceId = null)
        {
            try
            {
                return GetOpenOrdersAsync(includeTrades, userReferenceId).Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get the time from the Kraken server
        /// </summary>
        /// <returns>The current time from the Kraken server</returns>
        public DateTime GetServerTime()
        {
            try
            {
                return GetServerTimeAsync().Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the trade balance
        /// </summary>
        /// <param name="asset"></param>
        /// <returns>The raw result from the api</returns>
        public String GetTradeBalance(String asset)
        {
            try
            {
                return GetTradeBalanceAsync(asset).Result;
            }
            catch (AggregateException ex)
            {
                RethrowKrakenException(ex);
                throw;
            }
        }

        #endregion non-async methods

        #endregion public

        #region private / protected

        /// <summary>
        /// Create the signature, which is used for user authentification
        /// </summary>
        /// <param name="path">The API path</param>
        /// <param name="nonce">The nonce</param>
        /// <param name="postData">All parameters as a url query string</param>
        /// <returns>A signature in base 64</returns>
        private String CreateSignature(String path, String nonce, String postData)
        {
            byte[] secret = Convert.FromBase64String(_secret);
            string noncePostData = String.Concat(nonce, postData);
            byte[] pathBytes = Encoding.UTF8.GetBytes(path);

            var sha256 = SHA256.Create();
            byte[] hash256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(noncePostData));

            byte[] sigBytes = new byte[pathBytes.Length + hash256.Length];
            pathBytes.CopyTo(sigBytes, 0);
            hash256.CopyTo(sigBytes, pathBytes.Length);

            using (var sha512 = new HMACSHA512(secret))
            {
                byte[] sign = sha512.ComputeHash(sigBytes);
                return Convert.ToBase64String(sign);
            }
        }

        /// <summary>
        /// Get a list of errors, if there are errors in the given json
        /// </summary>
        /// <param name="json">The json response from the Kraken API</param>
        /// <returns>A list of errors</returns>
        private List<Error> GetErrorsFromJson(String json)
        {
            var list = new List<Error>();
            if (String.IsNullOrWhiteSpace(json))
                return list;

            JObject jObj = JObject.Parse(json);
            JToken token = jObj.SelectToken("error");

            foreach (JToken eToken in token.Children())
            {
                var errorText = (string) eToken;
                if (!String.IsNullOrWhiteSpace(errorText))
                    list.Add(new Error(errorText));
            }

            return list;
        }

        private void RethrowKrakenException(AggregateException ex)
        {
            if (ex.InnerException.GetType() == typeof(KrakenException))
                throw ex.InnerException;
        }

        #endregion private / protected

        #endregion Methods
    }
}