using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace httpclinet
{
    /// <summary>
    /// 股票每日交易資訊
    /// </summary>
    public class StocksTradeMonthly
    {
        public List<List<string>> data { get; set; }
    }

   
}
