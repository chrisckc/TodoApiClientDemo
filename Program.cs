using System;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

class Program
{
    static string baseUrl = "http://localhost:5000/api/";
    static string resourcePath = "todo/10000000-1111-1111-1111-111111111111/Notes";

    static void Main(string[] args)
    {

        Debug.WriteLine("baseUrl: {0}", baseUrl);
        Debug.WriteLine("resourcePath: {0}", resourcePath);
        //Test it
        TestApi().Wait();
        
    }

    public static async Task TestApi()
    {
        Tuple<dynamic,string> result = await GetObject(resourcePath);
        //Setup the serializer
        JsonSerializerSettings settings = new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };
        settings.Converters.Add(new StringEnumConverter());
        //Serialize the response so we can see it
        string jsonString = JsonConvert.SerializeObject(result.Item1,settings);
        //Print it
        Console.WriteLine("Status: {0}\r\napiResponse:\r\n{1}", result.Item2 ,jsonString);

    }

    /// <summary>
    /// For testing Api responses
    /// </summary>
    /// <param name="resourcePath"> The resource path relative to the base url.</param>
    /// <returns>A dymamic object with either a HttpClientResult property or HttpClientError property.</returns>
    public static async Task<Tuple<dynamic,string>> GetObject(string resourcePath)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = null;
                try
                {
                    
                    Debug.WriteLine(new Uri(client.BaseAddress, resourcePath).ToString());
                    //Performs a Http GET request
                    response = await client.GetAsync(resourcePath);
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic apiResponse = await response.Content.ReadAsAsync<dynamic>();
                        // Get the string to help with testing
                        string responseBody = await response.Content.ReadAsStringAsync();
                        string result = string.Format("StatusCode: {0} '{1}'\r\nResponseBody:\r\n{2}", (int)response.StatusCode, response.StatusCode, responseBody);
                        return new Tuple<dynamic,string>(apiResponse,result);
                    }
                    else
                    {
                        //Assumes that if a non success status code is encountered, the response may not always be Json
                        string responseBody = await response.Content.ReadAsStringAsync();
                        string error = string.Format("ERROR: StatusCode: {0} '{1}'\r\nResponseBody:\r\n{2}", (int)response.StatusCode, response.StatusCode, responseBody);
                        return new Tuple<dynamic,string>(null,error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception:\r\n{0}", ex);
                    string error = string.Format("Exception: {0}\r\n{1}\r\n", ex.ToString(), ex.InnerException != null ? ex.InnerException.ToString() : "");
                    return new Tuple<dynamic,string>(null,error);;
                }
            }
        }
}
