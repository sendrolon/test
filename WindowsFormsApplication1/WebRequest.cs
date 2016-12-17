using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NiceJson;

namespace WindowsFormsApplication1
{

    public class SpecialInfo
    {
        string url_10day_zhangting = "http://m.iwencai.com/wap/search?qs=gx_wapclient&w=10%E6%97%A5%E5%86%85%E6%B6%A8%E5%81%9C&source=phone&queryarea=all&querytype=&tid=stockpick&perpage=20000&userid=&usertype=";
        string url_20day_fupai = "http://m.iwencai.com/wap/search?qs=gx_wapclient&w=20%E6%97%A5%E5%86%85%E5%A4%8D%E7%89%8C&source=phone&queryarea=all&querytype=&tid=stockpick&perpage=20000&userid=&usertype=";
        string url_3month_jiejin = "http://m.iwencai.com/wap/search?qs=gx_wapclient&w=%E6%9C%AA%E6%9D%A52%E4%B8%AA%E6%9C%88%E5%86%85%E8%A7%A3%E7%A6%81&source=phone&queryarea=all&querytype=&tid=stockpick&perpage=20000&userid=&usertype=";
        string url_2month_jianchi = "http://m.iwencai.com/wap/search?qs=gx_wapclient&w=1%E4%B8%AA%E6%9C%88%E5%86%85%E5%87%8F%E6%8C%81&source=phone&queryarea=all&querytype=&tid=stockpick&perpage=20000&userid=&usertype=";
        string url_1month_dazong = "http://m.iwencai.com/wap/search?qs=gx_wapclient&w=1%E4%B8%AA%E6%9C%88%E5%86%85%E5%A4%A7%E5%AE%97%E4%BA%A4%E6%98%93&source=phone&queryarea=all&querytype=&tid=stockpick&perpage=20000&userid=&usertype=";

        public string result_zhangting;
        public string result_fupai;
        public string result_jiejin;
        public string result_dazong;
        public string result_jianchi;
        public SpecialInfo()
        {
            //HttpHelper req;
            result_zhangting = new HttpHelper().HttpGet(url_10day_zhangting);
            result_fupai = new HttpHelper().HttpGet(url_20day_fupai);
            result_jiejin = new HttpHelper().HttpGet(url_3month_jiejin);
            result_dazong = new HttpHelper().HttpGet(url_1month_dazong);
            result_jianchi = new HttpHelper().HttpGet(url_2month_jianchi);
        }
    }
    public class WebRequest
    {

        static string DailyKImage = "http://image.sinajs.cn/newchart/daily/n/namename.gif";
        static string TimeImage = "http://image.sinajs.cn/newchart/min/n/namename.gif";

        public static string GetTimeImageUrl(string code)
        {
            string name = Toolbox.formFullCode(code);
            String url = new String(TimeImage.ToArray());
            url = url.Replace("namename", name);
            return url;
        }


        public static string GetDailyKImgUrl(string code)
        {
            string name = Toolbox.formFullCode(code);
            String url = new String(DailyKImage.ToArray());
            url = url.Replace("namename", name);
            return url;

        }

        Boolean checkTencent(string data)
        {
            if (!data.Contains("v_"))
                return false;
            return true;
        }

        

        public String GetDailyTencentURL(string name)
        {
            if (name == null)
                return null;
            StringBuilder sb = new StringBuilder(this.TencentDailyProtocol);
            sb.Replace("SYMBOL", Toolbox.formFullCode(name));
            sb.Replace("DAYS", EnvVar.Special_day_num.ToString());

            return sb.ToString();

        }
        public void FillStock(StockOrder stock)
        {
            //string fname = Toolbox.formFullCode(stock.name);
            string url = makeTencentRequestUrl(stock.name);
            string data = GetRequest(url);
            parseTencent(data, stock);

            if (EnvVar.SP_Daily_Check)
            {
                FillDailyStock(stock);
            }
            
            
        }
        void parseTencent(string data, StockOrder stock)
        {
            if (!checkTencent(data))
                return;

            string[] d = data.Split('~');
            if (d.Length <= 49)
                return;
            try
            {
                stock.CName = d[1];
                stock.lastPrice = Convert.ToSingle(d[3]);
                stock.Offset = Convert.ToSingle(d[32]);
                stock.TodayHigh = Convert.ToSingle(d[33]);
                stock.TodayLow = Convert.ToSingle(d[34]);
                stock.TodayVol = Convert.ToInt64(d[36]) * 100;
                stock.MarketFloatValue = Convert.ToSingle(d[44]);
                stock.MarketValue = Convert.ToSingle(d[45]);
                stock.Wave = Convert.ToSingle(d[43]);
            }
            catch
            { }
        }

