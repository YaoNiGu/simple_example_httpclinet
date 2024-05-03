using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using httpclinet;
using httpcustom;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using httpclinet._Define;
using System.Runtime.InteropServices;
using System.Data;
using System.Globalization;

public class DataProcessingHandler
{
    //dataProcessingService(用於處理資料與DB溝通的服務)
    private static DataProcessingService dataProcessingService;
    //GetTwseDataService(用於與證券平台溝通的服務)
    private static GetTwseDataService getTwseDataService;

    private static GetOldTwseDataService getOldTwseDataService;
    private static GetTpexDataService getTpexDataService;
    /// <summary>
    /// 資料處理服務
    /// </summary>
    /// <param name="httpClientFactory">httpClientFactory</param>
    /// <param name="configuration">configuration</param>
    public DataProcessingHandler(IServiceProvider serviceProvider)
    {
        dataProcessingService = serviceProvider.GetService<DataProcessingService>()!;
        getTwseDataService = serviceProvider.GetService<GetTwseDataService>()!;
        getOldTwseDataService = serviceProvider.GetService<GetOldTwseDataService>()!;
        getTpexDataService = serviceProvider.GetService<GetTpexDataService>()!;
    }

    public void SetStockDailyTradingTable()
    {
        var stockDailyTradingInfoResult = getTwseDataService!.GetStockDailyTradingInfo().Result;
        setData(stockDailyTradingInfoResult);
    }

    public void SetStockDailyTradingTable(DateTimeOffset date)
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();

        var stocks = dataProcessingService!.QueryCodeByTableName(existTableName).ToList();

        var stockDailyTradingsDic = new Dictionary<DateTimeOffset, List<StockDailyTrading>>();
        stocks.ForEach(stock =>
        {
            Console.WriteLine($"開始轉換{date.ToString("yyyyMM")}的{stock.Item1}的資料...");
            var datas = StockDailyTradings(getOldTwseDataService!.GetStockDailyTradingInfoByDateAndStockNo(date, stock.Item1).Result, stock.Item1, stock.Item2);

            foreach (var data in datas)
            {
                if (stockDailyTradingsDic.TryGetValue(data.Key, out var value))
                    stockDailyTradingsDic[data.Key].Add(data.Value);
                else
                    stockDailyTradingsDic.Add(data.Key, new List<StockDailyTrading>() { data.Value });
            }
        });
        foreach (var stock in stockDailyTradingsDic)
        {
            Console.WriteLine($"開始寫入Key)的歷史資料...");

            setData(stock.Value.ToArray(), stock.Key);
        }
    }




    private static void setData(StockDailyTrading[] stockDailyTradingInfoResult, DateTimeOffset? setDate = null)
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();

        //我就不拆服務了
        #region 沒table就建table
        var codeDic = stockDailyTradingInfoResult.ToDictionary(n =>
        @$"_{n.Code}_{n.Name!.Replace("+", "plus")
                   .Replace("&", "and")
                   .Replace("-", "dash")
                   .Replace(" ", "")
                   .Replace("*", "star")}", n => new StockDailyTradingDbModel(n));
        var createTableList = codeDic.Keys.Except(existTableName).ToList();
        dataProcessingService!.CreateStockDailyTradingTable(createTableList);
        #endregion
        //塞資料
        dataProcessingService!.InsertOrUpdateStockDailyTradingTable(codeDic, setDate);
    }

    public void CheckStockDailyTradingTable()
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();
        dataProcessingService!.CheckTableColumns(existTableName);
    }

    public void CalculateMovingAverageType(DateTimeOffset? targetDate = null)
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.FiveDayMovingAverage, targetDate);
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.TenDayMovingAverage, targetDate);
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.TwentyDayMovingAverage, targetDate);
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.SixtyDayMovingAverage, targetDate);

        dataProcessingService!.UpdateTradeVolumeMovingAverage(existTableName, TradeVolumeMovingAverageType.TradeVolumeFiveDayMovingAverage, targetDate);
        dataProcessingService!.UpdateTradeVolumeMovingAverage(existTableName, TradeVolumeMovingAverageType.TradeVolumeTenDayMovingAverage, targetDate);
        dataProcessingService!.UpdateTradeVolumeMovingAverage(existTableName, TradeVolumeMovingAverageType.TradeVolumeTwentyDayMovingAverage, targetDate);
        dataProcessingService!.UpdateTradeVolumeMovingAverage(existTableName, TradeVolumeMovingAverageType.TradeVolumeSixtyDayMovingAverage, targetDate);


    }


    private static Dictionary<DateTimeOffset, StockDailyTrading> StockDailyTradings(StocksTradeMonthly stocksTradeMonthly, string code, string name)
    {
        if (stocksTradeMonthly?.data == null)
            return new Dictionary<DateTimeOffset, StockDailyTrading>();
        return stocksTradeMonthly.data.ToDictionary(
             n => transferChineseYearToCE(n[0]),
             n => new StockDailyTrading(code,
                 name,
                 long.TryParse(n[1].Replace(",", ""), out var n1) ? n1 : 0,
                 long.TryParse(n[2].Replace(",", ""), out var n2) ? n2 : 0,
                 decimal.TryParse(n[3], out var n3) ? n3 : 0,
                 decimal.TryParse(n[4], out var n4) ? n4 : 0,
                 decimal.TryParse(n[5], out var n5) ? n5 : 0,
                 decimal.TryParse(n[6], out var n6) ? n6 : 0,
                 decimal.TryParse(n[7].Replace("+", "").Replace("X", ""), out var n7) ? n7 : 0,
                 long.Parse(n[8].Replace(",", ""))
                 ));
    }


    private static DateTimeOffset transferChineseYearToCE(string cy)
    {
        CultureInfo culture = new CultureInfo("zh-TW");
        culture.DateTimeFormat.Calendar = new TaiwanCalendar();
        return DateTimeOffset.Parse(cy, culture);
    }
    // Tpex
    public void SetTpexTable()
    {
        var TpexResult = getTpexDataService!.GetclosingQuotesOfStocksInfo().Result;
        setTpexData(TpexResult);
    }

    private static void setTpexData(TpexMainBoardQuote[] TpexInfoResult, DateTimeOffset? setDate = null)
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();

        //我就不拆服務了
        #region 沒table就建table
        var codeDic = TpexInfoResult.ToDictionary(n =>
        @$"_{n.SecuritiesCompanyCode}_{n.CompanyName!.Replace("+", "plus")
                   .Replace("&", "and")
                   .Replace("-", "dash")
                   .Replace(" ", "")
                   .Replace("*", "star")}", n => new TpexMainBoardQuoteDBModel(n));
        var createTableList = codeDic.Keys.Except(existTableName).ToList();
        dataProcessingService!.CreateTpexTable(createTableList);
        #endregion
        //塞資料
        dataProcessingService!.InsertOrUpdateTpexTable(codeDic, setDate);
    }
        public void CheckTpexTable()
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();
        dataProcessingService!.CheckTpexTableColumns(existTableName);
    }
    
}


