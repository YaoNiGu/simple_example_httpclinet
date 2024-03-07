// See https://aka.ms/new-console-template for more information
using httpclinet;
using Microsoft.Extensions.DependencyInjection;


Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
Startup startup = new Startup();
startup.ConfigureServices(services);
IServiceProvider serviceProvider = services.BuildServiceProvider();

var dataProcessingHandler = new DataProcessingHandler(serviceProvider);

//塞入資料
dataProcessingHandler.SetStockDailyTradingTable();
//確認欄位(平常可以關掉)
dataProcessingHandler.CheckStockDailyTradingTable();
//計算平均
dataProcessingHandler.CalculateMovingAverageType();
