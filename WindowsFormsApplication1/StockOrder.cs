using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceJson;

namespace WindowsFormsApplication1
{
    public class StockOrder
    {
        public Boolean sp_zhangting;
        public Boolean sp_jianchi;
        public Boolean sp_dazong;
        public Boolean sp_fupai;
        public Boolean sp_jiejin;

        public float Offset;
        public float Wave;
        public long TodayVol;
        public float TodayHigh;
        public float TodayLow;
        public float MainForcePercentage;
        public float MarketFloatValue;
        public float MarketValue;
        public string CName;
        public DateTime mTime;
        public String name;
        public Dictionary<float, PriceOrders> mBuyPriceOrders = new Dictionary<float, PriceOrders>();
        public Dictionary<float, PriceOrders> mSellPriceOrders = new Dictionary<float, PriceOrders>();
        public List<OrderSeq> mAllOrders = new List<OrderSeq>();

        public Dictionary<string, List<OrderSeq>> foundBuyOrders = new Dictionary<string, List<OrderSeq>>();
        public Dictionary<string, List<OrderSeq>> foundSellOrders = new Dictionary<string, List<OrderSeq>>();

        //public List<OrderSeq> mBuySplits = new List<OrderSeq>();
        public Dictionary<string, List<OrderSeq>> mBuySplits = new Dictionary<string, List<OrderSeq>>();
        public Dictionary<string, List<OrderSeq>> mSellSplits = new Dictionary<string, List<OrderSeq>>();
        public Dictionary<string, List<OrderSeq>> mTractorOrders = new Dictionary<string, List<OrderSeq>>();
        public Dictionary<string, List<OrderSeq>> mSellTractorOrders = new Dictionary<string, List<OrderSeq>>();

        public List<OrderSeq> mAllBuyOrders = new List<OrderSeq>();
        public List<OrderSeq> mAllSellOrders = new List<OrderSeq>();
        public int mScore = 0;

        public List<StockOrder> mHistory = new List<StockOrder>();

        public double mBuySplitAvePrice = 0;
        public long mTotalMainForceBuy = 0;
        public float lastPrice = 0;
        public List<MarketDay> mMarketDays = new List<MarketDay>();

        public float mSP_Wave = 0;
        public float mSP_Drop = 0;
        public float mSP_TrapPercent = 0;
        public float mSP_MiddlePrice = 0;
        public Boolean mSP_flag = false;


        public double TrapPercent()
        {
            if (this.lastPrice == 0)
                return 0;
            if (this.mBuySplitAvePrice >= this.lastPrice)
            {
                Double trapp = (mBuySplitAvePrice - this.lastPrice) / mBuySplitAvePrice;
                
                return trapp*100;
            }
            return 0;
        }

        public void PrepareOutput()
        {
            MainForcePercentage = Convert.ToSingle(Convert.ToDouble(mTotalMainForceBuy) / Convert.ToDouble(TodayVol));
            MainForcePercentage *= 100;
            this.CalculateBuyAverage();
        }

        //public Boolean WorthOutput(Boolean isExact)
        //{
        //    if (isExact)
        //        return true;
        //    Boolean worth = false;
        //    foreach (string key in mBuySplits.Keys)
        //    {
        //        if (mBuySplits[key].Count == 2)
        //    }

        //}

        public void FillMarketDays(JsonArray jarray)
        {
            MarketDay day;
            MarketDay day0 = null;
            foreach (Object obj in jarray)
            {
                day = new MarketDay(obj as JsonArray);
                if (day.Updated)
                {
                    if (day0 == null)
                    {
                        day0 = day;
                        continue;
                    }

                    this.mMarketDays.Add(day);
                }
            }

            if (mMarketDays.Count == 0)
                return;
            float hightest = float.MinValue;
            float lowest = float.MaxValue;
            float open = day0.ClosePrice;
            float close = this.mMarketDays.Last().ClosePrice;

            foreach (MarketDay m in mMarketDays)
            {
                if (m.HighPrice > hightest)
                    hightest = m.HighPrice;
                if (m.LowPrice < lowest)
                    lowest = m.LowPrice;
            }

            float middle = (hightest + lowest) / 2;
            if (close < open)
                this.mSP_Drop = (Math.Abs(close - open) / open)*100;
            else
                this.mSP_Drop = 0;
            this.mSP_MiddlePrice = middle;

            this.mSP_Wave = ((hightest - lowest) / open) * 100;

        }

