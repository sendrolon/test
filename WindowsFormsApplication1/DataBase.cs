using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WindowsFormsApplication1
{
    public class DataBase
    {
        /*
         *  create table found_db (
            name char(6) not null,
            date date not null,
            time time not null,
            seq char(64) not null,
            price float(6,3) not null,
            type tinyint not null);
         */

        MySqlConnection connection;
        const string scmd_connect_mysql = "Data Source=localhost;Persist Security Info=yes;UserId=root; PWD=godset;";
        const string scmd_use_db = "use sendrolon;";
        //const string templete_create_stock_table = "";

        const string templete_insert_one = "insert into found_db values(\"CODE\", \"DATE\", \"TIME\", \"SEQ\",PRICE,\"TYPE\");";

        const string templete_delete_by_date = "delete from found_db where date=\"DATE\";";

        const string templete_found_by_one_date = "select * from found_db where date=\"DATE\" and name=\"NAME\";";

        public DataBase()
        {
            connection = new MySqlConnection(scmd_connect_mysql);
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(scmd_use_db,connection);
            cmd.ExecuteReader().Close();
        }

        Boolean DeleteAllByDate(string date)
        {
            StringBuilder sb = new StringBuilder(templete_delete_by_date);
            sb.Replace("DATE", date);
            this.ExecuteOneCommand(sb.ToString());
            return true;
        }

        public void ExecuteOneCommand(string cmd_str)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                return;
            MySqlCommand cmd = new MySqlCommand(cmd_str, connection);
            cmd.ExecuteNonQuery();
        }

        public Boolean InsertOneStock(StockOrder stock, string date)
        {

            if (connection.State != System.Data.ConnectionState.Open)
                return false;
            StringBuilder sb0 = new StringBuilder(templete_insert_one);
            sb0.Replace("CODE", stock.name);
            sb0.Replace("DATE", date);
            string sb0_temp = sb0.ToString();
            foreach(string key in stock.foundBuyOrders.Keys)
            {
                foreach (OrderSeq os in stock.foundBuyOrders[key])
                {
                    StringBuilder sb1 = new StringBuilder(sb0_temp);
                    sb1.Replace("TIME", os.mTime.Substring(0, 8));
                    sb1.Replace("SEQ", key);
                    sb1.Replace("PRICE", os.mPrice.ToString());
                    sb1.Replace("TYPE", "1");
                    ExecuteOneCommand(sb1.ToString());
                }
            }
            foreach (string key in stock.mBuySplits.Keys)
            {
                foreach (OrderSeq os in stock.mBuySplits[key])
                {
                    StringBuilder sb1 = new StringBuilder(sb0_temp);
                    sb1.Replace("TIME", os.mTime.Substring(0, 8));
                    sb1.Replace("SEQ", key);
                    sb1.Replace("PRICE", os.mPrice.ToString());
                    sb1.Replace("TYPE", "2");
                    ExecuteOneCommand(sb1.ToString());
                }
            }
            foreach (string key in stock.mTractorOrders.Keys)
            {
                foreach (OrderSeq os in stock.mTractorOrders[key])
                {
                    StringBuilder sb1 = new StringBuilder(sb0_temp);
                    sb1.Replace("TIME", os.mTime.Substring(0, 8));
                    if (key.Length >=64)
                        sb1.Replace("SEQ", key.Substring(0, 64));
                    else
                        sb1.Replace("SEQ", key);
                    sb1.Replace("PRICE", os.mPrice.ToString());
                    sb1.Replace("TYPE", "3");
                    ExecuteOneCommand(sb1.ToString());
                }
            }

            return true;

        }

        public Boolean UpdateGoodStocks(List<StockOrder> list, string date, Boolean forceUpdate = true)
        {
            if (forceUpdate)
                this.DeleteAllByDate(date);

            foreach (StockOrder stock in list)
            {
                this.InsertOneStock(stock, date);
            }
            return true;
        }

        public StockOrder GetBydate(string name, string date)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                return null;
            StringBuilder sb = new StringBuilder(templete_found_by_one_date);
            sb.Replace("DATE", date);
            sb.Replace("NAME", name);
            MySqlDataReader reader;
            MySqlCommand cmd = new MySqlCommand(sb.ToString(), connection);
            reader = cmd.ExecuteReader();

            StockOrder stock = null;
            string time;
            string seq;
            float price;
            int type;
            while (reader.Read())
            {
                if (stock == null)
                {
                    stock = new StockOrder(name, reader.GetDateTime(1));                    
                }
                time = reader.GetString(2);
                if (time.Length == 12)
                    time = time.Substring(0, 8);
                seq = reader.GetString(3);
                price = reader.GetFloat(4);
                type = reader.GetInt16(5);
                stock.InsertOneSeq(time, seq, price,type);
            }

            //cmd.ExecuteNonQuery();
            reader.Close();
            return stock;
        }

        
    }
}
