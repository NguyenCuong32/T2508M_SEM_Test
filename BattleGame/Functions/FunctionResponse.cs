using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using BattleGame.DTOs;
using BattleGame.Services;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions;

internal static class FunctionResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectRequiredConstructorParameters = true
    };

    public static async Task<T> ReadAsync<T>(HttpRequestData request)
    {
        return await JsonSerializer.DeserializeAsync<T>(request.Body, JsonOptions)
            ?? throw new ValidationException("Request body is required.");
    }

    public static async Task<HttpResponseData> ExecuteAsync<T>(HttpRequestData request,
        Func<Task<T>> action, HttpStatusCode successStatus, string message, ILogger logger)
    {
        try
        {
            var data = await action();
            return await WriteAsync(request, successStatus, new ApiResponseDto<T>(true, message, data));
        }
        catch (JsonException)
        {
            return await ErrorAsync(request, HttpStatusCode.BadRequest, "Request body must be valid JSON with all required fields and correct types.");
        }
        catch (ValidationException exception)
        {
            return await ErrorAsync(request, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (DuplicateEmailException exception)
        {
            return await ErrorAsync(request, HttpStatusCode.Conflict, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request failed for {Path}", request.Url.AbsolutePath);
            return await ErrorAsync(request, HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    private static Task<HttpResponseData> ErrorAsync(HttpRequestData request, HttpStatusCode status, string message) =>
        WriteAsync(request, status, new ApiResponseDto<object>(false, message, null));

    private static async Task<HttpResponseData> WriteAsync<T>(HttpRequestData request, HttpStatusCode status, T body)
    {
        var response = request.CreateResponse(status);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(body, JsonOptions));
        return response;
    }
}
