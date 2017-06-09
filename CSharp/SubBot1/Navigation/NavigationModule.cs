namespace SubBot1.Navigation
{
    using Autofac;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    public class NavigationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .RegisterType<NavigationScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
        }
    }
}