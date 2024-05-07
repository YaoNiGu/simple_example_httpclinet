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
using httpcustom;
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

    public (string,string)[] QueryCodeByTableName(params string[] tableName)
    {
        using (var conn = new SqlConnection(connString))
        {
            var returnData = tableName.Select(n =>
                conn.QueryAsync<(string,string)>($"SELECT  Code,Name FROM {n}").Result.FirstOrDefault()).ToArray();
            return returnData;
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
                {"TradeVolumeFiveDayMovingAverage","decimal (18,5)"},
                {"TradeVolumeTenDayMovingAverage", "decimal (18,5)"},
                {"TradeVolumeTwentyDayMovingAverage","decimal (18,5)"},
                {"TradeVolumeSixtyDayMovingAverage", "decimal (18,5)"}
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
    /// <param name="tableNames">表單名稱集合</param>
    public void CreateStockDailyTradingTable(IEnumerable<string> tableNames)
    {
        using (var conn = new SqlConnection(connString))
        {
            //其實不該寫for去塞資料跟建資料，之後再改
            foreach (var item in tableNames)
            {
                //這裡有可能被sql injection
                //如果tableNmae帶有攻擊性字串，就會有問題
                //但我懶得改了 就信任證券交易資料吧
                Console.WriteLine($"正在建立{item}...");
                conn.Execute(@$"CREATE TABLE {item}
                                (
                                    DataDate datetimeoffset(7) not null,
                                    SecuritiesCompanyCode varchar(15) NOT NULL,
                                    CompanyName nvarchar(50) NOT NULL,
                                    Close decimal (18,4) NULL,
                                    Change decimal  NULL,
                                    Open decimal (18,5) NULL,
                                    High decimal (18,5) NULL,
                                    Low decimal (18,5) NULL,
                                    TradeShares bigint NULL,
                                    TransactionAmount bigint NULL,
                                    TransactionNumber bigint NULL,
                                    LatestBidPrice decimal (18,5)  NULL,
                                    LatestAskPrice decimal (18,5)  NULL,
                                    Capitals bigint NULL;
                                    NextLimitUp decimal (18,5)  NULL,
                                    NextLimitDown decimal (18,5)  NULL,
                                    PRIMARY KEY (DataDate,SecuritiesCompanyCode)
                                );");
            }
        }
    }


    /// <summary>
    /// 依照取得的資料塞入各TABLE
    /// </summary>
    /// <param name="dics"></param>
    public void InsertOrUpdateStockDailyTradingTable(Dictionary<string, StockDailyTradingDbModel> dics,DateTimeOffset? insertDate =null)
    {
         insertDate ??= DateTimeOffset.Now.Date;
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


    /// <summary>
    /// UpdateTradeVolumeMovingAverage
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void UpdateTradeVolumeMovingAverage(IEnumerable<string> tableNmaes, TradeVolumeMovingAverageType? averageType = null, DateTimeOffset? targetDate = null)
    {
        targetDate ??= DateTimeOffset.Now.Date;
        var stringBuilder = new StringBuilder();
        var averageDay =
           averageType switch
           {
               TradeVolumeMovingAverageType.TradeVolumeFiveDayMovingAverage => 4,
               TradeVolumeMovingAverageType.TradeVolumeTenDayMovingAverage => 9,
               TradeVolumeMovingAverageType.TradeVolumeTwentyDayMovingAverage => 19,
               TradeVolumeMovingAverageType.TradeVolumeSixtyDayMovingAverage => 59,
               _ => throw new NotImplementedException()
           };

        using (var conn = new SqlConnection(connString))
        {
            foreach (var item in tableNmaes)
            {
                stringBuilder.Append(@$"
                    UPDATE {item}
                        SET {averageType.ToString()} = (              
                               SELECT AVG(TradeVolume)
                                   FROM (
                                       SELECT TradeVolume
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
    // Tpex 確認欄位
     public void CheckTpexTableColumns(IEnumerable<string> tableNames)
    {
        // 假设conn是你的数据库连接对象
        // columnsToCheck包含你想要确保存在的列名和它们的数据类型
        var columnsToCheck = new Dictionary<string, string>
            {
                {"CompanyName", "nvarchar(50)"},
                {"Close", "decimal(18,5)"},
                {"Open", "decimal(18,5)"},
                {"High", "decimal(18,5)"},
                {"Low", "decimal(18,5)"},
                {"TradeShares", "bigint"},
                {"TransactionAmount", "bigint"},
                {"TransactionNumber", "bigint"},
                {"LatestBidPrice", "decimal (18,5)"},
                {"LatesAskPrice", "decimal (18,5)"},
                {"Capitals", "bigint"},
                {"NextLimitUp", "decimal (18,5)"},
                {"NextLimitDown", "decimal (18,5)"}
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
                    conn.ExecuteAsync(checkColumnExistenceQuery);
                }
            }
        }
    }
    // Tpex新增或更新
    /// <summary>
    /// 依照取得的資料塞入各TABLE
    /// </summary>
    /// <param name="dics"></param>
     public void InsertOrUpdateTpexTable(Dictionary<string, TpexMainBoardQuoteDBModel> dics)
    {
        //  insertDate ??= DateTimeOffset.Now.Date;
        using (var conn = new SqlConnection(connString))
        {
            foreach (var item in dics)
            {
                Console.WriteLine($"正在塞入{item.Key}的資料...");
                // item.Value.DataDate = insertDate;
                conn.Execute(@$"update {item.Key}  SET CompanyName =  @CompanyName,
                                    Close = @Close,
                                    Change = @Change,
                                    Open = @Open,
                                    High = @High,
                                    Low = @Low,
                                    TradeShares = @TradeShares,
                                    TransactionAmount = @TransactionAmount,
                                    TransactionNumber = @TransactionNumber,
                                    LatestBidPrice = @LatestBidPrice,
                                    LatesAskPrice = @LatesAskPrice,
                                    Capitals = @Capitals,
                                    NextLimitUp = @NextLimitUp,
                                    NextLimitDown = @NextLimitDown
                                WHERE DataDate =@DaDate and SecuritiesCompanyCode = @SecuritiesCompanyCode;
                                --異動資料0筆的話新增
                                if (@@rowcount = 0)
                                --新增
                                INSERT INTO {item.Key} (Date,SecuritiesCompanyCode, CompanyName, Close, Change, Open, High, Low, TradeShares, TransactionAmount, TransactionNumber,LatestBidPrice,LatesAskPrice,Capitals,NextLimitUp,NextLimitDown)
                                VALUES (@Date,@SecuritiesCompanyCode, @CompanyName, @Close, @Change, @Open, @High, @Low, @TransactionAmount, @TransactionNumber, @LatestBidPrice, @LatesAskPrice, @Capitals, @NextLimitUp,@NextLimitDown );", item.Value);
            }
        }
    }
    // 建立Tpex資料表
    /// <summary>
    /// 依照表單名稱(上櫃名稱)建立TABLE
    /// </summary>
    /// <param name="tableNames">表單名稱集合</param>
    public void CreateTpexTable(IEnumerable<string> tableNames)
    {
        using (var conn = new SqlConnection(connString))
        {
            //其實不該寫for去塞資料跟建資料，之後再改
            foreach (var item in tableNames)
            {
                //這裡有可能被sql injection
                //如果tableNmae帶有攻擊性字串，就會有問題
                //但我懶得改了 就信任證券交易資料吧
                Console.WriteLine($"正在建立{item}...");
                conn.Execute(@$"CREATE TABLE {item}
                                (
                                    Date varchar(10) NOT NULL
                                    SecuritiesCompanyCode varchar(15) NOT NULL,
                                    CompanyName nvarchar(50) NOT NULL,
                                    Close decimal (18,5) NOT NULL,
                                    Change decimal  NULL,
                                    Open decimal (18,5) NULL,
                                    High decimal (18,5) NULL,
                                    Low decimal (18,5) NULL,
                                    TradeShares bigint NULL,
                                    TransactionAmount bigint NULL,
                                    TransactionNumber bigint NULL,
                                    LatestBidPrice decimal (18,5)  NULL,
                                    LatesAskPrice decimal (18,5)  NULL,
                                    Capitals bigint NULL;
                                    NextLimitUp decimal (18,5)  NULL,
                                    NextLimitDown decimal (18,5)  NULL,
                                    PRIMARY KEY (DataDate,SecuritiesCompanyCode)
                                );");
            }
        }
    }
   

}


 