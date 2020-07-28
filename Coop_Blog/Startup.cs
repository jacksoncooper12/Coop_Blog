using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Coop_Blog.Startup))]
namespace Coop_Blog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
