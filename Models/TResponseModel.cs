using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QnABot.Models
{
    public class TResponseModel
    {
        public string Reponse { get; set; }

        public bool Error { get; set; } = false;

        public string Exception { get; set; }

        public string MessageError { get; set; }
    }
}