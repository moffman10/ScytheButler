var builder = DistributedApplication.CreateBuilder(args);

// 1. API Service
var apiService = builder.AddProject<Projects.Dashboard_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

// 2. Web Dashboard
builder.AddProject<Projects.Dashboard_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

// 3. ScytheButler Bot
builder.AddProject<Projects.ScytheButler>("scythebutler")
    .WithReference(apiService) // Connects the bot to the API service
    .WaitFor(apiService);      // Ensures the API is running before launching the bot

builder.Build().Run();