using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using HtmlGenerator;

namespace WindowsFormsApplication1
{
    public class OutputResult
    {
        public static void DoOutputWork(List<StockOrder> stocks)
        {
            string out_filename;
            if (EnvVar.SP_Daily_Check)
            {
                out_filename = DateTime.Today.ToString("MM-dd") + "_N" + EnvVar.Special_day_num.ToString() + "_W" + EnvVar.SP_Threshold_wave.ToString()
                    + "_T" + EnvVar.SP_Threshold_trap.ToString() + "_D" + EnvVar.SP_Threadhold_drop.ToString() + ".html";
            }
            else
            {
                out_filename = DateTime.Today.ToString("MM-dd-") + DateTime.Now.ToFileTimeUtc() + ".html";
            }
            foreach (StockOrder stock in stocks)
            {
                stock.PrepareOutput();
            }
            OutputResult or = new OutputResult(stocks);
            or.OutputBuys(out_filename);
            //or.OutputBuys("test2.html");
        }

        List<StockOrder> goodStocks;
        public OutputResult(List<StockOrder> good)
        { 
            this.goodStocks = good;
        }

        //string js_jq = "";

        public void OutputBuys(string file)
        {
            HtmlDocument html = new HtmlDocument();
            //html.AddChild(Tag.Css("style.css"));
            HtmlMetaElement meta = new HtmlMetaElement();
            meta.AddAttribute(new HtmlHttpEquivAttribute("Content-Type"));
            meta.AddAttribute(new HtmlContentAttribute("text/html;charset=utf-8"));
            html.AddChild(meta);
            string js = File.ReadAllText("scripts.js");
            HtmlScriptElement script = Tag.Script;
            script.SetInnerText(js);
            script.WithType("text/javascript");
            html.AddChild(script);
            string add_css = File.ReadAllText("additional_css.css");
            
            HtmlStyleElement style = Tag.Style;
            style.SetInnerText("body { color: #000; background: #fff; margin: 0; padding: 0; } span { margin-left: 15px; width:20px; } table.merge_table { border: 0px solid black; border-top-width: 0px; margin-top: -1px; margin-bottom: -1px; margin-left: -1px; margin-right:-1px; } span.green { font-size:20px; color:green; } span.bold { color:blue; font-weight:bold; } span.span_history { font-weight:bold; color:purple; } span.mainTrap { font-weight:bold; color:red; } span.rank2 { color:blue; } h1,h2,h3,h4,h5,h6 { color: black; font-weight:normal; line-height:1px; font-size:15px; } h5{ margin-top: 5px; margin-bottom: 5px; padding-top: 9px; padding-bottom: 9px; } th.no_border_history { border: 1px solid lightblue; border-collapse:collapse; margin-top: -2px; margin-bottom: -2px; margin-left: -1px; margin-right:-1px; border-bottom-width: 0px !important; border-top-width: 0px !important; } th.no_border { border: 1px solid lightblue; border-collapse:collapse; margin-top: -1px; margin-bottom: -1px; margin-left: -1px; margin-right:-1px; border-left-width: 0px  !important; border-right-width: 0px !important; border-bottom-width: 0px !important; border-top-width: 0px !important; } th { border: 1px solid lightblue; border-collapse:collapse; text-align:left; margin-top: -1px; margin-bottom: -1px; margin-left: -1px; margin-right:-1px; vertical-align:top; } br { line-height:1px; } tr { width:100px } " + add_css);
            style.WithType("text/css");
            html.AddChild(style);
            foreach (StockOrder s in goodStocks)
            {
                if (EnvVar.SP_Daily_Check && !s.mSP_flag)
                    continue;
                HtmlElement element = GenerateOneStock(s);
                if (element != null)
                    html.AddChild(element);

                html.AddChild(Tag.Br);
            }

            StreamWriter sw = new StreamWriter(file);
            sw.Write(html.Serialize(HtmlSerializeType.PrettyPrint));
            sw.Close();
        }

        public OutputResult()
        {            
            
        }

