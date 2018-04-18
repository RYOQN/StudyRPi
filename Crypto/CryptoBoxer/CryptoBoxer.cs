﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UtilityBitflyer;
using UtilityTrade;
using UtilityCryptowatch;


namespace CryptoBoxer
{
    public delegate int updateView();

    public class Boxer
    {
        // 設定パラメタ
        private BoxerConfig m_config { get; set; }

        // 状態用
        private CandleBuffer m_candleBuf { get; set; }
        public double m_min { get; set; }
        public double m_max { get; set; }
        public Position m_position { get; set; }
        private List<Position> m_posArray { get; set; }

        private int m_curShortBollLv { get; set; }
        private int m_preShortBollLv { get; set; }

        private int m_curLongBollLv { get; set; }
        private int m_preLongBollLv { get; set; }


        // デリゲートメソッド
        private updateView UpdateViewDelegate { get; set; }

        // 認証用
        private AuthBitflyer m_authBitflyer { get; set; }

        private bool m_stopFlag { get; set; }
        public void setStopFlag(bool flag)
        {
            m_stopFlag = flag;
        }

        public bool getStopFlag()
        {
            return m_stopFlag;
        }

        private Boxer()
        {
            m_config = null;

            m_candleBuf = null;

            m_posArray = null;

            m_min = 0.0;
            m_max = 0.0;
            m_position = null;

            m_authBitflyer = null;

            m_stopFlag = false;

            m_curShortBollLv = -1;
            m_preShortBollLv = -1;
            m_curLongBollLv = -1;
            m_preLongBollLv = -1;
            return;
        }

        ~Boxer()
        {
            return;
        }

        public CandleBuffer getCandleBuffer()
        {
            return m_candleBuf;
        }

        public List<Position> getPositionList()
        {
            return m_posArray;
        }

        public Position getPosition()
        {
            return m_position;
        }

        public static Boxer createBoxer
        (
            updateView UpdateViewDelegate,
            string     configPath

        )
        {
            Boxer result = null;
            try
            {
                Boxer boxer = new Boxer();
                if (boxer == null)
                {
                    result = null;
                    return result;
                }

                BoxerConfig config = BoxerConfig.loadBoxerConfig(configPath);
                if (config == null)
                {
                    result = null;
                    return result;
                }

                CandleBuffer candleBuf = CandleBuffer.createCandleBuffer(config.buffer_num);
                if (candleBuf == null)
                {
                    result = null;
                    return result;
                }

                updateView _UpdateViewDelegate = null;
                if (UpdateViewDelegate != null)
                {
                    _UpdateViewDelegate = new updateView(UpdateViewDelegate);
                    if (_UpdateViewDelegate == null)
                    {
                        result = null;
                        return result;
                    }
                }

                Position position = new Position();
                if (position == null)
                {
                    result = null;
                    return result;
                }

                List<Position> posArray = new List<Position>();
                if (posArray == null)
                {
                    result = null;
                    return result;
                }
                posArray.Add(position);

                boxer.m_config              = config;
                boxer.m_posArray            = posArray;
                boxer.m_position            = position;
                boxer.UpdateViewDelegate    = _UpdateViewDelegate;
                boxer.m_candleBuf           = candleBuf;

                result = boxer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = null;
            }
            finally
            {
            }
            return result;
        }

        public string getPositionName()
        {
            string result = "NONE";
            try
            {
                if (m_position == null)
                {
                    result = "NONE";
                    return result;
                }

                result = m_position.getPositionStateStr();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = "NONE";
            }
            finally
            {
            }
            return result;
        }

