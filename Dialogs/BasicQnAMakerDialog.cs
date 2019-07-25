using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;

using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
		public async Task StartAsync(IDialogContext context)
        {
			/* Wait until the first message is received from the conversation and call MessageReceviedAsync 
            *  to process that message. */

			context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await result;
            

            var qnaAuthKey = SqlServices.qnaAuthKey;
            var qnaKBId = SqlServices.qnaKBId;
            var endpointHostName = SqlServices.endpointHostName;

            // QnA Subscription Key and KnowledgeBase Id null verification
            if (!string.IsNullOrEmpty(qnaAuthKey) && !string.IsNullOrEmpty(qnaKBId))
            {
                // Forward to the appropriate Dialog based on whether the endpoint hostname is present
                //if (string.IsNullOrEmpty(endpointHostName))
                //{
                //    await context.Forward(new BasicQnAMakerPreviewDialog(), AfterAnswerAsync, message, CancellationToken.None);
                //}
                //else
                //{
                //    await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);

                //    message.Text = "seleccion de talento";

                //    await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
                //}
            }
            else
            {
                await context.PostAsync("Please set QnAKnowledgebaseId, QnAAuthKey and QnAEndpointHostName (if applicable) in App Settings. Learn how to get them at https://aka.ms/qnaabssetup.");
            }
        }

        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message
            context.Wait(MessageReceivedAsync);
        }

        public static string GetSetting(string key)
        {
            var value = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(value) && key == "QnAAuthKey")
            {
                value = Utils.GetAppSetting("QnASubscriptionKey"); // QnASubscriptionKey for backward compatibility with QnAMaker (Preview)
            }
            return value;
        }
    }

    // Dialog for QnAMaker Preview service
    //[Serializable]
    //public class BasicQnAMakerPreviewDialog : QnAMakerDialog
    //{
    //    // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
    //    // Parameters to QnAMakerService are:
    //    // Required: subscriptionKey, knowledgebaseId, 
    //    // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
    //    public BasicQnAMakerPreviewDialog() : base(new QnAMakerService(new QnAMakerAttribute(SqlServices.qnaAuthKey, SqlServices.qnaKBId, "Lo siento, no entendí tu mensaje", 0.5)))
    //    { }
    //}

    //// Dialog for QnAMaker GA service
    //[Serializable]
    //public class BasicQnAMakerDialog : QnAMakerDialog
    //{
    //    // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
    //    // Parameters to QnAMakerService are:
    //    // Required: qnaAuthKey, knowledgebaseId, endpointHostName
    //    // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
    //    public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(SqlServices.qnaAuthKey, SqlServices.qnaKBId, "Lo siento, no entendí tu mensaje", 0.5, 1, SqlServices.endpointHostName)))
    //    { }

    //    protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
    //    {
    //        var answer = result.Answers.First().Answer;
    //        Activity reply = ((Activity)context.Activity).CreateReply();

    //        reply.Text = answer;

    //        //string url = "";
    //        //if (reply.Text.Equals("Not Match Found!"))
    //        //     reply.Text = "No good match in FAQ";


    //        //Ejemplo de responder con herocard

    //        //HeroCard card = new HeroCard
    //        //{
    //        //    Title = "Este es una prueba",
    //        //    Subtitle = "Este es una descripción de ejemplo de la imagen, que el bot responde con tipo de card \n Saludos!!",
    //        //};

    //        //card.Buttons = new List<CardAction>
    //        //{
    //        //    new CardAction(ActionTypes.OpenUrl, "Learn More", value: "www.google.com")
    //        //};

    //        //card.Images = new List<CardImage>
    //        //{
    //        //    new CardImage( url= "https://i.blogs.es/594843/chrome/450_1000.jpg")
    //        //};

    //        //reply.Attachments.Add(card.ToAttachment());

    //        if (string.IsNullOrEmpty(reply.Text)) {
    //            await context.PostAsync(answer);
    //        }
    //    }

    //    protected override async Task DefaultWaitNextMessageAsync(IDialogContext context, IMessageActivity message, QnAMakerResults results)
    //    {
    //        //await context.PostAsync("Lo siento, no entendí tu mensaje");
    //        //await context.PostAsync("Enseguida sera notificado el responsable de area para seguir con el proceso");
    //        await base.RespondFromQnAMakerResultAsync(context, message, results);
    //    }
    //}

	public class SqlServices
	{
        public static string qnaAuthKey = "a5d36313-2469-4df7-8010-f88fcc6168ff";
        public static string qnaKBId = "321a264c-93aa-4d19-8771-86a40a0ed541";
        public static string endpointHostName = "https://acqavending.azurewebsites.net/qnamaker";

        public static string _UserName = string.Empty;
		public static string _Area = string.Empty;
		public SqlServices(string user, string area)
		{
			_UserName = user;
			_Area = area;
		}

		public List<ModelQaNMaker> GetEndPointQaNMaker()
		{
			string strQuery = "exec sp_GetEndPointByUser '" + _UserName + "','" + _Area + "'";
			List<ModelQaNMaker> list_endPoint = new List<ModelQaNMaker>();

			/* string query */
			StringBuilder sb = new StringBuilder();
			sb.Append(strQuery);
			String queryString = sb.ToString();

			/* connection to date base */
			DbConnection _conn = new DbConnection();

			SqlDataReader _dReader = _conn.executeQuery(queryString);
			while (_dReader.Read())
			{
				ModelQaNMaker endpoint = new ModelQaNMaker();

				endpoint.qnaAuthKey = _dReader.GetString(0);
				endpoint.qnaKBId = _dReader.GetString(1);
				endpoint.endpointHostName = _dReader.GetString(2);

				list_endPoint.Add(endpoint);
			}

			//close current connection
			DbConnection._conn.Close();
			return list_endPoint;
		}

		public static string QnaAuthKey()
		{
			string strQuery = "exec sp_GetEndPointByUser '" + _UserName + "','" + _Area + "'";
			var _qnaAuthKey = string.Empty;
			StringBuilder sb = new StringBuilder();
			sb.Append(strQuery);
			/* connection to date base */
			DbConnection _conn = new DbConnection();

			SqlDataReader _dReader = _conn.executeQuery(sb.ToString());
			while (_dReader.Read())
			{
				_qnaAuthKey = _dReader.GetString(0);
			}
			DbConnection._conn.Close();
			return _qnaAuthKey;
		}

		public static string QnaKBId()
		{
			string strQuery = "exec sp_GetEndPointByUser '" + _UserName + "','" + _Area + "'";
			var _qnaKBId = string.Empty;
			StringBuilder sb = new StringBuilder();
			sb.Append(strQuery);
			/* connection to date base */
			DbConnection _conn = new DbConnection();

			SqlDataReader _dReader = _conn.executeQuery(sb.ToString());
			while (_dReader.Read())
			{
				_qnaKBId = _dReader.GetString(1);
			}
			DbConnection._conn.Close();
			return _qnaKBId;
		}

		public static string EndpointHostName()
		{
			string strQuery = "exec sp_GetEndPointByUser '" + _UserName + "','" + _Area + "'";
			var _endpointHostName = string.Empty;
			StringBuilder sb = new StringBuilder();
			sb.Append(strQuery);
			/* connection to date base */
			DbConnection _conn = new DbConnection();

			SqlDataReader _dReader = _conn.executeQuery(sb.ToString());
			while (_dReader.Read())
			{
				_endpointHostName = _dReader.GetString(2);
			}
			DbConnection._conn.Close();
			return _endpointHostName;
		}
	}


	public class ModelQaNMaker
	{
		public string qnaAuthKey { get; set; }
		public string qnaKBId { get; set; }
		public string endpointHostName { get; set; }
	}
}