using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public List<string> mExactStocks = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }
        List<StockOrder> goodStock = new List<StockOrder>();

        private void outputGoodStock(StreamWriter sw, StreamWriter sw_sell, StreamWriter sw_rich)
        {
            
            goodStock.Sort(new StockScoreCompair());
            goodStock.Reverse();
            foreach (StockOrder stock in goodStock)
            {
                Boolean printHead = false;


                foreach (string key in stock.foundBuyOrders.Keys)
                {
                    List<long> parseKey = Toolbox.ParseKeys(key);
                    if (parseKey == null)
                        continue;
                    if (parseKey.Count == 2 && stock.foundBuyOrders[key].Count == 2 && !Limits.Exact)
                        continue;
                    if (!printHead)
                    {
                        Debug.WriteLine("found valuable info for " + stock.name);
                        sw.WriteLine("===============");
                        sw.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());

                        sw_rich.WriteLine("===============");
                        sw_rich.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());
                        printHead = true;
                    }
                    if (parseKey.Sum() % 10 == 0 || parseKey.Sum() % 10 == 1 || parseKey.Sum() % 10 == 9)
                    {
                        Debug.WriteLine("*************");
                        sw.WriteLine("*************");
                        sw_rich.WriteLine("*************");
                        if (parseKey.Sum() % 100 == 0 || parseKey.Sum() % 100 == 1 || parseKey.Sum() % 100 == 9)
                        {
                            sw.WriteLine("$$$$$$$$$$$$$$$");
                            sw_rich.WriteLine("$$$$$$$$$$$$$$$");
                            if (parseKey.Sum() % 1000 == 0 || parseKey.Sum() % 1000 == 1 || parseKey.Sum() % 1000 == 9)
                            {
                                sw.WriteLine("@@@@@@@@@@@@@");
                                sw_rich.WriteLine("@@@@@@@@@@@@@");
                            }
                        }
                    }
                    Debug.WriteLine("sec " + key + "  found:");
                    sw.WriteLine("serial is: " + key);
                    sw_rich.WriteLine("serial is: " + key);

                    foreach (OrderSeq os in stock.foundBuyOrders[key])
                    {
                        Debug.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        sw.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                    }
                }

                foreach (string key in stock.mTractorOrders.Keys)
                {
                    if (!printHead)
                    {
                        Debug.WriteLine("found valuable info for " + stock.name);
                        sw_rich.WriteLine("===============");
                        sw_rich.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());
                        printHead = true;
                    }
                    sw_rich.WriteLine("-------------------------");
                    sw_rich.WriteLine("Tractor seqs : " + key);
                    foreach (OrderSeq os in stock.mTractorOrders[key])
                    {
                        sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                    }
                    sw_rich.WriteLine("-------------------------");
                }

                foreach (string key in stock.mBuySplits.Keys)
                {
                    if (!printHead)
                    {
                        Debug.WriteLine("found valuable info for " + stock.name);
                        sw_rich.WriteLine("===============");
                        sw_rich.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());
                        printHead = true;
                    }

                    sw_rich.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                    sw_rich.WriteLine("Entirly split orders:" + key);
                    foreach (OrderSeq os in stock.mBuySplits[key])
                        sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                    sw_rich.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                }

                if (printHead)
                {
                    Debug.WriteLine("==========================");
                    sw.WriteLine("====================");
                    sw.WriteLine();

                    sw_rich.WriteLine("====================");
                    sw_rich.WriteLine();
                }
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            string[] exact_stock = textBox1.Lines;
            mExactStocks.Clear();
            mExactStocks.AddRange(exact_stock);
            if (mExactStocks.Count > 0)
            {
                Limits.Exact = true;
                EnvVar.PrintAllStockOrders = true;
            }
            else
            {
                Limits.Exact = false;
                EnvVar.PrintAllStockOrders = false;
            }
            
            DateTime start = DateTime.Now;
            OpenFileDialog of = new OpenFileDialog();
            DialogResult result = of.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            string out_filename = DateTime.Today.ToString("MM-dd-") + DateTime.Today.Ticks.ToString() + ".txt";
            StreamWriter sw = new StreamWriter(out_filename);
            StreamWriter sw_sell = new StreamWriter("SELL_" + out_filename);
            StreamWriter sw_rich = new StreamWriter("Rich_" + out_filename);
            foreach (string str in EnvVar.MultipleThreadInit)
            {
                doWork(of.FileName, str, sw, sw_sell, sw_rich);
                GC.Collect();
                GC.Collect();
                GC.Collect();
                GC.Collect();
                GC.Collect();
                GC.Collect();

            }
            outputGoodStock(sw, sw_sell, sw_rich);
            sw.Close();
            sw_sell.Close();
            sw_rich.Close();



            DateTime end = DateTime.Now;
            MessageBox.Show("Completed! cost:" + (end - start).TotalSeconds.ToString());
        }

        void doWork(string infilename, string initial, StreamWriter sw, StreamWriter sw_sell,StreamWriter sw_rich)
        {
            Dictionary<string, StockOrder> stocks = new Dictionary<string, StockOrder>();
            StreamReader sr = new StreamReader(infilename);
            
            String line;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line.Contains("2cn") && line[line.Length - 1] == ',')
                {
                    string[] materials = line.Split('=');
                    string name;
                    if (!Toolbox.GetStockCode(materials[0], out name, DataType.TCP))
                        continue;
                    if (!name.StartsWith(initial))
                        continue;
                    if (Limits.Exact)
                    {
                        if (!mExactStocks.Contains(name))
                            continue;
                    }
                    string[] datas = materials[1].Split(',');
                    try
                    {
                        float f = Convert.ToSingle(datas[2]);
                        if (f >= 50.0)
                            continue;
                    }
                    catch (Exception err)
                    {
                        continue;
                    }
                    if (!stocks.Keys.Contains(name))
                        stocks.Add(name, new StockOrder(name, DateTime.Today));


                    stocks[name].AppendOneLine(datas);

                }
            }
            //StreamWriter sw = new StreamWriter(outfilename+".txt");
            
            foreach (StockOrder stock in stocks.Values)
            {
                stock.FoundBuy();
                stock.FoundBuySplitSingle();
                stock.FoundTractorsOrders();
                if (stock.foundBuyOrders.Count != 0 || stock.mTractorOrders.Count != 0 || stock.mBuySplits.Count != 0)
                {
                    Boolean printHead = false;
                    this.goodStock.Add(stock);
                    continue;

                    foreach (string key in stock.foundBuyOrders.Keys)
                    {
                        List<long> parseKey = Toolbox.ParseKeys(key);
                        if (parseKey == null)
                            continue;
                        if (parseKey.Count == 2 && stock.foundBuyOrders[key].Count == 2 && !Limits.Exact)
                            continue;
                        if (!printHead)
                        {
                            Debug.WriteLine("found valuable info for " + stock.name);
                            sw.WriteLine("===============");
                            sw.WriteLine("stock name=" + stock.name);

                            sw_rich.WriteLine("===============");
                            sw_rich.WriteLine("stock name=" + stock.name);
                            printHead = true;
                        }
                        if (parseKey.Sum() % 10 == 0 || parseKey.Sum() % 10 == 1 || parseKey.Sum() % 10 == 9)
                        {
                            Debug.WriteLine("*************");
                            sw.WriteLine("*************");
                            sw_rich.WriteLine("*************");
                            if (parseKey.Sum() % 100 == 0 || parseKey.Sum() % 100 == 1 || parseKey.Sum() % 100 == 9)
                            {
                                sw.WriteLine("$$$$$$$$$$$$$$$");
                                sw_rich.WriteLine("$$$$$$$$$$$$$$$");
                                if (parseKey.Sum() % 1000 == 0 || parseKey.Sum() % 1000 == 1 || parseKey.Sum() % 1000 == 9)
                                {
                                    sw.WriteLine("@@@@@@@@@@@@@");
                                    sw_rich.WriteLine("@@@@@@@@@@@@@");
                                }
                            }
                        }
                        Debug.WriteLine("sec " + key + "  found:");
                        sw.WriteLine("serial is: " + key);
                        sw_rich.WriteLine("serial is: " + key);

                        foreach (OrderSeq os in stock.foundBuyOrders[key])
                        {
                            Debug.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            sw.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        }
                    }

                    foreach (string key in stock.mTractorOrders.Keys)
                    {
                        if (!printHead)
                        {
                            Debug.WriteLine("found valuable info for " + stock.name);
                            sw_rich.WriteLine("===============");
                            sw_rich.WriteLine("stock name=" + stock.name);
                            printHead = true;
                        }
                        sw_rich.WriteLine("-------------------------");
                        sw_rich.WriteLine("Tractor seqs : " + key);
                        foreach (OrderSeq os in stock.mTractorOrders[key])
                        {
                            sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        }
                        sw_rich.WriteLine("-------------------------");
                    }

                    foreach (string key in stock.mBuySplits.Keys)
                    {
                        if (!printHead)
                        {
                            Debug.WriteLine("found valuable info for " + stock.name);
                            sw_rich.WriteLine("===============");
                            sw_rich.WriteLine("stock name=" + stock.name);
                            printHead = true;
                        }

                        sw_rich.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                        sw_rich.WriteLine("Entirly split orders:" + key);
                        foreach(OrderSeq os in stock.mBuySplits[key])
                            sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        sw_rich.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                    }

                    if (printHead)
                    {
                        Debug.WriteLine("==========================");
                        sw.WriteLine("====================");
                        sw.WriteLine();
                        
                        sw_rich.WriteLine("====================");
                        sw_rich.WriteLine();
                    }
                }
            }


            if (Limits.Exact)
            {
                foreach (StockOrder stock in stocks.Values)
                {
                    stock.FoundSell();
                    stock.FoundSellSplitSingle();
                    stock.FoundSellTractorOrders();
                    if (stock.foundSellOrders.Count != 0)
                    {
                        Boolean printHead = false;

                        foreach (string key in stock.foundSellOrders.Keys)
                        {
                            List<long> parseKey = Toolbox.ParseKeys(key);
                            if (parseKey == null)
                                continue;
                            if (parseKey.Count == 2 && stock.foundSellOrders[key].Count == 2 && !Limits.Exact)
                                continue;
                            if (!printHead)
                            {
                                Debug.WriteLine("found valuable info for " + stock.name);
                                sw_sell.WriteLine("===============");
                                sw_sell.WriteLine("stock name=" + stock.name);
                                printHead = true;
                            }
                            if (parseKey.Sum() % 10 == 0 || parseKey.Sum() % 10 == 1 || parseKey.Sum() % 10 == 9)
                            {
                                Debug.WriteLine("*************");
                                sw_sell.WriteLine("*************");
                                if (parseKey.Sum() % 100 == 0 || parseKey.Sum() % 100 == 1 || parseKey.Sum() % 100 == 9)
                                {
                                    sw_sell.WriteLine("$$$$$$$$$$$$$$$");
                                    if (parseKey.Sum() % 1000 == 0 || parseKey.Sum() % 1000 == 1 || parseKey.Sum() % 1000 == 9)
                                    {
                                        sw_sell.WriteLine("@@@@@@@@@@@@@");
                                    }
                                }
                            }
                            Debug.WriteLine("sec " + key + "  found:");
                            sw_sell.WriteLine("serial is:" + key);

                            foreach (OrderSeq os in stock.foundSellOrders[key])
                            {
                                Debug.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                                sw_sell.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            }
                        }


                        foreach (string key in stock.mSellTractorOrders.Keys)
                        {
                            if (!printHead)
                            {
                                Debug.WriteLine("found valuable info for " + stock.name);
                                sw_sell.WriteLine("===============");
                                sw_sell.WriteLine("stock name=" + stock.name);
                                printHead = true;
                            }
                            sw_sell.WriteLine("-------------------------");
                            sw_sell.WriteLine("Tractor seqs :" + key);
                            foreach (OrderSeq os in stock.mSellTractorOrders[key])
                            {
                                sw_sell.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            }
                            sw_sell.WriteLine("-------------------------");
                        }

                        foreach (string key in stock.mSellSplits.Keys)
                        {
                            if (!printHead)
                            {
                                Debug.WriteLine("found valuable info for " + stock.name);
                                sw_sell.WriteLine("===============");
                                sw_sell.WriteLine("stock name=" + stock.name);
                                printHead = true;
                            }

                            sw_sell.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                            sw_sell.WriteLine("Entirly split orders:" + key);
                            foreach (OrderSeq os in stock.mSellSplits[key])
                                sw_sell.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            sw_sell.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                        }

                        if (printHead)
                        {
                            Debug.WriteLine("==========================");
                            sw_sell.WriteLine("====================");
                            sw_sell.WriteLine();
                        }
                    }
                }
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {

            OpenFileDialog of = new OpenFileDialog();
            of.ShowDialog();
            Dictionary<string, StockOrder> stocks = new Dictionary<string, StockOrder>();
            StreamReader sr = new StreamReader(of.FileName);
            String line;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line.Contains("2cn") && line[line.Length - 1] == ',')
                {
                    string[] materials = line.Split('=');
                    string name;
                    if (!Toolbox.GetStockCode(materials[0], out name, DataType.TCP))
                        continue;
                    if (!name.StartsWith("000"))
                        continue;
                    string[] datas = materials[1].Split(',');
                    try
                    {
                        float f = Convert.ToSingle(datas[2]);
                        if (f >= 50.0)
                            continue;
                    }
                    catch (Exception err)
                    {
                        continue;
                    }
                    if (!stocks.Keys.Contains(name))
                        stocks.Add(name, new StockOrder(name, DateTime.Today));
                    

                    stocks[name].AppendOneLine(datas);

                }
            }
            StreamWriter sw = new StreamWriter("000_1.txt");
            foreach (StockOrder stock in stocks.Values)
            {
                stock.FoundBuy();
                if (stock.foundBuyOrders.Count != 0)
                {
                    Debug.WriteLine("found valuable info for " + stock.name);
                    sw.WriteLine("===============");
                    sw.WriteLine("stock name=" + stock.name);
                    foreach (string key in stock.foundBuyOrders.Keys)
                    {
                        Debug.WriteLine("sec " + key + "  found:");
                        sw.WriteLine("serial is:" + key);

                        foreach (OrderSeq os in stock.foundBuyOrders[key])
                        {
                            Debug.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            sw.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        }
                    }
                    Debug.WriteLine("==========================");
                    sw.WriteLine("====================");
                    sw.WriteLine();
                }
            }
            
            sw.Close();

            

        }
    }
}
