namespace PIQI.Service.WebTesting.Rest;

using Newtonsoft.Json;
/// <summary>
/// REST clients to ease web testing of HTTP REST resources.
/// </summary>
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class RestClient
{
    private const string MediaTypeJson = "application/json";

    public RestClient(string baseAddress)
        : this(new HttpClient { BaseAddress = new Uri(baseAddress) })
    {
    }

    public RestClient(HttpClient httpClient)
    {
        // Set the default timeout for all requests to 5 minutes
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        HttpClient = httpClient;

    }

    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Get a resource or a list of resources.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpStatusCode"></param>
    /// <returns></returns>
    public async Task<string> GetAsync(string url, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        var response = await HttpClient.GetAsync(url);
        if (response.StatusCode != httpStatusCode)
        {
            throw new Exception($"Unexpected status code: {response.StatusCode}, expected {httpStatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Get a resource or a list of resources and cast it into <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the data that will be sent back</typeparam>
    /// <param name="url">Resource URL</param>
    /// <param name="httpStatusCode">Expected HTTP status code, OK by default (HTTP 200)</param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string url, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        var stringResponse = await GetAsync(url, httpStatusCode);
        var output = JsonConvert.DeserializeObject<T>(stringResponse);

        if (output == null)
            throw new Exception("Deserialized object is null");

        return output;
    }

    public Task<string> PatchAsync(string url, string body, HttpStatusCode httpStatusCode =HttpStatusCode.NoContent)
    {
        return PatchAsync(url, new StringContent(body, Encoding.UTF8, MediaTypeJson), httpStatusCode);
    }

    public async Task<string> PatchAsync(string url, HttpContent bodyContent, HttpStatusCode httpStatusCode =HttpStatusCode.NoContent)
    {
        var response = await HttpClient.PatchAsync(url, bodyContent);
        if (response.StatusCode != httpStatusCode)
        {
            throw new Exception($"Unexpected status code: {response.StatusCode}, expected {httpStatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public Task<string> PostAsync(string url, string body, HttpStatusCode httpStatusCode = HttpStatusCode.Created)
    {
        return PostAsync(url, new StringContent(body, Encoding.UTF8, MediaTypeJson), httpStatusCode);
    }

    public async Task<string> PostAsync(string url, HttpContent bodyContent, HttpStatusCode httpStatusCode =HttpStatusCode.Created)
    {
        var response = await HttpClient.PostAsync(url, bodyContent);
        if (response.StatusCode != httpStatusCode)
        {
            throw new Exception($"Unexpected status code: {response.StatusCode}, expected {httpStatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public Task<T> PostAsync<T>(string url, string body, HttpStatusCode httpStatusCode = HttpStatusCode.Created)
    {
        return PostAsync<T>(url, new StringContent(body, Encoding.UTF8, MediaTypeJson), httpStatusCode);
    }

    public async Task<T> PostAsync<T>(string url, HttpContent bodyContent, HttpStatusCode httpStatusCode =HttpStatusCode.Created)
    {
        var stringResponse = await PostAsync(url, bodyContent, httpStatusCode);

        var output = JsonConvert.DeserializeObject<T>(stringResponse);
        if (output == null)
            throw new Exception("Deserialized object is null");

        return output;
    }

    public Task<string> PutAsync(string url, string input, HttpStatusCode httpStatusCode =HttpStatusCode.NoContent)
    {
        return PutAsync(url, new StringContent(input, Encoding.UTF8, MediaTypeJson), httpStatusCode);
    }

    public async Task<string> PutAsync(string url, HttpContent bodyContent, HttpStatusCode httpStatusCode =HttpStatusCode.NoContent)
    {
        var response = await HttpClient.PutAsync(url, bodyContent);
        if (response.StatusCode != httpStatusCode)
            throw new Exception($"Unexpected status code: {response.StatusCode}, expected {httpStatusCode}");

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> DeleteAsync(string url, HttpStatusCode httpStatusCode = HttpStatusCode.NoContent)
    {
        var response = await HttpClient.DeleteAsync(url);
        if (response.StatusCode != httpStatusCode)
            throw new Exception($"Unexpected status code: {response.StatusCode}, expected {httpStatusCode}");
        
        return await response.Content.ReadAsStringAsync();
    }
}