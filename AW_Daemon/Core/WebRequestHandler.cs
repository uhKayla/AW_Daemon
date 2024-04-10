using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AW_Daemon;

public class WebRequestHelper
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<ResponseClass> MakeGetRequestAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return new ResponseClass
            {
                StatusCode = (int)response.StatusCode,
                Body = responseBody
            };
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error making GET request: {e.Message}");
            return new ResponseClass
            {
                StatusCode = 500, // Or another appropriate status code
                Body = "Error occurred"
            };
        }
    }
    
    public static async Task<ResponseClass> MakePostRequestAsync(string url,  string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        // response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return new ResponseClass
        {
            StatusCode = (int)response.StatusCode,
            Body = responseBody
        };
    }

    public class ResponseClass
    {
        public int StatusCode { get; set; }
        public string? Body { get; set; }
    }
}
