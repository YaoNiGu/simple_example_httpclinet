// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using httpclinet;
using Microsoft.Extensions.DependencyInjection;


Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
Startup startup = new Startup();
startup.ConfigureServices(services);
IServiceProvider serviceProvider = services.BuildServiceProvider();

var dataProcessingHandler = new DataProcessingHandler(serviceProvider);

//起始時間結束時間，手動設定的時間日期(歷史時間變數)
// var dt=new DateTime(2016,4,19);
// var endDate = new DateTimeOffset(dt);

// 設置時間計算變數，要抓的時間區間只需改動AddYears函數就好
// var lastDayOfMonth = new DateTime(endDate.AddYears(-1).Year, endDate.Month + 1, 1).AddDays(-1);

// 跑當天股票用的變數
// var endDate =  DateTimeOffset.Now;
// var today = endDate.AddDays(-1);

// for (var startDate =  today ; startDate < endDate; startDate = startDate.AddDays(1))
// {
//     // Console.WriteLine(startDate);
//     dataProcessingHandler.SetStockDailyTradingTable(startDate);
// }

// 計算均線，日期由過去往現在
var tmp = new DateTime(2024,4,1);
var end = DateTimeOffset.Now.AddDays(-1);
var begin_date = new DateTimeOffset(tmp);
for (var  i = begin_date;i<end;i = i.AddDays(1)){
    // Console.WriteLine(i);
    dataProcessingHandler.CalculateMovingAverageType(i);
}


// 原本計算均線的程式 
// for (var startDate = endDate.AddYears(-2); startDate < endDate; startDate =startDate.AddDays(1))
// {
//     dataProcessingHandler.CalculateMovingAverageType(startDate);
// }


//塞入資料
// dataProcessingHandler.SetStockDailyTradingTable();
// //確認欄位(平常可以關掉)
// dataProcessingHandler.CheckStockDailyTradingTable();
////計算平均
//dataProcessingHandler.CalculateMovingAverageType();