        HtmlElement GenerateOneStock(StockOrder stock)
        {
            HtmlTableElement table = new HtmlTableElement();
            HtmlTrElement head = GenerateStockHead(stock);
            if (head != null)
                table.AddChild(head);

            HtmlTrElement history = GenerateHistory(stock);
            if (history != null)
                table.AddChild(history);

            HtmlTrElement display = new HtmlTrElement();
            display.WithId("display");
            HtmlThElement display0 = new HtmlThElement();
            display0.WithId("display0");
            
            HtmlThElement display1 = new HtmlThElement();
            display1.WithId("display1");

            HtmlTableElement display_table = new HtmlTableElement();
            display_table.WithClass("merge_table");

            table.AddChild(display);
            HtmlThElement display_th = new HtmlThElement();

            //display.AddChild(display0);
            //display.AddChild(display1);

            display.AddChild(display_th);
            display_th.AddChild(display_table);
            HtmlTrElement display_table_tr = new HtmlTrElement();
            display_table.AddChild(display_table_tr);
            display0.AddAttribute(new HtmlWidthAttribute("400px"));
            display_table_tr.AddChild(display0);
            display_table_tr.AddChild(display1);


            HtmlTableElement data_table = new HtmlTableElement();
            display0.AddChild(data_table);
            display0.WithClass("datatable");
            data_table.WithClass("merge_table");
            HtmlTableElement chart_table = new HtmlTableElement();
            chart_table.WithClass("merge_table");
            display1.AddChild(chart_table);
            data_table.WithId("data_table");

            HtmlTrElement repeat = GenerateRepeatSplits(stock);
            if (repeat != null)
                data_table.AddChild(repeat);

            HtmlTrElement splits = GenerateSplits(stock);
            if (splits != null)
                data_table.AddChild(splits);

            HtmlTrElement tractors = GenerateTractors(stock);
            if (tractors != null)
                data_table.AddChild(tractors);

            HtmlTrElement dailyK = GenerateDailyK(stock);
            chart_table.AddChild(dailyK);

            //HtmlTrElement timemin = GenerateTimeMinuteChart(stock);
            //chart_table.AddChild(timemin);

            return table;
        }

        HtmlTrElement GenerateHistory(StockOrder stock)
        {
            if (stock.mHistory.Count == 0 && !stock.sp_dazong && !stock.sp_jianchi && !stock.sp_jiejin)
                return null;
            HtmlTrElement tr = new HtmlTrElement();
            HtmlThElement th = new HtmlThElement();
            th.WithClass("no_border_history");
            tr.AddChild(th);

            HtmlH5Element h5 = new HtmlH5Element();
            HtmlSpanElement sp_his = new HtmlSpanElement();
            
            sp_his.WithInnerText("5日历史:" + stock.mHistory.Count.ToString() + "次");
            sp_his.WithClass("span_history");
            if (stock.mHistory.Count != 0)
            {
                h5.AddChild(sp_his);

                foreach (StockOrder his_stock in stock.mHistory)
                {
                    HtmlSpanElement sp = new HtmlSpanElement();
                    sp.WithInnerText(his_stock.mTime.ToString("MM-dd") + " " +
                        (his_stock.mTotalMainForceBuy / 100).ToString() + "手 " + his_stock.mBuySplitAvePrice.ToString("f3") + "元");
                    h5.AddChild(sp);
                }
            }

            HtmlSpanElement sp_special = new HtmlSpanElement();
            StringBuilder sb_special = new StringBuilder();

            if (stock.sp_dazong)
                sb_special.Append("近1个月大宗 ");
            if (stock.sp_jianchi)
                sb_special.Append("近1个月减持 ");
            if (stock.sp_jiejin)
                sb_special.Append("未来2个月解禁");

            sp_special.WithClass("bold");
            sp_special.WithInnerText(sb_special.ToString());
            h5.AddChild(sp_special);
            th.AddChild(h5);
            return tr;
        }

        HtmlTrElement GenerateDailyK(StockOrder stock)
        {
            HtmlTrElement tr = new HtmlTrElement();
            //HtmlThElement th = new HtmlThElement();
            //tr.AddChild(th);
            string url = WebRequest.GetTimeImageUrl(stock.name);

            HtmlImgElement img = new HtmlImgElement();
            img = Tag.Image(url);
            tr.AddChild(img);
            //tr.WithAttribute()
            //HtmlAttribute attr = new HtmlAttribute();
            //tr.WithStyle("align=\"center\" valign=\"bottom\"");
            
            tr.AddChild(Tag.Br);
            tr.AddChild(Tag.Br);

            url = WebRequest.GetDailyKImgUrl(stock.name);

            img = new HtmlImgElement();
            img = Tag.Image(url);
            tr.AddChild(img);
            return tr;

        }

        HtmlTrElement GenerateTractors(StockOrder stock)
        {
            if (stock.mTractorOrders.Count == 0)
                return null;

            HtmlTrElement tr = new HtmlTrElement();
            HtmlThElement th = new HtmlThElement();
            tr.AddChild(th);
            th.WithClass("no_border");
            tr.WithClass("no_border");

            foreach (string key in stock.mTractorOrders.Keys)
            {
                HtmlH5Element h5_tractorhead = new HtmlH5Element();
                HtmlPElement pe = new HtmlPElement();
                HtmlSpanElement span_head = new HtmlSpanElement();
                pe.WithInnerText("Tractors:" + key);
                span_head.AddChild(pe);
                h5_tractorhead.AddChild(span_head);
       
                th.AddChild(h5_tractorhead);
                foreach (OrderSeq os in stock.mTractorOrders[key])
                {
                    HtmlH5Element h5_one = new HtmlH5Element();
                    HtmlSpanElement span_time = new HtmlSpanElement();
                    HtmlSpanElement span_price = new HtmlSpanElement();

                    span_time.SetInnerText(os.mTime);
                    span_price.SetInnerText(os.mPrice.ToString());
                    h5_one.AddChild(span_time);
                    h5_one.AddChild(span_price);

                    th.AddChild(h5_one);
                }
                th.AddChild(Tag.Br);
            }
            return tr;

        }

