using Microsoft.EntityFrameworkCore;
using StudentViolations.API.Class;
using StudentViolations.API.IRepository;
using StudentViolationsAPI.Data;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "StudentViolations.API", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) => true);
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });
});

builder.Services.AddScoped<ILoginRepository, LoginClass>();
builder.Services.AddScoped<IRegisterRepository, RegisterClass>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IViolationRepository, ViolationRepository>();
builder.Services.AddScoped<IGuardRepository, GuardRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();