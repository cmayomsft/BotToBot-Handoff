namespace MainBot.Navigation
{
    using Autofac;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    public class NavigationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Register NavigationScorable as middleware to intercept every message to the conversation.
            builder
                .RegisterType<NavigationScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
        }
    }
}