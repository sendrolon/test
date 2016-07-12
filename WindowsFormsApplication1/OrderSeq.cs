using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class OrderSeq
    {
        public List<long> mSeqs = new List<long>();
        public string mTime;
        public float mPrice;
        public StockDirection direction;
        public OrderSeq mOriginalSeq = null;
        public int chooseLevel = 0;

        public OrderSeq()
        {
 
        }

        public OrderSeq(string seq, string time, string price, StockDirection direction)
        {
            this.direction = direction;
            mTime = time;
            try
            {
                mPrice = Convert.ToSingle(price);
            }
            catch (Exception err)
            {
                Debug.WriteLine("Convert price error in OrderSeq" + err.ToString());
            }

            string[] seqs = seq.Split('|');
            foreach (string s in seqs)
            {
                if (s == "")
                    continue;
                try
                {
                    mSeqs.Add(Convert.ToInt64(s));
                }
                catch (Exception err)
                {
                    Debug.WriteLine("Convert seq error in OrderSeq " + err.ToString());
                }
            }

            
        }
    }
}
