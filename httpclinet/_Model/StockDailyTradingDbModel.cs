using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpclinet
{
    /// <summary>
    /// 股票每日交易模組
    /// </summary>
    public class StockDailyTradingDbModel : StockDailyTrading
    {
        public StockDailyTradingDbModel(StockDailyTrading stock)
        {
            Code = stock.Code;
            Name = stock.Name;
            TradeVolume = stock.TradeVolume;
            TradeValue = stock.TradeValue;
            OpeningPrice = stock.OpeningPrice;
            HighestPrice = stock.HighestPrice;
            LowestPrice = stock.LowestPrice;
            ClosingPrice = stock.ClosingPrice;
            Change = stock.Change;
            Transaction = stock.Transaction;
        }

        /// <summary>
        /// 資料塞入日期
        /// </summary>
        public DateTimeOffset? DataDate { get; set; }
    }
}
