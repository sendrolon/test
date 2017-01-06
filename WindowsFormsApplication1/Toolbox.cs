﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class StockNameCompair : IComparer<StockOrder>
    {

        public int Compare(StockOrder x, StockOrder y)
        {
            //throw new NotImplementedException();
            return string.Compare(x.name, y.name);
        }
    }

    public class OrderTimeCompair : IComparer<OrderSeq>
    {

        public int Compare(OrderSeq x, OrderSeq y)
        {
            //throw new NotImplementedException();
            return string.Compare(x.mTime, y.mTime);
        }
    }

    public class StockScoreCompair : IComparer<StockOrder>
    {

        public int Compare(StockOrder x, StockOrder y)
        {
            try
            {
                //if (x.TrapPercent() == 0 && y.TrapPercent() >= 0)
                //    return -1;
                if (x.mScore == y.mScore)
                    return 0;
                if (x.mScore > y.mScore)
                    return 1;
                if (x.mScore < y.mScore)
                    return -1;
            }
            catch { }

            return 0;
        }
    }

    public class OrderAboutEqual : IEqualityComparer<long>
    {
    
        public bool Equals(long x, long y)
        {
            return Toolbox.aboutEqual(x, y);
 	        //throw new NotImplementedException();
        }

        public int GetHashCode(long obj)
        {
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return obj.ToString().GetHashCode();
            }
        }
}
    public class Toolbox
    {
        public Toolbox()
        { }

        public static string formFullCode (string name)
        {
            if (name == null)
                return null;
            if (name[0] == '6')
                return "sh" + name;
            else
                return "sz" + name;
        }

        public static Boolean isSwapPair(string k0, string k1)
        {
            List<long> kl0 = Toolbox.ParseKeys(k0);
            List<long> kl1 = Toolbox.ParseKeys(k1);
            if (kl0.Count != 2 || kl1.Count != 2)
                return false;
            if (Toolbox.SeqContains2(kl0, kl1) >= 0)
                return true;
            return false;
        }

        public static int GetFallBackNums(int seq_order_nums)
        {
            if (seq_order_nums <= 25)
                return 1;
            return 2;
        }

        public static Boolean JudgeSplitSeq(List<long> seq)
        {
            long sum = seq.Sum();
            if (seq.Sum() % 10000 == 0 && sum >= 10000)
                return true;
            else
                return false;
        }

        public static Boolean JudgeSplitSeq_l2(float price, List<long> seq, string time0, string time1)
        {


            TimeSpan t0 = new TimeSpan(Convert.ToInt32(time0.Substring(0, 2)),
                                        Convert.ToInt32(time0.Substring(3, 2)),
                                        Convert.ToInt32(time0.Substring(6, 2)));
            TimeSpan t1 = new TimeSpan(Convert.ToInt32(time1.Substring(0, 2)),
                            Convert.ToInt32(time1.Substring(3, 2)),
                            Convert.ToInt32(time1.Substring(6, 2)));

            TimeSpan dt = t1 - t0;
            if (dt.TotalSeconds <= 10)
            {
                foreach (long ord in seq)
                {
                    //long o = ord / 100;
                    float tp = (float)ord * price;
                    if (tp < EnvVar.BigOrder || ord % 1000 == 0)
                        return false;
                }
                Debug.WriteLine("JudgeSplitSeq_l2 found!");
                return true;
            }
            else
                return false;

        }

        public static Boolean isCloseTime(string tstring0, string tsting1)
        {
            string[] t0 = tstring0.Split(':');
            string[] t1 = tsting1.Split(':');
            if (t0.Length != 3 || t1.Length != 3)
                return false;
            t0[2] = t0[2].Substring(0, 2);
            t1[2] = t1[2].Substring(0, 2);

            TimeSpan ts0 = new TimeSpan(Convert.ToInt32(t0[0]), Convert.ToInt32(t0[1]), Convert.ToInt32(t0[2]));
            TimeSpan ts1 = new TimeSpan(Convert.ToInt32(t1[0]), Convert.ToInt32(t1[1]), Convert.ToInt32(t1[2]));

            TimeSpan tsdelta = ts1 - ts0;

            if (tsdelta.TotalSeconds <= 120)
                return true;

            return false;
        }

        public static Boolean isTwoGarbages(OrderSeq os0, OrderSeq os1)
        {
            if (os0.mPrice == os1.mPrice)
            {
                if (isCloseTime(os0.mTime, os1.mTime))
                    return true;
            }
            return false;
        }


        public static Boolean isTractorSeq(List<long> seq)
        {
            if (seq.Distinct().Count() == 1)
                return true;
            else
                return false;
        }

        public static DateTime TimeCovertor(string time)
        {
            DateTime dt;
            //dt = DateTime.Today;
            if (!DateTime.TryParse(time, out dt))
            {
                return DateTime.Today;
            }
            return dt;
        }

        public static Boolean JudgeTcpDatas(string[] datas)
        {

            if (datas.Length != 12)
            {
            
                return false;
            }
            if (datas[0].Length!=EnvVar.TCP_TIME_LEN)
            {
                return false;
            }
            if (datas[1].Length != EnvVar.TCP_TIME_LEN)
                return false;
            if (datas[2] == "0.000")
                return false;
            if (datas[5] == "0.000")
                return false;

            if (datas[9] != "")
                return false;

            if (datas[11] != "")
                return false;

            return true;
        }

        public static string GenerateSampleName(List<long> list)
        {
            if (list == null)
                return "null";
            if (list.Count == 0)
                return "0";
            StringBuilder sb = new StringBuilder();
            foreach (long l in list)
            {
                sb.Append(l/100);
                sb.Append("_");
            }
            return sb.ToString();
        }

        public static void CalculateStocksScores(List<StockOrder> list, SpecialInfo spinfo=null)
        {
            foreach (StockOrder stock in list)
            {
                stock.CalculateScore(spinfo);
            }
        }

        public static Boolean GetStockCode(string data, out string code,DataType type)
        {
            if (type == DataType.TCP)
            {
                if (data.Length >= 19)
                {
                    code = data.Substring(6, 6);
                    try
                    {
                        Convert.ToInt64(code);
                    }
                    catch (Exception err)
                    {
                        return false;
                    }
                    foreach (string init in EnvVar.CodeInitial)
                    {
                        if (code.StartsWith(init))
                            return true;
                    }

                }
            }


            code = null;
            return false;
        }

        public static Boolean aboutEqual(long l1, long l2)
        {
            if (Math.Abs(l1 - l2) <= 201)
                return true;
            return false;
        }

        public static Boolean AboutContains(List<long> list, long val)
        { 
            //if (list.Contains(val,
            return true;
        }

        public static Boolean ContainNRemove(List<long> list, long var)
        {
            Boolean contain = false;
            long l = 0;
            foreach(long l2 in list)
            {
                if (Toolbox.aboutEqual(l2, var))
                {
                    l = l2;
                    contain = true;
                    break;
                }
            }
            if (contain)
                list.Remove(l);

            return contain;
        }

        public static int SeqContains2(List<long> s0, List<long> s1)
        {
            if (s1.Count > s0.Count || s1.Count == 0)
                return -1;

            List<long> s00 = new List<long>(s0.ToArray());
            List<long> ss0;
            int count = 0;
            for (int i = 0; i <= s0.Count - s1.Count; i++)
            {
                ss0 = s0.GetRange(i, s1.Count);
                count = 0;
                foreach(long l in s1)
                {
                    if (l >= Limits.OrderSplitMin())
                    {
                        if (Toolbox.ContainNRemove(ss0,l))
                        {
                           
                            count++;
                        }
                    }
                    else
                    {
                        if (ss0.Contains(l))
                        {
                            count++;
                            ss0.Remove(l);
                        }
                    }
                }

                if (count >= s1.Count)
                    return 1;
                
            }

            return -1;


        }

        public static int SeqContains(List<long> s0, List<long> s1, Boolean overFirst = true)
        {
            if (s1.Count > s0.Count || s1.Count == 0)
                return -1;
            if (s1.Count >= 3 || Toolbox.RankOrder(s1) >=2)
                return Toolbox.SeqContains2(s0, s1);
            int start = -1;
            Boolean found = false;
            int i = 0;
            while ((start = s0.IndexOf(s1[0], start+1 )) >= 0)
            {

                found = false;
                for (i = 0; i < s1.Count; i++)
                {
                    if (i + start >= s0.Count)
                        break;
                    if (s0[i + start] == s1[i])
                    //if (Toolbox.aboutEqual(s0[i + start], s1[i]))
                    {
                        if (i == s1.Count - 1)
                            found = true;
                    }
                    else
                    {
                        break;
                    }
                }
                if (found)
                    return start;
                if (!overFirst)
                    break;

            }
          
            
            return -1;
        }

        public static Boolean isValueOrders(List<long> orders, float price, Boolean oddCheck=true)
        {
            float total = 0.0f;
            foreach (long l in orders)
            {
                if (l * price < Limits.SingleOrderValue())
                    return false;
                total += l * price;
            }

            if (total < Limits.BigOrderValue())
                return false;

            float intege_count = 0;
            if (EnvVar.OddSplitOrdersCheck && oddCheck)
            {
                Boolean ret = false;
                foreach (long l in orders)
                {
                    if (l % 1000 != 0)
                        ret = true;
                    else
                        intege_count += 1;
                }
                float intege_rate = intege_count / Convert.ToSingle(orders.Count);
                if (intege_rate >= 0.5)
                    ret = false;
                return ret;
            }

            return true;
        }

        public static List<long> ParseKeys(string str)
        {
            string[] s = str.Split('_');
            if (s.Length == 0)
                return null;
            List<long> ret = new List<long>();

            foreach (string ls in s)
            {
                try
                {
                    long l = Convert.ToInt64(ls);
                    ret.Add(l*100);
                }
                catch (Exception err)
                {
                    continue;
                }
            }
            return ret;
        }

        public static int RankOrder(List<long> list)
        {
            Boolean odd = allOdd(list);
            int level = 0;
            if (wholeJudge(list, 100000))
                level = 3;
            else if (wholeJudge(list, 10000))
                level = 2;
            else
                level = 0;
            if (odd)
            {
                if (level != 0)
                    return level;
                else
                    return 1;
            }
            else
            {
                if (level != 0)
                    return 1;
                else
                    return 0;
            }
            
        }

        public static Boolean wholeJudge(List<long> list, long whole)
        {
            if (list.Sum() % whole == 0 || list.Sum() % whole == 1 || (list.Sum() + 1) % whole == 0)
                return true;
            return false;
        }

        public static Boolean allOdd(List<long> list)
        {
            foreach (long l in list)
            {
                if (l % 1000 == 0)
                    return false;
            }
            return true;

        }
        public static DateTime foundDaysAgo(int ago)
        {
            DateTime now = DateTime.Now;
            
            while (ago != 0)
            {
                now = now.AddDays(-1);
                if (now.DayOfWeek != DayOfWeek.Sunday && now.DayOfWeek != DayOfWeek.Saturday)
                {
                    ago--;
                }
            }
            return now;
        }
    }

    public class EnvVar
    {

        public static Boolean SP_Daily_Check = false;
        public static int Special_day_num = 3; // N value
        public static float SP_Threshold_wave = 15; //W Value
        public static float SP_Threshold_trap = 1; //T Value
        public static float SP_Threadhold_drop = 5; //D Value
        public static long OrderSplitMinNum = 7000;
        public static long OrderSplitMinNum_exact = 3500;

        public static long BigOrder = 200000;
        public static long BigOrder_exact = 150000;

        public static long SingleOrder = 27000;
        public static long SingleOrder_exact = 20100;

        //public static long BigOrder = 1;
        //public static long SingleOrder = 1;
        public static int[] SplitOrderNum = {6,5,4,3,2};
        public static int TCP_TIME_LEN = 12;
        public static string[] CodeInitial = { "60", "30", "00", "15", "51" };
        public static string[] MultipleThreadInit = { "000", "002", "300", "600", "601", "603", "15" };

        public static int SplitOrdersMin = 3;
        public static int SplitOrderMax = 18;


        public static Boolean PrintAllStockOrders = false;
        public static Boolean OddSplitOrdersCheck = true;
    }

    public class Limits
    {
        public static Boolean Exact = false;
        public static long OrderSplitMin()
        {
            if (Limits.Exact)
            {
                return EnvVar.OrderSplitMinNum_exact;
            }
            else
                return EnvVar.OrderSplitMinNum;
        }

        public static long BigOrderValue()
        {
            if (Limits.Exact)
            {
                return EnvVar.BigOrder_exact;
            }
            else
                return EnvVar.BigOrder;
        }

        public static long SingleOrderValue()
        {
            if (Limits.Exact)
            {
                return EnvVar.SingleOrder_exact;
            }
            else
                return EnvVar.SingleOrder;
        }
    }

    public enum DataType
    {
        TCP,
        L2Print,
        RAW
    };


    

    public enum StockDirection
    {
        BUY,
        SELL
    }
}
