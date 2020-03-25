using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Wox.Plugin;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace Exchange
{
    public class Class1 : IPlugin
    {
        public void Init(PluginInitContext context)
        {
            // 初始化
        }

        public List<Result> Query(Query query)
        {
            if (query.Search.Length < 5) 
            {
                // USD 1     所以长度至少是5
                return new List<Result>();
            }

            string[] keys = query.Search.Split(' ');
            if (keys.Length == 2 && IsNumeric(keys[1]))
            {
                if (keys[0].Length == 3)
                {
                    return Get(keys[1], keys[0].ToUpper(), "CNY,USD,AUD,CAD,EUR,GBP,HKD,JPY,RUB");
                }
            }

            if (keys.Length == 3)
            {
                if (keys[0].Length == 3 && keys[1].Length == 3 && IsNumeric(keys[2]))
                {
                    return Get(keys[2], keys[0].ToUpper(), keys[1].ToUpper());
                }
            }

            return new List<Result>();
        }

        // 处理内容
        public List<Result> Get(string number, string sourceCode, string targetCodes)
        {
            List<Result> results = new List<Result>();
            JObject jObject = Request(number, sourceCode, targetCodes);

            if (jObject.Value<String>("code").Equals("0"))
            {
                JArray array = jObject.Value<JArray>("data");
                foreach (JObject obj in array)
                {
                    if (obj.Value<Int32>("success") == 1)
                    {
                        string targetValue = obj.Value<String>("targetValue");
                        if (targetValue.Contains(".")) 
                        {
                            double value = Double.Parse(targetValue);
                            targetValue = value.ToString("0.00");
                        }
                        results.Add(new Result()
                        {
                            Title = "   " + number + " " + sourceCode + " = " + targetValue + " " + obj.Value<String>("target"),
                            SubTitle = "    回车复制结果",
                            IcoPath = "img/exchange.png",  //相对于插件目录的相对路径
                            Action = e =>
                            {
                                // 处理用户选择之后的操作
                                Clipboard.SetText(targetValue);
                                //返回false告诉Wox不要隐藏查询窗体，返回true则会自动隐藏Wox查询窗口
                                return true;
                            }
                        });
                    }
                }
            }
            return results;
        }

        // 网络请求
        public JObject Request(string number,string sourceCode,string targetCodes) 
        {
            var request = (HttpWebRequest)WebRequest.Create("https://www.erning.cn:10205/base/exchange?money=" + number + "&sourceCode=" + sourceCode + "&targetCodes="+targetCodes);
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return Str2Json(responseString);
        }

        // 是不是数字
        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }

        // 字符串转json
        public static JObject Str2Json(string jsonStr)
        {
            return JObject.Parse(jsonStr);
        }
    }
}
