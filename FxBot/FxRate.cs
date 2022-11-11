using Newtonsoft.Json;
using System;

namespace FxBot
{
    public class FxRate
    {
        [JsonProperty("Cur_ID")]
        public int CurrencyId { get; set; }

        [JsonProperty("Date")]
        public DateTime Date { get; set; }

        [JsonProperty("Cur_OfficialRate")]
        public double Value { get; set; }
    }
}