        public void FillDailyStock(StockOrder stock)
        {
            string json = GetTencentDailyJason(stock.name);
            JsonObject jobj = (JsonObject) JsonNode.ParseJsonString(json);
            try
            {
                JsonArray jarry = (JsonArray)jobj["data"][Toolbox.formFullCode(stock.name)]["qfqday"];
                stock.FillMarketDays(jarry);

            }
            catch (Exception err)
            { }
        }

        public string GetTencentDailyJason(string name)
        {
            string url = GetDailyTencentURL(name);
            string rets = new HttpHelper().HttpGet(url);
            
            return rets.Remove(0, "kline_monthqfq=".Length);
        }

        string TencentDailyProtocol = "http://web.ifzq.gtimg.cn/appstock/app/fqkline/get?_var=kline_monthqfq&param=SYMBOL,day,,,DAYS,qfq";

        string TencentProtocol = "http://qt.gtimg.cn/q=";
        string makeTencentRequestUrl(string stockName)
        {
            return TencentProtocol + Toolbox.formFullCode(stockName);
        }
        public WebRequest()
        {
 
        }
       
        string GetRequest(string url)
        {
           // return Get(url);
            //return Post(url, "a");
            return new HttpHelper().HttpGet(url);
        }
    }

  

	/// <summary>
	/// Http操作类.
	/// </summary>
	public class HttpHelper
	{
		private const int ConnectionLimit = 100;
		//编码
		private Encoding _encoding = Encoding.Default;
		//浏览器类型
		private string[] _useragents = new string[]{
			"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0)",
			"Mozilla/5.0 (Windows NT 6.1; rv:36.0) Gecko/20100101 Firefox/36.0",
			"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:31.0) Gecko/20130401 Firefox/31.0"
		};
		
		private String _useragent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36";
		//接受类型
		private String _accept = "text/html, application/xhtml+xml, application/xml, */*";
		//超时时间
		private int _timeout = 30*1000;
		//类型
		private string _contenttype = "application/x-www-form-urlencoded";
		//cookies
		private String _cookies = "";
		//cookies
		private CookieCollection _cookiecollection;
		//custom heads
		private Dictionary<string,string> _headers = new Dictionary<string, string>();
		
		public HttpHelper()
		{
			_headers.Clear();
			//随机一个useragent
			_useragent = _useragents[new Random().Next(0,_useragents.Length)];
			//解决性能问题?
			ServicePointManager.DefaultConnectionLimit = ConnectionLimit;
		}
		
		public void InitCookie()
		{
			_cookies = "";
			_cookiecollection = null;
			_headers.Clear();
		}
		
		/// <summary>
		/// 设置当前编码
		/// </summary>
		/// <param name="en"></param>
		public void SetEncoding(Encoding en)
		{
			_encoding = en;
		}
		
		/// <summary>
		/// 设置UserAgent
		/// </summary>
		/// <param name="ua"></param>
		public void SetUserAgent(String ua)
		{
			_useragent = ua;
		}
		
		public void RandUserAgent()
		{
			_useragent = _useragents[new Random().Next(0,_useragents.Length)];
		}
		
		public void SetCookiesString(string c)
		{
			_cookies = c;
		}
		
		/// <summary>
		/// 设置超时时间
		/// </summary>
		/// <param name="sec"></param>
		public void SetTimeOut(int msec)
		{
			_timeout = msec;
		}
		
