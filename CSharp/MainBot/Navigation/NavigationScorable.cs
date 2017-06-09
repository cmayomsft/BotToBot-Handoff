#pragma warning disable 1998

namespace MainBot.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;
    using Properties;

    public class NavigationScorable : ScorableBase<IActivity, string, double>
    {
        private IDialogStack stack;
        private IDialogTask task;

        // TODO: Move navigation commands to the Root dialog so these commands are only defined in a single location.
        // List of navigation commands that will, if matched to the text of the incoming message, trigger navigation to another dialog/conversation flow.
        private List<string> navigationCommands;
        
        public NavigationScorable(IDialogStack stack, IDialogTask task)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.task, nameof(task), task);

            this.navigationCommands = new List<string>();

            this.navigationCommands.Add(Resources.MainBot_Menu);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                // Does the incoming message match one of the navigation commands?
                var command = (from cmd in this.navigationCommands
                               where message.Text.Equals(cmd, StringComparison.InvariantCultureIgnoreCase)
                               select cmd).FirstOrDefault();

                // If the message text matched a navigation command, return the message text as state to signal a scorable match.
                if (command != null)
                {
                    return message.Text;
                }
            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            // If the call to PrepareAsync returned state (a navigation command was matched), then the scorable has a score.
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            // Return 1.0, since navigation commands match as an exact match.
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var message = item as IMessageActivity;

            // If the message is not null, the message was returned from PrepareAsync because it matched a navigation command.
            if (message != null)
            {
                // To navigate to another dialog/conversation flow, reset the dialog stack so the stack is in a pristine state.
                this.stack.Reset();

                var rootDialog = new RootDialog();

                // Forward the navigation command to the RootDialog so it can add the correct dialog to the stack to handle the change in conversation flow.
                await this.stack.Forward(rootDialog, null, message, CancellationToken.None);
                await this.task.PollAsync(CancellationToken.None);
            }
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}