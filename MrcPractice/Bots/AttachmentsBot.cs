using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Connector;
using System.IO;
using System;

namespace EchoBot.Bots
{
    public class GetImgAttachmentParams
    {
        public ITurnContext<IMessageActivity> turnContext;
        public CancellationToken cancellation;
        public string serviceUrl;
        public string conversationId;
        public string imgPath;
    }

    public class AttachmentsBot: ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if(turnContext.Activity.Attachments.Count > 0)
            {
                // show attaches that the user send
                await DisplayPicturesFromTheUserAsync(turnContext, cancellationToken);
            }

            var reply = MessageFactory.Text("hola");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = DisplayOptionsAsync(turnContext, cancellationToken); // card
            var card2 = Cards.GetHeroCard(); // Second herocard
            var signingCard = Cards.GetSignInCard(); // sign-in card
            var adaptiveCard = Cards.CreateAdaptiveCardAttachment(); // adaptive card attachment


            var reply = MessageFactory.Attachment(adaptiveCard);
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        // more advanced way to attach a image
        private static async Task<Attachment> GetImgAttachment(GetImgAttachmentParams parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.serviceUrl))
            {
                throw new ArgumentNullException(nameof(parameters.serviceUrl));
            }

            if (string.IsNullOrWhiteSpace(parameters.conversationId))
            {
                throw new ArgumentNullException(nameof(parameters.conversationId));
            }

            //
            var connector = parameters.turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;

            //
            var attachments = new Attachments(connector);

            //
            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                parameters.conversationId,
                new AttachmentData
                {
                    Name = Path.GetFileName(parameters.imgPath),
                    OriginalBase64 = File.ReadAllBytes(parameters.imgPath),
                    Type = Path.GetExtension(parameters.imgPath)
                }, parameters.cancellation);

            // uri for the new attachment
            var attachmentUri = attachments.GetAttachmentUri(response.Id);

            // new attachment to return
            var attachment = new Attachment
            {
                Name = Path.GetFileName(parameters.imgPath),
                ContentType = Path.GetExtension(parameters.imgPath),
                ContentUrl = attachmentUri,
            };

            return attachment;
        }

        // simple way to attach a image
        private static Attachment GetImgAttachment()
        {
            var imgPath = Path.Combine(Environment.CurrentDirectory, @"Resources", "batman.jpg");


            var imgData = Convert.ToBase64String(File.ReadAllBytes(imgPath));

            var attachment = new Attachment
            {
                Name = @"Resources\batman.jpg",
                ContentType = "image/jpg",
                ContentUrl = $"data:image/jpg;base64,{imgData}",
            };

            return attachment;
        }

        private static async Task DisplayPicturesFromTheUserAsync(ITurnContext<IMessageActivity> turnContext,  CancellationToken cancellationToken)
        {
            var attachments = new List<Attachment>();
            foreach (var picturePath in turnContext.Activity.Attachments)
            {
                GetImgAttachmentParams parameter = new GetImgAttachmentParams
                {
                    cancellation = cancellationToken,
                    turnContext = turnContext,
                    conversationId = turnContext.Activity.Conversation.Id,
                    serviceUrl = turnContext.Activity.ServiceUrl,
                    imgPath = picturePath.ThumbnailUrl
                };
                var attachment = await GetImgAttachment(parameter);

                attachments.Add(attachment);
            }
            var reply = MessageFactory.Text("Pictures that you uploaded");

            //reply.Attachments = new List<Attachment>() { GetImgAttachment() };

            // attach pictures in the response to the user
            reply.Attachments = attachments;

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // hero card
        private static HeroCard DisplayOptionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var imgPath = Path.Combine(Environment.CurrentDirectory, @"Resources", "batman.jpg");
            var imgData = Convert.ToBase64String(File.ReadAllBytes(imgPath));

            var card = new HeroCard
            {
                Text = "You can upload an image or select one of the following choices",
                Images = new List<CardImage>()
                {
                    new CardImage(url: $"data:image/jpg;base64,{imgData}", alt: "batman.jpg"),
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.ImBack, title: "1. Inline Attachment", value: "1"),
                    new CardAction(ActionTypes.ImBack, title: "2. Internet Attachment", value: "2"),
                    new CardAction(ActionTypes.ImBack, title: "3. Uploaded Attachment", value: "3"),
                },
            };

            return card;
        }
    }
}
