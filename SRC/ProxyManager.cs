using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace proxyTest
{
    class ProxyManager
    {

        List<ProxyConEvent> events = new List<ProxyConEvent>();
        List<Action<proxyEventFinish>> finEvents = new List<Action<proxyEventFinish>>();

        List<String> proxyList;
        string prox;
        bool proxUseList;

        string url;

        List<Thread> activeThreads = new List<Thread>();
        List<Thread> threadsRan = new List<Thread>();
        List<String> successfulProx = new List<String>();
        List<String> failedProx = new List<String>();

        private int curProx = 0;


        public string postHeaders = "";


        public ProxyManager(List<String> proxList, string url)
        {
            this.proxyList = proxList;
            proxUseList = true;
            this.url = url;
        }

        public ProxyManager(string prox, string url)
        {
            this.prox = prox;
            proxUseList = false;
            this.url = url;
        }



        public void registerEvent(Action<proxyEventReturn> method)
        {
            events.Add(new ProxyConEvent(method));
        }

        public void registerEventFinish(Action<proxyEventFinish> method)
        {
            finEvents.Add(method);
        }

        public void callEvents(proxyEventReturn e)
        {
            foreach (ProxyConEvent s in events)
            {
                s.runMethod(e);
            }
        }

        public void callFinishEvent(proxyEventFinish e)
        {
            foreach(Action< proxyEventFinish> method in finEvents)
            {
                method(e);
            }
        }


        public void setHeaders(string Headers)
        {
            postHeaders = Headers;
        }

        /*
         * 
         * Proxy connect, return bool for completion
         * 
         */
        private bool proxConnect(string prox, int tCount = -1)
        {
            proxyEventReturn e = new proxyEventReturn();
            if (proxUseList)
            {
                e.totalProxies = proxyList.Count();
            }
            else
            {
                e.totalProxies = 1;
            }
            Thread tr = null;

            if (tCount != -1)
            {
                tr = activeThreads[tCount - 1];
            }
            try
            {
               
                string[] sp = prox.Split(':');
                string proxIP = sp[0];
                int proxPort = Convert.ToInt32(sp[1]);
                
                e.proxyIP = proxIP;
                e.proxyPort = proxPort;

                //Console.WriteLine(proxIP);

                WebClient cli = new WebClient();
                
                WebProxy myproxy = new WebProxy(proxIP, proxPort);
                cli.Proxy = myproxy;

                if (postHeaders != "")
                {
                    cli.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string HtmlResult = cli.UploadString(url, postHeaders);
                }
                string returnStr = cli.DownloadString(url);

                e.response = returnStr;

                successfulProx.Add(prox);
                //curProx += 1;
                if (tCount != -1)
                {
                    e.activeProxies = activeThreads.Count();
                    e.proxiesUsed = threadsRan.Count();
                    activeThreads.Remove(tr);
                    callEvents(e);
                    tr.Abort();
                }
                callEvents(e);
                
                return true; 
            }
            catch(Exception er)
            {
                //curProx += 1;
                activeThreads.Remove(tr);
                e.error = er.Message;
                failedProx.Add(prox);
                
                return false;
            }
        }
        


        /*
         * 
         * Proxy connect, return string for completion
         * 
         */


        private string proxConnectStr(string prox)
        {
            proxyEventReturn e = new proxyEventReturn();
            if (proxUseList)
            {
                e.totalProxies = proxyList.Count();
            }
            else
            {
                e.totalProxies = 1;
            }
            try
            {
                string[] sp = prox.Split(':');
                string proxIP = sp[0];
                int proxPort = Convert.ToInt32(sp[1]);

                e.proxyIP = proxIP;
                e.proxyPort = proxPort;



                WebClient cli = new WebClient();
                WebProxy myproxy = new WebProxy(proxIP, proxPort);
                cli.Proxy = myproxy;

                string returnStr = cli.DownloadString(url);

                e.response = returnStr;

                callEvents(e);
                successfulProx.Add(prox);
                return returnStr;
            }
            catch (Exception er)
            {
                e.error = er.Message;
                failedProx.Add(prox);
                return er.Message;
            }
        }
        private string proxConnectStr(string proxIP, int port)
        {
            proxyEventReturn e = new proxyEventReturn();
            if (proxUseList)
            {
                e.totalProxies = proxyList.Count();
            }
            else
            {
                e.totalProxies = 1;
            }
            try
            {
                e.proxyIP = proxIP;
                e.proxyPort = port;
                WebClient cli = new WebClient();
                WebProxy myproxy = new WebProxy(proxIP, port);
                cli.Proxy = myproxy;

                string returnStr = cli.DownloadString(url);
                e.response = returnStr;
                successfulProx.Add(prox);
                return returnStr;
            }
            catch (Exception er)
            {
                failedProx.Add(prox);
                e.error = er.Message;
                return er.Message;
            }
        }




        public void connectToSite(bool Threading, int threadLimit)
        {

            


            if (proxUseList)
            {

                if (Threading)
                {
                    while(curProx < (proxyList.Count() - 1))
                    {
                        Console.WriteLine(curProx < (proxyList.Count() - 1));
                        Console.WriteLine((proxyList.Count() - 1) - curProx);
                        Console.WriteLine(threadsRan.Count());
                        if (activeThreads.Count < threadLimit && curProx < proxyList.Count())
                        {
                            string s = proxyList.ElementAt(curProx);
                            //Console.WriteLine(s);
                            Thread t = new Thread(() => proxConnect(s, activeThreads.Count()));
                            activeThreads.Add(t);
                            threadsRan.Add(t);
                            t.Start();
                            curProx += 1;
                        }
                        
                        Thread.Sleep(100);
                    }
                }
                else
                {



                    foreach (string s in proxyList)
                    {
                        try
                        {
                            proxConnect(s);
                            //return cli.DownloadString(url);
                        }
                        catch { }

                    }
                }
            }
            else
            {
                try
                {
                    proxConnect(prox);
                    //return cli.DownloadString(url);
                }
                catch {
                }
            }

            abortThreads();
            activeThreads.Clear();
            Thread.Sleep(2000);
            proxyEventFinish pef = new proxyEventFinish();


            pef.proxiesUsed = threadsRan.Count();
            pef.proxiesSuccessfull = successfulProx.Count();
            pef.proxiesFailed = failedProx.Count();
            pef.proxFailedList = failedProx;
            pef.proxiesSuccessfullList = successfulProx;


            callFinishEvent(pef);

        }

        private void abortThreads()
        {
            if (activeThreads.Count() > 0)
            {
                try
                {
                    foreach(Thread t in activeThreads)
                    {
                        t.Abort();
                    }
                }
                catch
                {
                    abortThreads();
                }
            }

        }

    }

    

    class ProxySingle {

           
        public static string proxyConnectStr(string proxyIP, int port, string url)
        {

            try
            {
                WebClient cli = new WebClient();
                WebProxy myproxy = new WebProxy(proxyIP, port);
                cli.Proxy = myproxy;
                return cli.DownloadString(url);
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        public static string proxyConnectStr(string prox, string url)
        {

            try
            {
                string[] sp = prox.Split(':');
                string proxyIP = sp[0];
                int port = Convert.ToInt32(sp[1]);
                WebClient cli = new WebClient();
                WebProxy myproxy = new WebProxy(proxyIP, port);
                cli.Proxy = myproxy;
                return cli.DownloadString(url);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }



        public static bool proxyConnect(string proxyIP, int port, string url)
        {

            try
            {
                WebClient cli = new WebClient();
                WebProxy myproxy = new WebProxy(proxyIP, port);
                cli.Proxy = myproxy;
                cli.DownloadString(url);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool proxyConnect(string prox, string url)
        {

            try
            {
                string[] sp = prox.Split(':');
                string proxyIP = sp[0];
                int port = Convert.ToInt32(sp[1]);
                WebClient cli = new WebClient();
                WebProxy myproxy = new WebProxy(proxyIP, port);
                cli.Proxy = myproxy;
                cli.DownloadString(url);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


    }



    class ProxyData
    {
        private static List<String> proxDataList = new List<String>();
        public static List<String> getProxyList()
        {
            return proxDataList;
        }


        public static void proxyListSetup(string proxylistURL)
        {
            WebClient cli = new WebClient();

            //string proxieListStr = cli.DownloadString("http://rebro.weebly.com/uploads/2/7/3/7/27378307/rebroproxy-1000-113421072014.txt");
            string proxieListStr = cli.DownloadString(proxylistURL);

            proxDataList.Clear();
            string[] splitProx = proxieListStr.Split('\n');

            foreach (string s in splitProx)
            {
                if (s.Contains(" "))
                {
                    string st = s.Split(' ')[0];
                    proxDataList.Add(st);
                }
                else
                {
                    //Console.WriteLine(s);
                    proxDataList.Add(s);
                }
            }
        }

       


        public static string[] getProxyAt(int index)
        {
            string proxRet = proxDataList.ElementAt(index);
            return proxRet.Split(':');
        }
        public static string getProxyStrAt(int index)
        {
            return proxDataList.ElementAt(index);
        }

    }



    class ProxyConEvent
    {

        Action<proxyEventReturn> d;

        public ProxyConEvent(Action<proxyEventReturn> m)
        {
            this.d = m;
        }

        public void runMethod(proxyEventReturn e)
        {
            d(e);
        }

    }


    class proxyEventReturn {

        public string proxyIP;
        public int proxyPort;
        public string response;
        public string error;
        public int activeProxies;
        public int proxiesUsed;
        public int totalProxies;
    }

    class proxyEventFinish
    {
        public int proxiesUsed;
        public int proxiesSuccessfull;
        public int proxiesFailed;
        public List<String> proxFailedList;
        public List<String> proxiesSuccessfullList;
    }

}
