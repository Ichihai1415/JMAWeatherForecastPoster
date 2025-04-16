using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Nodes;

namespace JMAWeatherForecastPoster
{
    [SupportedOSPlatform("windows7.0")]
    internal class Program//6:20以降くらい起動目安
    {
        internal static HttpClient client = new();
        static void Main(string[] args)
        {
            try
            {
                var getDt = DateTime.Parse(client.GetStringAsync("https://www.jma.go.jp/bosai/amedas/data/latest_time.txt").Result);
                Console.WriteLine("get dateTime: " + getDt.ToString());
                var url_amedas = $"https://www.jma.go.jp/bosai/amedas/data/point/56227/{getDt:yyyyMMdd}_{Get3H(getDt)}.json";
                var url_forecast = "https://www.jma.go.jp/bosai/forecast/data/forecast/170000.json";
                var url_warning = "https://www.jma.go.jp/bosai/warning/data/warning/170000.json";
                var url_overview = "https://www.jma.go.jp/bosai/forecast/data/overview_forecast/170000.json";
                var jsonRaw_amedas = client.GetStringAsync(url_amedas).Result;
                Console.WriteLine("GET: " + url_amedas);
                var jsonRaw_forecast = client.GetStringAsync(url_forecast).Result;
                Console.WriteLine("GET: " + url_forecast);
                var jsonRaw_warning = client.GetStringAsync(url_warning).Result;
                Console.WriteLine("GET: " + url_warning);
                var jsonRaw_overview = client.GetStringAsync(url_overview).Result;
                Console.WriteLine("GET: " + url_overview);
                var json_amedas = JsonNode.Parse(jsonRaw_amedas);
                var json_forecast = JsonNode.Parse(jsonRaw_forecast);
                var json_warning = JsonNode.Parse(jsonRaw_warning);
                var json_overview = JsonNode.Parse(jsonRaw_overview);
                Console.WriteLine("Parse finish");

                var image = new Bitmap(1000, 1000);
                using var g = Graphics.FromImage(image);
                g.Clear(Color.White);
                var text = new StringBuilder();

                text.AppendLine("[金沢のアメダス観測情報]");
                var amedasData = json_amedas[getDt.ToString("yyyyMMddHH") + "0000"];
                text.Append("観測: ");
                text.Append(getDt.Day);
                text.Append('日');
                text.Append(getDt.Hour);
                text.Append("時　気温: ");
                text.Append((double?)amedasData["temp"][0]);
                text.Append(AQC2String[((int?)amedasData["temp"][1]) ?? -1]);
                text.Append("℃　24h降水量: ");
                text.Append((double?)amedasData["precipitation24h"][0]);
                text.Append(AQC2String[((int?)amedasData["precipitation24h"][1]) ?? -1]);
                text.Append("mm　風向風速: ");
                text.Append(WindDirection2String[((int?)amedasData["windDirection"][0]) ?? -1]);
                text.Append(AQC2String[((int?)amedasData["windDirection"][1]) ?? -1]);
                text.Append(' ');
                text.Append((double?)amedasData["wind"][0]);
                text.Append(AQC2String[((int?)amedasData["wind"][1]) ?? -1]);
                text.Append("m/s\n積雪深: ");
                text.Append((int?)amedasData["snow"][0]);
                text.Append(AQC2String[((int?)amedasData["snow"][1]) ?? -1]);
                text.Append("cm　24h降雪量: ");
                text.Append((int?)amedasData["snow24h"][0]);
                text.Append(AQC2String[((int?)amedasData["snow24h"][1]) ?? -1]);
                text.Append("cm　湿度: ");
                text.Append((int?)amedasData["humidity"][0]);
                text.Append(AQC2String[((int?)amedasData["humidity"][1]) ?? -1]);
                text.Append("%　気圧: ");
                text.Append((double?)amedasData["pressure"][0]);
                text.Append(AQC2String[((int?)amedasData["pressure"][1]) ?? -1]);
                text.Append("hPa　自動観測天気: ");
                text.Append(Weather2String[((int?)amedasData["weather"][0]) ?? -1]);
                text.AppendLine(AQC2String[((int?)amedasData["weather"][1]) ?? -1]);


                text.AppendLine("[金沢市の気象警報・注意報]");
                text.Append((string)json_warning["publishingOffice"]);
                text.Append('　');
                text.Append(DateTime.Parse((string)json_warning["reportDatetime"]).ToString("yyyy/MM/dd HH:mm:ss"));
                text.AppendLine("発表");
                text.AppendLine((string)json_warning["headlineText"]);
                text.AppendLine();
                text.Append("発表中: ");
                foreach (var areaType in json_warning["areaTypes"].AsArray())
                    foreach (var area in areaType["areas"].AsArray())
                        if ((string)area["code"] == "1720100")
                        {
                            Console.WriteLine("code: 1720100  found");
                            var warnings = area["warnings"].AsArray();
                            var c = 0;
                            foreach (var warning in warnings)
                            {
                                var code = int.Parse((string)warning["code"]);
                                var name = GetWarningName(code);
                                if (code == 0) break;
                                else if ((string)warning["status"] == "解除") continue;
                                text.Append(name);
                                text.Append('　');
                                c++;
                            }
                            if (c == 0) text.Append("なし");
                        }
                text.AppendLine();
                text.AppendLine();
                text.AppendLine();
                var forecast = json_forecast[0]["timeSeries"][0]["areas"][0];
                var d = forecast["weathers"].AsArray().Count;
                text.AppendLine("[石川県加賀の天気予報（" + (d == 2 ? "明日" : "明後日") + "までの詳細）]");
                text.Append("今日　: ");
                text.Append(((string)forecast["weathers"][0]).Replace("　", " "));
                text.Append('　');
                text.AppendLine(((string)forecast["winds"][0]).Replace("　", " "));

                text.Append("明日　: ");
                text.Append(((string)forecast["weathers"][1]).Replace("　", " "));
                text.Append('　');
                text.AppendLine(((string)forecast["winds"][1]).Replace("　", " "));

                if (d == 3)
                {
                    text.Append("明後日: ");
                    text.Append(((string)forecast["weathers"][2]).Replace("　", " "));
                    text.Append('　');
                    text.AppendLine(((string)forecast["winds"][2]).Replace("　", " "));
                }

                text.AppendLine();
                text.AppendLine();
                text.AppendLine("[石川県の天気概況]");
                text.AppendLine(((string)json_overview["text"]).Replace("\\n", "\n").Replace("\n\n", "\n").Replace("\n＜", "\n\n＜"));


                g.DrawString(text.ToString(), new Font("Meiryo", 16), Brushes.Black, new RectangleF(10, 10, 990, 990));
                var path = "output\\" + DateTime.Now.ToString("yyyyMM");
                Directory.CreateDirectory(path);
                path += "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
                image.Save(path, ImageFormat.Png);
                XPost("[自動]気象情報をお知らせします。\n\n※現在準備中です", path);
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory("error\\" + DateTime.Now.ToString("yyyyMM"));
                File.WriteAllText("error\\" + DateTime.Now.ToString("yyyyMM\\\\yyyyMMddHHmmss"),ex.ToString());
            }
        }


