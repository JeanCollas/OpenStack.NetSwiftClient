using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace NetSwiftClient
{
    public static class StringExts
    {
        public static string UrlEncoded(this string str) => WebUtility.UrlEncode(str);

        internal static bool IsNullOrEmpty(this string s) => String.IsNullOrEmpty(s);

        internal static string IfNullOrEmpty(this string src, string substitute)
            => src.IsNullOrEmpty() ? substitute : src;

        public static T SafeConvert<T>(this string s, T defaultValue)
        {
            if (String.IsNullOrEmpty(s))
                return defaultValue;
            if (!(typeof(T)).IsValueType)
                return (T)Convert.ChangeType(s, typeof(T));
            if (Nullable.GetUnderlyingType(typeof(T)) != null)// Nullable<T>
                return (T)Convert.ChangeType(s, Nullable.GetUnderlyingType(typeof(T)));
            return (T)Convert.ChangeType(s, typeof(T));
        }

        public static object SafeConvert(this string s, Type t, object defaultValue)
        {
            if (String.IsNullOrEmpty(s))
                return defaultValue;
            if (!t.IsValueType)
                return Convert.ChangeType(s, t);
            if (Nullable.GetUnderlyingType(t) != null)// Nullable<T>
                return Convert.ChangeType(s, Nullable.GetUnderlyingType(t));
            return Convert.ChangeType(s, t);
        }

        public static object SafeConvert(this string s, Type t, Func<object> defaultValueGenerator)
        {
            // Converting from US numbers formatting
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");

            if (String.IsNullOrEmpty(s))
                return defaultValueGenerator();
            if (!t.IsValueType)
                return Convert.ChangeType(s, t);
            if (Nullable.GetUnderlyingType(t) != null)// Nullable<T>
                return Convert.ChangeType(s, Nullable.GetUnderlyingType(t));
            return Convert.ChangeType(s, t);
        }

        public static byte[] GenerateHMACSHA1Signature(this string content, string sigKey)
        {
            var hmac = new System.Security.Cryptography.HMACSHA1();
            hmac.Key = Encoding.UTF8.GetBytes(sigKey);
            var contentBytes = Encoding.UTF8.GetBytes(content);
            return hmac.ComputeHash(contentBytes);
        }

        public static string GenerateHMACSHA1SignatureHexDigest(this string content, string sigKey)
        {
            var sig = content.GenerateHMACSHA1Signature(sigKey);
            return String.Concat(Array.ConvertAll(sig, x => x.ToString("x2")));
        }
        public static string GenerateHMACSHA1SignatureBase64(this string content, string sigKey)
        {
            var sig = content.GenerateHMACSHA1Signature(sigKey);
            return Convert.ToBase64String(sig);
        }

        public static string GenerateHMACSHA1SignatureBase64UrlSafe(this string content, string sigKey)
        {
            var sig = content.GenerateHMACSHA1Signature(sigKey);
            return Convert.ToBase64String(sig).Replace("+", "-").Replace("/", "_");
        }

        public static byte[] GenerateHMACSHA256Signature(this string content, string sigKey)
        {
            var hmac = new System.Security.Cryptography.HMACSHA256();
            hmac.Key = Encoding.UTF8.GetBytes(sigKey);
            var contentBytes = Encoding.UTF8.GetBytes(content);
            return hmac.ComputeHash(contentBytes);
        }

        public static string GenerateHMACSHA256SignatureHexDigest(this string content, string sigKey)
        {
            var sig = content.GenerateHMACSHA256Signature(sigKey);
            return String.Concat(Array.ConvertAll(sig, x => x.ToString("x2")));
        }
        public static string GenerateHMACSHA256SignatureBase64(this string content, string sigKey)
        {
            var sig = content.GenerateHMACSHA256Signature(sigKey);
            return Convert.ToBase64String(sig);
        }
        public static string GenerateHMACSHA256SignatureBase64UrlSafe(this string content, string sigKey)
        {
            var sig = content.GenerateHMACSHA256Signature(sigKey);
            return Convert.ToBase64String(sig).Replace("+", "-").Replace("/", "_");
        }

    }
}
