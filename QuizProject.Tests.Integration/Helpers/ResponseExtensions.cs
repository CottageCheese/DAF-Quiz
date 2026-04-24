using System.Net.Http.Json;
using FluentAssertions;

namespace QuizProject.Tests.Integration.Helpers;

public static class ResponseExtensions
{
    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>();
        result.Should().NotBeNull("response body should deserialize to {0}", typeof(T).Name);
        return result!;
    }

    public static void ShouldBeSuccess(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            "expected success status but got {0}", (int)response.StatusCode);
    }
}
