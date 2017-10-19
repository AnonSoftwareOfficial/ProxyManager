# ProxyManager


Create a new class within your C# development environment and call it "ProxyManager.cs"
after paste in the code from corisponding files.





USAGE:


call ProxyData Setup


ProxyData.proxyListSetup(url); // This grabs a proxy list


===============================================================

Create a object to hold class

Arg1: Proxy list
Arg2: Url to connec to
ProxyManager pm = new ProxyManager(ProxyData.getProxyList(), "http://.....");


=================================================================

Register a event if wanted that is called after every connection completion



public static void CompletEevent(proxyEventReturn e){}

pm.registerEvent(CompletEevent);


==================================================================

Register a event if wanted that is called once all connections are complete

public static void ProxyFinish(proxyEventFinish e){}

pm.registerEventFinish(ProxyFinish);


==================================================================

Set headers of the HTTPRequest, EG post requests

pm.setHeaders("post1=hello&post2=goobye&post3=no");


==================================================================

Finally tell the proxyManager to connection

Args1: if to cycle through all proxies in list
Args2: amount of threads used at 1 given time to cycle through all proxies (Only used if Arg1 = true)


pm.connectToSite(true, 60);

