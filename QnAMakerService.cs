// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Cognitive Services based Dialogs for Bot Builder:
// https://github.com/Microsoft/BotBuilder-CognitiveServices 
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//



using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Sample.QnABot
{

    public sealed class QnAMakerService
    {
        private readonly QnAMakerAttribute qnaInfo;

        /// <summary>
        /// The base URI for accessing QnA Service.
        /// </summary>
        public static readonly Uri UriBaseV2 = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases");

        /// <summary>
        /// Construct the QnA service using the qnaInfo information.
        /// </summary>
        /// <param name="qnaInfo">The QnA knowledgebase information.</param>
        public QnAMakerService(QnAMakerAttribute qnaInfo)
        {
            SetField.NotNull(out this.qnaInfo, nameof(qnaInfo), qnaInfo);
        }

        public Uri BuildRequest(string queryText, out QnAMakerRequestBody postBody, out string authKey)
        {
            var knowledgebaseId = this.qnaInfo.KnowledgebaseId;

            Uri UriBase;

            // Check if the hostname was passed
            if (string.IsNullOrEmpty(this.qnaInfo.EndpointHostName))
            {
                // No hostname passed, add the V2 URI
                UriBase = UriBaseV2;

                // V2 subacription Key
                authKey = this.qnaInfo.AuthKey.Trim();
            } else
            {
                // Hostname was passed, build the V4 endpoint URI
                string hostName = this.qnaInfo.EndpointHostName.ToLower();

                // Remove https
                if (hostName.Contains("https://"))
                {
                    hostName = hostName.Split('/')[2];
                }

                // Remove qnamaker
                if (hostName.Contains("qnamaker"))
                {
                    hostName = hostName.Split('/')[0];
                }

                // Trim any trailing /
                hostName = hostName.TrimEnd('/');

                // Create the V4 Uri based on the hostname
                UriBase = new Uri("https://" + hostName + "/qnamaker/knowledgebases");

                // Check if key has endpoint in it
                if (this.qnaInfo.AuthKey.ToLower().Contains("endpointkey"))
                    authKey = this.qnaInfo.AuthKey.Trim(' ');
                else
                    authKey = "EndpointKey " + this.qnaInfo.AuthKey.Trim(' ');
            }

            var builder = new UriBuilder($"{UriBase}/{knowledgebaseId}/generateanswer");

            postBody = new QnAMakerRequestBody { question = queryText, top = this.qnaInfo.Top };

            return builder.Uri;
        }

        public async Task<List<QnAMakerResults>> QueryServiceAsync(Uri uri, QnAMakerRequestBody postBody, string authKey)
        {
            string json = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                //Add the key header according to V2 or V4 format
                if (authKey.ToLower().Contains("endpointkey"))
                    client.DefaultRequestHeaders.Add("Authorization", authKey);
                else
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", authKey);

                var response = await client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(postBody), Encoding.UTF8, "application/json"));
                if (response != null && response.Content != null)
                {
                    json = await response.Content.ReadAsStringAsync();
                }
            }           

            try
            {
                var qnaMakerResults = JsonConvert.DeserializeObject<QnAMakerResults>(json);

                //Adding internal service cfg reference [used when checking configured threshold to provide an answer]
                qnaMakerResults.ServiceCfg = this.qnaInfo;

                var filteredQnAResults = new List<QnAMakerResult>();
                foreach (var qnaMakerResult in qnaMakerResults.Answers)
                {
                    qnaMakerResult.Score /= 100;
                    if (qnaMakerResult.Score >= this.qnaInfo.ScoreThreshold)
                    {
                        qnaMakerResult.Answer = HttpUtility.HtmlDecode(qnaMakerResult.Answer);
                        qnaMakerResult.Questions = qnaMakerResult.Questions.Select(x => HttpUtility.HtmlDecode(x)).ToList();
                        filteredQnAResults.Add(qnaMakerResult);
                    }
                }
                qnaMakerResults.Answers = filteredQnAResults;

                var _listQnAMakerResults = new List<QnAMakerResults>();
                _listQnAMakerResults.Add(qnaMakerResults);

                return _listQnAMakerResults;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the QnA service response.", ex);
            }
        }

        public async Task<bool> ActiveLearnAsync(Uri uri, QnAMakerTrainingRequestBody postBody, string authKey)
        {
            try
            {
                string json = string.Empty;

                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri)
                                      {
                                          Content =
                                              new StringContent(
                                              JsonConvert
                                              .SerializeObject(
                                                  postBody),
                                              Encoding.UTF8,
                                              "application/json")
                                      };

                    //Add the subscription key header
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", authKey);

                    var response = await client.SendAsync(request);
                    if (response != null && response.Content != null)
                    {
                        json = await response.Content.ReadAsStringAsync();
                    }
                }
            
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class QnAMakerRequestBody
    {
        [JsonProperty("question")]
        public string question { get; set; }

        [JsonProperty("top")]
        public int top { get; set; }
    }

    public class QnAMakerTrainingRequestBody
    {
        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }

        [JsonProperty("feedbackRecords")]
        public List<FeedbackRecord> FeedbackRecords { get; set; }
    }

    [Serializable]
    public class FeedbackRecord
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userQuestion")]
        public string UserQuestion { get; set; }

        [JsonProperty("kbQuestion")]
        public string KbQuestion { get; set; }

        [JsonProperty("kbAnswer")]
        public string KbAnswer { get; set; }
    }

    public class Extensions
    {
        public async Task<List<QnAMakerResults>> QueryServiceAsync(string text, string aKey, string kId, string host)
        {
            QnAMakerRequestBody postBody;
            QnAMakerService qnAMakerService = new QnAMakerService(new QnAMakerAttribute(aKey, kId, "Lo siento, no entendi tu mensaje", host));
            string authKey;
            var uri = qnAMakerService.BuildRequest(text, out postBody, out authKey);

            return await qnAMakerService.QueryServiceAsync(uri, postBody, authKey);
        }
    }
}
