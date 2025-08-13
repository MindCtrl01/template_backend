using FastEndpoints;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.CapturePayPalPayment;

/// <summary>
/// PayPal payment capture endpoint
/// </summary>
public class CapturePayPalPaymentEndpoint : Endpoint<CapturePayPalPaymentRequest, CapturePayPalPaymentResponse>
{
    private readonly IPayPalService _payPalService;
    private readonly ILogger<CapturePayPalPaymentEndpoint> _logger;

    public CapturePayPalPaymentEndpoint(
        IPayPalService payPalService,
        ILogger<CapturePayPalPaymentEndpoint> logger)
    {
        _payPalService = payPalService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/paypal/capture");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Capture a PayPal payment";
            s.Description = "Captures a PayPal payment for an existing order";
        });
    }

    public override async Task HandleAsync(CapturePayPalPaymentRequest req, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Capturing PayPal payment for order {OrderId}", req.OrderId);

            // Create PayPal capture request
            var payPalRequest = new PayPalCaptureRequest
            {
                NoteToPayer = req.NoteToPayer
            };

            // Add payment instruction if provided
            if (req.PaymentInstruction != null)
            {
                payPalRequest.PaymentInstruction = new PayPalPaymentInstruction
                {
                    DisbursementMode = req.PaymentInstruction.DisbursementMode,
                    PlatformFees = req.PaymentInstruction.PlatformFees?.Select(fee => new PayPalPlatformFee
                    {
                        Amount = new PayPalMoney
                        {
                            CurrencyCode = fee.Amount.CurrencyCode,
                            Value = fee.Amount.Value
                        },
                        Payee = fee.Payee != null ? new PayPalPayee
                        {
                            EmailAddress = fee.Payee.EmailAddress,
                            MerchantId = fee.Payee.MerchantId
                        } : null
                    }).ToList()
                };
            }

            // Capture PayPal payment
            var payPalResponse = await _payPalService.CapturePaymentAsync(req.OrderId, payPalRequest);

            var response = new CapturePayPalPaymentResponse
            {
                CaptureId = payPalResponse.Id,
                OrderId = req.OrderId,
                Status = payPalResponse.Status,
                Amount = decimal.Parse(payPalResponse.Amount.Value),
                Currency = payPalResponse.Amount.CurrencyCode,
                CaptureTime = payPalResponse.CreateTime,
                FinalCapture = payPalResponse.FinalCapture,
                SellerProtectionStatus = payPalResponse.SellerProtection?.Status ?? string.Empty,
                GrossAmount = payPalResponse.SellerReceivableBreakdown?.GrossAmount != null 
                    ? decimal.Parse(payPalResponse.SellerReceivableBreakdown.GrossAmount.Value) 
                    : 0,
                PayPalFee = payPalResponse.SellerReceivableBreakdown?.PayPalFee != null 
                    ? decimal.Parse(payPalResponse.SellerReceivableBreakdown.PayPalFee.Value) 
                    : null,
                NetAmount = payPalResponse.SellerReceivableBreakdown?.NetAmount != null 
                    ? decimal.Parse(payPalResponse.SellerReceivableBreakdown.NetAmount.Value) 
                    : 0,
                Success = true
            };

            _logger.LogInformation("PayPal payment captured successfully. Capture ID: {CaptureId}, Status: {Status}", 
                response.CaptureId, response.Status);

            await SendAsync(response, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing PayPal payment for order {OrderId}", req.OrderId);

            var errorResponse = new CapturePayPalPaymentResponse
            {
                CaptureId = string.Empty,
                OrderId = req.OrderId,
                Status = "FAILED",
                Amount = 0,
                Currency = string.Empty,
                CaptureTime = DateTime.UtcNow,
                FinalCapture = false,
                SellerProtectionStatus = string.Empty,
                GrossAmount = 0,
                PayPalFee = null,
                NetAmount = 0,
                Success = false,
                ErrorMessage = ex.Message
            };

            await SendAsync(errorResponse, cancellation: ct);
        }
    }
} 