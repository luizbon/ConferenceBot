using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Extensions
{
    public static class DialogContextExtensions
    {
        public static IMessageActivity CreateMessage(this IDialogContext context)
        {
            var message = context.MakeMessage();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Type = ActivityTypes.Message;
            message.TextFormat = TextFormatTypes.Plain;

            return message;
        }

        public static IMessageActivity CreateMessage(this IDialogContext context, IList<Attachment> attachments)
        {
            var message = CreateMessage(context);
            message.Attachments = attachments;

            return message;
        }

        public static IMessageActivity CreateMessage(this IDialogContext context, Attachment attachment)
        {
            return CreateMessage(context, new List<Attachment> { attachment });
        }

        public static async Task SendTyping(this IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Type = ActivityTypes.Typing;
            await context.PostAsync(message);
        }
    }
}