        HtmlTrElement GenerateSplits(StockOrder stock)
        {
            if (stock.mBuySplits.Keys.Count == 0)
                return null;

            HtmlTrElement tr = new HtmlTrElement();
            tr.WithId("splits_tr");
            HtmlThElement th = new HtmlThElement();
            th.WithId("splits_th");
            
            tr.AddChild(th);
            th.WithClass("no_border");
            tr.WithClass("no_border");

            foreach (string key in stock.mBuySplits.Keys)
            {
                HtmlH5Element h5_onesplithead = new HtmlH5Element();
                HtmlSpanElement span_splithead = new HtmlSpanElement();
                span_splithead.SetInnerText("Splits:" + key);
                h5_onesplithead.AddChild(span_splithead);
                th.AddChild(h5_onesplithead);

                foreach (OrderSeq os in stock.mBuySplits[key])
                {
                    HtmlH5Element h5_onesplit = new HtmlH5Element();
                    HtmlSpanElement span_time = new HtmlSpanElement();
                    HtmlSpanElement span_price = new HtmlSpanElement();

                    span_time.SetInnerText(os.mTime);
                    span_price.SetInnerText(os.mPrice.ToString());
                    h5_onesplit.AddChild(span_time);
                    h5_onesplit.AddChild(span_price);
                    th.AddChild(h5_onesplit);
                }
                th.AddChild(Tag.Br);
            }
            return tr;
        }

        HtmlTrElement GenerateRepeatSplits(StockOrder stock)
        {
            if (stock.foundBuyOrders.Keys.Count == 0)
                return null;

            HtmlTrElement tr = new HtmlTrElement();
            HtmlThElement th = new HtmlThElement();
            tr.AddChild(th);
            th.WithClass("no_border");
            tr.WithClass("no_border");
            foreach (string key in stock.foundBuyOrders.Keys)
            {
                HtmlH5Element h5_oneseqhead = new HtmlH5Element();
                HtmlSpanElement span_contSeqLabel = new HtmlSpanElement();
                span_contSeqLabel.WithInnerText("Serial:");

                HtmlSpanElement span_serial = new HtmlSpanElement();
                span_serial.SetInnerText(key);
                //We can set class/color for serials
                int rank = Toolbox.RankOrder(Toolbox.ParseKeys(key));
                if (rank >= 2)
                    span_serial.WithClass("rank2");

                h5_oneseqhead.AddChild(span_contSeqLabel);
                h5_oneseqhead.AddChild(span_serial);
                th.AddChild(h5_oneseqhead);

                foreach (OrderSeq os in stock.foundBuyOrders[key])
                {
                    HtmlH5Element h5_oneseq = new HtmlH5Element();
                    HtmlSpanElement span_time = new HtmlSpanElement();
                    HtmlSpanElement span_price = new HtmlSpanElement();

                    span_time.SetInnerText(os.mTime);
                    span_price.SetInnerText(os.mPrice.ToString());
                    h5_oneseq.AddChild(span_time);
                    h5_oneseq.AddChild(span_price);

                    th.AddChild(h5_oneseq);
                }

                th.AddChild(Tag.Br);
            }

            return tr;
        }

