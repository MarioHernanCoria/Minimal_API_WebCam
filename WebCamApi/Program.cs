using Serilog;
using Emgu.CV;
    
VideoCapture captureStream = null;
DateTime? lastCapture = null;

Task Warn()
{
    return Task.Run(() =>
    {
        Log.Information("Inicializando");
        captureStream = new VideoCapture(0);
        captureStream.Start();
        Log.Information("Captura Iniciada");
    });
}
 
async Task<byte[]> GetFrame(bool small = false)
{
    Log.Information("Captura requerida");
    var frame = small? captureStream.QuerySmallFrame() : captureStream.QueryFrame();
    Log.Debug("Frame Capture");
    lastCapture = DateTime.Now;
    var tempFile = "temp.jpg";
    frame.Save(tempFile);
    Log.Debug("Archivo temporal guardado");
    var content = await File.ReadAllBytesAsync(tempFile); 
    File.Delete(tempFile);
    Log.Debug("Archivo temporal eliminado");
    return content;
}


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog(Log.Logger);

var app = builder.Build();
app.UseSerilogRequestLogging();

_ = Warn();

app.Map("/", () => new
{
    LastCapture = lastCapture
});

app.Map("/Frame", async () =>
{
    var content = await GetFrame();
    return Results.File(content, "image/jpeg");
});

app.Map("/framesmall", async () =>
{
    var content = await GetFrame(true);
    return Results.File(content, "image/jpeg");

});
app.Run();
      
