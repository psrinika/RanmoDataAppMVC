using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RanmoDataAppMVC.Startup))]
namespace RanmoDataAppMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
