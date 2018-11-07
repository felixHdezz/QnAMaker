using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class QnAMakerResults
    {
        public QnAMakerResults() { }

        public QnAMakerResults(List<QnAMakerResult> qnaMakerResults)
        {
            Answers = qnaMakerResults;
        }

        [JsonProperty(PropertyName = "answers")]
        public List<QnAMakerResult> Answers { get; set; }

        internal QnAMakerAttribute ServiceCfg { get; set; }
    }

    [Serializable]
    public class QnAMakerResult
    {
        public QnAMakerResult() { }

        public QnAMakerResult(string answer, List<string> questions, double score)
        {
            Answer = answer;
            Questions = questions;
            Score = score;
        }

        [JsonProperty(PropertyName = "questions")]
        public List<string> Questions { get; set; }

        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }
}
