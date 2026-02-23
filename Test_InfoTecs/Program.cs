using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Test_InfoTecs.Models;
using Test_InfoTecs.Models.DB;
using Test_InfoTecs.Services;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IValidator<ValueRecord>, ValueRecordValidator>();
builder.Services.AddScoped<IValidator<ResultsFilter>, ResultsFilterValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<ITimescaleService, TimescaleService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();          
    app.UseSwaggerUI();       
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();