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

namespace SICampionaAPI.Common
{
    /// <summary>
    /// Classe di supporto utilizzata per accedere alle impostazioni
    /// di configurazione e dell'ambiente
    /// </summary>
    public static class AppSettingsHelper
    {
        private static IConfiguration _config;
        private static IWebHostEnvironment _environment;

        public static void AppSettingsConfigure(IConfiguration config, IWebHostEnvironment environment)
        {
            // E' usato per iniettare la configurazione a runtime in Program.cs con il comando:
            // AppSettingsHelper.AppsettingsConfigure(app.Services.GetRequiredService<IConfiguration>())
            _config = config;

            _environment = environment;
        }

        /// <summary>
        /// Lettura impostazione di configurazione
        /// </summary>
        /// <param name="key">Chiave di configurazione, anche gerarchica es. SMTPSettings:host</param>
        /// <returns></returns>
        public static string Setting(string key)
        {
            return _config!.GetSection(key).Value;
        }

        public static string CartellaRadiceApplicazione()
        {
            return _environment!.ContentRootPath;
        }

        public static string CartellaCampionamenti()
        {
            return Path.Combine(CartellaRadiceApplicazione(), "File", "Campionamenti");
        }
    }
}