        //public 

        public void InsertOneSeq(string time, string seq, float price, int type=1)
        {
            Dictionary<string, List<OrderSeq>> target;
            if (type == 1)
                target = foundBuyOrders;
            else
                target = mBuySplits;
            if (!target.Keys.Contains(seq))
            {
                target.Add(seq, new List<OrderSeq>());
            }
            OrderSeq os = new OrderSeq();
            os.mPrice = price;
            os.mTime = time;
            target[seq].Add(os);
        }

        

        public void TrimSameFoundBuy()
        {
            Dictionary<string, string> same_map = new Dictionary<string, string>();
            foreach (string key in foundBuyOrders.Keys)
            {
                foreach (string key2 in foundBuyOrders.Keys)
                {
                    if (key2 == key)
                        break;
                    if (foundBuyOrders[key2][0].mTime == foundBuyOrders[key][0].mTime)
                    {
                        same_map.Add(key, key2);
                        break;
                    }
                    else if (Toolbox.isSwapPair(key, key2))
                    {
                        same_map.Add(key, key2);
                        break;
                    }
                }
            }

            foreach (string k in same_map.Keys)
            {
                for (int i = 1; i < foundBuyOrders[k].Count; i++)
                {
                    foundBuyOrders[same_map[k]].Add(foundBuyOrders[k][i]);
                }
                foundBuyOrders.Remove(k);
            }
            foreach (string k in foundBuyOrders.Keys)
            {
                foundBuyOrders[k].Sort(new OrderTimeCompair());
            }
            
        }

        public void TrimGarbageOrder(Boolean isExact)
        {
            if (isExact)
                return;
            List<string> garbages = new List<string>();
            if (name == "002376")
                Debug.WriteLine("test");
            foreach (string key in foundBuyOrders.Keys)
            {
                if (foundBuyOrders[key].Count >= 4)
                    continue;

                List<long> parseKey = Toolbox.ParseKeys(key);
                int rank = Toolbox.RankOrder(parseKey);
                if (rank < 2 && parseKey.Count == 2 && foundBuyOrders[key].Count == 2)
                {
                    garbages.Add(key);
                    continue;
                }

                List<OrderSeq> orders = foundBuyOrders[key];
                List<OrderSeq> garbageorders = new List<OrderSeq>();
                for (int i = 1; i < orders.Count; i++)
                {
                    if (Toolbox.isTwoGarbages(orders[i-1], orders[i]))
                    {
                        garbageorders.Add(orders[i]);
                    }
                }
                foreach (OrderSeq os in garbageorders)
                {
                    orders.Remove(os);
                }
                if (orders.Count <= 1)
                    garbages.Add(key);
            }
            foreach (string key in garbages)
            {
                foundBuyOrders.Remove(key);
            }
        }

        public double getLastPrice()
        {
            if (mAllBuyOrders.Count != 0)
                return mAllBuyOrders[mAllBuyOrders.Count - 1].mPrice;
            else
                return 0;
        }

        public StockOrder(string name, DateTime time)
        {
            this.name = name;
            this.mTime = time;
        }

        public void TrimOrders()
        {
            foreach (PriceOrders po in mBuyPriceOrders.Values)
            {
                foreach (OrderSeq os in po.mOrderSeqs)
                {
                    this.mAllBuyOrders.Add(os);
                }
            }
            foreach (PriceOrders po in mSellPriceOrders.Values)
            {
                foreach (OrderSeq os in po.mOrderSeqs)
                {
                    this.mAllSellOrders.Add(os);
                }
            }
            this.mAllBuyOrders.Sort(new OrderTimeCompair());
            
            this.mAllSellOrders.Sort(new OrderTimeCompair());
        }

        

