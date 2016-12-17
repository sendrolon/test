using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace WindowsFormsApplication1
{
    public class L2OrderMongoDB
    {
       // public ObjectId Id { get; set; }
        public string _id { get; set; }
        public string[] s_orders { get; set; }
        public string[] b_orders { get; set; }
        public DateTime date { get; set; }
        public string data_type { get; set; }
        public long b_batches { get; set; }
        public long s_batches { get; set; }
        public float s_price { get; set; }
        public float b_price { get; set; }
        public string endtime { get; set; }
        public string starttimme { get; set; }
        public string symbol { get; set; }
        public long b_total { get; set; }
        public long s_total { get; set; }
    }
}
