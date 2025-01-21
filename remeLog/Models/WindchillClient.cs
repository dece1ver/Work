using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using remeLog.Infrastructure;

namespace remeLog.Models
{
    public class WindchillClient : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _serverUrl;
        private readonly string _localType;
        private bool _isAuthorized;

        public WindchillClient(string serverUrl, string username, string password, string localType)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            _client = new HttpClient(handler);
            _serverUrl = serverUrl;
            _localType = localType;

            var authString = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}")
            );
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
        }

        public async Task<bool> AuthorizeAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/Windchill/app/")
                {
                    Headers =
                    {
                        Accept = { new MediaTypeWithQualityHeaderValue("text/html") },
                        AcceptEncoding = { new StringWithQualityHeaderValue("gzip"), new StringWithQualityHeaderValue("deflate") },
                        AcceptLanguage = { new StringWithQualityHeaderValue("ru-RU"), new StringWithQualityHeaderValue("ru", 0.9) },
                        CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(0) },
                        ConnectionClose = false
                    }
                };

                request.Headers.Add("Sec-Fetch-Dest", "document");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-Site", "none");
                request.Headers.Add("Sec-Fetch-User", "?1");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
                request.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");

                var response = await _client.SendAsync(request);
                _isAuthorized = response.IsSuccessStatusCode;
                return _isAuthorized;
            }
            catch (Exception)
            {
                _isAuthorized = false;
                throw;
            }
        }

        public async Task<string> SearchAsync(string searchQuery)
        {
            if (!_isAuthorized)
                throw new InvalidOperationException("Необходимо выполнить авторизацию перед поиском");

            // Настраиваем заголовки для AJAX-запросов
            ConfigureAjaxHeaders();

            // Выполнение поиска
            var searchResponse = await ExecuteSearch(searchQuery);
            await EnsureSuccessResponse(searchResponse, "выполнения поиска");

            // Получение результатов
            var resultsResponse = await GetSearchResults(searchQuery);
            await EnsureSuccessResponse(resultsResponse, "получения результатов");

            var result = await resultsResponse.Content.ReadAsStringAsync();
            return result;
        }

        private void ConfigureAjaxHeaders()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.Add("X-Prototype-Version", "1.6.1");
            _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        }

        private async Task<HttpResponseMessage> ExecuteSearch(string searchQuery)
        {
            // Формируем URL для поиска
            var searchUrl = GenerateCadSearchUrl(_serverUrl, searchQuery);

            // Настраиваем заголовки для AJAX-запросов
            ConfigureAjaxHeaders();

            // Формируем запрос
            var request = new HttpRequestMessage(HttpMethod.Get, searchUrl)
            {
                Headers =
                {
                    Referrer = new Uri($"{_serverUrl}/Windchill/app/"),
                    AcceptLanguage = { new StringWithQualityHeaderValue("ru-RU"), new StringWithQualityHeaderValue("ru", 0.9) }
                }
            };

            // Выполняем запрос
            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> GetSearchResults(string searchQuery)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("null___keywordkeywordField_SearchTextBox___textbox", searchQuery),
                new KeyValuePair<string, string>($"WCTYPE|wt.epm.EPMDocument|local.{_localType}.DefaultEPMDocument", "on"),
                new KeyValuePair<string, string>("searchType", $"WCTYPE|wt.epm.EPMDocument|local.{_localType}.DefaultEPMDocument"),
                new KeyValuePair<string, string>("portlet", "poppedup")
            });

            return await _client.PostAsync(
                $"{_serverUrl}/Windchill/ptc1/searchResultsComp",
                content
            );
        }

        private async Task EnsureSuccessResponse(HttpResponseMessage response, string operationName)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var ex = new HttpRequestException($"Ошибка {operationName}: {response.StatusCode}");
                Util.WriteLog(ex, error);
                throw ex;
            }
        }

        private string GenerateCadSearchUrl(string baseUrl, string searchKeyword)
        {
            // Кодировка ключевого слова
            var encodedKeyword = Uri.EscapeDataString(searchKeyword);

            // Формирование URL
            return $"{baseUrl}/Windchill/wtcore/jsp/com/ptc/windchill/search/Search.jsp" +
                   $"?search_keyword={encodedKeyword}" +
                   $"&preSelectionItems=WCTYPE|wt.epm.EPMDocument|local.{_localType}.DefaultEPMDocument" +
                   $"&searchType=WCTYPE|wt.epm.EPMDocument|local.{_localType}.DefaultEPMDocument" +
                   $"&fireSearch=true";
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
