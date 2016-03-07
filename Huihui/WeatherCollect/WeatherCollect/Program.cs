using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherCollect
{
    using System.IO;
    using System.Net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class WeatherLine
    {
        public string City { get; set; }

        public string Time { get; set; }

        public string Weather { get; set; }

        public string MaxTemp { get; set; }

        public string MinTemp { get; set; }

        public string WindDirction { get; set; }

        public string WindPower { get; set; }

        public override string ToString()
        {
            var properties = this.GetType().GetProperties();

            var sb = new StringBuilder();

            foreach (var info in properties)
            {
                var value = info.GetValue(this, null) ?? "(null)";
                sb.Append(value.ToString() + ",");
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            ConvertFromJsonToWeatherLine();
        }

        public static void ConvertFromJsonToWeatherLine()
        {
            var file = @"D:\Git\TestCode\Huihui\WeatherCollect\WeatherCollect\bin\Debug\result.txt";
            var lines = File.ReadAllLines(file);
            var weatherList = new List<WeatherLine>();
            foreach (var line in lines)
            {
                dynamic obj = JObject.Parse(line);
                var body = obj.showapi_res_body;
                var list = body.list;
                foreach (var item in list)
                {
                    item.city = body.area;
                    //Console.WriteLine(item.Count);
                    weatherList.Add(
                        new WeatherLine
                            {
                                City = item.city,
                                MaxTemp = item.max_temperature,
                                MinTemp = item.min_temperature,
                                Time = item.time,
                                Weather = item.weather,
                                WindDirction = item.wind_direction,
                                WindPower = item.wind_power
                            });
                }
            }

            File.WriteAllLines("converted.txt", weatherList.Select(i => i.ToString()).ToList());

        }

        public static void CollectWeatherData()
        {
            var uriformat = "http://route.showapi.com/9-7?showapi_appid={0}&showapi_timestamp={1}&areaid=&area={2}&month={3}&showapi_sign={4}";
            //var city = "北京";
            //string[] citys = { "北京", "廊坊", "天津", "沧州", "德州", "济南", "泰安", "曲阜", "滕州", "枣庄", "徐州" };
            string[] citys = { "宿州", "蚌埠", "定远", "滁州", "南京", "镇江", "丹阳", "常州", "无锡", "苏州", "昆山", "上海" };
            //var appid = 15880;
            //var secret = "36036516ff7249cca58c0ba086b0840b";
            var appid = 15886;
            var secret = "7fbe1cf4f41941af9d03f36db24e5095";
            var timestamp = GetTimestamp(DateTime.Now);

            foreach (var city in citys)
            {
                for (int i = 0; i < 7; i++)
                {
                    DateTime time = new DateTime(2015, i + 1, 1);
                    var datestr = time.ToString("yyyyMM");
                    var url = string.Format(
                        uriformat,
                        appid,
                        timestamp,
                        city,
                        datestr,
                        secret);

                    WebRequest webRequest = WebRequest.Create(url);
                    WebResponse webResp = webRequest.GetResponse();

                    Console.WriteLine("Content length is {0}", webResp.ContentLength);
                    Console.WriteLine("Content type is {0}", webResp.ContentType);

                    // Get the stream associated with the response.
                    Stream receiveStream = webResp.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    using (StreamWriter sw = new StreamWriter("result.txt", true, Encoding.UTF8))
                    {
                        using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            var content = readStream.ReadToEnd();
                            Console.Write(content);
                            sw.WriteLine(content);
                            System.Threading.Thread.Sleep(1000);
                        }
                    }

                    Console.WriteLine("Response stream received.");
                }
            }


            Console.Read();
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }
    }
}
