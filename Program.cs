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
        dynamic apiResponse = await GetObject(resourcePath);
        //Setup the serializer
        JsonSerializerSettings settings = new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };
        settings.Converters.Add(new StringEnumConverter());
        //Serialize the response so we can see it
        string jsonString = JsonConvert.SerializeObject(apiResponse,settings);
        //Print it
        Console.WriteLine("apiResponse:\r\n{0}", jsonString);

    }

    /// <summary>
    /// For testing Api responses
    /// </summary>
    /// <param name="resourcePath"> The resource path relative to the base url.</param>
    /// <returns>A dymamic object with either a HttpClientResult property or HttpClientError property.</returns>
    public static async Task<dynamic> GetObject(string resourcePath)
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
                        if (apiResponse == null) apiResponse = new ExpandoObject();
                        apiResponse.HttpClientResult = string.Format("StatusCode: {0} '{1}'", (int)response.StatusCode, response.StatusCode);
                        return apiResponse;
                    }
                    else
                    {
                        //Assumes that if a non success status code is encountered, the response may not always be Json
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic apiResponse = new ExpandoObject();
                        apiResponse.HttpClientError = string.Format("ERROR: StatusCode: {0} '{1}' ResponseBody: {2}", (int)response.StatusCode, response.StatusCode, responseBody);
                        return apiResponse;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception:\r\n{0}", ex);
                    dynamic apiResponse = new ExpandoObject();
                    apiResponse.HttpClientError = string.Format("Exception: {0}\r\n{1}\r\n", ex.ToString(), ex.InnerException != null ? ex.InnerException.ToString() : "");
                    return apiResponse;
                }
            }
        }
}
