using System;
using System.Net.Http;
using Newtonsoft.Json;
using OpenQA.Selenium.Remote;

namespace CSharpSeleniumFramework.Engine
{
    class RemoteWebdriverSessionInfo
    {
        public void WriteRemoteSessionInfo(RemoteWebDriver driver)
        {
            var sessionId = driver.SessionId;
            var httpClient = new HttpClient();
            var clusterBaseUri = TestRunSettings.SeleniumCluster.GetLeftPart(UriPartial.Authority);
            var sessionInfoUri = new Uri(clusterBaseUri + "/grid/api/testsession?session=" + sessionId);
            try
            {
                var result = httpClient.GetAsync(sessionInfoUri).GetAwaiter().GetResult();
                var content = result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<testsessionResponse>(content.Result);
                Console.WriteLine($"Test running on Remote Host {response.proxyId} with sessionId {response.session}");
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        private class testsessionResponse
        {
            public string proxyId { get; set; }
            public string session { get; set; }
        }
    }


}
