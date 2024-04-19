using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace httpclinet
{
    /// <summary>
    /// 股票每日交易資訊
    /// </summary>
    public class StockDailyTrading
    {
        public StockDailyTrading(string? code = null,
            string? name = null,
            long? tradeVolume = null,
            long? tradeValue = null,
            decimal? openingPrice = null,
            decimal? highestPrice = null,
            decimal? lowestPrice = null,
            decimal? closingPrice = null,
            decimal? change = null,
            long? transaction = null)
        {
            Code = code;
            Name = name;
            TradeVolume = tradeVolume;
            TradeValue = tradeValue;
            OpeningPrice = openingPrice;
            HighestPrice = highestPrice;
            LowestPrice = lowestPrice;
            ClosingPrice = closingPrice;
            Change = change;
            Transaction = transaction;
        }



        /// <summary>證券代號</summary>
        public string? Code { get; set; }
        /// <summary>證券名稱</summary>
        public string? Name { get; set; }
        /// <summary>成交股數</summary>
        [JsonConverter(typeof(StringToLongConverter))]
        public long? TradeVolume { get; set; }
        /// <summary>成交金額</summary>
        [JsonConverter(typeof(StringToLongConverter))]
        public long? TradeValue { get; set; }
        /// <summary>開盤價</summary>
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? OpeningPrice { get; set; }
        /// <summary>最高價</summary>
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? HighestPrice { get; set; }
        /// <summary>最低價</summary>
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? LowestPrice { get; set; }
        /// <summary>收盤價</summary>
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? ClosingPrice { get; set; }
        /// <summary>漲跌價差</summary>  
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? Change { get; set; }
        /// <summary> 成交筆數 </summary>       
        [JsonConverter(typeof(StringToLongConverter))]
        public long? Transaction { get; set; }
    }


    //之後再移動吧
    public sealed class StringToLongConverter : JsonConverter<long?>
    {
        public override long? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return long.TryParse(reader.GetString(), out long value) ? value : null;
        }

        public override void Write(
            Utf8JsonWriter writer,
            long? value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }

    public sealed class StringToDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return decimal.TryParse(reader.GetString(), out decimal value) ? value : null;
        }

        public override void Write(
            Utf8JsonWriter writer,
            decimal? value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }
}
