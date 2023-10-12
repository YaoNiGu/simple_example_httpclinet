// See https://aka.ms/new-console-template for more information
using httpclinet;
using Microsoft.Extensions.DependencyInjection;


Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
Startup startup = new Startup();
startup.ConfigureServices(services);
IServiceProvider serviceProvider = services.BuildServiceProvider();


//以下才是正式呼叫的程式
var service = serviceProvider.GetService<GetTwseDataService>();
//爬每日檔案
var stockDailyTradingInfoResult = service!.GetStockDailyTradingInfo().Result;


var dataService = serviceProvider.GetService<DataProcessingService>();

#region 沒table就建table
var existTableName = dataService!.QueryTableName().Result.ToArray();
var codeDic = stockDailyTradingInfoResult.ToDictionary(n =>
@$"{n.Name!.Replace("+", "plus")
           .Replace("&", "and")
           .Replace("-", "dash")
           .Replace(" ", "")
           .Replace("*", "star")}_{n.Code}", n => new StockDailyTradingDbModel(n));
var createTableList = codeDic.Keys.Except(existTableName).ToList();
dataService!.CreateStockDailyTradingTable(createTableList);
#endregion

dataService!.InsertOrUpdateStockDailyTradingTable(codeDic);


Console.WriteLine(stockDailyTradingInfoResult);