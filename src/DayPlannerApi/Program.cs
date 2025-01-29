using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);

//Logger
builder.Logging
    .ClearProviders()
    .AddJsonConsole();

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

// Configure JWT Authentication
var cognitoRegion = "us-east-1";
var userPoolId = "us-east-1_sPFxcSTJa"; 
var issuer = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{userPoolId}";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,  
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        // Log token validation failures
        // options.Events = new JwtBearerEvents
        // {
        //     OnAuthenticationFailed = context =>
        //     {
        //         Console.WriteLine($"Authentication failed: {context.Exception}");
        //         return Task.CompletedTask;
        //     },
        //     OnTokenValidated = context =>
        //     {
        //         Console.WriteLine($"Token validated: {context.Principal?.Identity?.IsAuthenticated}");
        //         return Task.CompletedTask;
        //     }
        // };
    });

builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();

string region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.USEast2.SystemName;
builder.Services
    .AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region)))
    .AddScoped<IDynamoDBContext, DynamoDBContext>();

// Add AWS Lambda support. When running the application as an AWS Serverless application, Kestrel is replaced
// with a Lambda function contained in the Amazon.Lambda.AspNetCoreServer package, which marshals the request into the ASP.NET Core hosting framework.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173", // Local development
                "http://localhost:5000",
                "https://dayplanner.smaartit.com"
            )
            .AllowAnyHeader() // Allow any headers (e.g., Authorization)
            .AllowAnyMethod(); // Allow HTTP methods (GET, POST, PUT, DELETE, etc.)
    });
});

var app = builder.Build();

// Enable Swagger only in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DayPlanner API V1"));
}
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Welcome to DayPlanner API v1");


app.Run();