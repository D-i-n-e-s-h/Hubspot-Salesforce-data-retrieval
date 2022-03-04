namespace POC_dotnet
{
    public class SalesForceValidationResponse
    {
        public string access_token { get; set; }

        public string instance_url { get; set; }

        public string id { get; set; }

        public string token_type { get; set; }

        public string issued_at { get; set; }

        public string signature { get; set; }

    }

    public class SalesForceCredentials
    {
        public static string USERNAME = "dreamdinesh123-0tk12@force.com";
        public static string PASSWORD = "Duc3n@123";
        public static string TOKEN = "MPNjrcHM7ndOdlmc783UpT1wR";
        public static string CONSUMER_KEY = "3MVG9pRzvMkjMb6kFNNHKhlqtwfqC_8dgyBd8uBMutc7OKyKHEsaUfh_8nNTaTpVzf_w9uZa.FfQoZHVXnbQt";
        public static string CONSUMER_SECRET = "92E73F2784BAC551227D89E67D532E1363D94A218DB9A5A2F3957061EFD623C4";

        public static string TOKEN_REQUEST_ENDPOINTURL = "https://login.salesforce.com/services/oauth2/token";
        public static string TOKEN_REQUEST_ACCOUNT_QUERYURL = "/services/data/v43.0/query?q=select+Id+,name+from+account";
        public static string TOKEN_REQUEST_LEADS_QUERYURL = "/services/data/v43.0/query?q=select+Id+,name+from+lead";
        public static string TOKEN_REQUEST_TASKS_QUERYURL = "/services/data/v53.0/query?q=select+fields(custom)+from+task+limit+25";
        public static string TOKEN_REQUEST_CONTACTS_QUERYURL = "/services/data/v53.0/query?q=select+Id+,name+from+contact";
        public static string TOKEN_REQUEST_CAMPAIGNS_QUERYURL = "/services/data/v53.0/query?q=select+Id+,name+from+campaign";
        public static string TOKEN_REQUEST_QUERYURL = "/services/data/v42.0/sobjects/Account";
    }
    public class SalesForceAccount
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