        public static string Get3H(DateTime dt)
        {
            if (dt.Hour < 3) return "00";
            else if (dt.Hour < 6) return "03";
            else if (dt.Hour < 9) return "06";
            else if (dt.Hour < 12) return "09";
            else if (dt.Hour < 15) return "12";
            else if (dt.Hour < 18) return "15";
            else if (dt.Hour < 21) return "18";
            else return "21";
        }

        public static string GetWarningName(int code) => Warning_Code2Name.TryGetValue(code, out string? value) ? value : "(名称取得失敗)";


        public static readonly Dictionary<int, string> Warning_Code2Name = new()
        {
            { 0, "解除" },
            { 2, "暴風雪警報" },
            { 3, "大雨警報" },
            { 4, "洪水警報" },
            { 5, "暴風警報" },
            { 6, "大雪警報" },
            { 7, "波浪警報" },
            { 8, "高潮警報" },
            { 10, "大雨注意報" },
            { 12, "大雪注意報" },
            { 13, "風雪注意報" },
            { 14, "雷注意報" },
            { 15, "強風注意報" },
            { 16, "波浪注意報" },
            { 17, "融雪注意報" },
            { 18, "洪水注意報" },
            { 19, "高潮注意報" },
            { 20, "濃霧注意報" },
            { 21, "乾燥注意報" },
            { 22, "なだれ注意報" },
            { 23, "低温注意報" },
            { 24, "霜注意報" },
            { 25, "着氷注意報" },
            { 26, "着雪注意報" },
            { 27, "その他の注意報" },
            { 32, "暴風雪特別警報" },
            { 33, "大雨特別警報" },
            { 35, "暴風特別警報" },
            { 36, "大雪特別警報" },
            { 37, "波浪特別警報" },
            { 38, "高潮特別警報" }
        };