        public Boolean AppendOneLine(string[] datas)
        {

            if (!Toolbox.JudgeTcpDatas(datas))
                return false;

            OrderSeq buyorder = new OrderSeq(datas[8], datas[0], datas[2], StockDirection.BUY);
            

            mAllOrders.Add(buyorder);
            
                

            if (!mBuyPriceOrders.Keys.Contains(buyorder.mPrice))
            {
                mBuyPriceOrders.Add(buyorder.mPrice, new PriceOrders());

            }
            mBuyPriceOrders[buyorder.mPrice].AppendOrderSeq(buyorder);

            if (Limits.Exact)
            {
                OrderSeq sellorder = new OrderSeq(datas[10], datas[0], datas[5], StockDirection.SELL);
                mAllOrders.Add(sellorder);
                if (!mSellPriceOrders.Keys.Contains(sellorder.mPrice))
                {
                    mSellPriceOrders.Add(sellorder.mPrice, new PriceOrders());
                }
                mSellPriceOrders[sellorder.mPrice].AppendOrderSeq(sellorder);
            }

            return true;
        }

        public void CalculateBuyAverage()
        {
            double sum = 0;
            long count = 0;
            foreach (string key in foundBuyOrders.Keys)
            {
                double sum2 = 0;
                long count2 = 0;
                List<long> sample = Toolbox.ParseKeys(key);
                foreach (long l in sample)
                {
                    sum2 += foundBuyOrders[key][0].mPrice * l;
                    count2 += l;
                }
                count2 *= foundBuyOrders[key].Count;
                count += count2;
                sum2 *= foundBuyOrders[key].Count;
                sum += sum2;
            }

            foreach (string key in mBuySplits.Keys)
            {
                double sum2 = 0;
                long count2 = 0;
                List<long> sample = Toolbox.ParseKeys(key);
                foreach (long l in sample)
                {
                    sum2 += mBuySplits[key][0].mPrice * l;
                    count2 += l;
                }
                count2 *= mBuySplits[key].Count;
                count += count2;
                sum2 *= mBuySplits[key].Count;
                sum += sum2;
            }

            this.mBuySplitAvePrice = sum / count;
            this.mTotalMainForceBuy = count;
        }

