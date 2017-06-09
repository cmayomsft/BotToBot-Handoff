namespace SubBot2
{
    using System.Web.Http;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Navigation;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            this.RegisterModules();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        private void RegisterModules()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new ReflectionSurrogateModule());

            // Wire up all global navigation commands via scorables.
            builder.RegisterModule<NavigationModule>();

            builder.Update(Conversation.Container);
        }
    }
}
