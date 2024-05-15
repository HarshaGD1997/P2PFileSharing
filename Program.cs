using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P2PFileSharing.Data;
using P2PFileSharing.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5117); // Listen on port 5117 for any IP address
});

// Add DbContext configuration with retry on failure
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // To serve static files (e.g., downloaded files)

// Endpoint to get all files
app.MapGet("/api/files", async (ApplicationDbContext dbContext) =>
{
    return await dbContext.Files.ToListAsync();
})
.WithName("GetFiles");

// Endpoint to upload a file
app.MapPost("/api/files", async (HttpRequest request, ApplicationDbContext dbContext) =>
{
    if (!request.Form.Files.Any())
    {
        return Results.BadRequest("No file uploaded.");
    }

    var file = request.Form.Files[0];
    var filePath = Path.Combine("Uploads", file.FileName);

    if (!Directory.Exists("Uploads"))
    {
        Directory.CreateDirectory("Uploads");
    }

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    var fileMetadata = new FileMetadata
    {
        FileName = file.FileName,
        FilePath = filePath,
        FileSize = file.Length,
        UploadedAt = DateTime.Now
    };

    dbContext.Files.Add(fileMetadata);
    await dbContext.SaveChangesAsync();

    return Results.Ok(fileMetadata);
})
.WithName("UploadFile");

// Endpoint to download a file by ID...
app.MapGet("/api/files/{id}", async (int id, ApplicationDbContext dbContext) =>
{
    var fileMetadata = await dbContext.Files.FindAsync(id);
    if (fileMetadata == null)
    {
        return Results.NotFound();
    }

    var filePath = fileMetadata.FilePath;
    var fileBytes = await File.ReadAllBytesAsync(filePath);
    return Results.File(fileBytes, "application/octet-stream", fileMetadata.FileName);
})
.WithName("DownloadFile");

app.Run();