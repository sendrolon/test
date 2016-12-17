using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class MultipleWork
    {
        List<StockOrder> stocks;
        int Target = 0;
        int mCount = 0;
        public MultipleWork(List<StockOrder> s)
        {
            this.stocks = s;
        }

        public void go()
        {
            foreach (StockOrder stock in stocks)
            {
                ThreadPool.UnsafeQueueUserWorkItem(DoOneWork, stock);
            }
            while (mCount < Target) ;
        }

        void DoOneWork(object param)
        {
            StockOrder stock = (StockOrder)param;
            WebRequest web = new WebRequest();
            web.FillStock(stock);
            mCount++;
            Debug.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + " done for" + stock.CName);

        }
    }
}
