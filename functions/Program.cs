var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
    })
    .Build();

host.Run();
