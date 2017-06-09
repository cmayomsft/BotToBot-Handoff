#pragma warning disable 1998

namespace SubBot2.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Properties;

    public class SubBot2_1_Dialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.FirstPromptResumeAfter, new[] { Resources.MoreReply }, "SubBot 2.1 Dialog dialog text...", "I'm sorry, I don't understand. Please try again.");
        }

        private async Task FirstPromptResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = await result;

                if (message == Resources.MoreReply)
                {
                    PromptDialog.Choice(context, this.SecondPromptResumeAfter, new[] { Resources.MoreReply }, "SubBot 2.1 Dialog second dialog text...", "I'm sorry, I don't understand. Please try again.");
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Fail(new TooManyAttemptsException("Too many attempts."));
            }
        }

        private async Task SecondPromptResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = await result;

                if (message == Resources.MoreReply)
                {
                    PromptDialog.Choice(context, this.ThirdPromptResumeAfter, new[] { Resources.MoreReply }, "SubBot 2.1 Dialog third dialog text...", "I'm sorry, I don't understand. Please try again.");
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Fail(new TooManyAttemptsException("Too many attempts."));
            }
        }

        private async Task ThirdPromptResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = await result;

                if (message == Resources.MoreReply)
                {
                    PromptDialog.Choice(context, this.FourthPromptResumeAfter, new[] { Resources.SubBot2_Menu, Resources.MainBot_Menu }, "SubBot 2.1 Dialog is done. What do you want to do next?...", "I'm sorry, I don't understand. Please try again.");
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Fail(new TooManyAttemptsException("Too many attempts."));
            }
        }

        private async Task FourthPromptResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = await result;

                // If we got here, it's because something other than a navigation command happened, and this dialog only supports navigation commands.
                await this.StartOverAsync(context, $"I'm sorry, I don't understand '{ message }'.");
            }
            catch (TooManyAttemptsException)
            {
                context.Fail(new TooManyAttemptsException("Too many attempts."));
            }
        }

        private async Task StartOverAsync(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync(message);
            await this.StartAsync(context);
        }
    }
}