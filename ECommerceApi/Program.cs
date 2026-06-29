using ECommerceApi.Common;
using ECommerceApi.Data;
using ECommerceApi.Repositories.Base;
using ECommerceApi.Repositories.Interfaces;
using ECommerceApi.UnitOfWork;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----- Services -----
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Data access: Repository<T> + UnitOfWork
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Validation: tự nạp validator + auto-validate request
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddFluentValidationAutoValidation();

// Global error handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ----- Pipeline -----
app.UseExceptionHandler();   // phải đứng đầu để bắt mọi lỗi phía dưới

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
