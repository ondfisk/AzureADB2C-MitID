var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddScoped(_ => new GraphServiceClient(new DefaultAzureCredential()));
        services.AddScoped<IGraphHelper, GraphHelper>();
    })
    .Build();

await host.RunAsync();
