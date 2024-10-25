// Generated with EchoBot .NET Template version v4.22.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot.Bots
{
    public class SuggestedActionBot : ActivityHandler
    {
        public const string WelcomeText = "This bot will introduce you to suggestedActions. Please answer the question:";

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text.ToLowerInvariant();

            var responseText = ProcessInput(text);

            await turnContext.SendActivityAsync(responseText, cancellationToken: cancellationToken);

            await SendSuggestedActionsAsync(turnContext, cancellationToken: cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync($"Welcome to SuggestedActionsBot {member.Name}. {WelcomeText}",
                        cancellationToken: cancellationToken);
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
            }
        }

        private static string ProcessInput(string text)
        {
            const string colorText = "is the best color, I agree.";

            switch(text)
            {
                case "red":
                    {
                        return $"Red {colorText}";
                    }

                case "blue":
                    {
                        return $"Blue {colorText}";
                    }

                case "yellow":
                    {
                        return $"Yellow {colorText}";
                    }

                default:
                    {
                        return "Please select a color from the suggested action choices";
                    }
            }
        }

        // Creates and sends an activity with suggested actions to the user. When the user
        // clicks one of the buttons the text value from the "CardAction" will be
        // displayed in the channel just as if the user entered the text. There are multiple
        // "ActionTypes" that may be used for different situations.
        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("What is your favorite color?");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red", Image = "https://via.placeholder.com/20/FF0000?text=R", ImageAltText = "R" },
                    new CardAction() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow", Image = "https://via.placeholder.com/20/FFFF00?text=Y", ImageAltText = "Y" },
                    new CardAction() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue", Image = "https://via.placeholder.com/20/0000FF?text=B", ImageAltText = "B" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
