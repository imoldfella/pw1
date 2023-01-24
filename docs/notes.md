
# copied from boa constrictor

Still working out the best way to do this. The nuget package pulls in selenium which defeats the purpose of this POC.

https://github.com/microsoft/playwright/issues/10455

For anyone finding this issue, I finally figured out what I was doing wrong. If you are serving your own trace files and want to view them via https://trace.playwright.dev, you will need:

To serve your files with a Access-Control-Allow-Origin: https://trace.playwright.dev header (or *)
To allow HEAD requests, and to have the above CORS header on this response
A Content-Length header on the HEAD reponse.
The last 2 points took me a while to figure out.

