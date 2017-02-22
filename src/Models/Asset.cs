using System;

using Newtonsoft.Json;

namespace Kraken.Models
{
    /// <summary>
    /// A asset from the Kraken API
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// The class / typ of the asset
        /// </summary>
        [JsonProperty("aclass")]
        public String AClass { get; set; }

        /// <summary>
        /// The aseet's name
        /// </summary>
        [JsonProperty("altname")]
        public String Name { get; set; }

        /// <summary>
        /// The number of decimals
        /// </summary>
        [JsonProperty("decimals")]
        public Int32 Decimals { get; set; }

        /// <summary>
        /// The number of decimals that will be displayed
        /// </summary>
        [JsonProperty("display_decimals")]
        public Int32 DisplayDecimals { get; set; }
    }
}