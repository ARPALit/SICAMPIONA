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
using System.Configuration;

namespace SICampionaBG
{
    internal class Configurazione
    {
        public string StringaConnessioneDB { get; set; }
        public string CartellaRadiceCampionamenti { get; set; }
        public bool CreazionePDFVerbaleAbilitata { get; set; } = false;
        public bool LetturaNumeroCampioneAbilitata { get; set; } = false;
        public bool LetturaStatoCampioneAbilitata { get; set; } = false;
        public bool LetturaRapportoDiProvaAbilitata { get; set; } = false;
        public bool LetturaTemperaturaAccettazioneAbilitata { get; set; } = false;
        public bool InvioEmailVerbaleAbilitato { get; set; } = false;
        public string APICampioniBaseUrl { get; set; }
        public string APICampionamentiAPIKey { get; set; }
        public string APICampioniNumeroCampione { get; set; }
        public string APICampioniStatoCampione { get; set; }
        public string APICampioniRapportoDiProva { get; set; }
        public string EmailSender { get; set; }
        public string SMTPServerName { get; set; }
        public int? SMTPServerPort { get; set; }
        public bool? SMTPServerEnableSSL { get; set; }
        public string SMTPServerUsername { get; set; }
        public string SMTPServerPassword { get; set; }
        public string DestinationEmailOverride { get; set; }

        /// <summary>
        /// Codici stato invio verbale utilizzati nel DB
        /// </summary>
        public readonly string StatoEmailInvioVerbale_DaInviare = "DA INVIARE";
        public readonly string StatoEmailInvioVerbale_Inviata = "INVIATA";
        public readonly string StatoEmailInvioVerbale_Errore = "ERRORE";

        // Percorso template email
        public string TemplateEmail
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, "Features", "EmailVerbale", "TemplateEmail.txt");
            }
        }

        public void Valida()
        {
            if (string.IsNullOrEmpty(StringaConnessioneDB))
                throw new ConfigurationErrorsException("StringaConnessioneDB non presente nella configurazione");

            // Verifica esistenza configurazione cartella radice campionamenti
            if (string.IsNullOrEmpty(CartellaRadiceCampionamenti))
                throw new ConfigurationErrorsException("CartellaRadiceCampionamenti non presente nella configurazione");
            // Verifica raggiungibilità cartella radice campionamenti
            if (!System.IO.Directory.Exists(CartellaRadiceCampionamenti))
                throw new ConfigurationErrorsException($"CartellaRadiceCampionamenti '{CartellaRadiceCampionamenti}' non raggiungibile");

            // Verifica esistenza configurazione API Campioni e endpoint
            if (string.IsNullOrEmpty(APICampioniBaseUrl))
                throw new ConfigurationErrorsException("APICampioniBaseUrl non presente nella configurazione");

            if (string.IsNullOrEmpty(APICampionamentiAPIKey))
                throw new ConfigurationErrorsException("APICampionamentiAPIKey non presente nella configurazione");

            if (string.IsNullOrEmpty(APICampioniNumeroCampione))
                throw new ConfigurationErrorsException("APICampioniNumeroCampione non presente nella configurazione");

            if (string.IsNullOrEmpty(APICampioniStatoCampione))
                throw new ConfigurationErrorsException("APICampioniStatoCampione non presente nella configurazione");

            if (string.IsNullOrEmpty(APICampioniRapportoDiProva))
                throw new ConfigurationErrorsException("APICampioniRapportoDiProva non presente nella configurazione");

            // Verifica presenza configurazione email
            if (string.IsNullOrEmpty(EmailSender))
                throw new ConfigurationErrorsException("EmailSender non presente nella configurazione");

            if (string.IsNullOrEmpty(SMTPServerName))
                throw new ConfigurationErrorsException("SMTPServerName non presente nella configurazione");

            if (!SMTPServerPort.HasValue)
                throw new ConfigurationErrorsException("SMTPServerPort non presente nella configurazione");

            if (!SMTPServerEnableSSL.HasValue)
                throw new ConfigurationErrorsException("SMTPServerEnableSSL non presente nella configurazione");

            if (string.IsNullOrEmpty(SMTPServerUsername))
                throw new ConfigurationErrorsException("SMTPServerUsername non presente nella configurazione");

            if (string.IsNullOrEmpty(SMTPServerPassword))
                throw new ConfigurationErrorsException("SMTPServerPassword non presente nella configurazione");
        }
    }
}
