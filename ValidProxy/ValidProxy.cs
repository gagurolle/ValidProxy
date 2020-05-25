using MihaZupan;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using Telegram.Bot;
using System.Text.RegularExpressions;


namespace ProxySocks
{
  
    public class ValidProxy
    {

        //static string adminchatid = "443644346";
        //static string bot_key = "1220777579:AAGbCYHHH4RlwUe0QZZWnNFH5zW6tXmVhZU";
        static string adminchatid { get; set; }
        static string bot_key { get; set; }
        static IWebDriver driver;
        static string[][] proxy_array;

        /// <summary>
        /// Метод, который возвращает вам socks5 с самым лучшим uptime без проверки его работоспособности в телеграмм.
        /// </summary>
        /// <returns></returns>
        static public string GetProxy()
        {
            GetWebSiteElements();
            CutEmptyElems(ref proxy_array);
            SortArrayDesc(ref proxy_array);
            return proxy_array[0][0];
        }
        /// <summary>
        /// метод, который возвращает вам socks5 с самым лучшим uptime с проверкой в телеграмм.
        /// </summary>
        /// <param name="botKey">Токен бота</param>
        /// <returns></returns>
        static public string GetProxy(string botKey)
        {
            bot_key = botKey;
            GetWebSiteElements();
            CutEmptyElems(ref proxy_array);
            SortArrayDesc(ref proxy_array);
            int j = 0;
            while (j++ != proxy_array.Length)
            {
                if (CheckProxy(proxy_array[j][0]))
                {
                    return proxy_array[j][0];
                }
            }
            return null;
        }
        /// <summary>
        /// метод, который возвращает вам socks5 с самым лучшим uptime с проверкой в телеграмм. При указаниии chatId, во время проверки, на данный chatId придет оповещание о том, какой прокси валидный.
        /// </summary>
        /// <param name="botKey">Токен бота</param>
        /// <param name="chatId">Ваш чат Id</param>
        /// <returns></returns>
        static public string GetProxy(string botKey, string chatId)
        {
            bot_key = botKey;
            adminchatid = chatId;
            GetWebSiteElements();
            CutEmptyElems(ref proxy_array);
            SortArrayDesc(ref proxy_array);
            int j = 0;
            while (j++ != proxy_array.Length-1)
            {
                if (CheckProxy(proxy_array[j][0]))
                {
                    return proxy_array[j][0];
                }
            }
            return null;
        }
        static string[][] GetWebSiteElements()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArguments("--no-sandbox");
            driver = new ChromeDriver(chromeOptions);
            driver.Url = @"http://spys.one/socks/";
            System.Threading.Thread.Sleep(3000);
            IList<string> proxyList = new List<string>();
            IWebElement tableElement = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[4]/td/table/tbody"));
            IList<IWebElement> tableRow = tableElement.FindElements(By.TagName("tr"));
            IList<IWebElement> rowTD;
            proxy_array = new string[tableRow.Count][];
            object[] proxy_list = null;
            int i = 0;
            foreach (IWebElement row in tableRow)
            {
                rowTD = row.FindElements(By.TagName("td"));
                int value;
                if (rowTD.Count > 5 && rowTD[0].Text != "IP адрес:порт")
                {
                    proxy_array[i++] = new string[2] { rowTD[0].Text, ParseUpTime(rowTD[8].Text) };
                }
            }
            driver.Close();

