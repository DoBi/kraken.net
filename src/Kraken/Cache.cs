using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Kraken.Models;

namespace Kraken
{
    /// <summary>
    /// An internal cache class for caching some not necessarily important objects
    /// </summary>
    internal class Cache
    {
        /// <summary>
        /// A reference of the api object
        /// </summary>
        private Api _api;
        /// <summary>
        /// The timespan to cache the objects
        /// </summary>
        private TimeSpan _timespan;

        /// <summary>
        /// All saved assets
        /// </summary>
        private KeyValuePair<IList<Asset>, DateTime> _assets;
        /// <summary>
        /// All saved asset pairs
        /// </summary>
        private KeyValuePair<IList<AssetPair>, DateTime> _assetPairs;

        /// <summary>
        /// Returns if the cache is activated
        /// </summary>
        public Boolean Active { get; }

        /// <summary>
        /// Create a new cache object, with default timespan
        /// </summary>
        /// <param name="api">The current api object</param>
        /// <param name="active">Should some objects be cached</param>
        public Cache(Api api, bool active) : this(api, active, new TimeSpan(0, 15, 0))
        {}

        /// <summary>
        /// Create a new cache object
        /// </summary>
        /// <param name="api">The current api object</param>
        /// <param name="active">Should some objects be cached</param>
        /// <param name="timespan">The default timespan to cache the objects</param>
        public Cache(Api api, bool active, TimeSpan timespan)
        {
            _api = api;
            Active = active;
            _timespan = timespan;
        }

        /// <summary>
        /// Get all assets
        /// </summary>
        /// <returns>A list of all available assets</returns>
        public async Task<IList<Asset>> GetAssetsAsync()
        {
            if (Active && DateTime.Now - _assets.Value < _timespan)
                return _assets.Key;

            var assets = await _api.GetNoCacheAssetsAsync();
            _assets = new KeyValuePair<IList<Asset>, DateTime>(assets, DateTime.Now);
            return assets;
        }

        /// <summary>
        /// Get all asset pairs
        /// </summary>
        /// <returns>A list of all available asset pairs</returns>
        public async Task<IList<AssetPair>> GetAssetPairsAsync()
        {
            if (Active && DateTime.Now - _assetPairs.Value < _timespan)
                return _assetPairs.Key;

            var pairs = await _api.GetNoCacheAssetPairsAsync(String.Empty, InfoLevel.All);
            _assetPairs = new KeyValuePair<IList<AssetPair>, DateTime>(pairs, DateTime.Now);
            return pairs;
        }
    }
}