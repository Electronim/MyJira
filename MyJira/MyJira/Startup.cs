using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyJira.Startup))]
namespace MyJira
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
