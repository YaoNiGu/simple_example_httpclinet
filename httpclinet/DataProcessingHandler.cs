using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using httpclinet;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using httpclinet._Define;

public class DataProcessingHandler
{
    //dataProcessingService(用於處理資料與DB溝通的服務)
    private static DataProcessingService dataProcessingService;
    //GetTwseDataService(用於與證券平台溝通的服務)
    private static GetTwseDataService getTwseDataService;
    /// <summary>
    /// 資料處理服務
    /// </summary>
    /// <param name="httpClientFactory">httpClientFactory</param>
    /// <param name="configuration">configuration</param>
    public DataProcessingHandler(IServiceProvider serviceProvider)
    {
        dataProcessingService = serviceProvider.GetService<DataProcessingService>()!;
        getTwseDataService= serviceProvider.GetService<GetTwseDataService>()!;
    }

    public void SetStockDailyTradingTable()
    {
        //爬每日檔案
        var stockDailyTradingInfoResult = getTwseDataService!.GetStockDailyTradingInfo().Result;

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
        dataProcessingService!.InsertOrUpdateStockDailyTradingTable(codeDic);
    }

    public void CheckStockDailyTradingTable()
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();
        dataProcessingService!.CheckTableColumns(existTableName);
    }

    public void CalculateMovingAverageType()
    {
        var existTableName = dataProcessingService!.QueryTableName().Result.ToArray();
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.FiveDayMovingAverage);
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.TenDayMovingAverage);
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.TwentyDayMovingAverage);
        dataProcessingService!.UpdateMovingAverage(existTableName, MovingAverageType.SixtyDayMovingAverage);



    }
}


