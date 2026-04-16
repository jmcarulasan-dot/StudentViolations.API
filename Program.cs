using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentViolations.API.Class;
using StudentViolations.API.IRepository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS — allows Flutter, Blazor, PHP to call the API from any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentViolations.API", Version = "v1" });

    // Display groups in system flow order
    c.OrderActionsBy(api => api.GroupName switch
    {
        "Authentication" => "1",
        "Guard" => "2",
        "Guidance" => "3",
        "Student" => "4",
        "Admin" => "5",
        "Notifications" => "6",
        _ => "7"
    });

    c.DocInclusionPredicate((docName, apiDesc) => true);
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });

    // Bearer token input in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token only"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Dependency Injection — connects each interface to its implementation
builder.Services.AddScoped<ILoginRepository, LoginClass>();
builder.Services.AddScoped<IRegisterRepository, RegisterClass>();
builder.Services.AddScoped<IStudentRepository, StudentClass>();
builder.Services.AddScoped<IViolationRepository, ViolationClass>();
builder.Services.AddScoped<IGuardRepository, GuardClass>();
builder.Services.AddScoped<ISAORepository, SAOClass>();
builder.Services.AddScoped<INotificationRepository, NotificationClass>();

var app = builder.Build();

// Middleware pipeline — order matters
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();