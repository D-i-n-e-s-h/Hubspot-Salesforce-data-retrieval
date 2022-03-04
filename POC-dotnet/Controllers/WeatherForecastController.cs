using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POC_dotnet.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using Salesforce.Force;
using Newtonsoft.Json.Linq;

namespace POC_dotnet.Controllers
{
    [ApiController]
    [Route("login")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private static readonly string apiKey = "0b88cf20-84c6-4b24-aea6-e5c8f1950245";
        private static readonly string hubSpotAccessToken = "pat-na1-0efc2e1d-3d23-44cf-ad8d-689a775fc3a8";
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserService _userService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("Users")]
        public IEnumerable<User> GetUsers()
        {
            return _userService.Get();
        }

        [HttpPost]
        [Route("authenticate")]
        public bool Authenticate([FromBody] User userDetails)
        {
            return _userService.Authenticate(userDetails);
        }

        [HttpGet]
        [Route("Hubspot/Contact")]
        // get Contact HubSpot Api from dotnet
        public IActionResult GetHubSpotContact()
        {
            string url = "https://api.hubapi.com/contacts/v1/lists/all/contacts/all?hapikey=" + apiKey;
            return this.GetDetailsByApiKey(url);            
        }

        [HttpGet]
        [Route("Hubspot/Company")]
        // get HubSpot Api from dotnet
        public IActionResult GetHubSpotCompanies()
        {
            string url = "https://api.hubapi.com/companies/v2/companies/paged?hapikey=" + apiKey;
            return this.GetDetailsByApiKey(url);

            /*var api = new HubSpotApi(apiKey);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var properties = api.CompanyProperties.GetAll();
            return this.Ok(properties);*/
        }

        [HttpGet]
        [Route("Hubspot/Deals")]
        // get Contact HubSpot Api from dotnet
        public IActionResult GetHubSpotDeals()
        {
            string url = "https://api.hubapi.com/deals/v1/deal/paged";
            return this.GetDetailsByOAuth(url, hubSpotAccessToken, "Bearer ");
        }

        [HttpGet]
        [Route("Salesforce/Accounts")]
        // get Accounts from Salesforce Api
        public async Task<IActionResult> GetSalesforceAccounts()
        {
            return await this.GetSalesforceDetails(SalesForceCredentials.TOKEN_REQUEST_ACCOUNT_QUERYURL);
        }

        [HttpGet]
        [Route("Salesforce/Leads")]
        // get Accounts from Salesforce Api
        public async Task<IActionResult> GetSalesforceLeads()
        {
            return await this.GetSalesforceDetails(SalesForceCredentials.TOKEN_REQUEST_LEADS_QUERYURL);
        }

        [HttpGet]
        [Route("Salesforce/Contacts")]
        // get Accounts from Salesforce Api
        public async Task<IActionResult> GetSalesforceContacts()
        {
            return await this.GetSalesforceDetails(SalesForceCredentials.TOKEN_REQUEST_CONTACTS_QUERYURL);
        }

        [HttpGet]
        [Route("Salesforce/Campaigns")]
        // get Accounts from Salesforce Api
        public async Task<IActionResult> GetSalesforceCampaigns()
        {
            return await this.GetSalesforceDetails(SalesForceCredentials.TOKEN_REQUEST_CAMPAIGNS_QUERYURL);
        }