        public double getEntryPrice()
        {
            double result = 0.0;
            try
            {
                if (m_position == null)
                {
                    result = 0.0;
                    return result;
                }

                result = m_position.entry_price;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        public double getExitPrice()
        {
            double result = 0.0;
            try
            {
                if (m_position == null)
                {
                    result = 0.0;
                    return result;
                }

                result = m_position.exit_price;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        public double calcProfit()
        {
            double result = 0.0;
            try
            {
                if (m_position == null)
                {
                    result = 0.0;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = 0.0;
                    return result;
                }

                result = m_position.calcProfit(curCandle.last);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        // 過去のキャンドル情報群をバッファに適用
        private int applyCandlestick(ref BitflyerOhlc ohlc)
        {
            int result = 0;
            try
            {
                if (ohlc == null)
                {
                    Console.WriteLine("failed to GetOhlcAfterAsync()");
                    result = -1;
                    return result;
                }

                if (ohlc.result == null)
                {
                    Console.WriteLine("ohlc's result is null");
                    result = -1;
                    return result;
                }

                List<List<double>> candles = ohlc.result.getResult(m_config.periods);

                if (candles == null)
                {
                    Console.WriteLine("ohlc's candle is null. periods={0}", m_config.periods);
                    result = -1;
                    return result;
                }

                foreach (List<double> candleFactor in candles)
                {
                    if (candleFactor == null)
                    {
                        continue;
                    }

                    double closeTime = candleFactor[0];
                    double openPrice = candleFactor[1];
                    double highPrice = candleFactor[2];
                    double lowPrice = candleFactor[3];
                    double closePrice = candleFactor[4];
                    double volume = candleFactor[5];

                    // Cryptowatchでとれるohlcは閉じてないキャンドルの値も取得される。
                    //  1回目 2018/04/11 10:14:00, open=743093, close=743172, high=743200, low=743093
                    //  2回目 2018/04/11 10:14:00, open=743093, close=743194, high=743200, low=743020
                    // Timestampが10:14:00なら、10:13:00～10:13:59のキャンドル


                    // 2018/04/10 19:21:00
                    DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds((long)closeTime).LocalDateTime;
                    Console.WriteLine("{0}, open={1}, close={2}, high={3}, low={4}", timestamp.ToString(), openPrice, closePrice, highPrice, lowPrice);

                    Candlestick candle = m_candleBuf.addCandle(highPrice, lowPrice, openPrice, closePrice, timestamp.ToString());
                    if (candle == null)
                    {
                        Console.WriteLine("failed to addCandle.");
                        continue;
                    }

                    calcIndicator(ref candle);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public int calcIndicator(ref Candlestick candle)
        {
            int result = 0;
            try
            {
                if (candle == null)
                {
                    result = -1;
                    return result;
                }

                // EMAを算出
                double ema = 0.0;
                if (m_candleBuf.calcEma(out ema, m_config.ema_sample_num) == 0)
                {
                    candle.ema = ema;
                }

                // 標準偏差、移動平均、ボリンジャーバンドを算出
                double stddev = 0.0;
                double ma = 0.0;
                if (m_candleBuf.calcStddevAndMA(out stddev, out ma, m_config.boll_sample_num) == 0)
                {
                    candle.stddev = stddev;
                    candle.ma = ma;

                    candle.boll_high = ma + (2.0 * stddev);
                    candle.boll_low = ma - (2.0 * stddev);

                    // MAX更新
                    if (candle.boll_high < candle.last)
                    {
                        if (m_max < candle.last)
                        {
                            m_max = candle.last;
                        }
                    }
                    else
                    {
                        m_max = candle.boll_high;
                    }

                    // MIN更新
                    if (candle.boll_low > candle.last)
                    {
                        if (m_min > candle.last)
                        {
                            m_min = candle.last;
                        }
                    }
                    else
                    {
                        m_min = candle.boll_low;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async void MainLoop()
        {
            try
            {
                // Cryptowatchから過去のデータを取得
                BitflyerOhlc ohlc = await BitflyerOhlc.GetOhlcAfterAsync(m_config.product_cryptowatch, m_config.periods, m_candleBuf.m_buffer_num * m_config.periods);
                if (applyCandlestick(ref ohlc) != 0)
                {
                    Console.WriteLine("failed to applyCandlestick()");
                    return;
                }

                // CandleStick更新用
                double open_price = 0.0;
                double high_price = 0.0;
                double low_price = 0.0;

                // 過去のデータ群の最後のキャンドルを取得
                //  閉じたキャンドルでなければ更新することになる。
                Candlestick curCandle = m_candleBuf.getLastCandle();
                DateTime nextCloseTime = DateTime.Parse(curCandle.timestamp);

                bool isLastConnect = false;
                int pre_tick_id = 0;
                int cycle_cnt = 0;
                while (true)
                {
                    // Tickerを取得
                    Ticker ticker = await Ticker.GetTickerAsync(m_config.product_bitflyer);
                    if (ticker == null)
                    {
                        continue;
                    }
                    int tick_id = ticker.tick_id;
                    double cur_value = ticker.ltp;

                    if (pre_tick_id == tick_id)
                    {
                        continue;
                    }

                    DateTime dateTimeUtc = DateTime.Parse(ticker.timestamp);// 2018-04-10T10:34:16.677 UTCタイム
                    DateTime cur_timestamp = System.TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, System.TimeZoneInfo.Local);


                    bool isClose = false;
                    if (!isLastConnect)
                    {

                        // 過去取得した最後のキャンドルが閉じているかチェック
                        //  過去最後のTimestampが10:14:00なら、10:13:00～10:13:59のキャンドル
                        //  現時刻が10:14:00～なら過去最後のキャンドルは閉まっている
                        //  現時刻が10:13:00～なら過去最後のキャンドルは閉まっていない

                        TimeSpan span = cur_timestamp - nextCloseTime;
                        double elapsed_sec = span.TotalSeconds;

                        if (elapsed_sec < 0)
                        {
                            // 現時刻が過去最後のキャンドルの閉めより前だった場合、過去最後のキャンドルは閉まっていない
                            open_price = curCandle.open;
                            high_price = curCandle.high;
                            low_price = curCandle.low;

                            Console.WriteLine("prev is not closed. next={0}, cur={1}, elapsed={2}, open={3}, cur={4}, high={5}, low={6}"
                                , nextCloseTime
                                , cur_timestamp
                                , elapsed_sec
                                , open_price
                                , cur_value
                                , high_price
                                , low_price
                            );
                        }
                        else
                        {
                            // 現時刻が過去最後のキャンドルの閉めと一緒もしくはそれ以降だった場合、過去最後のキャンドルは閉まっている

                            nextCloseTime = nextCloseTime.AddSeconds(m_config.periods);

                            Console.WriteLine("prev is closed. next={0}, cur={1}, elapsed={2}", nextCloseTime, cur_timestamp, elapsed_sec);
                            isClose = true;
                            curCandle = null;
                        }
                        isLastConnect = true;
                    }
                    else
                    {
                        TimeSpan diff = nextCloseTime - cur_timestamp;
                        double diff_sec = diff.TotalSeconds;

                        // キャンドルを閉じるべきか判断
                        //Console.WriteLine("is close?. next={0}, cur={1}, diff={2}", nextCloseTime, cur_timestamp, diff_sec);
                        if (diff_sec<=0.0)
                        {
                            // 次の更新時間になったらキャンドルを閉じる
                            //Console.WriteLine("need close. next={0}, cur={1}, diff={2}", nextCloseTime, cur_timestamp, diff_sec);

                            isClose = true;
                        }
                    }

                    // 足の最高値更新
                    if (high_price <= double.Epsilon)
                    {
                        high_price = cur_value;
                    }
                    else if (high_price < cur_value)
                    {
                        high_price = cur_value;
                    }

                    // 足の最低値更新
                    if (low_price <= double.Epsilon)
                    {
                        low_price = cur_value;
                    }
                    else if (low_price > cur_value)
                    {
                        low_price = cur_value;
                    }

                    if (isClose == true)
                    {
                        // キャンドルを閉じる
                        if (curCandle != null)
                        {
                            bool trend = curCandle.isTrend();
                            string candleTrend = "";
                            int candleType = -1;
                            if (trend)
                            {
                                candleTrend = " UP ";
                                candleType = curCandle.getUpCandleType();
                            }
                            else
                            {
                                candleTrend = "DOWN";
                                candleType = curCandle.getDownCandleType();
                            }


                            // CloseTimeは次の更新時間を使用する。
                            curCandle.timestamp = nextCloseTime.ToString();
                            Console.WriteLine("closed candle. timestamp={0},last={1},ema={2:0},diff={3:0},trend={4},type={5},curL={6},preL={7},curS={8},preS={9}"
                                              , curCandle.timestamp
                                              , curCandle.last
                                              , curCandle.ema
                                              , curCandle.last - curCandle.ema
                                              , candleTrend
                                              , candleType
                                              , m_curLongBollLv
                                              , m_preLongBollLv
                                              , m_curShortBollLv
                                              , m_preShortBollLv
                            );

                            //Console.WriteLine("closed candle. timestamp={0}, open={1}, close={2}, high={3}, low={4}"
                            //    , curCandle.timestamp
                            //    , curCandle.open
                            //    , curCandle.last
                            //    , curCandle.high
                            //    , curCandle.low
                            //);


                            // ENTRY/ENTRYロジック
                            //await tryEntryOrder();
                            //await tryExitOrder();
                        }
                        // 次の更新時間を更新
                        nextCloseTime = nextCloseTime.AddSeconds(m_config.periods);

                        // 新たなキャンドルを追加
                        open_price = cur_value;
                        curCandle = m_candleBuf.addCandle(high_price, low_price, open_price, cur_value, cur_timestamp.ToString());
                        if (curCandle == null)
                        {
                            Console.WriteLine("failed to addCandle.");
                            return;
                        }

                        //Console.WriteLine("add candle. timestamp={0}, open={1}, close={2}, high={3}, low={4}"
                        //    , curCandle.timestamp
                        //    , curCandle.open
                        //    , curCandle.last
                        //    , curCandle.high
                        //    , curCandle.low
                        //);

                        // 最高値・最低値リセット
                        high_price = 0.0;
                        low_price = 0.0;
                    }
                    else
                    {
                        // 現在のキャンドルを更新
                        if (curCandle != null)
                        {
                            curCandle.high = high_price;
                            curCandle.low = low_price;
                            curCandle.open = open_price;
                            curCandle.last = cur_value;
                            curCandle.timestamp = cur_timestamp.ToString();
                            //Console.WriteLine("update Candle. timestamp={0}, open={1}, cur={2}, high={3}, low={4}", cur_timestamp, open_price, cur_value, high_price, low_price);

                        }
                    }

                    // インジケータ更新
                    if (curCandle != null)
                    {
                        calcIndicator(ref curCandle);
                    }

                    // ENTRY/ENTRYロジック
                    await tryEntryOrder();
                    await tryExitOrder();

                    // Losscutロジック
                    await tryLosscutOrder();

                    // 注文状況確認ロジック
                    await checkEntry();
                    await checkExit();

                    // 表示を更新
                    if (UpdateViewDelegate != null)
                    {
                        UpdateViewDelegate();
                    }

                    pre_tick_id = ticker.tick_id;

                    if( System.IO.File.Exists(@"./StopCode.txt") )
                    {
                        m_stopFlag = true;
                        System.IO.File.Delete(@"./StopCode.txt");
                    }

                    if (m_stopFlag)
                    {
                        Console.WriteLine("recieved StopCode.");
                        break;
                    }

                    System.Threading.Thread.Sleep(0);
                    cycle_cnt++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return;
        }

        public int loadAuthBitflyer(string filePath)
        {
            int result = 0;
            try
            {
                m_authBitflyer = AuthBitflyer.createAuthBitflyer(filePath);
                if (m_authBitflyer == null)
                {
                    result = -1;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> tryEntryOrder()
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }

                // NONEポジションの場合
                int curLongBollLv = 0;
                int prevLongBollLv = 0;
                int curShortBollLv = 0;
                int prevShortBollLv = 0;
                bool isLong = isConditionLongEntry(ref curLongBollLv, ref prevLongBollLv);
                bool isShort =isConditionShortEntry(ref curShortBollLv, ref prevShortBollLv);
                m_curLongBollLv = curLongBollLv;
                m_preLongBollLv = prevLongBollLv;
                m_curShortBollLv = curShortBollLv;
                m_preShortBollLv = prevShortBollLv;

                if (isLong)
                {
                    //Console.WriteLine("Try Long Entry Order.");

                    SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        Console.WriteLine("failed to Long Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功
                    Console.WriteLine("Long Entry Order ID = {0}", retObj.child_order_acceptance_id);
                    m_position.entryLongOrder(retObj.child_order_acceptance_id);
                }
                else if(isShort)
                {
                    //Console.WriteLine("Try Short Entry Order.");

                    SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        Console.WriteLine("failed to Short Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功
                    Console.WriteLine("Short Entry Order ID = {0}", retObj.child_order_acceptance_id);
                    m_position.entryShortOrder(retObj.child_order_acceptance_id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> checkEntry()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryActive())
                {
                    result = 1;
                    return result;
                }
                // ENTRYアクティブの場合

                GetchildorderResponse responce = await SendChildOrder.getChildOrderAveragePrice(m_authBitflyer, m_config.product_bitflyer, m_position.entry_id);
                if(responce==null)
                {
                    //Console.WriteLine("Order is not completed.");
                    result = 1;
                    return result;
                }
                
                if (responce.child_order_state == "REJECTED")
                {
                    Console.WriteLine("Order is rejected. entry_price={0}", responce.average_price);
                    result = -1;
                    return result;
                }

                if (responce.child_order_state != "COMPLETED")
                {
                    Console.WriteLine("Order is not completed. entry_price={0}", responce.average_price);
                    result = 1;
                    return result;
                }

                // 注文確定
                Console.WriteLine("Order is completed. entry_price={0} id={1}", responce.average_price, m_position.entry_id);
                m_position.entry(responce.average_price);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> tryExitOrder()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryCompleted())
                {
                    result = 1;
                    return result;
                }
                // エントリーが完了している場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // EXITが未だの場合


                if (m_position.isLong())
                {// LONGの場合

                    if (isConditionLongExit())
                    {
                        //Console.WriteLine("Try Long Exit Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                        if (retObj == null)
                        {
                            Console.WriteLine("failed to Long Exit Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        Console.WriteLine("Long Exit Order ID = {0}", retObj.child_order_acceptance_id);
                        m_position.exitOrder(retObj.child_order_acceptance_id);
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    if (isConditionShortExit())
                    {
                        //Console.WriteLine("Try Short Exit Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                        if (retObj == null)
                        {
                            Console.WriteLine("failed to Short Exit Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        Console.WriteLine("Short Exit Order ID = {0}", retObj.child_order_acceptance_id);
                        m_position.exitOrder(retObj.child_order_acceptance_id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> tryLosscutOrder()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryCompleted())
                {
                    result = 1;
                    return result;
                }
                // エントリーが完了している場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // EXITが未だの場合


                if (m_position.isLong())
                {// LONGの場合

                    if (isConditionLongLosscut())
                    {
                        //Console.WriteLine("Try Long Losscut Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                        if (retObj == null)
                        {
                            Console.WriteLine("failed to Long Losscut Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        Console.WriteLine("Long Losscut Order ID = {0}", retObj.child_order_acceptance_id);
                        m_position.exitOrder(retObj.child_order_acceptance_id);
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    if (isConditionShortLosscut())
                    {
                        //Console.WriteLine("Try Short Losscut Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                        if (retObj == null)
                        {
                            Console.WriteLine("failed to Short Losscut Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        Console.WriteLine("Short Losscut Order ID = {0}", retObj.child_order_acceptance_id);
                        m_position.exitOrder(retObj.child_order_acceptance_id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> checkExit()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isExitActive())
                {
                    result = 1;
                    return result;
                }
                // EXITアクティブの場合

                GetchildorderResponse responce = await SendChildOrder.getChildOrderAveragePrice(m_authBitflyer, m_config.product_bitflyer, m_position.exit_id);
                if (responce == null)
                {
                    //Console.WriteLine("Order is not completed.");
                    result = 1;
                    return result;
                }

                if (responce.child_order_state == "REJECTED")
                {
                    Console.WriteLine("Order is rejected. entry_price={0}", responce.average_price);
                    result = -1;
                    return result;
                }

                if (responce.child_order_state != "COMPLETED")
                {
                    Console.WriteLine("Order is not completed. entry_price={0}", responce.average_price);
                    result = 1;
                    return result;
                }

                // 注文確定
                m_position.exit(responce.average_price);
                Console.WriteLine("Order is completed. profit={0} exit_price={1} id={2}", m_position.getProfit(), responce.average_price, m_position.exit_id);

                m_position = new Position();
                m_posArray.Add(m_position);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }



        public bool isConditionLongExit()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                bool isOverEma = false;
                if (curCandle.ema <= curCandle.last)
                {
                    isOverEma = true;
                }

                bool isNearEma = false;
                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff <= m_config.ema_diff_near)
                {
                    isNearEma = true;
                }

                if (isNearEma || isOverEma)
                {
                    int curLastLv = curCandle.getLastLevel();
                    int curOpenLv = curCandle.getOpenLevel();
                    if (curCandle.isTrend())
                    {//上昇キャンドルなら
                        if (curLastLv == 4 && curOpenLv == 0)
                        {// 大陽線
                            // SKIP
                            Console.WriteLine("skip LONG-EXIT. curLastLv={0} curOpenLv={1}", curLastLv, curOpenLv);
                            result = false;
                            return result;
                        }
                        else
                        {// 上髭
                            // EXIT
                            result = true;
                            return result;
                        }
                    }
                    else
                    {//下降キャンドルなら
                        // EXIT
                        result = true;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortExit()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                bool isUnderEma = false;
                if (curCandle.ema >= curCandle.last)
                {
                    isUnderEma = true;
                }

                bool isNearEma = false;
                double ema_diff = curCandle.last - curCandle.ema;
                if (ema_diff <= m_config.ema_diff_near)
                {
                    isNearEma = true;
                }

                if (isNearEma || isUnderEma)
                {
                    int curLastLv = curCandle.getLastLevel();
                    int curOpenLv = curCandle.getOpenLevel();
                    if (!curCandle.isTrend())
                    {//下降キャンドルなら
                        if (curLastLv == 0 && curOpenLv==4)
                        {// 大陰線
                            // SKIP
                            Console.WriteLine("skip LONG-EXIT. curLastLv={0} curOpenLv={1}", curLastLv, curOpenLv);
                            result = false;
                            return result;
                        }
                        else
                        {//下髭
                            // EXIT
                            result = true;
                            return result;
                        }
                    }
                    else
                    {//上昇キャンドルなら
                        // EXIT
                        result = true;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongLosscut()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                double profit = curCandle.last - m_position.entry_price;
                if (profit <= -4000)
                {
                    result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortLosscut()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                double profit = m_position.entry_price - curCandle.last;
                if (profit <= -4000)
                {
                    result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }



        public bool isConditionShortEntry(ref int curShortBollLv, ref int prevShortBollLv)
        {
            bool result = false;
            curShortBollLv = -5;
            prevShortBollLv = -5;
            try
            {
                if(m_candleBuf==null)
                {
                    result = false;
                    return result;
                }

                if (!m_candleBuf.isFullBuffer())
                {
                    result = false;
                    return result;
                }

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if(prevCandle==null)
                {
                    result = false;
                    return result;
                }

                curShortBollLv = curCandle.getShortBollLevel();
                prevShortBollLv = prevCandle.getShortBollLevel();

                double ema_diff = curCandle.last - curCandle.ema;
                if (ema_diff < m_config.ema_diff_far)
                {
                    result = false;
                    return result;
                }


                if (prevCandle.isTouchBollLow())
                {
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollLow())
                {
                    result = false;
                    return result;
                }

                if (!prevCandle.isTouchBollHigh())
                {
                    //前回がBOLL_HIGHにタッチしていない場合
                    result = false;
                    return result;
                }
                //前回がBOLL_HIGHにタッチしている場合

                if (prevShortBollLv < 0)
                {
                    //前回のSHORTレベルが低い
                    // 何もしない
                    result = false;
                    return result;
                }
                else
                {
                    //前回のSHORTレベルが0以上

                    if (curShortBollLv <= 0)
                    {// 現在のSHORTレベルが0以下
                     // 何もしない
                        result = false;
                        return result;
                    }
                    else
                    {// 現在のSHORTレベルが0より高い
                     // ENTRY
                        result = true;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                curShortBollLv = -5;
                prevShortBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongEntry(ref int curLongBollLv, ref int prevLongBollLv)
        {
            bool result = false;
            curLongBollLv = -5;
            prevLongBollLv = -5;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                if (!m_candleBuf.isFullBuffer())
                {
                    result = false;
                    return result;
                }

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }               

                curLongBollLv = curCandle.getLongBollLevel();
                prevLongBollLv = prevCandle.getLongBollLevel();

                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff < m_config.ema_diff_far)
                {
                    result = false;
                    return result;
                }

                if (prevCandle.isTouchBollHigh())
                {
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollHigh())
                {
                    result = false;
                    return result;
                }

                if (!prevCandle.isTouchBollLow())
                {
                    //前回がBOLL_LOWにタッチしていない場合
                    // LONGすべきでない
                    result = false;
                    return result;
                }
                //前回がBOLL_LOWにタッチしている場合


                if (prevLongBollLv < 0)
                {
                    //前回のLONGレベルが低い
                    // 何もしない
                    result = false;
                    return result;
                }
                else
                {
                    //前回のLONGレベルが0以上

                    if (curLongBollLv <= 0)
                    {
                        // 現在のLONGレベルが0以下
                        // 何もしない
                        result = false;
                        return result;
                    }
                    else
                    {
                        // 現在のLONGレベルが0より高い
                        // ENTRY
                        result = true;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                curLongBollLv = -5;
                prevLongBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

    }
}