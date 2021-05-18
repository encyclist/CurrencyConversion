using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Wox.Plugin;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using Wox.Infrastructure.Storage;

namespace Exchange
{
    public class Class1 : IPlugin,ISettingProvider
    {

        public void Init(PluginInitContext context)
        {
            // 初始化
        }

        public List<Result> Query(Query query)
        {
            // 至少输入了两段内容
            if (query.Terms.Length < 2)
            {
                return null;
            }

            string amount = ""; // 金额
            string baseCode = null; // 源币种代码
            List<string> targetCode = new List<string>();

            int numberCount = 0; // 数字数量
            for (int i = 0; i < query.Terms.Length; i++)
            {
                string term = query.Terms[i];
                if (IsNumeric(term)) // 是金额
                {
                    if (amount.Length == 0)
                    {
                        amount = term;
                    }
                    numberCount++;
                }
                else // 是币种代码
                {
                    if (term.Length != 3) // 根据观察，所有币种的代码都是3位
                    {
                        return null;
                    }
                    if (baseCode == null) // 第一个是源币种代码
                    {
                        baseCode = term;
                    }
                    else 
                    {
                        targetCode.Add(term.ToUpper());
                    }
                }
            }
            // 必须只有一个是数字
            if (numberCount != 1)
            {
                return new List<Result>();
            }

            if (targetCode.Count < 1) // 目标币种代码是空的，添加默认数据
            {
                targetCode.Add("CNY");
                targetCode.Add("USD");
                targetCode.Add("AUD");
                targetCode.Add("CAD");
                targetCode.Add("EUR");
                targetCode.Add("GBP");
                targetCode.Add("HKD");
                targetCode.Add("JPY");
                targetCode.Add("RUB");
            }

            return Get(amount, baseCode.ToUpper(), targetCode);
        }

        // 是不是数字
        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }

        // 处理内容
        public List<Result> Get(string number, string baseCode, List<string> targetCodes)
        {
            List<Result> results = new List<Result>();

            JObject jObject = Request(baseCode);
            if (!jObject.Value<String>("result").Equals("success")) 
            {
                results.Add(new Result()
                {
                    Title = "   " + jObject.Value<String>("error-type"),
                    SubTitle = "    See detail in provider",
                    IcoPath = "img/exchange_white.png",  //相对于插件目录的相对路径
                    Action = e =>
                    {
                        // 处理用户选择之后的操作
                        Process.Start("https://www.exchangerate-api.com/docs/standard-requests");
                        //返回false告诉Wox不要隐藏查询窗体，返回true则会自动隐藏Wox查询窗口
                        return true;
                    }
                });
                return results;
            }

            JObject array = jObject.Value<JObject>("conversion_rates");
            foreach (string target in targetCodes)
            {
                double exchange = Double.Parse(array.Value<string>(target));
                double result = Double.Parse(number) * exchange;
                results.Add(new Result()
                {
                    Title = "   " + number + " " + baseCode + " = " + result + " " + target,
                    SubTitle = "    Press enter to copy the result",
                    IcoPath = "img/exchange_white.png",  //相对于插件目录的相对路径
                    Action = e =>
                    {
                        // 处理用户选择之后的操作
                        Clipboard.SetText(""+result);
                        //返回false告诉Wox不要隐藏查询窗体，返回true则会自动隐藏Wox查询窗口
                        return true;
                    }
                });
            }
            return results;
        }

        // 网络请求
        public JObject Request(string baseCode)
        {

            PluginJsonStorage<Settings> storage = new PluginJsonStorage<Settings>();
            Settings settings = storage.Load();
            var request = (HttpWebRequest)WebRequest.Create("https://v6.exchangerate-api.com/v6/" + settings.apiKey + "/latest/" + baseCode);
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return Str2Json(responseString);
        }

        // 字符串转json
        public static JObject Str2Json(string jsonStr)
        {
            return JObject.Parse(jsonStr);
        }

        public System.Windows.Controls.Control CreateSettingPanel()
        {
            return new SettingsControl();
        }
    }
}
