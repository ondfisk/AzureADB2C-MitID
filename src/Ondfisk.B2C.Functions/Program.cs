var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddScoped(_ => new GraphServiceClient(new DefaultAzureCredential()));
        services.AddScoped<IGraphHelper, GraphHelper>();
    })
    .Build();

await host.RunAsync();