		public void SetContentType(String type)
		{
			_contenttype = type;
		}
		
		public void SetAccept(String accept)
		{
			_accept = accept;
		}
		
		/// <summary>
		/// 添加自定义头
		/// </summary>
		/// <param name="key"></param>
		/// <param name="ctx"></param>
		public void AddHeader(String key,String ctx)
		{
			//_headers.Add(key,ctx);
			_headers[key] = ctx;
		}
		
		/// <summary>
		/// 清空自定义头
		/// </summary>
		public void ClearHeader()
		{
			_headers.Clear();
		}
		
		/// <summary>
		/// 获取HTTP返回的内容
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		private String GetStringFromResponse(HttpWebResponse response)
		{
			String html = "";
			try
			{
				Stream stream = response.GetResponseStream();
				StreamReader sr = new StreamReader(stream,_encoding);
				html = sr.ReadToEnd();
					
				sr.Close();
				stream.Close();
			}
			catch(Exception e)
			{
				Trace.WriteLine("GetStringFromResponse Error: " + e.Message);
			}
			
			return html;
		}
		
		/// <summary>
		/// 检测证书
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		private bool CheckCertificate(object sender,X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
		    return true;
		}
		
		/// <summary>
		/// 发送GET请求
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public String HttpGet(String url)
		{
			return HttpGet(url,url);
		}
		
		
		/// <summary>
		/// 发送GET请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="refer"></param>
		/// <returns></returns>
		public String HttpGet(String url,String refer)
		{
			String html;
			try
			{
				ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckCertificate);
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.UserAgent = _useragent;
				request.Timeout = _timeout;
				request.ContentType = _contenttype;
				request.Accept = _accept;
				request.Method = "GET";
				request.Referer = refer;
				request.KeepAlive = true;
				request.AllowAutoRedirect = true;
				request.UnsafeAuthenticatedConnectionSharing = true;
				request.CookieContainer = new CookieContainer();
				//据说能提高性能
				request.Proxy = null;
				if(_cookiecollection != null)
				{
					foreach(Cookie c in _cookiecollection)
					{
						c.Domain = request.Host;
					}
					
					request.CookieContainer.Add(_cookiecollection);
				}

				foreach(KeyValuePair<String, String> hd in _headers)
				{
					request.Headers[hd.Key] = hd.Value;
				}
				
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				html = GetStringFromResponse(response);
				if(request.CookieContainer != null)
				{
					response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
				}
				
				if(response.Cookies != null)
				{
					_cookiecollection = response.Cookies;
				}
				if(response.Headers["Set-Cookie"] != null)
				{
					string tmpcookie = response.Headers["Set-Cookie"];			
					_cookiecollection.Add(ConvertCookieString(tmpcookie));
				}
				
				response.Close();
				return html;
			}
			catch(Exception e)
			{
				Trace.WriteLine("HttpGet Error: " + e.Message);
				return String.Empty;
			}			
		}
		