        /*
         (()=>{var d=(o,e)=>()=>(e||o((e={exports:{}}).exports,e),e.exports);var C=d(f=>{(function(o){"use strict";var e={};e.DK={arp:"pressure",slp:"normalPressure",tem:"temp",hum:"humidity",vis:"visibility",snc:"snow",tenki:"weather",snf1h:"snow1h",snf6h:"snow6h",snf12h:"snow12h",snf24h:"snow24h",sun10m:"sun10m",sun1h:"sun1h",pre10m:"precipitation10m",pre1h:"precipitation1h",pre3h:"precipitation3h",pre24h:"precipitation24h",wdr:"windDirection",wsp:"wind",mntem:"minTemp",mxtem:"maxTemp",mxgust:"gust",mxgustd:"gustDirection",station:"obsStation"},e.IK={typeCode:"type",elemsCode:"elems",latitude:"lat",longitude:"lon",altitude:"alt",kjName:"kjName",knName:"knName",enName:"enName"},e.HASH_KEYS={amdNo:"amdno",format:"format",tableElem:"elems",graphElem:"elem",interval:"interval"},e.DATA_INTERVAL_VALUES={"1h":60,"10min":10},e.AQC_INFO={0:{code:0,symbol:""},1:{code:1,symbol:")"},2:{code:2,symbol:"#"},3:{code:3,symbol:"#"},4:{code:4,symbol:"]"},5:{code:5,symbol:"休止中"},6:{code:6,symbol:"×"},7:{code:null,symbol:" "}},e.WDR_INFO={0:"静穏",1:"北北東",2:"北東",3:"東北東",4:"東",5:"東南東",6:"南東",7:"南南東",8:"南",9:"南南西",10:"南西",11:"西南西",12:"西",13:"西北西",14:"北西",15:"北北西",16:"北"},e.WEATHER_INFO={0:"晴",1:"曇",2:"煙霧",3:"霧",4:"降水またはしゅう雨性の降水",5:"霧雨",6:"着氷性の霧雨",7:"雨",8:"着氷性の雨",9:"みぞれ",10:"雪",11:"凍雨",12:"霧雪",13:"しゅう雨または止み間のある雨",14:"しゅう雪または止み間のある雪",15:"ひょう",16:"雷",30:"天気不明",31:"欠測"},e.STATIONS_INFO=[{typeCode:"A",elemsCode:"11111011",typeName:"気象台",elemsName:"気温・降水量・風向風速・日照時間・湿度・気圧",rgb:[255,0,227],size:12},{typeCode:"A",elemsCode:"11111111",typeName:"気象台",elemsName:"気温・降水量・風向風速・日照時間・積雪深・湿度・気圧",rgb:[255,30,19],size:10}];o.Amedas=o.Amedas||{},o.Amedas.Const=e})(typeof window=="object"?window:typeof global=="object"?global:f)});C();})();
         */

        public static readonly Dictionary<int, string> WindDirection2String = new()
        {
            { -1, "" },
            { 0, "静穏" },
            { 1, "北北東" },
            { 2, "北東" },
            { 3, "東北東" },
            { 4, "東" },
            { 5, "東南東" },
            { 6, "南東" },
            { 7, "南南東" },
            { 8, "南" },
            { 9, "南南西" },
            { 10, "南西" },
            { 11, "西南西" },
            { 12, "西" },
            { 13, "西北西" },
            { 14, "北西" },
            { 15, "北北西" },
            { 16, "北" }
        };

        public static readonly Dictionary<int, string> Weather2String = new()
        {
            { -1, "" },
            { 0, "晴れ" },
            { 1, "曇り" },
            { 2, "煙霧" },
            { 3, "霧" },
            { 4, "降水/しゅう雨" },
            { 5, "霧雨" },
            { 6, "着氷性の霧雨" },
            { 7, "雨" },
            { 8, "着氷性の雨" },
            { 9, "みぞれ" },
            { 10, "雪" },
            { 11, "凍雨（あられ）" },
            { 12, "霧雪" },
            { 13, "しゅう雨/止み間のある雨" },
            { 14, "しゅう雪/止み間のある雪" },
            { 15, "ひょう" },
            { 16, "雷（雷雨）" },
            { 30, "天気不明" },
            { 31, "欠測"}
        };

        /*
コード	内容	記号
0	正常値	（空）
1	推定値	)
2	不確実	#
3	異常値	#
4	補正値	]
5	休止中	休止中
6	欠測	×
7	不明/無効データ	（空白）
            */
        public static readonly Dictionary<int, string> AQC2String = new()
        {
            { -1, "" },
            { 0, "" },
            { 1, "(推)" },
            { 2, "(不)" },
            { 3, "(異)" },
            { 4, "(補)" },
            { 5, "(休)" },
            { 6, "(欠)" },
            { 7, "(不/無)" }
        };

        /// <summary>
        /// XPosterV2Hostに送信します。
        /// </summary>
        /// <param name="text">ポストするテキスト</param>
        /// <param name="path">ポストする画像</param>
        internal static void XPost(string text, string path)
        {
            if (File.Exists("XPosterV2Host - Enable"))//念のため
                try
                {
                    Console.WriteLine("[XPosterV2Host]XPosterV2Host送信開始");
                    var sendText = $"{{ \"text\" : \"{text.Replace("\n", "\\\\n")}\", \"images\" : \"{Path.GetFullPath(path).Replace("\\", "\\\\")}\" }}";
                    var message = new byte[16 * 1024];
                    message = Encoding.UTF8.GetBytes(sendText);
                    using var tcpClient = new TcpClient("127.0.0.1", 31403);
                    using var networkStream = tcpClient.GetStream();
                    networkStream.Write(message, 0, message.Length);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    Console.WriteLine("[XPosterV2Host]XPosterV2Host送信終了");
                }
        }
    }
}
