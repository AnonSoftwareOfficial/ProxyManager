using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace proxyTest
{
    class Program
    {


        public static void print(string printMessage)
        {
            Console.WriteLine(printMessage);
        }


        public static void eventYeh(proxyEventReturn e)
        {

            string resp = "Failed";
            if(e.response != "")
            {
                resp = "Voted";
            }

            Console.Clear();
            print("======================");
            print("CompProxy IP: " +e.proxyIP );
            print("CompProxy Port: " + e.proxyPort);
            print("CompResp: " + resp);
            print("Active: " + e.activeProxies);
            print("Complete: " + e.proxiesUsed);
            print("");
            print("Total: " + e.totalProxies);
            print("Left: " + (e.totalProxies - e.proxiesUsed));
            print("======================");
            
        }

        public static void ProxyFinish(proxyEventFinish e)
        {
            Console.Clear();
            print("======================");
            print("Used: " + e.proxiesUsed);
            print("Successfull: " + e.proxiesSuccessfull);
            print("Failed: " + e.proxiesFailed);
            print("======================");
        }


        static void Main(string[] args)
        {



            ProxyData.proxyListSetup();

            print("Number of passes:");
            int passes = Convert.ToInt16(Console.ReadLine());
            
            

            for (int i = 0; i < passes; i++)
            {
                print("Pass number: " + i);
                List<String> prox = ProxyData.getProxyList();
                ProxyManager pm = new ProxyManager(prox, "");

                pm.registerEvent(eventYeh);
                pm.registerEventFinish(ProxyFinish);
                pm.setHeaders("options=126934570");
                pm.connectToSite(true, 60);
            }
            

            Console.ReadLine();
        }



    }
}
