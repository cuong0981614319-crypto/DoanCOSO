using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BanHang.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BanHang.Services
{
    public class MoMoService
    {
        private readonly HttpClient _httpClient;
        private readonly MoMoOption _options;

        public MoMoService(HttpClient httpClient, IOptions<MoMoOption> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string?> CreatePaymentAsync(string orderId, long amount, string orderInfo)
        {
            var requestId = Guid.NewGuid().ToString();
            var extraData = "";
            var requestType = "captureWallet";

            var rawHash =
                $"accessKey={_options.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={_options.IpnUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={_options.PartnerCode}" +
                $"&redirectUrl={_options.ReturnUrl}" +
                $"&requestId={requestId}" +
                $"&requestType={requestType}";

            var signature = SignHmacSha256(rawHash, _options.SecretKey);

            var payload = new
            {
                partnerCode = _options.PartnerCode,
                partnerName = "Test",
                storeId = "BanHang",
                requestId = requestId,
                amount = amount.ToString(),
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = _options.ReturnUrl,
                ipnUrl = _options.IpnUrl,
                lang = "vi",
                extraData = extraData,
                requestType = requestType,
                signature = signature
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_options.CreateEndpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("payUrl", out var payUrlElement))
            {
                return payUrlElement.GetString();
            }

            return null;
        }

        public bool VerifyReturnUrlSignature(IQueryCollection query)
        {
            var rawHash =
                $"accessKey={_options.AccessKey}" +
                $"&amount={query["amount"]}" +
                $"&extraData={query["extraData"]}" +
                $"&message={query["message"]}" +
                $"&orderId={query["orderId"]}" +
                $"&orderInfo={query["orderInfo"]}" +
                $"&orderType={query["orderType"]}" +
                $"&partnerCode={query["partnerCode"]}" +
                $"&payType={query["payType"]}" +
                $"&requestId={query["requestId"]}" +
                $"&responseTime={query["responseTime"]}" +
                $"&resultCode={query["resultCode"]}" +
                $"&transId={query["transId"]}";

            var expectedSignature = SignHmacSha256(rawHash, _options.SecretKey);
            var actualSignature = query["signature"].ToString();

            return expectedSignature == actualSignature;
        }

        public bool VerifyIpnSignature(MoMoIpnRequest model)
        {
            var rawHash =
                $"accessKey={_options.AccessKey}" +
                $"&amount={model.Amount}" +
                $"&extraData={model.ExtraData}" +
                $"&message={model.Message}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInfo}" +
                $"&orderType={model.OrderType}" +
                $"&partnerCode={model.PartnerCode}" +
                $"&payType={model.PayType}" +
                $"&requestId={model.RequestId}" +
                $"&responseTime={model.ResponseTime}" +
                $"&resultCode={model.ResultCode}" +
                $"&transId={model.TransId}";

            var expectedSignature = SignHmacSha256(rawHash, _options.SecretKey);
            return expectedSignature == model.Signature;
        }

        private static string SignHmacSha256(string rawData, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}