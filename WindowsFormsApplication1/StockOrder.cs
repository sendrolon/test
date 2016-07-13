using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class StockOrder
    {
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
        public int mScore = 0;

        public StockOrder(string name, DateTime time)
        {
            this.name = name;
            this.mTime = time;
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
                                this.mScore += sub.Count / 2;
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

                                this.mScore += sub.Count/2;

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
                StreamWriter sw = new StreamWriter(this.name + "_Sell.txt");
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
        public void FoundBuy()
        {
            List<float> orderKey = new List<float>();
            orderKey.AddRange(mBuyPriceOrders.Keys.ToArray());
            orderKey.Sort();
            if (EnvVar.PrintAllStockOrders)
            {
                StreamWriter sw = new StreamWriter(this.name + ".txt");
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
                                        if (os.mTime == "10:39:09.000" && mos.mTime == "10:40:39.000" && sample.Count == 3)
                                            Debug.WriteLine("test");
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
                                            if (this.name == "601398")
                                                Debug.WriteLine("test");
                                            if (sample.Sum() % 1000 == 0 || sample.Sum() % 1000 == 1 || sample.Sum() % 1000 == 9)
                                            {
                                                this.mScore += sample.Count * 3 / 2;
                                            }
                                            else if (sample.Sum() % 100 == 0 || sample.Sum() % 100 == 1 || sample.Sum() % 100 == 9)
                                            {
                                                this.mScore += sample.Count;
                                            }
                                            this.mScore += sample.Count;
                                        }

                                    }
                                }


                            }
                        }
                    }
                }
            }
        }


    }
}