            //CutEmptyElems(ref proxy_array);
            return proxy_array;
        }
        void ReloadPage()
        {
            driver.Url = @"http://spys.one/socks/";
            System.Threading.Thread.Sleep(300);
        }
        /// <summary>
        /// Проверяет прокси на валидность в телеграмм.
        /// </summary>
        /// <param name="proxy">Указать прокси, к примеру: "5.133.202.167:19619"</param>
        /// <param name="_bot_key">Обязательный параметр. Передать токен вашего бота</param>
        /// <param name="_chatId">Необязательный параметр. Укажите ваш чат айди, если хотите получить оповещание в телеграм о том, какой прокси валидный</param>
        /// <returns></returns>
        static public bool CheckProxy(string proxy, string _bot_key, string _chatId = null)
        {
            adminchatid = _chatId;
            bot_key = _bot_key;
            return CheckProxy(proxy);
        }
        /// <summary>
        /// Проверяет введенный прокси на валидность.
        /// </summary>
        /// <param name="proxy">Указать прокси, к примеру: "5.133.202.167:19619"</param>
        /// <returns></returns>
        static bool CheckProxy(string proxy)//проверяет единичный прокси
        {

            var proxyList = ParseProxy(proxy);
            if (proxyList != null)
            {
                try
                {
                    if (bot_key == null)
                    {
                        throw new Exception("Не заполнен обязательный параметр: bot_key. Пожалуйста, во время вызова данной функции, передайте в этот параметр ваш токен бота");
                    }
                    HttpToSocks5Proxy proxyf = new HttpToSocks5Proxy(proxyList[0], Int32.Parse(proxyList[1]));
                    Telegram.Bot.ITelegramBotClient botClient1 = new TelegramBotClient(bot_key, proxyf) { Timeout = TimeSpan.FromSeconds(3) };
                    while (true)
                    {
                        var me = botClient1.GetMeAsync();
                        var r = me.Result;
                        var s = me.Status;

                        Console.Write(s.ToString());
                        if (r != null)
                        {
                            // Console.Clear();
                            if (adminchatid != null)
                            {
                                botClient1.SendTextMessageAsync(chatId: adminchatid.ToString(), text: "Valid Proxy - " + proxyList[0] + ":" + Int32.Parse(proxyList[1]));
                            }
                            return true;
                        }
                        else
                        {
                            if (s.ToString() == "WaitingForActivation")
                            {

                                System.Threading.Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                }

                catch (System.ArgumentOutOfRangeException e)
                {
                    return false;
                }
                catch (System.AggregateException e)
                {
                    if (e.InnerException.Message == "Request timed out")
                    {
                        //Console.Clear();
                        //Console.WriteLine($"прокси {proxyList[2]} не подошел, Request timed out");
                        System.Threading.Thread.Sleep(1000);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        /// <summary>
        /// Получаете список прокси, полученных с сайта spys.one в количестве 25 штук без проверки на доступ к телеграм и отсортированных в порядке убывания по UpTime
        /// </summary>
        /// <returns></returns>
        static public List<string> GetProxyList()
        {
            List<string> proxy = new List<string>();
            GetWebSiteElements();
            CutEmptyElems(ref proxy_array);
            SortArrayDesc(ref proxy_array);
            int j = 0;
            while (j++ != proxy_array.Length - 1)
            {
                proxy.Add(proxy_array[j][0]);
            }
            return proxy;
        }
        /// <summary>
        /// Получаете список прокси, полученных с сайта spys.one в количестве 25 штук с проверкой доступа к телеграм и отсортированных в порядке убывания по UpTime
        /// </summary>
        /// <param name="botKey">Передать токен вашего бота</param>
        /// <returns></returns>
        static public List<string> GetProxyList(string botKey)
        {
            List<string> proxy = new List<string>();
            bot_key = botKey;
            GetWebSiteElements();
            CutEmptyElems(ref proxy_array);
            SortArrayDesc(ref proxy_array);
            int j = 0;
            while (j++ != proxy_array.Length)
            {
                if (CheckProxy(proxy_array[j][0]))
                {
                    proxy.Add(proxy_array[j][0]);
                }
            }
            return proxy;
        }
        /// <summary>
        /// Получаете список прокси, полученных с сайта spys.one в количестве 25 штук с проверкой доступа к телеграм и отсортированных в порядке убывания по UpTime.
        /// Передайте параметр ChatId, если хотите чтобы вам в телеграм пришли все проверенные прокси из этого списка
        /// </summary>
        /// <param name="botKey">Передать токен вашего бота</param>
        /// <param name="chatId">Передать ChatId</param>
        /// <returns></returns>
        static public List<string> GetProxyList(string botKey, string chatId)
        {
            List<string> proxy = new List<string>();
            bot_key = botKey;
            adminchatid = chatId;
            GetWebSiteElements();
            CutEmptyElems(ref proxy_array);
            SortArrayDesc(ref proxy_array);
            int j = 0;
            while (j++ != proxy_array.Length)
            {
                if (CheckProxy(proxy_array[j][0]))
                {
                    proxy.Add(proxy_array[j][0]);
                }
            }
            return proxy;
        }
        /// <summary>
        /// проверяет ваш прокси список на валидность в телеграм
        /// </summary>
        /// <param name="proxy_List">Ваш прокси список</param>
        /// <param name="botKey">Обязательный параметр. Передать токен вашего бота</param>
        /// <param name="chatId">Передать ChatId вашего бота, если хотите получить уведомление в телеграм о том, какие прокси валидные</param>
        /// <returns></returns>
        static public bool CheckProxyList(ref List<string> proxy_List, string botKey, string chatId = null)//проверяет Proxy лист
        {
            byte i = 0;
            List<string> temp_List = new List<string>();
            if (proxy_List.Count != 0)
            {
                int j = 0;
                foreach (string pr in proxy_List)
                {
                    j++;
                    if (CheckProxy(pr, botKey, chatId))
                    {
                        temp_List.Add(pr);
                        i++;
                    }
                }
                if (i != 0)
                {
                    proxy_List = new List<string>(temp_List);
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        static List<string> ParseProxy(string proxy)
        {
            List<string> proxyList = null;
            try
            {
                proxyList = new List<string>();
                string[] words = proxy.Split(':');
                proxyList.Add(words[0].ToString());
                proxyList.Add(Int32.Parse(words[1]).ToString());
                proxyList.Add(proxy);
            }
            catch (Exception e)
            {
                return null;
            }
            return proxyList;
        }
        static string ParseUpTime(string uptime)
        {
            string pattern = ".*%";
            string result = Regex.Match(uptime, pattern).Value;
            return result == "" ? "0" : (result.Replace("%", ""));
        }
        static string[][] SortArrayDesc(ref string[][] s_array)
        {
            string[] dub;
            for (int i = 0; i < s_array.Length - 1; i++)
            {
                for (int j = 0; j < s_array.Length - 1; j++)
                {
                    if (Int32.Parse(s_array[i][1]) > Int32.Parse(s_array[j][1]))
                    {
                        dub = s_array[i];
                        s_array[i] = s_array[j];
                        s_array[j] = dub;
                    }
                }
            }
            return s_array;
        }
        static void CutEmptyElems(ref string[][] s_array)
        {
            int count = 0;
            foreach (string[] f in s_array)
            {
                if (f == null)
                {
                    count++;
                }
            }
            string[][] new_array = new string[s_array.Length - count][];
            for (int i = 0; i < new_array.Length; i++)
            {
                new_array[i] = s_array[i];
            }
            s_array = new_array;
        }
    }
}


