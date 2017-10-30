using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kraken.Net.Models
{
    public class AssetPair
    {
        [JsonProperty("altname")]
        public String Name { get; set; }

        [JsonProperty("base")]
        public String BaseAlias { get; set; }

        public Asset Base { get; set; }

        [JsonProperty("quote")]
        public String QuoteAlias { get; set; }

        public Asset Quote { get; set; }

        [JsonProperty("lot")]
        public String Lot { get; set; }

        [JsonProperty("pair_decimals")]
        public Int32 Decimals { get; set; }

        [JsonProperty("lot_decimals")]
        public Int32 LotDecimals { get; set; }

        [JsonProperty("lot_multiplier")]
        public Int32 LotMultiplier { get; set; }

        [JsonProperty("leverage_buy")]
        public IList<Int32> LeverageBuy { get; set; }

        [JsonProperty("leverage_sell")]
        public IList<Int32> LeverageSell { get; set;}

        [JsonProperty("fees")]
        public IList<IList<Decimal>> Fees { get; set; }

        [JsonProperty("fees_maker")]
        public IList<IList<Decimal>> MakerFees { get; set; }

        [JsonProperty("fee_volume_currency")]
        public String FeeVolumeCurrencyAlias { get; set;}

        [JsonProperty("margin_call")]
        public Int32 MarginCall { get; set; }

        [JsonProperty("margin_stop")]
        public Int32 MarginStop { get; set; }
    }
}