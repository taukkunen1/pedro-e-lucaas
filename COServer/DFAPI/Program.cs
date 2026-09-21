using API;
using API.Models;

// Remove before run the api things
Utils.RemoveLockedConfigCreator();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.IncludeXmlComments(Utils.XmlCommentsFilePath);
});
builder.Services.AddSingleton<JsonAuthStore>();
builder.Services.AddDbContext<GameDbContext>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else
{
    //app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
