using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using httpclinet;

public class GetTpexDataService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string tpexUrl;

    //之後再看要不要都搬去組態
    //上市個股日成交資訊
    private readonly string closingQuotesOfStocks = "/tpex_mainboard_quotes";
    /// <summary>
    /// 取得臺灣證券交易所資料的服務
    /// </summary>
    /// <param name="httpClientFactory">httpClientFactory</param>
    /// <param name="configuration">configuration</param>
    public GetTpexDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        tpexUrl = configuration["tpexUrl"] ?? "";
    }

    /// <summary>
    /// 股票每日交易資訊
    /// </summary>
    /// <returns></returns>
    public async Task<TpexMainBoardQuote[]> GetclosingQuotesOfStocksInfo()
        => await SendRequest<TpexMainBoardQuote[]>(closingQuotesOfStocks);



    //先預設都get，之後有要改再說
    //parameters先隨便用dictory接，有要改再說
    private async Task<T> SendRequest<T>(string url, Dictionary<string, string> parameters = null)
    {
        using var httpClient = httpClientFactory.CreateClient();
        url = twseUrl + url;
        if (parameters != null)
            url = url + "?" + string.Join('&', parameters.Select(n => $"{n.Key}={n.Value}"));
        using var responseMessage = await httpClient.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode) throw new HttpRequestException(await responseMessage.Content.ReadAsStringAsync());
        var response = await responseMessage.Content.ReadFromJsonAsync<T>();
        return response;
    }
}

