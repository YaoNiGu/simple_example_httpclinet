using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace httpclinet
{

    public class TpexMainBoardQuote
    {
        public string Date { get; set; }
        public string SecuritiesCompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Close { get; set; }
        public string Change { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string TradingShares { get; set; }
        public string TransactionAmount { get; set; }
        public string TransactionNumber { get; set; }
        public string LatestBidPrice { get; set; }
        public string LatesAskPrice { get; set; }
        public string Capitals { get; set; }
        public string NextLimitUp { get; set; }
        public string NextLimitDown { get; set; }
    }
}
