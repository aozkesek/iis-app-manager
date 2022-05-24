using Org.Apps.Crypto;
using PostSharp.Aspects;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Caching;

namespace Org.Apps.Aspect
{
    [Serializable]
    public class LicenceCheckAttribute : MethodInterceptionAspect
    {
        private const string _publicAndPrivateKeyXml = "<RSAKeyValue><Modulus>intWXN8gsHYdQOZK3O/esgycheTd0cyuvd3aLWv8eFVTUech/9sAywwnH5jr8fRVEaCySbl0ZQLKL2bBDFowOdJ7lwsSp0mCs8LFJSyR1hygmX5XOCiI1W0Ydz/RANYnFlyO2Kv7/XvtLhbRyvO2znVjRFwjD2cVM413sYtS14l5Z4lNSQNfzOalrQfJWqxp+pmjR9Qr2svnJ0lQyZfc2t4qHO7N9vzm8t8Sc/1rhUvVB3CdWZm27rOpJTZlcU+cr6bzBedqRycwHIdLNsQD+2tLrG5amqXX9NC4R3QpXCQ3pzUhewM9HGTSBig28cvw93La9FQXr+AsIURRsPugvw==</Modulus><Exponent>AQAB</Exponent><P>wnlH+0BNrRjhytlCujejHSPZn9q6c/+K7FhAqUiSrSs/xWh+uvtbod4mdSngVBO7ql4wRlxfpjyNkgsXmF1EwRnYNFvOAqikQ4SbDxNKqJap1tZDjzN8VoiPRq2j7H0v/83dIU7Q5H0uldRzsNUElQFYvmLj8vY2DzIRypte4VU=</P><Q>tksu8BAEf+G59VZuwBhDtbJa3L7QrqOy4ifVqNiUu0mlvMN98VZGNe1wJzHvUYJ69oB7z5E5ke+eZlTVW7kWGJYCD4N9h/jO3CfPUJOpWQ0uko6W1Tlwn4yTmfb5wWDSxj90v96rH56qVKpptcEkzfJcX71OHlOrLM5r0sKiCcM=</Q><DP>hNIAb4FDi+1qiPhJspU9OjG5+IuSvJbTcwfzYarHQq4/J7xukZYKx0rKSUsIg2PW0Ezz+5orWYYRyqT4wH8y7g+QsnHhCb/UMKjonis22l3MMYa1LaCkUxWBhWwigOofVj5rwMvrr6IvpvUq7qMONhZtXdp8hPvTgB1aOV5UNuE=</DP><DQ>rb2OaOL7Rt7em4PbXl5B+mSY0RdUUX5XudB5hMmXR8Fzoys3V9MAa9l3MQQYsbkIdPQUDMq+8eZNM/7asub+tkQShrKt6/ApSA7xOjziBKvZnXklLOfn/UjATNflRnd/q89C+LPCqQpSEQuyYLZK27aIb/8++wyHctbyUReL1H0=</DQ><InverseQ>MU8d3jC45J7/r3wJDC9XDk9GFlfWiPbIEcc86idBbajpXgX4msz5IEHOhEb8XA7IdU8i1oBMt/7RqE5XydPL/Me3RdVg/Zl2BF/VbRHeZDJDZ9jYIGnd/nUQktI9XUMjSjGNGDRuEi2EeDDsNiuvCTb9hBCNCNWfxSALzveNzyU=</InverseQ><D>E6a09rDjHqdkN1V2wit32qRXgdicca0uDttwnol4ZksVa8X02S3myumKk71LniYh/EJlini6v6rJrjhSpMIXndz0lNaJaxvPZFr7Ru8wMsYVNDLthZaa9E5q7mEr84ZwPYgc7Tpao/n2ClhKpRY4lsuproW0o/bo10v39EXlF5jBHmL5bCUZJSZjsomQ5eJsNEn2Vz0eBbgwX2mpIZXzrpS+oD57788gM17hA0VblTXZFP/uh7D6S/idaSGm6FVbZplRMZVlA0quGnrAIsRN0MeTPL0ObiaSRv+QXhc5pwhv6nqJIXdS63TvAg/nyVkj9fvy09eQOOEYik4oQK47IQ==</D></RSAKeyValue>";
        private const int _keySize = 2048;

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            bool? isLicenced = HttpContext.Current.Cache["isLicenced"] as bool?;

            if (!isLicenced.HasValue)
            {
                string licenceKeyPath = HttpContext.Current.Server.MapPath("/") + "Licence.lic";
                string licenceKey = GetLicenceKey(licenceKeyPath);

                // If key is valid
                var decrypted = AsymmetricEncryption.DecryptText(licenceKey, _keySize, _publicAndPrivateKeyXml);
                var keys = decrypted.Split('-');

                var expireDate = DateTime.FromFileTimeUtc(Convert.ToInt64(keys.Last()));

                DateTime currentDate;
                try
                {
                    currentDate = GetNetworkTime();
                }
                catch
                {
                    currentDate = DateTime.Now;

                    // Clock Manipulation Control
                    bool isManipulated = DetectClockManipulation(currentDate);
                    if (isManipulated)
                    {
                        throw new UnauthorizedAccessException("System Clock Manipulation Detected!");
                    }
                }

                if (expireDate > currentDate)
                {
                    var hwId = decrypted.Replace(keys.Last(), string.Empty);
                    string uniqueId = HardwareInfo.GenerateUniqueId() + "-";

                    // Hardware control
                    if (hwId != uniqueId)
                    {
                        throw new UnauthorizedAccessException("System Change Detected!");
                    }

                    isLicenced = true;
                    HttpContext.Current.Cache.Insert("isLicenced",
                                                      isLicenced,
                                                      new CacheDependency(licenceKeyPath),
                                                      DateTime.Now.AddDays(1),
                                                      TimeSpan.Zero);
                }
                else
                {
                    isLicenced = false;
                }
            }

            if (isLicenced.Value)
            {
                base.OnInvoke(args);
                return;
            }
            else
            {
                throw new UnauthorizedAccessException("Your licence is expired or invalid!");
            }
        }

        private static DateTime GetNetworkTime()
        {
            const string ntpServer = "time.windows.com";
            var ntpData = new byte[48];

            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }

            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        // stackoverflow.com/a/3294698/162671
        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

        private static string GetLicenceKey(string licenceKeyPath)
        {
            string key = string.Empty;
            using (StreamReader sr = new StreamReader(licenceKeyPath))
            {
                key = sr.ReadToEnd();
            }
            return key;
        }

        private static bool DetectClockManipulation(DateTime thresholdTime)
        {
            DateTime adjustedThresholdTime = new DateTime(thresholdTime.Year, thresholdTime.Month, thresholdTime.Day, 23, 59, 59);

            using (EventLog eventLog = new EventLog("system"))
            {
                foreach (EventLogEntry entry in eventLog.Entries)
                {
                    if (entry.TimeWritten > adjustedThresholdTime)
                        return true;
                }
                return false;
            }
        }
    }
}