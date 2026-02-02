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

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using SICampiona.Model;
using SICampiona.Services;


namespace SICampiona
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");
			builder.RootComponents.Add<HeadOutlet>("head::after");

			builder.Services.AddFluentUIComponents();

			builder.Services.AddBlazoredLocalStorage();
			
			builder.Configuration.AddJsonFile("appsettings.json");

			// Configurazione logging da appsettings
			builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

			// Configurazione client http per risorse statiche
			builder.Services.AddHttpClient("RisorseLocali", httpClient =>
			httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

			// Configurazione client http specifico per le API SICampiona
            builder.Services.AddHttpClient("APISICampiona", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["APISICampionaBaseUrl"]);
            });

			// Archivio locale campionamento
			builder.Services.AddScoped<ICampionamentiStorage, CampionamentiStorage>();

			// Info operatore
			builder.Services.AddScoped<IConfigurazioneOperatoreService, ConfigurazioneOperatoreService>();

			// Anagrafiche
			builder.Services.AddScoped<IAnagrafiche, Anagrafiche>();

			// Archivio campionamenti
			builder.Services.AddScoped<IArchivio, Archivio>();

            // Autenticazione OIDC
            string oidcAuthority = builder.Configuration["OidcAuthority"];
            string oidcClientId = builder.Configuration["OidcClientId"];            
            string oidcRedirectUri = builder.Configuration["OidcRedirectUri"];

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.Authority = oidcAuthority;
                options.ProviderOptions.ClientId = oidcClientId;
                options.ProviderOptions.MetadataUrl = "custom-is-metadata-regione.json";
                options.ProviderOptions.RedirectUri = oidcRedirectUri;

                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.ResponseMode = "query";

                options.ProviderOptions.DefaultScopes.Add("offline_access"); // Richiede il refresh token
            });

            await builder.Build().RunAsync();
		}
	}
}
