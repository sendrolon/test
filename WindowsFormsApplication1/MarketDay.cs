using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceJson;

namespace WindowsFormsApplication1
{
    public class MarketDay
    {
        public string Date { get; set; }
        public float HighPrice { get; set; }
        public float LowPrice { get; set; }
        public float OpenPrice { get; set; }
        public float ClosePrice { get; set; }
        public long Volum { get; set; }

        public Boolean Updated = false;

        public MarketDay()
        {

        }

        public MarketDay(JsonArray jarry)
        {
            this.FillByJsonArray(jarry);
        }

        public void FillByJsonArray(JsonArray jarry)
        {
            if (jarry == null)
                return;
            if (jarry.Count != 6)
                return;
            try
            {
                this.Date = jarry[0];
                string str = jarry[1];
                this.OpenPrice = Convert.ToSingle(str);
                str = jarry[2];
                this.ClosePrice = Convert.ToSingle(str);
                str = jarry[3];
                this.HighPrice = Convert.ToSingle(str);
                str = jarry[4];
                this.LowPrice = Convert.ToSingle(str);
                str = jarry[5];
                this.Volum = Convert.ToInt64(Convert.ToDouble(str));
            }
            catch (Exception err)
            {
                return;
            }
            this.Updated = true;
        }

    }
}
