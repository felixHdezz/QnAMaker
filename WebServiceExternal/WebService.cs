using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using QnABot.DataServices;
using QnABot.Models;

namespace QnABot.WebServiceExternal
{
    public class WebService: IDisposable
    {
        // Private const properties
        private const string MediaTypeJson = "application/json";
        private const string MediaTypeXml = "application/xml";
        private static StringContent requestContent = null;

        #region Public Constructors

        public WebService()
        {
            // Code here
        }

        #endregion Public Constructors

        #region Public Properties

        public static WebService Instance
        {
            get
            {
                return ISingletonInstance<WebService>.GetEntityIntance;
            }
        }
        #endregion Public Properties

        #region Public Methods

        public async Task<TResponseModel> PostAsync(UriBuilder url, object postFrom, string data = null)
        {
            var _ReponseModel = new TResponseModel();
            if (!string.IsNullOrEmpty(url.ToString()))
            {
                using (var _httpClient = new HttpClient())
                {
                    requestContent = StringContent(postFrom, data);

                    using (var response = await _httpClient.PostAsync(url.Uri, requestContent))
                    {
                        if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _ReponseModel.Reponse = await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            _ReponseModel.Error = true;
                            _ReponseModel.MessageError = " Error " + (int)response.StatusCode + ", EndPoint (" + url.Uri + ") " + response.ReasonPhrase + ", ";
                        }
                    }
                }
            }
            return _ReponseModel;
        }

        public async Task<string> GetAsync(UriBuilder url)
        {
            if (!string.IsNullOrEmpty(url.Uri.ToString()))
            {
                using (var _htppClient = new HttpClient())
                {
                    var result = await _htppClient.GetAsync(url.Uri);

                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsStringAsync();
                    }
                }
            }
            return string.Empty;
        }

        #endregion Public Methods

        #region Private Methods

        private static StringContent StringContent(object postFrom, string data = null)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new StringContent(JsonConvert.SerializeObject(postFrom), Encoding.UTF8, MediaTypeJson);
            }
            return new StringContent(data.ToString(), Encoding.UTF8, MediaTypeJson);
        }

        private static string ConvertToJsonString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings());
        }

        private static string NormalizeBaseUrl(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }

        #endregion Private Methods

        #region IDisposable Support

        private bool disposedValue = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}