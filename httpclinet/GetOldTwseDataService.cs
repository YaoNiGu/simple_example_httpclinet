using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using httpclinet;

public class GetOldTwseDataService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string oldTwseUrl;

    //之後再看要不要都搬去組態
    //上市個股日成交資訊
    private readonly string stockDailyTradingInfoByDate = "/exchangeReport/STOCK_DAY";

    /// <summary>
    /// 取得臺灣證券交易所資料的服務
    /// </summary>
    /// <param name="httpClientFactory">httpClientFactory</param>
    /// <param name="configuration">configuration</param>
    public GetOldTwseDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        oldTwseUrl = configuration["oldTwseUrl"] ?? "";
    }


    /// <summary>
    /// 藉由時間去抓股票每日交易資訊
    /// </summary>
    /// <param name="date">時間</param>
    /// <returns></returns>
    public async Task<StocksTradeMonthly> GetStockDailyTradingInfoByDateAndStockNo(DateTimeOffset date, string stockNo)
        => await
        SendRequest<StocksTradeMonthly>(stockDailyTradingInfoByDate,
            new Dictionary<string, string>() {
                {"response","json" },
                { nameof(date), date.ToString("yyyyMMdd") },
                {nameof(stockNo),stockNo }
            });


    //先預設都get，之後有要改再說
    //parameters先隨便用dictory接，有要改再說
    private async Task<T> SendRequest<T>(string url, Dictionary<string, string> parameters = null)
    {
        using var httpClient = httpClientFactory.CreateClient();
        url = oldTwseUrl + url;
        if (parameters != null)
            url = url + "?" + string.Join('&', parameters.Select(n => $"{n.Key}={n.Value}"));
        using var responseMessage = await httpClient.GetAsync(url);
        // 印出抓取的url，確認有沒有錯
        Console.WriteLine(url);
        if (!responseMessage.IsSuccessStatusCode) throw new HttpRequestException(await responseMessage.Content.ReadAsStringAsync());
        var response = await responseMessage.Content.ReadFromJsonAsync<T>();
        return response;
    }
}

