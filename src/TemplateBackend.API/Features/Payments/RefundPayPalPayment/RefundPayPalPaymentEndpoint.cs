using FastEndpoints;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.RefundPayPalPayment;

/// <summary>
/// PayPal payment refund endpoint
/// </summary>
public class RefundPayPalPaymentEndpoint : Endpoint<RefundPayPalPaymentRequest, RefundPayPalPaymentResponse>
{
    private readonly IPayPalService _payPalService;
    private readonly ILogger<RefundPayPalPaymentEndpoint> _logger;

    public RefundPayPalPaymentEndpoint(
        IPayPalService payPalService,
        ILogger<RefundPayPalPaymentEndpoint> logger)
    {
        _payPalService = payPalService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/paypal/refund");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Refund a PayPal payment";
            s.Description = "Refunds a PayPal payment using the capture ID";
            s.ExampleRequest = new RefundPayPalPaymentRequest
            {
                CaptureId = "2GG903537H481924B",
                Amount = 29.99m,
                Currency = "USD",
                NoteToPayer = "Refund for cancelled order",
                Reason = "Customer requested cancellation"
            };
            //s.ExampleResponse = new RefundPayPalPaymentResponse
            //{
            //    RefundId = "5O190127TN364715V",
            //    CaptureId = "2GG903537H481924B",
            //    Status = "COMPLETED",
            //    Amount = 29.99m,
            //    Currency = "USD",
            //    RefundTime = DateTime.UtcNow,
            //    InvoiceId = "INV-12345",
            //    NoteToPayer = "Refund for cancelled order",
            //    GrossAmount = 29.99m,
            //    PayPalFee = 0.00m,
            //    NetAmount = 29.99m,
            //    TotalRefundedAmount = 29.99m,
            //    Success = true
            //};
        });
    }

    public override async Task HandleAsync(RefundPayPalPaymentRequest req, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Refunding PayPal payment for capture {CaptureId}", req.CaptureId);

            // Create PayPal refund request
            var payPalRequest = new PayPalRefundRequest
            {
                NoteToPayer = req.NoteToPayer,
                InvoiceId = req.InvoiceId,
                Reason = req.Reason
            };

            // Add amount if specified (partial refund)
            if (req.Amount.HasValue)
            {
                payPalRequest.Amount = new PayPalAmount
                {
                    CurrencyCode = req.Currency,
                    Value = req.Amount.Value.ToString("F2")
                };
            }

            // Refund PayPal payment
            var payPalResponse = await _payPalService.RefundPaymentAsync(req.CaptureId, payPalRequest);

            var response = new RefundPayPalPaymentResponse
            {
                RefundId = payPalResponse.Id,
                CaptureId = req.CaptureId,
                Status = payPalResponse.Status,
                Amount = decimal.Parse(payPalResponse.Amount.Value),
                Currency = payPalResponse.Amount.CurrencyCode,
                RefundTime = payPalResponse.CreateTime,
                InvoiceId = payPalResponse.InvoiceId,
                NoteToPayer = payPalResponse.NoteToPayer,
                GrossAmount = payPalResponse.SellerPayableBreakdown?.GrossAmount != null 
                    ? decimal.Parse(payPalResponse.SellerPayableBreakdown.GrossAmount.Value) 
                    : 0,
                PayPalFee = payPalResponse.SellerPayableBreakdown?.PayPalFee != null 
                    ? decimal.Parse(payPalResponse.SellerPayableBreakdown.PayPalFee.Value) 
                    : null,
                NetAmount = payPalResponse.SellerPayableBreakdown?.NetAmount != null 
                    ? decimal.Parse(payPalResponse.SellerPayableBreakdown.NetAmount.Value) 
                    : 0,
                TotalRefundedAmount = payPalResponse.SellerPayableBreakdown?.TotalRefundedAmount != null 
                    ? decimal.Parse(payPalResponse.SellerPayableBreakdown.TotalRefundedAmount.Value) 
                    : null,
                Success = true
            };

            _logger.LogInformation("PayPal payment refunded successfully. Refund ID: {RefundId}, Status: {Status}", 
                response.RefundId, response.Status);

            await SendAsync(response, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding PayPal payment for capture {CaptureId}", req.CaptureId);

            var errorResponse = new RefundPayPalPaymentResponse
            {
                RefundId = string.Empty,
                CaptureId = req.CaptureId,
                Status = "FAILED",
                Amount = 0,
                Currency = string.Empty,
                RefundTime = DateTime.UtcNow,
                InvoiceId = null,
                NoteToPayer = null,
                GrossAmount = 0,
                PayPalFee = null,
                NetAmount = 0,
                TotalRefundedAmount = null,
                Success = false,
                ErrorMessage = ex.Message
            };

            await SendAsync(errorResponse, cancellation: ct);
        }
    }
} 