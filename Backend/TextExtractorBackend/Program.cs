using BackendLibrary;
using SQL3cs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<ShopTicketService>();
builder.Services.AddSingleton<SQL3cs.CustomerData>();

// Load detection models
builder.Services.AddSingleton<DetectModelService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DetectModelService>>();
    var formViewModelPath = "Services/Models/form_best.onnx";
    var sectionViewModelPath = "Services/Models/section_best.onnx";
    return new DetectModelService(logger, formViewModelPath, sectionViewModelPath);
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "hh:mm:ss.fff ";
    options.SingleLine = true;
});

// Enable CORS for frontend communication
var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
var corsOrigins = new[] { frontendUrl, "http://localhost:3000", "https://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Ensure SQLite tables exist at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SQL3cs.CustomerData>();
    db.CreateTables();
    var stService = scope.ServiceProvider.GetRequiredService<ShopTicketService>();
    var loggerFactory2 = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var existing = db.LoadTickets(loggerFactory2);
    if (existing.Count > 0)
    {
        stService.History = existing.Cast<ShopTicket?>().ToList();
    }
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Health check endpoint
app.MapHealthChecks("/health");

// API Endpoints
app.MapGet("/api/export/{val}/{index}", (String val, int index, ShopTicketService sTService) =>
{
    var ticket = sTService.GetHistoryTicket(index + 1);
    if (ticket == null)
    {
        return Results.NotFound();
    }

    if (val.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        var json = ticket.ToJson();
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(json),
            "application/json",
            $"{Path.GetFileNameWithoutExtension(ticket.FileName)}_info.json"
        );
    }
    else
    {
        var csv = ticket.ToCsv();
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv",
            $"{Path.GetFileNameWithoutExtension(ticket.FileName)}_info.csv"
        );
    }
});

app.MapGet("/api/export/batch/{val}", (bool val, string? ids, ShopTicketService sTService) =>
{
    if (string.IsNullOrWhiteSpace(ids)) return Results.BadRequest("ids query missing");
    var parts = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var tickets = parts.Select(p => {
        if (int.TryParse(p, out var idx))
        {
            try { return sTService.GetHistoryTicket(idx + 1); } catch { return null; }
        }
        return null;
    }).Where(t => t != null).ToList();

    if (!tickets.Any()) return Results.NotFound();

    if (val)
    {
        var jsonParts = tickets.Select(t => t!.ToJson());
        var combined = "[" + string.Join(",", jsonParts) + "]";
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(combined),
            "application/json",
            $"batch_export_{DateTime.UtcNow:yyyyMMddHHmmss}.json"
        );
    }
    else
    {
        var csvLines = new List<string>();
        var firstCsv = tickets[0]!.ToCsv().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var header = firstCsv[0];
        csvLines.Add(header);
        foreach (var t in tickets)
        {
            var lines = t!.ToCsv().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1) csvLines.Add(lines[1]);
        }
        var combined = string.Join("\n", csvLines);
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(combined),
            "text/csv",
            $"batch_export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        );
    }
});

// Preload packages to reduce first-use latency
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var preloadLogger = loggerFactory.CreateLogger<PreloadService>();
PreloadService.PreloadPackages(preloadLogger);

// Get processing history
app.MapGet("/api/history", (ShopTicketService sTService) =>
{
    var history = sTService.History ?? new List<ShopTicket?>();
    var response = history
        .Where(t => t != null)
        .Select((t, index) => new
        {
            id = index + 1,
            fileName = t!.FileName,
            processedDate = t!.dateTimeExtracted,
            status = "Completed",
            projectNumber = t.ProjectNumber,
            projectName = t.ProjectName
        })
        .ToList();
    return Results.Ok(response);
});

// Get specific ticket
app.MapGet("/api/ticket/{id}", (int id, ShopTicketService sTService) =>
{
    var ticket = sTService.GetHistoryTicket(id);
    if (ticket == null)
        return Results.NotFound();

    return Results.Json(new
    {
        id = id,
        fileName = ticket.FileName,
        projectNumber = ticket.ProjectNumber,
        projectName = ticket.ProjectName,
        designNumber = ticket.DesignNumber,
        piecesRequired = ticket.PiecesRequired,
        weight = ticket.Weight,
        controlNumbers = ticket.ControlNumbers,
        processedDate = ticket.dateTimeExtracted,
        numberOfPages = ticket.NumberOfPages,
        pageNames = ticket.PageNames
    });
});

// Upload and process PDF
app.MapPost("/api/upload", async (IFormFile file, ShopTicketService sTService, DetectModelService dMService, ILoggerFactory loggerFactory) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("No file provided");

    try
    {
        // Read file into memory
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        // Process PDF using backend services
        var ticket = new ShopTicket(loggerFactory, file.FileName, fileBytes, dMService);
        
        // Add to history
        sTService.History.Add(ticket);

        return Results.Ok(new
        {
            success = true,
            ticketId = sTService.History.Count,
            fileName = file.FileName,
            projectNumber = ticket.ProjectNumber,
            projectName = ticket.ProjectName,
            message = "File processed successfully"
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, message = ex.Message });
    }
});

app.Run();