        [HttpGet]
        [Route("Salesforce/Accounts/ForceClient")]
        // get Accounts from Salesforce Api with ForceClient nuget
        public async Task<IActionResult> GetSalesforceAccountsForceClient()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://login.salesforce.com/services/oauth2/token");
            request.Method = "POST";
            string postData = "grant_type=password&client_id=3MVG9pRzvMkjMb6kFNNHKhlqtwfqC_8dgyBd8uBMutc7OKyKHEsaUfh_8nNTaTpVzf_w9uZa.FfQoZHVXnbQt&client_secret=92E73F2784BAC551227D89E67D532E1363D94A218DB9A5A2F3957061EFD623C4&username=dreamdinesh123-0tk12@force.com&password=Duc3n@123MPNjrcHM7ndOdlmc783UpT1wR";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = bytes.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);

            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var response = JsonConvert.DeserializeObject<SalesForceValidationResponse>(result);

                    var client = new ForceClient(response.instance_url, response.access_token, "v43.0");
                    var accounts = await client.QueryAsync<SalesForceAccount>("SELECT id, name, description FROM Account");
                    return this.Ok(accounts);
                }
            }
            catch (Exception ex)
            {
                return this.Ok(ex.Message);
            }
        }

        [HttpGet]
        [Route("Salesforce/multiple")]
        // get Contact HubSpot Api from dotnet
        public async Task<string> GetSalesforceMultipleRequestAccounts()
        {
            var a = "";
            for (int i = 0; i < 5; i++)
            {
                var timer = new Stopwatch();
                timer.Start();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://login.salesforce.com/services/oauth2/token");
                request.Method = "POST";
                string postData = "grant_type=password&client_id=3MVG9pRzvMkjMb6kFNNHKhlqtwfqC_8dgyBd8uBMutc7OKyKHEsaUfh_8nNTaTpVzf_w9uZa.FfQoZHVXnbQt&client_secret=92E73F2784BAC551227D89E67D532E1363D94A218DB9A5A2F3957061EFD623C4&username=dreamdinesh123-0tk12@force.com&password=Duc3n@123MPNjrcHM7ndOdlmc783UpT1wR";
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(postData);

                request.ContentType = "application/x-www-form-urlencoded";

                request.ContentLength = bytes.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                try
                {
                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var response = JsonConvert.DeserializeObject<SalesForceValidationResponse>(result);
                        timer.Stop();
                        Console.WriteLine("Time taken for access token: " + timer.Elapsed.ToString(@"m\:ss\.fff"));
                        var timer1 = new Stopwatch();
                        timer1.Start();
                        /*string url = response.instance_url + "/services/data/v42.0/query/?q=SELECT+name+from+Account";
                        return this.GetDetailsByOAuth(url, response.access_token, response.token_type);*/
                        // return this.Ok(await this.GetDetailsByHttpClient(response.access_token, response.instance_url + Constants.TOKEN_REQUEST_ACCOUNT_QUERYURL));
                        a = string.Concat(a, await this.GetDetailsByHttpClient(response.access_token, response.instance_url + SalesForceCredentials.TOKEN_REQUEST_ACCOUNT_QUERYURL));
                        timer1.Stop();
                        Console.WriteLine("Time taken for data: " + timer1.Elapsed.ToString(@"m\:ss\.fff"));
                    }
                }
                catch
                {
                    a = string.Concat(a, i);
                    // return this.Ok(ex.Message);
                }
                finally
                {
                    newStream.Close();
                }
            }
            return a;
        }

        private IActionResult GetDetailsByApiKey(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return this.Ok(result);
            }
        }

        private IActionResult GetDetailsByOAuth(string url, string accessToken, string tokenType)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", tokenType + accessToken);
            // httpWebRequest.Accept = "application/json";

            try {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return this.Ok(result);
                }
            }
            catch (Exception ex)
            {
                return this.Ok(ex.Message);
            }
        }

        private async Task<string> GetDetailsByHttpClient(string token, string url)
        {
            HttpClient _httpClient = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = null
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await new HttpClient().SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return response;
        }

        private async Task<IActionResult> GetSalesforceDetails(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://login.salesforce.com/services/oauth2/token");
            request.Method = "POST";
            string postData = "grant_type=password&client_id=3MVG9pRzvMkjMb6kFNNHKhlqtwfqC_8dgyBd8uBMutc7OKyKHEsaUfh_8nNTaTpVzf_w9uZa.FfQoZHVXnbQt&client_secret=92E73F2784BAC551227D89E67D532E1363D94A218DB9A5A2F3957061EFD623C4&username=dreamdinesh123-0tk12@force.com&password=Duc3n@123MPNjrcHM7ndOdlmc783UpT1wR";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = bytes.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);

            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var response = JsonConvert.DeserializeObject<SalesForceValidationResponse>(result);
                    /*string url = response.instance_url + "/services/data/v42.0/query/?q=SELECT+name+from+Account";
                    return this.GetDetailsByOAuth(url, response.access_token, response.token_type);*/
                    return this.Ok(await this.GetDetailsByHttpClient(response.access_token, response.instance_url + url));
                }
            }
            catch (Exception ex)
            {
                return this.Ok(ex.Message);
            }
        }
    }
}
