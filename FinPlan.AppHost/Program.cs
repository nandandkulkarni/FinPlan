var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.FinPlan_ApiService>("apiservice");

builder.AddProject<Projects.FinPlan_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
