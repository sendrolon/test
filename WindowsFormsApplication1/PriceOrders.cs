using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class PriceOrders
    {
        public List<OrderSeq> mOrdersOrigin = new List<OrderSeq>();
        public List<OrderSeq> mOrderSeqs = new List<OrderSeq>();

        public float mPrice;

        public PriceOrders()
        { }

        public Boolean AppendOrderSeq(OrderSeq seq)
        {
            if (seq.mPrice == 20.54 && seq.direction == StockDirection.BUY)
                Debug.WriteLine("test");
            mOrdersOrigin.Add(seq);
            Merge(seq);
            
            return true;
        }

        OrderSeq MergeList(List<OrderSeq> list)
        {
            OrderSeq merge = new OrderSeq();
            list.Reverse();
            foreach (OrderSeq os in list)
            {
                merge.direction = os.direction;
                merge.mPrice = os.mPrice;
                merge.mTime = os.mTime;
                merge.mSeqs.AddRange(os.mSeqs);
            }

            return merge;
        }


        OrderSeq FindLatestSeq(OrderSeq nseq)
        {
            if (mOrderSeqs.Count == 0)
                return null;

            OrderSeq s0 = mOrderSeqs.Last();
            if (s0.mSeqs.Count >= nseq.mSeqs.Count)
            {
                return s0;
            }
            List<OrderSeq> usedSeq = new List<OrderSeq>();
            int sum = 0;
            for (int i = mOrderSeqs.Count - 1; i >= 0; i--)
            {
                if (sum >= nseq.mSeqs.Count)
                {
                    break;
                }
                else
                {
                    usedSeq.Add(mOrderSeqs[i]);
                    sum += mOrderSeqs[i].mSeqs.Count;
                }
            }
            return MergeList(usedSeq);
        }

        void Merge(OrderSeq seq1)
        {
            if (seq1.mTime == "09:59:12.000")
                Debug.WriteLine("test");
            if (mOrderSeqs.Count == 0)
            {
                mOrderSeqs.Add(seq1);
                return;
            }
            OrderSeq seq0 = FindLatestSeq(seq1);
            List<long> s0 = seq0.mSeqs;
            List<long> s1 = seq1.mSeqs;


            List<long> subs1;
            if (seq1.mSeqs.Count <= 1)
                return;

            int min_len;

            if (s1.Count > s0.Count)
            {
                subs1 = s1.GetRange(0, s0.Count);
                min_len = s0.Count;
            }
            else
            {
                min_len = s1.Count;
                subs1 = s1;
            }

            int i1, i11, i0;
            int target_index = -1;
            Boolean found = false;
            for (i1 = subs1.Count - 1; i1 > 0; i1--)
            {
                int count = 0;
                i11 = 0;
                for (i0 = s0.Count - 1; i0 >= (s0.Count - subs1.Count); i0--)
                {
                    if (subs1[i1 - i11] == s0[i0])
                        count++;
                    else
                    {

                        //This break is the BUG. but keep it first.
                        //break;
                        if (count >= 2)
                            break;
                        else
                            continue;
                    }

                    /*
                    if (count > 2 || count >= s1.Count)  //The two need to think think
                    {
                        target_index = i1;
                        break;
                    }
                     */
                    i11++;
                    if (i11 > i1)
                        break;
                }

                if (count >= 2 || count >= min_len)  //The two need to think think
                {
                    target_index = i1;
                    break;
                }
            }

            if (target_index != -1)
            {
                /*
                Debug.WriteLine("=======================");
                Debug.Write("s0 = ");
                foreach (long s in s0)
                    Debug.Write(s.ToString() + "|");
                Debug.WriteLine("");
                Debug.Write("s1 = ");
                foreach (long s in s1)
                    Debug.Write(s.ToString() + "|");
                Debug.WriteLine("");
                */
                seq1.mSeqs.RemoveRange(0, target_index + 1);
                /*
                Debug.Write("ss1 = ");
                foreach (long s in seq1.mSeqs)
                    Debug.Write(s.ToString() + "|");
                Debug.WriteLine("");
                Debug.WriteLine("=======================");
                */
            }

            if (seq1.mSeqs.Count != 0)
                this.mOrderSeqs.Add(seq1);



            //seq0.mSeqs.
        }
    }
}
