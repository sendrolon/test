using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
//using System.Linq;

namespace WindowsFormsApplication1
{

    public class MDataBase
    {
        string mongo_url = "mongodb://10.193.101.110:27017";
        //string mongo_url = "mongodb://10.193.101.110:27017";

        public async void dowork(string sym)
        {
            MongoClient client = new MongoClient(mongo_url);
            var db = client.GetDatabase("stock");
#if false
            var collection = db.GetCollection<L2OrderMongoDB>("l2_orders");
            var list = collection.Find(x => x.symbol == "sz002099").ToList();
#endif
            var collection = db.GetCollection<BsonDocument>("l2_orders");
            //var documents = await collection.Find(new BsonDocument()).ToListAsync();
            //var find = collection.Find(new BsonDocument("symbol", "sz002099"));
            
            var list = await collection.Find(new BsonDocument("symbol", sym)).ToListAsync();

            //new BsonDocument()

            foreach (var order in list)
            {
                //Debug.WriteLine("order" + order["symbol"]);
            }

            Debug.WriteLine("symbol done." + sym);

        }
        public MDataBase()
        {
            for (int i = 0; i< 20; i++) { 
                dowork("sz002099");
                dowork("sz150153");
                dowork("sz000673");
                dowork("sz603311");
                dowork("sz150195");
            }





        }
    }
}
