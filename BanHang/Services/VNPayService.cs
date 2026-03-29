using BanHang.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BanHang.Services
{
    public class VNPayService
    {
        private readonly VNPayOptions _options;

        public VNPayService(IOptions<VNPayOptions> options)
        {
            _options = options.Value;
        }

        public string CreatePaymentUrl(HttpContext context, int orderId, decimal amount, string orderInfo)
        {
            var vnPayData = new SortedList<string, string>(new VnPayCompare());

            vnPayData.Add("vnp_Version", "2.1.0");
            vnPayData.Add("vnp_Command", "pay");
            vnPayData.Add("vnp_TmnCode", _options.TmnCode);
            vnPayData.Add("vnp_Amount", ((long)(amount * 100)).ToString());
            vnPayData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnPayData.Add("vnp_CurrCode", "VND");
            vnPayData.Add("vnp_IpAddr", GetIpAddress(context));
            vnPayData.Add("vnp_Locale", "vn");
            vnPayData.Add("vnp_OrderInfo", RemoveVietnameseSigns(orderInfo));
            vnPayData.Add("vnp_OrderType", "other");
            vnPayData.Add("vnp_ReturnUrl", _options.ReturnUrl);
            vnPayData.Add("vnp_TxnRef", orderId.ToString());
            vnPayData.Add("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            var queryBuilder = new StringBuilder();
            var hashBuilder = new StringBuilder();

            foreach (var item in vnPayData)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    queryBuilder.Append(WebUtility.UrlEncode(item.Key));
                    queryBuilder.Append('=');
                    queryBuilder.Append(WebUtility.UrlEncode(item.Value));
                    queryBuilder.Append('&');

                    hashBuilder.Append(WebUtility.UrlEncode(item.Key));
                    hashBuilder.Append('=');
                    hashBuilder.Append(WebUtility.UrlEncode(item.Value));
                    hashBuilder.Append('&');
                }
            }

            if (queryBuilder.Length > 0) queryBuilder.Length--;
            if (hashBuilder.Length > 0) hashBuilder.Length--;

            var secureHash = HmacSHA512(_options.HashSecret, hashBuilder.ToString());

            return $"{_options.BaseUrl}?{queryBuilder}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(IQueryCollection query, out SortedList<string, string> responseData)
        {
            responseData = new SortedList<string, string>(new VnPayCompare());

            foreach (var key in query.Keys)
            {
                if (!string.IsNullOrEmpty(key) &&
                    key.StartsWith("vnp_") &&
                    key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    responseData.Add(key, query[key].ToString());
                }
            }

            var hashBuilder = new StringBuilder();

            foreach (var item in responseData)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    hashBuilder.Append(WebUtility.UrlEncode(item.Key));
                    hashBuilder.Append('=');
                    hashBuilder.Append(WebUtility.UrlEncode(item.Value));
                    hashBuilder.Append('&');
                }
            }

            if (hashBuilder.Length > 0) hashBuilder.Length--;

            var checkHash = HmacSHA512(_options.HashSecret, hashBuilder.ToString());
            var inputHash = query["vnp_SecureHash"].ToString();

            return checkHash.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
                return "127.0.0.1";

            if (ipAddress == "::1")
                return "127.0.0.1";

            return ipAddress;
        }

        private static string RemoveVietnameseSigns(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string[] vietnameseSigns =
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return text;
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.CompareOrdinal(x, y);
        }
    }
}