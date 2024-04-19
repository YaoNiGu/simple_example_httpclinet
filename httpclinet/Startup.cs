using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

public class Startup
{
    IConfiguration Configuration { get; }

    public Startup()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("config.json");
        Configuration = builder.Build();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IConfiguration>(Configuration);
        services.AddSingleton<GetTwseDataService>();
        services.AddSingleton<GetOldTwseDataService>();
        services.AddSingleton<DataProcessingService>();
    }
}