        public void FoundSellTractorOrders()
        {
            Boolean found = false;
            foreach (float key in mSellPriceOrders.Keys)
            {
                foreach (OrderSeq os in mSellPriceOrders[key].mOrderSeqs)
                {
                    found = false;
                    if (os.mSeqs.Count < EnvVar.SplitOrdersMin)
                    {
                        continue;
                    }
                    for (int len = os.mSeqs.Count; len >= EnvVar.SplitOrdersMin; len--)
                    {
                        for (int i = 0; i <= (os.mSeqs.Count - len); i++)
                        {
                            List<long> sub = os.mSeqs.GetRange(i, len);
                            if (!Toolbox.isValueOrders(sub, os.mPrice, false))
                                continue;
                            if (Toolbox.isTractorSeq(sub))
                            {
                                string seq_name = Toolbox.GenerateSampleName(sub);
                                if (!this.mSellTractorOrders.Keys.Contains(seq_name))
                                {
                                    this.mSellTractorOrders.Add(seq_name, new List<OrderSeq>());
                                }
                                this.mSellTractorOrders[seq_name].Add(os);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                    if (found)
                        continue;
                }
            }
        }

        public void FoundTractorsOrders()
        {
            Boolean found = false;
            foreach (float key in mBuyPriceOrders.Keys)
            {
                foreach (OrderSeq os in mBuyPriceOrders[key].mOrderSeqs)
                {
                  //  if (os.mTime == "11:05:42.000")
                  //      Debug.WriteLine("test");

                    found = false;
                    if (os.mSeqs.Count < EnvVar.SplitOrdersMin)
                    {
                        continue;
                    }
                    for (int len = os.mSeqs.Count; len >= EnvVar.SplitOrdersMin; len--)
                    {
                        for (int i = 0; i <= (os.mSeqs.Count - len); i++)
                        {
                            List<long> sub = os.mSeqs.GetRange(i, len);
                            if (!Toolbox.isValueOrders(sub, os.mPrice,false))
                                continue;
                            if (Toolbox.isTractorSeq(sub))
                            {
                                string seq_name = Toolbox.GenerateSampleName(sub);
                                if (!this.mTractorOrders.Keys.Contains(seq_name))
                                {
                                    this.mTractorOrders.Add(seq_name, new List<OrderSeq>());
                                }
                                this.mTractorOrders[seq_name].Add(os);
                                //this.mScore += sub.Count / 2;
                                //this.mScore += 1;
                                found = true;
                                os.Mark = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                    if (found)
                        continue;
                }
            }
        }

        public void FoundBuySplitSingle()
        {
            Boolean found = false;
            foreach (float key in mBuyPriceOrders.Keys)
            {
                foreach (OrderSeq os in mBuyPriceOrders[key].mOrderSeqs)
                {
                    found = false;
                    if (os.mSeqs.Count < EnvVar.SplitOrdersMin || os.mSeqs.Count > EnvVar.SplitOrderMax)
                    {
                        continue;
                    }
                    if (os.Mark)
                        continue;
                    for (int len = EnvVar.SplitOrdersMin; len <= os.mSeqs.Count; len++)
                    {
                        for (int i = 0; i <= (os.mSeqs.Count - len); i++)
                        {
                            List<long> sub = os.mSeqs.GetRange(i, len);
                            if (!Toolbox.isValueOrders(sub, os.mPrice))
                                continue;
                            if (Toolbox.JudgeSplitSeq(sub))
                            {
                                string seq_name = Toolbox.GenerateSampleName(sub);
                                if (!this.mBuySplits.Keys.Contains(seq_name))
                                {
                                    this.mBuySplits.Add(seq_name,new List<OrderSeq>());
                                }
                                this.mBuySplits[seq_name].Add(os);

                                //this.mScore += sub.Count/2;
                                //this.mScore += 1;

                                found = true;
                                break;
                            }                            
                        }
                        if (found)
                            break;
                    }
                    if (found)
                        continue;

                }
            }
        }

        public void FoundSellSplitSingle()
        {
            Boolean found = false;
            foreach (float key in mSellPriceOrders.Keys)
            {
                foreach (OrderSeq os in mSellPriceOrders[key].mOrderSeqs)
                {
                    found = false;
                    if (os.mSeqs.Count < EnvVar.SplitOrdersMin || os.mSeqs.Count > EnvVar.SplitOrderMax)
                    {
                        continue;
                    }
                    for (int len = EnvVar.SplitOrdersMin; len <= os.mSeqs.Count; len++)
                    {
                        for (int i = 0; i <= (os.mSeqs.Count - len); i++)
                        {
                            List<long> sub = os.mSeqs.GetRange(i, len);
                            if (!Toolbox.isValueOrders(sub, os.mPrice))
                                continue;
                            if (Toolbox.JudgeSplitSeq(sub))
                            {
                                string seq_name = Toolbox.GenerateSampleName(sub);
                                if (!this.mSellSplits.Keys.Contains(seq_name))
                                {
                                    this.mSellSplits.Add(seq_name, new List<OrderSeq>());
                                }
                                this.mSellSplits[seq_name].Add(os);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                    if (found)
                        continue;

                }
            }
        }

        public void FoundSell()
        {
            List<float> orderKey = new List<float>();
            orderKey.AddRange(mSellPriceOrders.Keys.ToArray());
            orderKey.Sort();
            if (EnvVar.PrintAllStockOrders)
            {
                StreamWriter sw = new StreamWriter("details\\" + this.name + "_Sell.txt");
                foreach (float key in mSellPriceOrders.Keys)
                {
                    foreach (OrderSeq os in mSellPriceOrders[key].mOrderSeqs)
                    {
                        sw.Write("price:" + key.ToString() + "  Time:" + os.mTime + " order:");
                        foreach (long l in os.mSeqs)
                        {
                            sw.Write(l.ToString());
                            sw.Write("|");
                        }
                        sw.WriteLine("");
                    }
                }
                sw.WriteLine("");
                sw.Close();
            }
            List<long> sample;
            float[] keys = mSellPriceOrders.Keys.ToArray();
            for (int poi = 0; poi < mSellPriceOrders.Keys.Count; poi++)
            //foreach (PriceOrders po in mBuyPriceOrders.Values)
            {
                List<OrderSeq> orderseq = mSellPriceOrders[keys[poi]].mOrderSeqs;
                for (int osi = 0; osi < orderseq.Count; osi++)
                //foreach (OrderSeq os in po.mOrderSeqs)
                {
                    OrderSeq os = orderseq[osi];


                    foreach (int len in EnvVar.SplitOrderNum)
                    {
                        if (os.mSeqs.Count >= len)
                        {
                            for (int i = 0; i <= os.mSeqs.Count - len; i++)
                            {
                                sample = os.mSeqs.GetRange(i, len);

                                if (!Toolbox.isValueOrders(sample, os.mPrice))
                                    continue;
                                if (foundSellOrders.Keys.Contains(Toolbox.GenerateSampleName(sample)))
                                    continue;

                                for (int mpoi = poi + 1; mpoi < mSellPriceOrders.Keys.Count; mpoi++)
                                //foreach (PriceOrders mpo in mBuyPriceOrders.Values)
                                {
                                    List<OrderSeq> morderseq = mSellPriceOrders[keys[mpoi]].mOrderSeqs;

                                    for (int mosi = osi + 1; mosi < morderseq.Count; mosi++)
                                    //foreach (OrderSeq mos in .mOrderSeqs)
                                    {
                                        OrderSeq mos = morderseq[mosi];
                                        if (sample[0] == 29700 && sample[1] == 30300 && os.mTime == "09:53:06.000")
                                            Debug.WriteLine("test");
                                        if (os.mTime == "09:48:06.000" && mos.mTime == "09:53:06.000")
                                            Debug.WriteLine("test");
                                        if (mos.mTime == os.mTime)
                                            continue;
                                        if (mos.Mark)
                                            continue;
                                        if (Toolbox.SeqContains(mos.mSeqs, sample) >= 0)
                                        {
                                            // Debug.WriteLine("Found!!!");
                                            // Debug.WriteLine("s0.time=" + os.mTime + "  s1.time=" + mos.mTime);
                                            string sname = Toolbox.GenerateSampleName(sample);
                                            if (os.chooseLevel >= sample.Count)
                                                continue;
                                            if (mos.chooseLevel >= sample.Count)
                                                continue;
                                            if (!foundSellOrders.Keys.Contains(sname))
                                            {
                                                foundSellOrders.Add(sname, new List<OrderSeq>());
                                                foundSellOrders[sname].Add(os);
                                                os.Mark = true;

                                            }
                                            mos.Mark = true;
                                            mos.chooseLevel = sample.Count;
                                            foundSellOrders[sname].Add(mos);

                                            
                                        }

                                    }
                                }


                            }
                        }
                    }
                }
            }
        }
        public void findsell2()
        {
            if (EnvVar.PrintAllStockOrders)
            {
                StreamWriter sw = new StreamWriter("details\\" + this.name + "_Sell.txt");
                float price = -1f;
                foreach (OrderSeq os in mAllSellOrders)
                {
                    if (price < 0)
                    {
                        price = os.mPrice;
                    }
                    else
                    {
                        if (price != os.mPrice)
                        {
                            sw.WriteLine("");
                            price = os.mPrice;
                        }
                    }
                    sw.Write( "Time:" + os.mTime + "  price:" + os.mPrice.ToString() + " order:");
                    foreach (long l in os.mSeqs)
                    {
                        sw.Write((l/100).ToString());
                        sw.Write("_");
                    }
                    sw.WriteLine("");
                    
                }
                
                sw.WriteLine("");
                sw.Close();
            }

            int p0 = 0;
            for (p0 = 0; p0 < mAllSellOrders.Count; p0++)
            {
                OrderSeq os = mAllSellOrders[p0];
                if (os.Mark)
                    continue;
                List<long> sample;
                foreach (int len in EnvVar.SplitOrderNum)
                {
                    if (os.mSeqs.Count >= len)
                    {
                        for (int i = 0; i <= os.mSeqs.Count - len; i++)
                        {
                            sample = os.mSeqs.GetRange(i, len);
                            if (!Toolbox.isValueOrders(sample, os.mPrice))
                                continue;
                            for (int p1 = p0 + 1; p1 < mAllSellOrders.Count; p1++)
                            {
                                OrderSeq mos = mAllSellOrders[p1];
                                if (mos.Mark)
                                    continue;
                                //if (mos.mPrice == os.mPrice)
                                //    continue;
                                if (Toolbox.SeqContains(mos.mSeqs, sample) >= 0)
                                {
                                    //Debug.WriteLine("found " + Toolbox.GenerateSampleName(sample));
                                    os.Mark = true;
                                    mos.Mark = true;
                                    string sname = Toolbox.GenerateSampleName(sample);
                                    if (!foundSellOrders.Keys.Contains(sname))
                                    {
                                        foundSellOrders.Add(sname, new List<OrderSeq>());
                                        foundSellOrders[sname].Add(os);

                                    }
                                    foundSellOrders[sname].Add(mos);
                                    //if (this.name == "600153")
                                    //  Debug.WriteLine("test");
                                    //if (sample.Sum() % 10000 == 0 || sample.Sum() % 10000 == 1 || sample.Sum() % 10000 == 9)
                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        public void findbuy2()
        {
            if (EnvVar.PrintAllStockOrders)
            {
                StreamWriter sw = new StreamWriter("details\\" + this.name + ".txt");

                float price = -1f;
                foreach (OrderSeq os in mAllBuyOrders)
                {
                    if (price < 0)
                    {
                        price = os.mPrice;
                    }
                    else
                    {
                        if (price != os.mPrice)
                        {
                            sw.WriteLine("");
                            price = os.mPrice;
                        }
                    }
                    sw.Write( "Time:" + os.mTime + "  price:" + os.mPrice.ToString() +  " order:");
                    foreach (long l in os.mSeqs)
                    {
                        sw.Write((l/100).ToString());
                        sw.Write("_");
                    }
                    sw.WriteLine("");
                   
                }
                
                sw.WriteLine("");
                sw.Close();
            }

            int p0 = 0;
            for (p0 = 0; p0 < mAllBuyOrders.Count; p0++)
            {
                OrderSeq os = mAllBuyOrders[p0];
                if (os.Mark)
                    continue;
                List<long> sample;
                foreach (int len in EnvVar.SplitOrderNum)
                {
                    if (os.mSeqs.Count >= len)
                    {
                        for (int i = 0; i <= os.mSeqs.Count - len; i++)
                        {
                            sample = os.mSeqs.GetRange(i, len);
                            if (!Toolbox.isValueOrders(sample, os.mPrice))
                                continue;
                            for (int p1 = p0 + 1; p1 < mAllBuyOrders.Count; p1++)
                            {
                                OrderSeq mos = mAllBuyOrders[p1];
                                if (mos.Mark)
                                    continue;
                                //if (mos.mPrice == os.mPrice)
                                //    continue;
                                if (Toolbox.SeqContains(mos.mSeqs, sample) >= 0)
                                {
                                    //Debug.WriteLine("found " + Toolbox.GenerateSampleName(sample));
                                    os.Mark = true;
                                    mos.Mark = true;
                                    string sname = Toolbox.GenerateSampleName(sample);
                                    if (!foundBuyOrders.Keys.Contains(sname))
                                    {
                                        foundBuyOrders.Add(sname, new List<OrderSeq>());
                                        foundBuyOrders[sname].Add(os);

                                    }
                                    foundBuyOrders[sname].Add(mos);
                                    //if (this.name == "600153")
                                    //  Debug.WriteLine("test");
                                    //if (sample.Sum() % 10000 == 0 || sample.Sum() % 10000 == 1 || sample.Sum() % 10000 == 9)
                                    if (Toolbox.RankOrder(sample) >=3)
                                    {
                                        //this.mScore += sample.Count * 3 / 2;
                                       // this.mScore += 2;
                                    }
                                    //else if (sample.Sum() % 1000 == 0 || sample.Sum() % 1000 == 1 || sample.Sum() % 1000 == 9)
                                    else if (Toolbox.RankOrder(sample) == 2)
                                    {
                                        //this.mScore += sample.Count;
                                        //this.mScore += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            CalculateBuyAverage();
            //if (this.mBuySplitAvePrice > this.getLastPrice())
            //    this.mScore += 5;


            //this.TrimGarbageOrder(false);
        }
        public void FoundBuy()
        {
            List<float> orderKey = new List<float>();
            orderKey.AddRange(mBuyPriceOrders.Keys.ToArray());
            //orderKey.Sort();
            if (EnvVar.PrintAllStockOrders)
            {
                StreamWriter sw = new StreamWriter("details\\" + this.name + ".txt");
                foreach (float key in mBuyPriceOrders.Keys)
                {
                    foreach (OrderSeq os in mBuyPriceOrders[key].mOrderSeqs)
                    {
                        sw.Write("price:" + key.ToString() + "  Time:" + os.mTime + " order:");
                        foreach (long l in os.mSeqs)
                        {
                            sw.Write(l.ToString());
                            sw.Write("|");
                        }
                        sw.WriteLine("");
                    }
                }
                sw.WriteLine("");
                sw.Close();
            }
            List<long> sample;
            float[] keys = mBuyPriceOrders.Keys.ToArray();
            for (int poi = 0; poi < mBuyPriceOrders.Keys.Count; poi++)
            //foreach (PriceOrders po in mBuyPriceOrders.Values)
            {
                List<OrderSeq> orderseq = mBuyPriceOrders[keys[poi]].mOrderSeqs;
                for (int osi = 0; osi < orderseq.Count;osi++)
                //foreach (OrderSeq os in po.mOrderSeqs)
                {
                    OrderSeq os = orderseq[osi];

                    
                    foreach(int len in EnvVar.SplitOrderNum)
                    {
                        if (os.mSeqs.Count >= len)
                        {
                            for (int i = 0; i <= os.mSeqs.Count - len; i++)
                            {
                                sample = os.mSeqs.GetRange(i, len);

                                if (!Toolbox.isValueOrders(sample, os.mPrice))
                                    continue;
                                if (foundBuyOrders.Keys.Contains(Toolbox.GenerateSampleName(sample)))
                                    continue;

                                for (int mpoi = poi + 1; mpoi < mBuyPriceOrders.Keys.Count; mpoi++)
                                //foreach (PriceOrders mpo in mBuyPriceOrders.Values)
                                {
                                    List<OrderSeq> morderseq = mBuyPriceOrders[keys[mpoi]].mOrderSeqs;

                                    for (int mosi = osi+1;mosi < morderseq.Count; mosi++)
                                    //foreach (OrderSeq mos in .mOrderSeqs)
                                    {
                                        OrderSeq mos = morderseq[mosi];
                                        if (os.mTime == "09:39:09.000" && sample.Count == 2)
                                            Debug.WriteLine("test");
                                        //if (sample[0] == 8300 && sample[1] == 11700 && mos.mTime == "10:11:06.000")
                                         //   Debug.WriteLine("test");
                                        if (mos.mTime == os.mTime)
                                            continue;
                                        if (Toolbox.SeqContains(mos.mSeqs, sample) >=0)
                                        {
                                           // Debug.WriteLine("Found!!!");
                                           // Debug.WriteLine("s0.time=" + os.mTime + "  s1.time=" + mos.mTime);
                                            string sname = Toolbox.GenerateSampleName(sample);
                                            if (os.chooseLevel >= sample.Count)
                                                continue;
                                            if (mos.chooseLevel >= sample.Count)
                                                continue;
                                            if (!foundBuyOrders.Keys.Contains(sname))
                                            {
                                                foundBuyOrders.Add(sname, new List<OrderSeq>());
                                                foundBuyOrders[sname].Add(os);
                                                
                                            }
                                            mos.chooseLevel = sample.Count;
                                            foundBuyOrders[sname].Add(mos);
                                            //if (this.name == "600153")
                                              //  Debug.WriteLine("test");
                                            if (sample.Sum() % 10000 == 0 || sample.Sum() % 10000 == 1 || sample.Sum() % 10000 == 9)
                                            {
                                                //this.mScore += sample.Count * 3 / 2;
                                                //this.mScore += 2;
                                            }
                                            else if (sample.Sum() % 1000 == 0 || sample.Sum() % 1000 == 1 || sample.Sum() % 1000 == 9)
                                            {
                                                //this.mScore += sample.Count;
                                                //this.mScore += 1;
                                            }
                                            //this.mScore += 1;
                                        }

                                    }
                                }


                            }
                        }
                    }
                }
            }
        }

        public void CalculateScore(SpecialInfo spinfo = null)
        {
            if (spinfo != null)
            {
                if (spinfo.result_zhangting.Contains(this.name))
                    this.sp_zhangting = true;
                if (spinfo.result_dazong.Contains(this.name))
                    this.sp_dazong = true;
                if (spinfo.result_fupai.Contains(this.name))
                    this.sp_fupai = true;
                if (spinfo.result_jianchi.Contains(this.name))
                    this.sp_jianchi = true;
                if (spinfo.result_jiejin.Contains(this.name))
                    this.sp_jiejin = true;
            }
            this.mScore = 0;
            foreach (string key in foundBuyOrders.Keys)
            {
                
                int rank = Toolbox.RankOrder(Toolbox.ParseKeys(key));
                if (rank >= 2)
                {
                    mScore += 12 * foundBuyOrders[key].Count;
                }
                else
                    mScore += 10 * foundBuyOrders[key].Count;
            }

            foreach (string key in mBuySplits.Keys)
            {
                mScore += 8 * mBuySplits[key].Count;
            }

            float cSum = Convert.ToSingle(mScore);

            if (this.TrapPercent() > 0)
            {
                //cSum += 0.2f * (Convert.ToSingle(mScore));
                cSum += 0.4f * Convert.ToSingle(this.TrapPercent());
            }

            cSum += (Convert.ToSingle(mScore) * this.MainForcePercentage * 4f);
            if (this.MarketFloatValue < 50)
                cSum += (Convert.ToSingle(mScore) * 0.05f);

            cSum += (Convert.ToSingle(mScore) + this.mHistory.Count * 0.1f);

            this.mScore = Convert.ToInt32(cSum);

            /*    if (this.sp_fupai)
                    this.mScore += 200;
                if (this.sp_zhangting)
                    this.mScore += 100;
                    */

            if (EnvVar.SP_Daily_Check)
            {
                if (this.mSP_Wave >= EnvVar.SP_Threshold_wave)
                {
                    if (this.TrapPercent() >= EnvVar.SP_Threshold_trap)
                    {
                        if (this.mSP_Drop >= EnvVar.SP_Threadhold_drop)
                        {
                            //if (this.lastPrice < this.mSP_MiddlePrice)
                            //{
                            this.mSP_flag = true;
                            //}
                        }
                    }
                }
            }



            
        }


    }
}
