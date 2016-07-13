using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{

    public class StockScoreCompair : IComparer<StockOrder>
    {

        public int Compare(StockOrder x, StockOrder y)
        {
            if (x.mScore == y.mScore)
                return 0;
            if (x.mScore > y.mScore)
                return 1;
            if (x.mScore < y.mScore)
                return -1;

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

        public static Boolean JudgeSplitSeq(List<long> seq)
        {
            long sum = seq.Sum();
            if (seq.Sum() % 10000 == 0 && sum >= 10000)
                return true;
            else
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
            if (s1.Count >= 3)
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

            
            if (EnvVar.OddSplitOrdersCheck && oddCheck)
            {
                Boolean ret = false;
                foreach (long l in orders)
                {
                    if (l % 1000 != 0)
                        ret = true;
                }
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
                    ret.Add(l);
                }
                catch (Exception err)
                {
                    continue;
                }
            }
            return ret;
        }
    }

    public class EnvVar
    {
        public static long OrderSplitMinNum = 7000;
        public static long OrderSplitMinNum_exact = 3500;

        public static long BigOrder = 200000;
        public static long BigOrder_exact = 150000;

        public static long SingleOrder = 27000;
        public static long SingleOrder_exact = 20100;

        //public static long BigOrder = 1;
        //public static long SingleOrder = 1;
        public static int[] SplitOrderNum = { 5,4,3,2};
        public static int TCP_TIME_LEN = 12;
        public static string[] CodeInitial = { "60", "30", "00", "15", "51" };
        public static string[] MultipleThreadInit = { "000", "002", "300", "600", "601", "603" };

        public static int SplitOrdersMin = 3;
        public static int SplitOrderMax = 12;


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
