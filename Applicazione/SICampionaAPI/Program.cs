/*
 * Nome del progetto: SICampiona
 * Copyright (C) 2025 Agenzia regionale per la protezione dell'ambiente ligure
 *
 * Questo programma è software libero: puoi ridistribuirlo e/o modificarlo
 * secondo i termini della GNU Affero General Public License pubblicata dalla
 * Free Software Foundation, sia la versione 3 della licenza, sia (a tua scelta)
 * qualsiasi versione successiva.
 *
 * Questo programma è distribuito nella speranza che possa essere utile,
 * ma SENZA ALCUNA GARANZIA; senza nemmeno la garanzia implicita di
 * COMMERCIABILITÀ o IDONEITÀ PER UNO SCOPO PARTICOLARE. Vedi la
 * GNU Affero General Public License per ulteriori dettagli.
 *
 * Dovresti aver ricevuto una copia della GNU Affero General Public License
 * insieme a questo programma. In caso contrario, vedi <https://www.gnu.org/licenses/>.
*/

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using SICampionaAPI.Common;
using SICampionaAPI.Data;
using SICampionaAPI.Services;
using SICampionaAPI.Services.AnagraficheAWS;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net;
using System.Reflection;

namespace SICampionaAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var logger = LogManager.Setup()
                .LoadConfigurationFromAppSettings()
                .GetCurrentClassLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Utilizzo di NLog come logger
                builder.Logging.ClearProviders();
                builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace); // Sovrascritto dal valore in appsettings
                builder.Host.UseNLog();

                logger.Info($"Init program - Version: {Utils.Version()} - Hostname: {System.Net.Dns.GetHostName()}");

                // CORS: tutte le origini
                builder.Services.AddCors(options =>
                options.AddDefaultPolicy(p => p
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()));

                // Aggiunta al controller policy sui nomi del json
                builder.Services.AddControllers().AddJsonOptions(options =>
                            options.JsonSerializerOptions.PropertyNamingPolicy = null);

                // Deserializzazione string in Enum
                builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));


                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    // Descrizione API basata sui commenti generata indicando nome e percorso del file di documentazione
                    // Richiede <GenerateDocumentationFile>true</GenerateDocumentationFile> in PropertyGroup
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

                    // Autenticazione
                    var securitySchema = new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\". Insert token value (without Bearer prefix)",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    };
                    options.AddSecurityDefinition("Bearer", securitySchema);
                    var securityRequirement = new OpenApiSecurityRequirement
                    {
                        { securitySchema, new[] { "Bearer" } }
                    };
                    options.AddSecurityRequirement(securityRequirement);
                });

                // Client http specifico per le anagrafiche
                builder.Services.AddHttpClient("APIAnagrafiche", httpClient =>
                httpClient.BaseAddress = new Uri(builder.Configuration["APIAnagraficheBaseUrl"]));

                // Client http specifico per l'identity server
                builder.Services.AddHttpClient("IdentityServer");

                builder.Services.AddHttpContextAccessor();

                builder.Services.AddScoped<IRepositoryCampionamenti, RepositoryCampionamenti>();

                builder.Services.AddScoped<IAnagrafiche, Anagrafiche>();

                var app = builder.Build();

                //Abilitazione swagger
                if (bool.Parse(builder.Configuration["SwaggerEnabled"]!))
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.DocExpansion(DocExpansion.None);
                    });
                }
                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.UseExceptionHandler(appError =>
                {
                    appError.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";
                        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (contextFeature != null)
                        {
                            logger.Error(contextFeature.Error);
                            await context.Response.WriteAsync(new ErrorDetails()
                            {
                                StatusCode = context.Response.StatusCode,
                                Message = $"Internal Server Error: {contextFeature.Error.Message}",
                                Path = contextFeature.Path
                            }.ToString());
                        }
                    });
                });

                app.UseCors();

                app.MapControllers();

                // Configurazione classe di supporto alla lettura delle impostazioni di configurazioni
                // da classi statiche
                AppSettingsHelper.AppSettingsConfigure(app.Services.GetRequiredService<IConfiguration>(), app.Services.GetRequiredService<IWebHostEnvironment>());

                // Middleware per autenticazione operatore
                app.AutenticazioneMiddleware();

                // Configurazione cartella contenente i file da servire
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(AppSettingsHelper.CartellaRadiceApplicazione(), "File")),
                    RequestPath = new PathString("/File")
                });

                app.Run();
            }
            catch (Exception exception)
            {
                logger.Error(Utils.GetExceptionDetails(exception), "Esecuzione interrotta per una eccezione");
                Environment.Exit(-1);
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}