        HtmlTrElement GenerateStockHead(StockOrder stock)
        {
            HtmlTrElement shead = new HtmlTrElement();
            HtmlThElement th = new HtmlThElement();
            HtmlH5Element h5 = new HtmlH5Element();
            HtmlSpanElement sp_name = new HtmlSpanElement();
            HtmlSpanElement sp_cname = new HtmlSpanElement();
            HtmlSpanElement sp_totalBuy = new HtmlSpanElement();
            HtmlSpanElement sp_percent = new HtmlSpanElement();
            HtmlSpanElement sp_trap = new HtmlSpanElement();
            HtmlSpanElement sp_marketv = new HtmlSpanElement();
            HtmlSpanElement sp_wave = new HtmlSpanElement();
            HtmlSpanElement sp_score = new HtmlSpanElement();
            HtmlSpanElement sp_special = new HtmlSpanElement();
            sp_name.SetInnerText(stock.name);
            sp_name.WithClass("green");
            sp_cname.SetInnerText(stock.CName);
            if (Limits.Exact)
                sp_totalBuy.SetInnerText("买入：" + ((stock.mTotalMainForceBuy) / 100).ToString() + "  @ " + stock.mBuySplitAvePrice.ToString("#.000") + "元");
            else
                sp_totalBuy.SetInnerText("买入："+ ((stock.mTotalMainForceBuy)/100).ToString());
            //HtmlTitleAttribute title = new HtmlTitleAttribute("test");
            //sp_totalBuy.AddAttribute(title);
            
            sp_trap.SetInnerText((stock.TrapPercent() > 0 ? "被套：" + stock.TrapPercent().ToString("f3") + "%": ""));
            sp_trap.WithClass("mainTrap");
            sp_percent.SetInnerText("比例：" + stock.MainForcePercentage.ToString("f3") + "%");
            if (stock.MainForcePercentage >= 3.8)
                sp_percent.WithClass("bold");
            sp_score.SetInnerText("S:" + stock.mScore.ToString());
            sp_marketv.SetInnerText("流通市值：" + stock.MarketFloatValue.ToString("f3") + "亿");
            sp_wave.SetInnerText("振幅：" + stock.Wave.ToString("f4") + "%");

            StringBuilder sb_special = new StringBuilder();
            if (stock.sp_zhangting)
                sb_special.Append("近10日涨停 ");
            if (stock.sp_fupai)
                sb_special.Append("近20日复牌 ");

            sp_special.WithClass("mainTrap");
            sp_special.SetInnerText(sb_special.ToString());

            h5.AddChild(sp_name);
            h5.AddChild(sp_cname);
            h5.AddChild(sp_totalBuy);
            h5.AddChild(sp_percent);

            h5.AddChild(sp_marketv);
            h5.AddChild(sp_wave);
            h5.AddChild(sp_score);
            h5.AddChild(sp_trap);
            h5.AddChild(sp_special);

            //if (stock.mHistory.Count != 0)
            //{
            //    HtmlSpanElement span_history_count = new HtmlSpanElement();
            //    h5.AddChild(Tag.Br);
            //    span_history_count.WithInnerText("History:" + stock.mHistory.Count.ToString());
            //    h5.AddChild(span_history_count);
            //}

           
            th.AddChild(h5);
            shead.AddChild(th);

            return shead;
        }

        void test()
        {
            HtmlDocument html = new HtmlDocument();
            HtmlTableElement table = new HtmlTableElement();
            html.AddChild(Tag.Css("style.css"));
            table.AddChild(Tag.H5.WithInnerText("test"));
            html.AddChild(table);
            //table.AddChild(Tag.Tr.WithInnerText("tr0"));
            //table.AddChild(Tag.Tr.WithInnerText("tr1"));
            HtmlTrElement tr0 = new HtmlTrElement();

            tr0.AddChild(Tag.Th.WithInnerText("tr00"));

            tr0.AddChild(Tag.Th.WithInnerText("tr01"));
            tr0.AddChild(Tag.Th.WithInnerText("tr02"));
            tr0.AddChild(Tag.Th.WithInnerText("tr03"));
            tr0.AddChild(Tag.Th.WithInnerText("tr04"));

            //HtmlStyleElement style = Tag.Style;
            //style.SetInnerText("body {color: #000; background: #f0f;  margin: 0;  padding: 0;}");
            //style.WithType("text/css");
            //html.AddChild(style);

            HtmlTrElement tre = new HtmlTrElement();
            tre.AddChild(Tag.Th.WithInnerText("th0"));
            tre.AddChild(Tag.Th.WithInnerText("th1"));
            tre.AddChild(Tag.Th.WithInnerText("th2"));

            //tre.AddChild(Tag.Th.WithInnerText("th3"));
            HtmlImgElement img = new HtmlImgElement();
            //img.AddChild();
            HtmlAttribute attr = new HtmlColorAttribute("red");
            HtmlSizeAttribute sizeattr = new HtmlSizeAttribute("18px");

            HtmlThElement th2 = Tag.Th;
            th2.AddChild(Tag.H5.WithInnerText("th21"));
            // th2.AddChild(Tag.Br);
            th2.AddChild(Tag.H5.WithInnerText("th22"));

            th2.AddAttribute(attr);
            th2.WithAttribute(sizeattr);
            tre.AddChild(th2);

            img = Tag.Image("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/logo_white_fe6da1ec.png");
            //tre.AddChild(Tag.Image("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/logo_white_fe6da1ec.png"));
            img.WithHeight("10");
            tre.AddChild(img);

            table.AddChild(tr0);
            table.AddChild(tre);
            HtmlOutputElement output = new HtmlOutputElement();
            string ser = html.Serialize(HtmlSerializeType.PrettyPrint);
            ser.Replace("th", "td");
            StreamWriter sw = new StreamWriter("test.html");
            sw.Write(ser);
            sw.Close();
        }
    }
}
