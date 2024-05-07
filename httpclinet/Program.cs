// See https://aka.ms/new-console-template for more information
using httpclinet;
using httpcustom;
using Microsoft.Extensions.DependencyInjection;


Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
Startup startup = new Startup();
startup.ConfigureServices(services);
IServiceProvider serviceProvider = services.BuildServiceProvider();

var dataProcessingHandler = new DataProcessingHandler(serviceProvider);

//起始時間結束時間你自己訂
// var endDate = DateTimeOffset.Now;

// for (var startDate = endDate.AddYears(-1); startDate < endDate; startDate.AddMonths(1))
// {
//     var lastDayOfMonth = new DateTime(startDate.Year, startDate.Month + 1, 1).AddDays(-1);
//     dataProcessingHandler.SetStockDailyTradingTable(lastDayOfMonth);
// }

// for (var startDate = endDate.AddYears(-1); startDate < endDate; startDate.AddDays(1))
// {
//     dataProcessingHandler.CalculateMovingAverageType(startDate);
// }


////塞入資料
// dataProcessingHandler.SetStockDailyTradingTable();
////確認欄位(平常可以關掉)
// dataProcessingHandler.CheckTpexTable();
dataProcessingHandler.SetTpexTable();
////計算平均
//dataProcessingHandler.CalculateMovingAverageType();
