﻿using CellWars.Threading;
using CCXT.NET.Shared.Coin.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

//#pragma warning disable EF1001

namespace CCXT.NET.Shared.Coin.Public
{
    /// <summary>
    ///
    /// </summary>
    public interface IPublicApi
    {
        /// <summary>
        ///
        /// </summary>
        XApiClient publicClient
        {
            get;
            set;
        }

        /// <summary>
        /// Preloading markets when reload is true or first function call
        /// </summary>
        /// <param name="reload">reload market information</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<Markets> LoadMarketsAsync(bool reload, Dictionary<string, object> args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="marketId"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<Market> LoadMarketAsync(string marketId, Dictionary<string, object> args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="currency_name">base coin or quote coin name</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<NameResult> LoadCurrencyIdAsync(string currency_name, Dictionary<string, object> args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="currency_name">base coin or quote coin name</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<NameResult> LoadCurrencyNickAsync(string currency_name, Dictionary<string, object> args);

        /// <summary>
        /// Fetch symbols, market ids and exchanger's information
        /// </summary>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<Markets> FetchMarketsAsync(Dictionary<string, object> args);

        /// <summary>
        /// Fetch current best bid and ask, as well as the last trade price.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<Ticker> FetchTickerAsync(string base_name, string quote_name, Dictionary<string, object> args);

        /// <summary>
        /// Fetch price change statistics
        /// </summary>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<Tickers> FetchTickersAsync(Dictionary<string, object> args);

        /// <summary>
        /// Fetch pending or registered order details
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<OrderBooks> FetchOrderBooksAsync(string base_name, string quote_name, int limits, Dictionary<string, object> args);

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="limits">maximum number of items (optional): default 20</param>
        ///// <param name="args">Add additional attributes for each exchange</param>
        ///// <returns></returns>
        //ValueTask<OrderBooks> FetchAllOrderBooks(int limits, Dictionary<string, object> args);

        /// <summary>
        /// Fetch array of symbol name and OHLCVs data
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<OHLCVs> FetchOHLCVsAsync(string base_name, string quote_name, string timeframe, long since, int limits, Dictionary<string, object> args);

        /// <summary>
        /// Fetch array of recent trades data
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        ValueTask<CompleteOrders> FetchCompleteOrdersAsync(string base_name, string quote_name, string timeframe, long since, int limits, Dictionary<string, object> args);

        ///// <summary>
        ///// Fetch array of recent trades all currency data
        ///// </summary>
        ///// <param name="timeframe">time frame interval (optional): default "1d"</param>
        ///// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        ///// <param name="limits">maximum number of items (optional): default 20</param>
        ///// <param name="args">Add additional attributes for each exchange</param>
        ///// <returns></returns>
        //ValueTask<AllCompleteOrders> FetchAllCompleteOrders(string timeframe, long since, int limits, Dictionary<string, object> args);
    }

    /// <summary>
    ///
    /// </summary>
    public class PublicApi : IPublicApi
    {
        private static AsyncLock __market_async_lock { get; } = new AsyncLock();

        /// <summary>
        ///
        /// </summary>
        public static ConcurrentDictionary<string, IMarkets> marketPool = new ConcurrentDictionary<string, IMarkets>();

        /// <summary>
        ///
        /// </summary>
        public virtual XApiClient publicClient
        {
            get;
            set;
        }

