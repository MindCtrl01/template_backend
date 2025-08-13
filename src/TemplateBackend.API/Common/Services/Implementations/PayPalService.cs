using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// PayPal service implementation
/// </summary>
public class PayPalService : IPayPalService
{
    private readonly HttpClient _httpClient;
    private readonly PayPalSettings _payPalSettings;
    private readonly ILogger<PayPalService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public PayPalService(
        HttpClient httpClient,
        IOptions<PayPalSettings> payPalSettings,
        ILogger<PayPalService> logger)
    {
        _httpClient = httpClient;
        _payPalSettings = payPalSettings.Value;
        _logger = logger;

        // Configure base address based on environment
        _httpClient.BaseAddress = new Uri(_payPalSettings.IsSandbox 
            ? "https://api-m.sandbox.paypal.com" 
            : "https://api-m.paypal.com");
    }

    public async Task<PayPalOrderResponse> CreateOrderAsync(PayPalOrderRequest request)
    {
        try
        {
            await EnsureValidAccessTokenAsync();

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v2/checkout/orders", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var orderResponse = JsonSerializer.Deserialize<PayPalOrderResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("PayPal order created successfully. Order ID: {OrderId}", orderResponse?.Id);
                return orderResponse!;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create PayPal order. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"PayPal API error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal order");
            throw;
        }
    }

    public async Task<PayPalCaptureResponse> CapturePaymentAsync(string orderId, PayPalCaptureRequest request)
    {
        try
        {
            await EnsureValidAccessTokenAsync();

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/v2/checkout/orders/{orderId}/capture", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var captureResponse = JsonSerializer.Deserialize<PayPalCaptureResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("PayPal payment captured successfully. Capture ID: {CaptureId}", captureResponse?.Id);
                return captureResponse!;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to capture PayPal payment. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"PayPal API error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing PayPal payment for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<PayPalOrderDetails> GetOrderDetailsAsync(string orderId)
    {
        try
        {
            await EnsureValidAccessTokenAsync();

            var response = await _httpClient.GetAsync($"/v2/checkout/orders/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var orderDetails = JsonSerializer.Deserialize<PayPalOrderDetails>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("PayPal order details retrieved successfully. Order ID: {OrderId}", orderId);
                return orderDetails!;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get PayPal order details. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"PayPal API error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PayPal order details for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<PayPalRefundResponse> RefundPaymentAsync(string captureId, PayPalRefundRequest request)
    {
        try
        {
            await EnsureValidAccessTokenAsync();

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/v2/payments/captures/{captureId}/refund", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var refundResponse = JsonSerializer.Deserialize<PayPalRefundResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("PayPal payment refunded successfully. Refund ID: {RefundId}", refundResponse?.Id);
                return refundResponse!;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to refund PayPal payment. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"PayPal API error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding PayPal payment for capture {CaptureId}", captureId);
            throw;
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        await EnsureValidAccessTokenAsync();
        return _accessToken!;
    }

    /// <summary>
    /// Ensures a valid access token is available
    /// </summary>
    private async Task EnsureValidAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry)
        {
            await RefreshAccessTokenAsync();
        }
    }

    /// <summary>
    /// Refreshes the PayPal access token
    /// </summary>
    private async Task RefreshAccessTokenAsync()
    {
        try
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_payPalSettings.ClientId}:{_payPalSettings.ClientSecret}"));
            
            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });
            
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<PayPalTokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _accessToken = tokenResponse?.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse?.ExpiresIn ?? 3600);

                // Set the authorization header for future requests
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                _logger.LogInformation("PayPal access token refreshed successfully");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to refresh PayPal access token. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"PayPal token refresh error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing PayPal access token");
            throw;
        }
    }

    /// <summary>
    /// Creates a simple PayPal order for testing
    /// </summary>
    public static PayPalOrderRequest CreateSimpleOrder(decimal amount, string currency = "USD", string description = "Test payment")
    {
        return new PayPalOrderRequest
        {
            Intent = "CAPTURE",
            PurchaseUnits = new List<PayPalPurchaseUnit>
            {
                new PayPalPurchaseUnit
                {
                    ReferenceId = Guid.NewGuid().ToString(),
                    Amount = new PayPalAmount
                    {
                        CurrencyCode = currency,
                        Value = amount.ToString("F2")
                    },
                    Description = description
                }
            },
            ApplicationContext = new PayPalApplicationContext
            {
                BrandName = "Template Backend",
                Locale = "en-US",
                LandingPage = "LOGIN",
                ShippingPreference = "NO_SHIPPING",
                UserAction = "PAY_NOW",
                ReturnUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            }
        };
    }
}

/// <summary>
/// PayPal settings configuration
/// </summary>
public class PayPalSettings
{
    /// <summary>
    /// PayPal client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// PayPal client secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use sandbox environment
    /// </summary>
    public bool IsSandbox { get; set; } = true;
}

/// <summary>
/// PayPal token response
/// </summary>
public class PayPalTokenResponse
{
    /// <summary>
    /// Access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Token type
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Expires in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// App ID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Nonce
    /// </summary>
    public string? Nonce { get; set; }
} 