using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace httpcustom
{
    /// <summary>
    /// 上櫃股票每日交易資訊
    /// </summary>
     public class TpexMainBoardQuote
    {

        public TpexMainBoardQuote() {
            
        }
        public TpexMainBoardQuote(string? date =null,
            string? securitiesCompanyCode = null,
            string? companyName = null,
            decimal? close = null,
            decimal? change = null,
            decimal? open = null,
            decimal? high = null,
            decimal? low = null,
            long? tradingShares = null,
            long? transactionAmount = null,
            long? transactionNumber = null,
            decimal? latestBidPrice = null,
            decimal? LatestAskPrice = null,
            long? capitals = null,
            decimal? nextLimitUp = null,
            decimal? nextLimitDown = null)
        {
            Date = date;
            SecuritiesCompanyCode  = securitiesCompanyCode;
            CompanyName  = companyName;
            Close = close;
            Change = change;
            Open = open;
            High = high;
            Low = low;
            TradingShares = tradingShares;
            TransactionAmount = transactionAmount;
            TransactionNumber = transactionNumber;
            LatestBidPrice = latestBidPrice;
            LatesAskPrice  =LatesAskPrice ;
            Capitals = capitals ;
            NextLimitUp = nextLimitUp;
            NextLimitDown = nextLimitDown;
        }
        public string? Date { get; set; }
        
        public string? SecuritiesCompanyCode { get; set; }

        public string? CompanyName { get; set; }
        
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? Close { get; set; }
        
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? Change { get; set; }
        
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? Open { get; set; }
        
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? High { get; set; }
        
        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? Low { get; set; }
        
        [JsonConverter(typeof(StringToLongConverter))]
        public long? TradingShares { get; set; }

        [JsonConverter(typeof(StringToLongConverter))]
        public long? TransactionAmount { get; set; }

        [JsonConverter(typeof(StringToLongConverter))]
        public long? TransactionNumber { get; set; }

        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? LatestBidPrice { get; set; }

        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? LatesAskPrice { get; set; }

        [JsonConverter(typeof(StringToLongConverter))]
        public long? Capitals { get; set; }

        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? NextLimitUp { get; set; }

        [JsonConverter(typeof(StringToDecimalConverter))]
        public decimal? NextLimitDown { get; set; }



    }



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