		/// <summary>
		/// 获取MINE文件
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public Byte[] HttpGetMine(String url)
		{
			Byte[] mine = null;
			try
			{
				ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckCertificate);
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.UserAgent = _useragent;
				request.Timeout = _timeout;
				request.ContentType = _contenttype;
				request.Accept = _accept;
				request.Method = "GET";
				request.Referer = url;
				request.KeepAlive = true;
				request.AllowAutoRedirect = true;
				request.UnsafeAuthenticatedConnectionSharing = true;
				request.CookieContainer = new CookieContainer();
				//据说能提高性能
				request.Proxy = null;
				if(_cookiecollection != null)
				{
					foreach(Cookie c in _cookiecollection)
						c.Domain = request.Host;
					request.CookieContainer.Add(_cookiecollection);
				}

				foreach(KeyValuePair<String, String> hd in _headers)
				{
					request.Headers[hd.Key] = hd.Value;
				}
				
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				Stream stream = response.GetResponseStream();
				MemoryStream ms = new MemoryStream();
				
				byte[] b = new byte[1024];
				while(true)
				{
					int s = stream.Read(b,0,b.Length);
					ms.Write(b,0,s);
					if(s == 0 || s < b.Length)
					{
						break;
					}
				}
				mine = ms.ToArray();
				ms.Close();
				
				if(request.CookieContainer != null)
				{
					response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
				}
				
				if(response.Cookies != null)
				{
					_cookiecollection = response.Cookies;
				}
				if(response.Headers["Set-Cookie"] != null)
				{
					_cookies = response.Headers["Set-Cookie"];
				}
				
				stream.Close();
				stream.Dispose();
				response.Close();
				return mine;
			}
			catch(Exception e)
			{
				Trace.WriteLine("HttpGetMine Error: " + e.Message);
				return null;
			}						
		}
		
		/// <summary>
		/// 发送POST请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public String HttpPost(String url,String data)
		{
			return HttpPost(url,data,url);
		}
		
		/// <summary>
		/// 发送POST请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <param name="refer"></param>
		/// <returns></returns>
		public String HttpPost(String url,String data,String refer)
		{
			String html;
			try
			{
				ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckCertificate);
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.UserAgent = _useragent;
				request.Timeout = _timeout;
				request.Referer = refer;
				request.ContentType = _contenttype;
				request.Accept = _accept;
				request.Method = "POST";
				request.KeepAlive = true;
				request.AllowAutoRedirect = true;
				request.CookieContainer = new CookieContainer();
				//据说能提高性能
				request.Proxy = null;

				if(_cookiecollection != null)
				{
					foreach(Cookie c in _cookiecollection)
						c.Domain = request.Host;
					request.CookieContainer.Add(_cookiecollection);
				}

				foreach(KeyValuePair<String, String> hd in _headers)
				{
					request.Headers[hd.Key] = hd.Value;
				}
				byte[] buffer = _encoding.GetBytes(data.Trim());
				request.ContentLength = buffer.Length;
				request.GetRequestStream().Write(buffer,0,buffer.Length);
				request.GetRequestStream().Close();
				
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				html = GetStringFromResponse(response);
				if(request.CookieContainer != null)
				{
					response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
				}
				if(response.Cookies != null)
				{
					_cookiecollection = response.Cookies;
				}
				if(response.Headers["Set-Cookie"] != null)
				{
					string tmpcookie = response.Headers["Set-Cookie"];
					_cookiecollection.Add(ConvertCookieString(tmpcookie));
				}			
				
				response.Close();
				return html;
			}
			catch(Exception e)
			{
				Trace.WriteLine("HttpPost Error: " + e.Message);
				return String.Empty;
			}
		}
				
				
		public string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = _encoding.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }
            
            return (sb.ToString());
        }
		
		/// <summary>
		/// 转换cookie字符串到CookieCollection
		/// </summary>
		/// <param name="ck"></param>
		/// <returns></returns>
		private CookieCollection ConvertCookieString(string ck)
		{
			CookieCollection cc = new CookieCollection();
			string[] cookiesarray = ck.Split(";".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
			for(int i = 0;i < cookiesarray.Length;i++)
			{
				string[] cookiesarray_2 = cookiesarray[i].Split(",".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
				for(int j = 0;j < cookiesarray_2.Length;j++)
				{
					string[] cookiesarray_3 = cookiesarray_2[j].Trim().Split("=".ToCharArray());
					if(cookiesarray_3.Length == 2)
					{
						string cname = cookiesarray_3[0].Trim();
						string cvalue = cookiesarray_3[1].Trim();
						if(cname != "domain" && cname != "path" && cname != "expires")
						{
							Cookie c = new Cookie(cname,cvalue);
							cc.Add(c);
						}
					}
				}
			}
			
			return cc;
		}
		
		
		public void DebugCookies()
		{
			Trace.WriteLine("**********************BEGIN COOKIES*************************");
			foreach(Cookie c in _cookiecollection)
			{
				Trace.WriteLine(c.Name + "=" + c.Value);
				Trace.WriteLine("Path=" + c.Path);
				Trace.WriteLine("Domain=" + c.Domain);
			}
			Trace.WriteLine("**********************END COOKIES*************************");
		}
	
	}
}

