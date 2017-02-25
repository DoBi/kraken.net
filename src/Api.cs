using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Kraken.Models;

namespace Kraken
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

        protected const String MediaType = "application/x-www-form-urlencoded";

        /// <summary>
        /// The API key
        /// </summary>
        protected String _key;
        /// <summary>
        /// The API secret
        /// </summary>
        protected String _secret;
        /// <summary>
        /// The URL to the API
        /// </summary>
        protected String _url;
        /// <summary>
        /// The API version
        /// </summary>
        protected String _version;

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
        public Api(String key, String secret, String url, String version)
        {
            _key = key;
            _secret = secret;
            _url = url;
            _version = version;
        }

        #endregion Constructors

        #region Methods

        #region public
        
        #region async Methods

        /// <summary>
        /// Get asset info
        /// </summary>
        /// <param name="asset">comma delimited list of assets to get info on</param>
        /// <returns>A list with the requested assets</returns>
        public async Task<IList<Asset>> GetAssetsAsync(String asset)
        {
            var parameters = new Dictionary<String, String>();
            if (!String.IsNullOrWhiteSpace(asset))
                parameters.Add("asset", asset);

            string json = await QueryPublicAsync("Assets", parameters);

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
            var parameters = new Dictionary<String, String>();
            parameters.Add("trades", includeTrades.ToString());
            
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

            using (var client = new HttpClient())
            {
                string address = String.Format("{0}/{1}/public/{2}", _url, _version, method);
                var content = new StringContent(postData, Encoding.UTF8, MediaType);

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

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("API-Key", _key);
                client.DefaultRequestHeaders.Add("API-Sign", signature);

                string address = String.Concat(_url, path);
                var content = new StringContent(postData, Encoding.UTF8, MediaType);

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
        /// Get asset info
        /// </summary>
        /// <param name="asset">comma delimited list of assets to get info on</param>
        /// <returns>A list with the requested assets</returns>
        public IList<Asset> GetAssets(String asset)
        {
            return GetAssetsAsync(asset).Result;
        }

        /// <summary>
        /// Get the current users account balance
        /// </summary>
        /// <returns>array of asset names and balance amount</returns>
        public String GetBalance()
        {
            return GetBalanceAsync().Result;
        }

        /// <summary>
        /// Gets all open orders from the current user
        /// </summary>
        /// <param name="includeTrades">Whether or not to include trades in output</param>
        /// <param name="userReferenceId">Restrict results to given user reference id</param>
        /// <returns></returns>
        public String GetOpenOrders(Boolean includeTrades = false, String userReferenceId = null)
        {
            return GetOpenOrdersAsync(includeTrades, userReferenceId).Result;
        }

        /// <summary>
        /// Get the time from the Kraken server
        /// </summary>
        /// <returns>The current time from the Kraken server</returns>
        public DateTime GetServerTime()
        {
            return GetServerTimeAsync().Result;
        }

        /// <summary>
        /// Gets the trade balance
        /// </summary>
        /// <param name="asset"></param>
        /// <returns>The raw result from the api</returns>
        public String GetTradeBalance(String asset)
        {
            return GetTradeBalanceAsync(asset).Result;
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

        #endregion private / protected

        #endregion Methods
    }
}