using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using httpclinet;
using System.Text;

public class DataProcessingService
{
    private readonly string connString;

    /// <summary>
    /// 取得臺灣證券交易所資料的服務
    /// </summary>
    /// <param name="httpClientFactory">httpClientFactory</param>
    /// <param name="configuration">configuration</param>
    public DataProcessingService(IConfiguration configuration)
    {
        connString = configuration["dbString"] ?? "";
    }

    /// <summary>
    /// 股票每日交易資訊
    /// </summary>
    /// <returns></returns>
    public async Task<string[]> QueryTableName()
    {
        using (var conn = new SqlConnection(connString))
        {



            var returnData = await conn.QueryAsync<string>("SELECT [name] FROM sys.tables");
            return returnData.ToArray();
        }
    }


    public void CreateStockDailyTradingTable(IEnumerable<string> tableNmaes)
    {
        using (var conn = new SqlConnection(connString))
        {
            //其實不該寫for去塞資料跟建資料，之後再改
            foreach (var item in tableNmaes)
            {
                //這裡有可能被sql injection
                //如果tableNmae帶有攻擊性字串，就會有問題
                //但我懶得改了 就信任證券交易資料吧
                conn.Execute(@$"CREATE TABLE {item}
                                (
                                    DataDate datetimeoffset(7) not null,
                                    Code varchar(10) NOT NULL,
                                    Name nvarchar(50) NOT NULL,
                                    TradeVolume bigint NULL,
                                    TradeValue decimal (18,5) NULL, 
                                    OpeningPrice decimal (18,5) NULL, 
                                    HighestPrice decimal (18,5) NULL, 
                                    LowestPrice decimal (18,5) NULL, 
                                    ClosingPrice decimal (18,5) NULL,
                                    Change decimal  NULL,
                                    [Transaction] int  NULL,
                                    PRIMARY KEY (DataDate,Code)
                                );");
            }
        }
    }



    public void InsertOrUpdateStockDailyTradingTable(Dictionary<string, StockDailyTradingDbModel> dics)
    {
        var insertDate = DateTimeOffset.Now.Date;
        using (var conn = new SqlConnection(connString))
        {
            foreach (var item in dics)
            {
                Console.WriteLine($"正在塞入{item.Key}的資料...");
                item.Value.DataDate = insertDate;
                conn.Execute(@$"update {item.Key}  SET TradeVolume = @TradeVolume,
                                    TradeValue = @TradeValue,
                                    OpeningPrice = @OpeningPrice,
                                    HighestPrice = @HighestPrice,
                                    LowestPrice = @LowestPrice,
                                    ClosingPrice = @ClosingPrice,
                                    Change = @Change,
                                    [Transaction] = @Transaction
                                WHERE DataDate =@DataDate and Code = @Code;
                                --異動資料0筆的話新增
                                if (@@rowcount = 0)
                                --新增
                                INSERT INTO {item.Key} (DataDate,Code, Name, TradeVolume, TradeValue, OpeningPrice, HighestPrice, LowestPrice, ClosingPrice, Change, [Transaction])
                                VALUES (@DataDate,@Code, @Name, @TradeVolume, @TradeValue, @OpeningPrice, @HighestPrice, @LowestPrice, @ClosingPrice, @Change, @Transaction);",item.Value);                    
            }
        }
    }
}

