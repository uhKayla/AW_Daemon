using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AW_Daemon.Utilities;

namespace AW_Daemon;

public class WebRequestHelper
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<ResponseClass> MakeGetRequestAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
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
        // HMAC
        var secret = Environment.GetEnvironmentVariable("HMAC");
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new Exception("HMAC Environment Variable is blank or missing! Please add your key!");
        }
        var signature = HMACHelper.ComputeHMACSHA256Signature(body, secret);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-Signature", signature);
        
        Console.WriteLine($"Message {body}");
        Console.WriteLine($"Key {secret}");
        Console.WriteLine($"Signature {signature}");
        
        // Request
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
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
