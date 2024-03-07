using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using httpclinet;
using System.Text;
using httpclinet._Define;
using System.Data.Common;

public class DataProcessingService
{
    private readonly string connString;

    /// <summary>
    /// 資料處理服務
    /// </summary>
    /// <param name="httpClientFactory">httpClientFactory</param>
    /// <param name="configuration">configuration</param>
    public DataProcessingService(IConfiguration configuration)
    {
        connString = configuration["dbString"] ?? "";
    }

    /// <summary>
    /// 股取得存在的表單名稱(股票名稱)
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


    public void CheckTableColumns(IEnumerable<string> tableNames)
    {
        // 假设conn是你的数据库连接对象
        // columnsToCheck包含你想要确保存在的列名和它们的数据类型
        var columnsToCheck = new Dictionary<string, string>
            {
                {"TradeVolume", "bigint"},
                {"TradeValue", "decimal(18,5)"},
                {"OpeningPrice", "decimal(18,5)"},
                {"HighestPrice", "decimal(18,5)"},
                {"LowestPrice", "decimal(18,5)"},
                {"ClosingPrice", "decimal(18,5)"},
                {"Change", "decimal"},
                {"Transaction", "int"},
                {"FiveDayMovingAverage","decimal (18,5)"},
                {"TenDayMovingAverage", "decimal (18,5)"},
                {"TwentyDayMovingAverage","decimal (18,5)"},
                {"SixtyDayMovingAverage", "decimal (18,5)"},
            };
        using (var conn = new SqlConnection(connString))
        {
            foreach (var tableName in tableNames)
            {
                foreach (var column in columnsToCheck)
                {
                    Console.WriteLine($"正在確認{tableName}的欄位:{column.Key}...");
                    var checkColumnExistenceQuery = @$"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                         WHERE TABLE_NAME = N'{tableName}' 
                                         AND COLUMN_NAME = '{column.Key}')
                                         BEGIN
                                         ALTER TABLE {tableName} ADD [{column.Key}] {column.Value} NULL
                                         END";
                    conn.Execute(checkColumnExistenceQuery);
                }
            }
        }
    }


    /// <summary>
    /// 依照表單名稱(股票名稱)建立TABLE
    /// </summary>
    /// <param name="tableNmaes">表單名稱集合</param>
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
                Console.WriteLine($"正在建立{item}...");
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
                                    FiveDayMovingAverage decimal (18,5)  NULL, 
                                    TenDayMovingAverage decimal (18,5)   NULL, 
                                    TwentyDayMovingAverage decimal (18,5)  NULL, 
                                    SixtyDayMovingAverage decimal (18,5) NULL 
                                    PRIMARY KEY (DataDate,Code)
                                );");
            }
        }
    }


    /// <summary>
    /// 依照取得的資料塞入各TABLE
    /// </summary>
    /// <param name="dics"></param>
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
                                VALUES (@DataDate,@Code, @Name, @TradeVolume, @TradeValue, @OpeningPrice, @HighestPrice, @LowestPrice, @ClosingPrice, @Change, @Transaction);", item.Value);
            }
        }
    }


    /// <summary>
    /// 更新移動平均線資料
    /// </summary>
    public void UpdateMovingAverage(IEnumerable<string> tableNmaes, MovingAverageType? averageType = null, DateTimeOffset? targetDate = null)
    {
        targetDate ??= DateTimeOffset.Now.Date;
        var stringBuilder = new StringBuilder();
        var averageDay =
           averageType switch
           {
               MovingAverageType.FiveDayMovingAverage => 4,
               MovingAverageType.TenDayMovingAverage => 9,
               MovingAverageType.TwentyDayMovingAverage => 19,
               MovingAverageType.SixtyDayMovingAverage => 59,
               _ => throw new NotImplementedException()
           };

        using (var conn = new SqlConnection(connString))
        {
            foreach (var item in tableNmaes)
            {
                stringBuilder.Append(@$"
                    UPDATE {item}
                        SET {averageType.ToString()} = (              
                               SELECT AVG(ClosingPrice)
                                   FROM (
                                       SELECT ClosingPrice
                                       FROM {item}
                                      WHERE DataDate >= DATEADD(DAY, -{averageDay}, @TargetDate) AND DataDate <= @TargetDate
                                   ) AS SubQuery
                               )
                        WHERE DataDate = @TargetDate;");
                Console.WriteLine($"正在塞入{item}的{averageType.ToString()}...");
                conn.Execute(stringBuilder.ToString(), new { TargetDate = targetDate });
                stringBuilder.Clear();
            }
        }
    }
}

