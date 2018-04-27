﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTrade
{
    public class Candlestick
    {
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double last { get; set; }
        public string timestamp { get; set; }

        // インジケータ
        public double ema { get; set; }
        public double stddev { get; set; }
        public double ma { get; set; }
        public double boll_high { get; set; }
        public double boll_low { get; set; }
        public double vola_ma { get; set; }
        public double ema_angle { get; set; }

        //public double boll_high_top { get; set; }
        //public double boll_low_top { get; set; }

        public Candlestick()
        {
            high = 0.0;
            low = 0.0;
            open = 0.0;
            last = 0.0;
            timestamp = string.Empty;
            ema = 0.0;
            stddev = 0.0;
            ma = 0.0;
            boll_high = 0.0;
            boll_low = 0.0;
            //boll_high_top = 0.0;
            //boll_low_top = 0.0;
            vola_ma = 0.0;
            ema_angle = 0.0;
            return;
        }

        public Candlestick
        (
            double _high,
            double _low,
            double _open,
            double _last,
            string _timestamp
        )
        {
            high = _high;
            low = _low;
            open = _open;
            last = _last;
            timestamp = _timestamp;
            return;
        }

        // トレンドを判断
        //  trueなら上昇トレンド
        //  falseなら下降
        public bool isTrend()
        {
            if (open <= last)
            {
                return true;
            }
            return false;
        }

        public double getVolatility()
        {
            return Math.Abs(last - open);
        }

        public double getVolatilityRate()
        {
            if (vola_ma <= double.Epsilon)
            {
                return 0.0;
            }
            return ((getVolatility() / vola_ma) * 100.0);
        }

        public int getLastLevel()
        {
            return getValueLevel(last, high, low);
        }

        public int getOpenLevel()
        {
            return getValueLevel(open, high, low);
        }

        private static int getValueLevel(double price, double high, double low)
        {
            int result = 0;

            double length = high - low;
            double pos = price - low;
            double rate = pos / length * 100.0;

            int level = 0;
            if (rate >= 80.0)
            {
                level = 4;
            }
            else if (rate >= 60.0)
            {
                level = 3;
            }
            else if (rate >= 40.0)
            {
                level = 2;
            }
            else if (rate >= 20.0)
            {
                level = 1;
            }
            else
            {
                level = 0;
            }

            result = level;

            return result;
        }

        public int getUpCandleType()
        {
            int result = -1;

            int last_level = getValueLevel(last, high, low);
            int open_level = getValueLevel(open, high, low);

            if (!isTrend())
            {
                result = -1;
                return result;
            }

            // 上昇キャンドルの場合
            if (last_level == 4 || last_level == 3)
            {
                // 最高
                if (open_level == 0)
                {
                    // 丸坊主
                    // 上昇継続
                    result = 7;
                    return result;
                }
                else if (open_level == 1)
                {
                    // 大陽線
                    // 上昇継続
                    result = 6;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 小陽線
                    // 上昇継続
                    result = 5;
                    return result;
                }
                else if (open_level == 3)
                {
                    // 小陽線/下髭陽線
                    // 上昇継続
                    result = 4;
                    return result;
                }
                else if (open_level == 4)
                {
                    // 下髭陽線
                    // 上昇継続
                    result = 3;
                    return result;
                }
            }
            else if (last_level == 2)
            {
                // 中間
                if (open_level == 0 || open_level == 1)
                {
                    // 上髭陽線
                    // 阻まれている
                    // だいぶ押された。そろそろ上昇が終わる
                    result = 2;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 寄引同事線
                    // 転換
                    // 動いたが、結局終値と始値が同じ 方向転換の可能性
                    result = 1;
                    return result;
                }
            }
            else if (last_level == 0 || last_level == 1)
            {
                // 低
                if (open_level == 0 || open_level == 1)
                {
                    // 上髭陽線
                    // 阻まれている
                    result = 0;
                    return result;
                }
            }
            return result;
        }

        public int getDownCandleType()
        {
            int result = -1;

            int last_level = getValueLevel(last, high, low);
            int open_level = getValueLevel(open, high, low);

            if (isTrend())
            {
                result = -1;
                return result;
            }

            // 下降キャンドルの場合
            if (last_level == 0 || last_level == 1)
            {
                // 最高
                if (open_level == 4)
                {
                    // 丸坊主
                    // 下降継続
                    result = 7;
                    return result;
                }
                else if (open_level == 3)
                {
                    // 大陰線
                    // 下降継続
                    result = 6;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 小陰線
                    // 下降継続
                    result = 5;
                    return result;
                }
                else if (open_level == 1)
                {
                    // 小陰線/上髭陰線
                    // 下降継続
                    result = 4;
                    return result;
                }
                else if (open_level == 0)
                {
                    // 上髭陰線
                    // 下降継続
                    result = 3;
                    return result;
                }
            }
            else if (last_level == 2)
            {
                // 中間
                if (open_level == 3 || open_level == 4)
                {
                    // 下髭陰線
                    // 阻まれている
                    // だいぶ押された。そろそろ上昇が終わる
                    result = 2;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 寄引同事線
                    // 転換
                    // 動いたが、結局終値と始値が同じ 方向転換の可能性
                    result = 1;
                    return result;
                }
            }
            else if (last_level == 3 || last_level == 4)
            {
                // 低
                if (open_level == 3 || open_level == 4)
                {
                    // 下髭陰線
                    // 阻まれている
                    result = 0;
                    return result;
                }
            }
            return result;
        }

        public int getShortLevel()
        {
            int result = -1;

            double vola_rate = getVolatilityRate();

            if (isTrend())
            {
                //上昇キャンドルの場合
                int up_type = getUpCandleType();
                if (up_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (up_type <= 2)
                {
                    if (vola_rate >= 300.0)
                    {
                        // 大きいローソク
                        // 上昇継続
                        // SHORT危険
                        result = -2;
                    }
                    else
                    {
                        // トレンド転換
                        // 下降するかもSHORTできるかも
                        result = 0;
                    }
                }
                else
                {
                    if (vola_rate <= 20.0)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 下降するかもSHORTできるかも
                        result = 0;
                    }
                    else
                    {
                        // 上昇継続
                        // SHORT危険
                        result = -2;
                    }
                }
            }
            else
            {
                //下降トレンドの場合
                int down_type = getDownCandleType();
                if (down_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (down_type <= 2)
                {
                    if (vola_rate >= 300.0)
                    {
                        // 大きいローソク
                        // 下降継続
                        // SHORT推奨
                        result = 2;
                    }
                    else
                    {
                        // トレンド転換
                        // 上昇するかもSHORTは注意
                        result = 1;
                    }
                }
                else
                {
                    if (vola_rate <= 20.0)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 上昇するかもSHORTは注意
                        result = 1;
                    }
                    else
                    {
                        // 下降継続
                        // SHORT推奨
                        result = 2;
                    }
                }
            }

            return result;
        }

        public int getLongLevel()
        {
            int result = -1;

            double vola_rate = getVolatilityRate();

            if (!isTrend())
            {
                //下降トレンドの場合
                int down_type = getDownCandleType();
                if (down_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (down_type <= 2)
                {
                    if (vola_rate >= 300.0)
                    {
                        // 大きいローソク
                        // 下降継続
                        // LONG注意
                        result = -2;
                    }
                    else
                    {
                        // トレンド転換
                        // 上昇するかもLONGできるかも
                        result = 0;
                    }
                }
                else
                {
                    if (vola_rate <= 20.0)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 上昇するかもLONGできるかも
                        result = 0;
                    }
                    else
                    {
                        // 下降継続
                        // LONG注意
                        result = -2;
                    }
                }
            }
            else
            {
                //上昇キャンドルの場合
                int up_type = getUpCandleType();
                if (up_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (up_type <= 2)
                {
                    if (vola_rate >= 300.0)
                    {
                        // 大きいローソク
                        // 上昇継続
                        // LONG推奨
                        result = 2;
                    }
                    else
                    {
                        // トレンド転換
                        // 下降するかもLONG注意
                        result = 1;
                    }
                }
                else
                {
                    if (vola_rate <= 20.0)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 下降するかもLONG注意
                        result = 1;
                    }
                    else
                    {
                        // 上昇継続
                        // LONG推奨
                        result = 2;
                    }
                }
            }

            return result;
        }

        //public int getShortBollLevel()
        //{
        //    int result = -1;

        //    int short_level = getShortLevel();


        //    if (isTrend())
        //    {
        //        //上昇キャンドルの場合
        //    }
        //    else
        //    {
        //        //下降トレンドの場合
        //    }

        //    return result;
        //}

        //public int getLongBollLevel()
        //{
        //    int result = -1;

        //    int long_level = getLongLevel();


        //    if (isTrend())
        //    {
        //        //上昇キャンドルの場合
        //    }
        //    else
        //    {
        //        //下降トレンドの場合
        //    }

        //    return result;
        //}

        public bool isTouchBollHigh()
        {
            if (boll_high <= high)
            {
                return true;
            }

            if (boll_high <= last)
            {
                return true;
            }

            if (boll_high <= open)
            {
                return true;
            }

            if (boll_high <= low)
            {
                return true;
            }
            return false;
        }

        public bool isTouchBollLow()
        {
            if (boll_low >= low)
            {
                return true;
            }

            if (boll_low >= last)
            {
                return true;
            }

            if (boll_low >= open)
            {
                return true;
            }

            if (boll_low >= high)
            {
                return true;
            }

            return false;
        }

        public bool isTouchBollHighLow()
        {
            if(isTouchBollHigh())
            {
                return true;
            }

            if(isTouchBollLow())
            {
                return true;
            }

            return false;
        }

        // キャンドル始値がボリンジャー高バンド以上か判断
        public bool isOverBBOpen()
        {
            if (boll_high <= open)
            {
                return true;
            }
            return false;
        }


        // キャンドル終値がボリンジャー高バンド以上か判断
        public bool isOverBBLast()
        {
            if (boll_high <= last)
            {
                return true;
            }
            return false;
        }

        // キャンドル高値がボリンジャー高バンド以上か判断
        public bool isOverBBHigh()
        {
            if (boll_high <= high)
            {
                return true;
            }
            return false;
        }

        // キャンドル低値がボリンジャー高バンド以上か判断
        public bool isOverBBLow()
        {
            if (boll_high <= low)
            {
                return true;
            }
            return false;
        }

        // キャンドル始値がボリンジャー低バンド以下か判断
        public bool isUnderBBOpen()
        {
            if (boll_low >= open)
            {
                return true;
            }
            return false;
        }

        // キャンドル終値がボリンジャー低バンド以下か判断
        public bool isUnderBBLast()
        {
            if (boll_low >= last)
            {
                return true;
            }
            return false;
        }

        // キャンドル低値がボリンジャー低バンド以上か判断
        public bool isUnderBBLow()
        {
            if (boll_low <= low)
            {
                return true;
            }
            return false;
        }

        // キャンドル高値がボリンジャー低バンド以上か判断
        public bool isUnderBBHigh()
        {
            if (boll_low <= low)
            {
                return true;
            }
            return false;
        }
    }

    public class CandleBuffer
    {
        private List<Candlestick> m_candleList { get; set; }
        public int m_buffer_num { get; set; }

        public CandleBuffer()
        {
            m_candleList = null;
            m_buffer_num = 60;
            return;
        }

        public static CandleBuffer createCandleBuffer
        (
            int buffer_num = 60
        )
        {
            CandleBuffer result = null;
            try
            {
                if (buffer_num <= 0)
                {
                    result = null;
                    return result;
                }

                CandleBuffer buffer = new CandleBuffer();
                if (buffer == null)
                {
                    result = null;
                    return result;
                }

                buffer.m_buffer_num = buffer_num;

                result = buffer;
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

        public int getCandleCount()
        {
            if (m_candleList == null)
            {
                return 0;
            }

            return m_candleList.Count;
        }

        public bool isFullBuffer()
        {
            if (m_buffer_num <= getCandleCount())
            {
                return true;
            }
            return false;
        }

        public List<Candlestick> getCandleList()
        {
            return m_candleList;
        }

        public Candlestick getLastCandle()
        {
            if (getCandleCount() <= 0)
            {
                return null;
            }

            return m_candleList.Last();
        }

        public Candlestick getCandle(int index)
        {
            int candle_cnt = getCandleCount();
            if (candle_cnt <= 0)
            {
                return null;
            }

            if(index < 0)
            {
                return null;
            }

            if(index >= candle_cnt)
            {
                return null;
            }

            return m_candleList[index];
        }

        public Candlestick addCandle
        (
            double _high,
            double _low,
            double _open,
            double _last,
            string _timestamp
        )
        {
            Candlestick result = null;
            try
            {
                if (m_candleList == null)
                {
                    m_candleList = new List<Candlestick>();
                    if (m_candleList == null)
                    {
                        result = null;
                        return result;
                    }
                }

                Candlestick candle = new Candlestick(_high, _low, _open, _last, _timestamp);
                if (candle == null)
                {
                    result = null;
                    return result;
                }
                m_candleList.Add(candle);

                // 保持数を超えた場合
                if (m_candleList.Count > m_buffer_num)
                {
                    // 何個消すか算出
                    int remove_cnt = m_candleList.Count - m_buffer_num;

                    // 古いCandlestickから削除
                    m_candleList.RemoveRange(0, remove_cnt);
                }

                result = candle;
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

        public int calcEma(out double ema, int sample_num)
        {
            int result = 0;
            ema = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double last_ema = 0.0;
                int elem_cnt = 0;
                for (int i= beg_idx; i< candle_cnt; i++)
                {
                    
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.last;
                    double elem_ema = elem_value * 2.0 / (elem_cnt + 1) + last_ema * (elem_cnt + 1 - 2) / (elem_cnt + 1);
                    last_ema = elem_ema;
                }

                ema = last_ema;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ema = 0.0;
                }
            }
            return result;
        }

        // 標準偏差と移動平均を算出
        public int calcStddevAndMA(out double stddev, out double ma, int sample_num)
        {
            int result = 0;
            stddev = 0.0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {
                    
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.last;

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    stddev = Math.Sqrt(((elem_cnt * square_sum) - (value_sum * value_sum)) / (elem_cnt * (elem_cnt + 1)));
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    stddev = 0.0;
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    stddev = 0.0;
                    ma = 0.0;
                }
            }
            return result;
        }

        // ボラリティの移動平均を算出(最大～最小)
        public int calcHighLowVolatility(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.high - candle.low;

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        // ボラリティの移動平均を算出(始値～終値)
        public int calcVolatilityMA(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.getVolatility();

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        public int calcEmaAngleMA(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }


                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick end_candle = m_candleList[i];
                    if (end_candle == null)
                    {
                        continue;
                    }

                    elem_cnt++;
                    double elem_value = 0.0;

                    if ((i - 5) >= 0)
                    {
                        Candlestick beg_candle = m_candleList[i-5];
                        if (beg_candle == null)
                        {
                            result = -1;
                            return result;
                        }
                        double ema_diff = (end_candle.ema - beg_candle.ema) / 5000.0 * 100.0;
                        double tan = ema_diff / sample_num;
                        double angle = Math.Atan(tan) * (180 / Math.PI);

                        elem_value = angle;
                    }
                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }
    }
}