        /// <summary>
        /// Preloading markets when reload is true or first function call
        /// </summary>
        /// <param name="reload">reload market information</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<Markets> LoadMarketsAsync(bool reload = false, Dictionary<string, object> args = null)
        {
            using (await __market_async_lock.LockAsync())
            {
                if (publicClient.ExchangeInfo.Markets == null)
                {
                    if (marketPool.ContainsKey(publicClient.DealerName))
                        publicClient.ExchangeInfo.Markets = marketPool[publicClient.DealerName] as Markets ?? new Markets();
                }

                if (publicClient.ExchangeInfo.Markets == null || reload)
                {
                    var _markets = await FetchMarketsAsync(args);
                    if (_markets.success == false)
                    {
                        _markets.message = _markets.message ?? "raise error while reading market data";
                        _markets.success = false;
                    }

                    publicClient.ExchangeInfo.Markets = _markets;

                    marketPool[publicClient.DealerName] = _markets;
                }

                return publicClient.ExchangeInfo.Markets;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="marketId"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<Market> LoadMarketAsync(string marketId, Dictionary<string, object> args = null)
        {
            var _result = new Market(marketId);

            var _markets = await this.LoadMarketsAsync();
            if (_markets.success)
            {
                if (_markets.result.ContainsKey(marketId))
                {
                    _result.result = _markets.result[marketId];

                    _result.SetResult(_markets);
                }
                else
                {
                    _result.SetFailure(
                            $"not exist market id '{marketId}' in markets",
                            ErrorCode.NotSupported
                        );
                }
            }
            else
            {
                _result.SetResult(_markets);
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="currency_name">base coin or quote coin name</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<NameResult> LoadCurrencyIdAsync(string currency_name, Dictionary<string, object> args = null)
        {
            var _result = new NameResult();

            var _markets = await this.LoadMarketsAsync();
            if (_markets.success)
            {
                if (_markets.CurrencyNames.ContainsValue(currency_name))
                {
                    _result.result = _markets.GetCurrencyId(currency_name);

                    _result.SetResult(_markets);
                }
                else
                {
                    _result.SetFailure(
                            $"not exist currency name '{currency_name}' in currencies",
                            ErrorCode.NotSupported
                        );
                }
            }
            else
            {
                _result.SetResult(_markets);
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="currency_name">base coin or quote coin name</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<NameResult> LoadCurrencyNickAsync(string currency_name, Dictionary<string, object> args = null)
        {
            var _result = new NameResult();

            var _markets = await this.LoadMarketsAsync();
            if (_markets.success)
            {
                if (publicClient.ExchangeInfo.CurrencyNicks.ContainsKey(currency_name))
                {
                    _result.result = publicClient.ExchangeInfo.CurrencyNicks[currency_name];
                    _result.SetResult(_markets);
                }
                else
                {
                    _result.SetFailure(
                            $"not exist currency name '{currency_name}' in currencies",
                            ErrorCode.NotSupported
                        );
                }
            }
            else
            {
                _result.SetResult(_markets);
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        /// Fetch symbols, market ids and exchanger's information
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask<Markets> FetchMarketsAsync(Dictionary<string, object> args = null)
        {
            var _result = new Markets();

            publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

            _result.result = new Dictionary<string, IMarketItem>();

            _result.SetFailure("not supported yet", ErrorCode.NotSupported);
            _result.supported = false;

            return await Task.FromResult(_result);
        }

        /// <summary>
        /// Fetch current best bid and ask, as well as the last trade price.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<Ticker> FetchTickerAsync(string base_name, string quote_name, Dictionary<string, object> args = null)
        {
            var _result = new Ticker(base_name, quote_name);

            var _market = await this.LoadMarketAsync(_result.marketId ?? "");
            if (_market.success)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                _result.result = new TickerItem();

                _result.SetFailure("not supported yet", ErrorCode.NotSupported);
                _result.supported = false;
            }
            else
            {
                _result.SetResult(_market);
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        /// Fetch price change statistics
        /// </summary>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<Tickers> FetchTickersAsync(Dictionary<string, object> args = null)
        {
            var _result = new Tickers();

            var _markets = await this.LoadMarketsAsync();
            if (_markets.success)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                _result.result = new List<ITickerItem>();

                _result.SetFailure("not supported yet", ErrorCode.NotSupported);
                _result.supported = false;
            }
            else
            {
                _result.SetResult(_markets);
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        /// Fetch pending or registered order details
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<OrderBooks> FetchOrderBooksAsync(string base_name, string quote_name, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new OrderBooks(base_name, quote_name);

            if (_result.marketId != null)
            {
                var _market = await this.LoadMarketAsync(_result.marketId ?? "");
                if (_market.success)
                {
                    publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                    _result.result = new OrderBook();

                    _result.SetFailure("not supported yet", ErrorCode.NotSupported);
                    _result.supported = false;
                }
                else
                {
                    _result.SetResult(_market);
                }
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        /// Fetch array of symbol name and OHLCVs data
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<OHLCVs> FetchOHLCVsAsync(string base_name, string quote_name, string timeframe = "1d", long since = 0, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new OHLCVs(base_name, quote_name);

            var _market = await this.LoadMarketAsync(_result.marketId ?? "");
            if (_market.success)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                var _timeframe = publicClient.ExchangeInfo.GetTimeframe(timeframe);
                var _timestamp = publicClient.ExchangeInfo.GetTimestamp(timeframe);

                _result.result = new List<IOHLCVItem>();

                _result.SetFailure("not supported yet", ErrorCode.NotSupported);
                _result.supported = false;
            }
            else
            {
                _result.SetResult(_market);
            }

            return await Task.FromResult(_result);
        }

        /// <summary>
        /// Fetch array of recent trades data
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public virtual async ValueTask<CompleteOrders> FetchCompleteOrdersAsync(string base_name, string quote_name, string timeframe = "1d", long since = 0, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new CompleteOrders(base_name, quote_name);

            var _market = await this.LoadMarketAsync(_result.marketId ?? "");
            if (_market.success)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                var _timeframe = publicClient.ExchangeInfo.GetTimeframe(timeframe);
                var _timestamp = publicClient.ExchangeInfo.GetTimestamp(timeframe);

                _result.result = new List<ICompleteOrderItem>();

                _result.SetFailure("not supported yet", ErrorCode.NotSupported);
                _result.supported = false;
            }
            else
            {
                _result.SetResult(_market);
            }

            return await Task.FromResult(_result);
        }
    }
}