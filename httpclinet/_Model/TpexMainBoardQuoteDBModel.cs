namespace httpcustom
{
    /// <summary>
    /// 上櫃股票每日交易模組
    /// </summary>
    public class TpexMainBoardQuoteDBModel : TpexMainBoardQuote
    {
        public TpexMainBoardQuoteDBModel(TpexMainBoardQuote boardQuote)
        {
            SecuritiesCompanyCode   = boardQuote.SecuritiesCompanyCode  ;
            CompanyName  = boardQuote.CompanyName ;
            Close  = boardQuote.Close ;
            Change  = boardQuote.Change ;
            Open  = boardQuote.Open ;
            High  = boardQuote.High ;
            Low  = boardQuote.Low ;
            TradingShares  = boardQuote.TradingShares ;
            Change = boardQuote.Change;
            TransactionAmount  = boardQuote.TransactionAmount ;
            TransactionNumber  = boardQuote.TransactionNumber ;
            LatestBidPrice   = boardQuote.LatestBidPrice;
            LatesAskPrice    = boardQuote.LatesAskPrice ;
            Capitals    = boardQuote.Capitals ;
            NextLimitUp    = boardQuote.NextLimitUp;
            NextLimitDown   = boardQuote.NextLimitDown ;
        }

        /// <summary>
        /// 資料塞入日期
        /// </summary>
        public DateTimeOffset? DataDate { get; set; }
    }
}
