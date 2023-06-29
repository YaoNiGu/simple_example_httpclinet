// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
Startup startup = new Startup();
startup.ConfigureServices(services);
IServiceProvider serviceProvider = services.BuildServiceProvider();


//以下才是正式呼叫的程式
var service = serviceProvider.GetService<GetTwseDataService>();
var stockDailyTradingInfoResult = service!.GetStockDailyTradingInfo().Result;

Console.WriteLine(stockDailyTradingInfoResult);