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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Internals.Fibers;
using System.Reflection;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker.Resource;
using System.Collections.ObjectModel;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class QnAMakerDialog : IDialog<object>
    {
        private QnAMakerResults qnaMakerResults;
        private string _strTest = string.Empty;

        public async Task StartAsync(IDialogContext context)
        {
            // Aquí se configurara para obtener parametros externos


            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var variable = _strTest;

            var message = await argument;

            var qnaAuthKey = SqlServices.qnaAuthKey;
            var qnaKBId = SqlServices.qnaKBId;
            var endpointHostName = SqlServices.endpointHostName;

            if (message != null && !string.IsNullOrEmpty(message.Text))
            {
                var ext = new Extensions();
                
                var tasks = await ext.QueryServiceAsync(message.Text, qnaAuthKey, qnaKBId, endpointHostName);

                //buscar nuevo pregunta a qnamaker
                var tasks1 = await ext.QueryServiceAsync("Seleccion de talento", qnaAuthKey, qnaKBId, endpointHostName);

                tasks.FirstOrDefault().Answers.First().Answer += " \n \n " + tasks1.FirstOrDefault().Answers.First().Answer;

                if (tasks.Count > 0) {
                    await this.sendMessageToUser(context, message, tasks);

                    //envia dos mensaje desde el mismo response
                    //await this.sendMessageToUser(context, message, tasks1);
                }
            }
        }

        protected virtual async Task sendMessageToUser(IDialogContext context, IMessageActivity message, List<QnAMakerResults> result)
        {
            var sendDefaultMessageAndWait = true;
            //obtien la lista de los datos de preguntas
            qnaMakerResults = result.FirstOrDefault();

            if (qnaMakerResults != null && qnaMakerResults.Answers != null && qnaMakerResults.Answers.Count > 0)
            {
                if (this.IsConfidentAnswer(qnaMakerResults))
                {
                    await this.RespondFromQnAMakerResultAsync(context, message, qnaMakerResults);
                }
                sendDefaultMessageAndWait = false;
            }

            //Si el api no devuelve ningun resultado se envia el mensaje por default
            if (sendDefaultMessageAndWait)
            {
                await context.PostAsync(qnaMakerResults.ServiceCfg.DefaultMessage);
            }

        }

        protected virtual bool IsConfidentAnswer(QnAMakerResults qnaMakerResults)
        {
            if (qnaMakerResults.Answers.Count <= 1)
            {
                return true;
            }
            return false;
        }

        protected virtual async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            await context.PostAsync(result.Answers.First().Answer);
        }

        protected virtual async Task DefaultWaitNextMessageAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            context.Done(true);
        }
    }
}