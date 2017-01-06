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
using MySql.Data.MySqlClient;
using NiceJson;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public List<string> mExactStocks = new List<string>();
        public List<string> mBlackList = new List<string>();

        public void initBlackList()
        {
            StreamReader sr = new StreamReader("blacklist.txt");
            while (!sr.EndOfStream)
            {
                mBlackList.Add(sr.ReadLine());
            }
            sr.Close();
        }
        public Form1()
        {
            InitializeComponent();
            initBlackList();

            textBox2.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }
        List<StockOrder> goodStock = new List<StockOrder>();


        private void updateHistory()
        {
            DataBase db = new DataBase();
            foreach (StockOrder stock in goodStock)
            { 
                for (int i = 1; i<5;i++)
                {
                    string date = Toolbox.foundDaysAgo(i).ToString("yyyy-MM-dd");
                    StockOrder so = db.GetBydate(stock.name, date);
                    if (so != null)
                    {
                        so.CalculateBuyAverage();
                        stock.mHistory.Add(so);
                    }
                }
            }
        }

        private void outputGoodStock(StreamWriter sw, StreamWriter sw_sell, StreamWriter sw_rich)
        {

            
            //OutputResult output = new OutputResult(goodStock);
            //output.OutputBuys("test2.html");
            //return;
            //goodStock.Sort(new StockNameCompair());
            foreach (StockOrder stock in goodStock)
            {
                Boolean printHead = false;


                foreach (string key in stock.foundBuyOrders.Keys)
                {
                    List<long> parseKey = Toolbox.ParseKeys(key);
                    if (parseKey == null)
                        continue;
                    int rank = Toolbox.RankOrder(parseKey);
                    if (rank<2 && parseKey.Count == 2 && stock.foundBuyOrders[key].Count == 2 && !Limits.Exact)
                        continue;
                    if (!printHead)
                    {
                        Debug.WriteLine("found valuable info for " + stock.name);
                        sw.WriteLine("===============");
                        sw.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());

                        if (stock.mBuySplitAvePrice >= stock.getLastPrice())
                        {
                            sw.WriteLine(">>>>>>>>>>>>>Main force Trapped :)");
                            sw_rich.WriteLine(">>>>>>>>>>>>>>Main force Trapped :)");
                        }
                        if (stock.mHistory.Count != 0)
                        {
                            sw.WriteLine("★★★★★ history:" + stock.mHistory.Count.ToString());
                            sw_rich.WriteLine("★★★★★ history:" + stock.mHistory.Count.ToString());
                        }
                        sw_rich.WriteLine("===============");
                        sw_rich.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());
                        printHead = true;
                    }

                    
                    if (rank == 3)
                    {
                        sw.WriteLine("@@@@@@@@@@@@@");
                        sw_rich.WriteLine("@@@@@@@@@@@@@");
                    }
                    else if (rank == 2)
                    {
                        sw.WriteLine("$$$$$$$$$$$$$$$");
                        sw_rich.WriteLine("$$$$$$$$$$$$$$$");
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
                if (stock.mBuySplits.Count != 0)
                    sw_rich.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");

                foreach (string key in stock.mBuySplits.Keys)
                {
                    if (!printHead)
                    {
                        Debug.WriteLine("found valuable info for " + stock.name);
                        sw_rich.WriteLine("===============");
                        sw_rich.WriteLine("stock name=" + stock.name + "  Score=" + stock.mScore.ToString());
                        printHead = true;
                    }

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
     
            
            DateTime start = DateTime.Now;
            OpenFileDialog of = new OpenFileDialog();
            DialogResult result = of.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            rework:
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

            this.goodStock = new List<StockOrder>();

            string out_filename = DateTime.Today.ToString("MM-dd-") + DateTime.Now.ToFileTimeUtc() + ".txt";
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
            MultipleWork work = new MultipleWork(goodStock);
            work.go();
            SpecialInfo spinfo = new SpecialInfo();

            this.updateHistory();
            Toolbox.CalculateStocksScores(goodStock, spinfo);
            goodStock.Sort(new StockScoreCompair());
            goodStock.Reverse();

            if (checkBox1.Checked)
            {
                checkBox1.Checked = false;
                foreach (StockOrder stock in goodStock)
                {
                    mExactStocks.Add(stock.name);
                }
                EnvVar.SingleOrder_exact = EnvVar.SingleOrder;
                EnvVar.BigOrder_exact = EnvVar.BigOrder;
                EnvVar.OrderSplitMinNum_exact = EnvVar.OrderSplitMinNum;
                goto rework;
            }

            outputGoodStock(sw, sw_sell, sw_rich);
            OutputResult.DoOutputWork(goodStock);
            sw.Close();
            sw_sell.Close();
            sw_rich.Close();



            DateTime end = DateTime.Now;
            MessageBox.Show("Completed! cost:" + (end - start).TotalSeconds.ToString());
        }

        void doWork(string infilename, string initial, StreamWriter sw = null, StreamWriter sw_sell = null,
            StreamWriter sw_rich= null)
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
                try
                {
                    //if (mBlackList.Contains(stock.name))
                    //    continue;
                    //stock.FoundBuy();
                    stock.TrimOrders();
                    stock.FoundTractorsOrders();
                    stock.findbuy2();
                    stock.FoundBuySplitSingle();
                    
                    
                    
                    stock.TrimSameFoundBuy();
                    stock.TrimGarbageOrder(Limits.Exact);
                }
                catch (Exception err)
                {
 
                }
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

                    if (stock.mTractorOrders.Count != 0)
                        sw_rich.WriteLine("-------------------------");

                    foreach (string key in stock.mTractorOrders.Keys)
                    {
                        if (!printHead)
                        {
                            Debug.WriteLine("found valuable info for " + stock.name);
                            sw_rich.WriteLine("===============");
                            sw_rich.WriteLine("stock name=" + stock.name);
                            printHead = true;
                        }
                        sw_rich.WriteLine("Tractor seqs : " + key);
                        foreach (OrderSeq os in stock.mTractorOrders[key])
                        {
                            sw_rich.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                        }
                        sw_rich.WriteLine("-------------------------");
                    }

                    if (stock.mBuySplits.Count != 0)
                        sw_rich.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");

                    foreach (string key in stock.mBuySplits.Keys)
                    {
                        if (!printHead)
                        {
                            Debug.WriteLine("found valuable info for " + stock.name);
                            sw_rich.WriteLine("===============");
                            sw_rich.WriteLine("stock name=" + stock.name);
                            printHead = true;
                        }

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
                    //stock.FoundSell();
                    stock.FoundSellSplitSingle();
                    stock.FoundSellTractorOrders();
                    stock.findsell2();
                    if (stock.foundSellOrders.Count != 0)
                    {
                        //continue;
                        Boolean printHead = false;

                        foreach (string key in stock.foundSellOrders.Keys)
                        {
                            List<long> parseKey = Toolbox.ParseKeys(key);
                            int rank = Toolbox.RankOrder(parseKey);
                            if (parseKey == null)
                                continue;
                            if (rank<2 && parseKey.Count == 2 && stock.foundSellOrders[key].Count == 2 && !Limits.Exact)
                                continue;
                            if (!printHead)
                            {
                                Debug.WriteLine("found valuable info for " + stock.name);
                                sw_sell.WriteLine("===============");
                                sw_sell.WriteLine("stock name=" + stock.name);
                                printHead = true;
                            }

                            
                            if (rank == 3)
                            {
                                sw.WriteLine("@@@@@@@@@@@@@");
                                sw_rich.WriteLine("@@@@@@@@@@@@@");
                            }
                            else if (rank == 2)
                            {
                                sw.WriteLine("$$$$$$$$$$$$$$$");
                                sw_rich.WriteLine("$$$$$$$$$$$$$$$");
                            }

                            Debug.WriteLine("sec " + key + "  found:");
                            sw_sell.WriteLine("serial is:" + key);

                            foreach (OrderSeq os in stock.foundSellOrders[key])
                            {
                                Debug.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                                sw_sell.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            }
                        }

                        if (stock.mSellTractorOrders.Count != 0)
                            sw_sell.WriteLine("-------------------------");

                        foreach (string key in stock.mSellTractorOrders.Keys)
                        {
                            if (!printHead)
                            {
                                Debug.WriteLine("found valuable info for " + stock.name);
                                sw_sell.WriteLine("===============");
                                sw_sell.WriteLine("stock name=" + stock.name);
                                printHead = true;
                            }
                            sw_sell.WriteLine("Tractor seqs :" + key);
                            foreach (OrderSeq os in stock.mSellTractorOrders[key])
                            {
                                sw_sell.WriteLine("Time:" + os.mTime + "    Price:" + os.mPrice.ToString());
                            }
                            sw_sell.WriteLine("-------------------------");
                        }
                        if (stock.mSellSplits.Count != 0)
                            sw_sell.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
                        foreach (string key in stock.mSellSplits.Keys)
                        {
                            if (!printHead)
                            {
                                Debug.WriteLine("found valuable info for " + stock.name);
                                sw_sell.WriteLine("===============");
                                sw_sell.WriteLine("stock name=" + stock.name);
                                printHead = true;
                            }

                            
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
            //JSON.Load()

            WebRequest req = new WebRequest();
            JsonArray jarry = new JsonArray();
            JsonObject jobj = (JsonObject) JsonNode.ParseJsonString(req.GetTencentDailyJason("600421"));
           

            return;
            SpecialInfo spinfo = new SpecialInfo();
            return;
            //OutputResult o = new OutputResult();
            MDataBase mongo = new MDataBase();
            return;
            WebRequest web = new WebRequest();
            StockOrder sstock = new StockOrder("002043", new DateTime());
            web.FillStock(sstock);

            Toolbox.foundDaysAgo(5);

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

        private void button3_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    MySqlConnection connection = new MySqlConnection("Data Source=localhost;Persist Security Info=yes;UserId=root; PWD=godset;");
            //    connection.Open();
            //    MySqlCommand cmdopen = new MySqlCommand("use test;", connection);
            //    cmdopen.ExecuteReader().Close(); ;
            //    MySqlCommand cmd = new MySqlCommand("select * from tabletest2;", connection);
            //    MySqlDataReader reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    { 

            //    }
            //}
            //catch (Exception err)
            //{
 
            //}
            DataBase db = new DataBase();
            //db.InsertOneStock(goodStock[0],"2016-07-24");
            //db.GetBydate("002766", "2016-07-24");
            db.UpdateGoodStocks(goodStock, textBox2.Text);


        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void checkBox_spEN_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                try
                {
                    EnvVar.Special_day_num = Convert.ToInt32(textBox_spN.Text);
                    EnvVar.SP_Threshold_wave = Convert.ToSingle(textBox_spW.Text);
                    EnvVar.SP_Threadhold_drop = Convert.ToSingle(textBox_spD.Text);
                    EnvVar.SP_Threshold_trap = Convert.ToSingle(textBox_spT.Text);

                }
                catch (Exception err)
                {
                    cb.Checked = false;
                    EnvVar.SP_Daily_Check = false;
                }

                EnvVar.SP_Daily_Check = true;
            }
            else
            {
                EnvVar.SP_Daily_Check = false;
            }
        }

        private void button_SP_Click(object sender, EventArgs e)
        {
            CheckBox cb = this.checkBox_spEN;
            if (cb.Checked)
            {
                try
                {
                    EnvVar.Special_day_num = Convert.ToInt32(textBox_spN.Text) + 1;
                    EnvVar.SP_Threshold_wave = Convert.ToInt32(textBox_spW.Text);
                    EnvVar.SP_Threadhold_drop = Convert.ToInt32(textBox_spD.Text);
                    EnvVar.SP_Threshold_trap = Convert.ToInt32(textBox_spT.Text);

                }
                catch (Exception err)
                {
                    cb.Checked = false;
                    EnvVar.SP_Daily_Check = false;
                }

                EnvVar.SP_Daily_Check = true;
            }
            else
            {
                EnvVar.SP_Daily_Check = false;
            }
            if (goodStock.Count == 0 || EnvVar.SP_Daily_Check == false)
                return;
            MultipleWork work = new MultipleWork(goodStock);
            work.go();
            SpecialInfo spinfo = new SpecialInfo();

            //this.updateHistory();
            Toolbox.CalculateStocksScores(goodStock, spinfo);
            goodStock.Sort(new StockScoreCompair());
            goodStock.Reverse();
            OutputResult.DoOutputWork(goodStock);

        }

        private void textBox_SP_Enter(object sender, EventArgs e)
        {
            try
            {
                TextBox textbox = (TextBox)sender;
                textbox.SelectAll();
            }
            catch (Exception err)
            { }
        }
    }
}
