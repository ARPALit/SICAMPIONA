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
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;
using SICampionaBG.Data;
using SICampionaBG.Features.CreazionePDFVerbale;
using SICampionaBG.Model;
using System.Reflection;
using System.Security.Authentication;

namespace SICampionaBG.Features.EmailVerbale
{
    internal class EmailVerbale(Configurazione configurazione, IRepositoryCampionamenti repositoryCampionamenti)
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal void Invio()
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name}");

            if (!configurazione.InvioEmailVerbaleAbilitato)
            {
                logger.Info("Invio email verbale disabilitato");
                return;
            }

            var campionamenti = repositoryCampionamenti.CampionamentiConVerbaleDaInviarePerEmail();
            if (campionamenti.Count == 0)
            {
                logger.Info("Nessun campionamento da elaborare");
                return;
            }
            foreach (var campionamento in campionamenti)
            {
                // Percorso file PDF verbale
                var filePDFVerbale = Path.Combine(configurazione.CartellaRadiceCampionamenti,
                    $"{campionamento.IdCampionamento}_{campionamento.SuffissoCartella}",
                    PdfVerbale.NomeFileVerbale(campionamento));

                // Verifica esistenza file PDF verbale
                if (!File.Exists(filePDFVerbale))
                {
                    logger.Error($"Campionamento {campionamento.IdCampionamento}: file PDF verbale {filePDFVerbale} non trovato");
                    continue;
                }

                // Titolo email
                var titoloEmail = $"Verbale campionamento {campionamento.SiglaVerbale}";

                // Corpo email
                var corpoEmail = TestoEmail(campionamento, configurazione);

                // Scomposizione elenco indirizzi email
                var indirizziEmail = campionamento.EmailInvioVerbale.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

                // L'invio è effettuato singolarmente per ogni indirizzo email
                // e registrato come fallito se almeno una degli invii fallisce
                bool erroreInvio = false;
                foreach (var indirizzoEmail in indirizziEmail)
                {
                    logger.Debug($"Invio email verbale a {indirizzoEmail}");
                    erroreInvio = !InvioSingolaEmail(configurazione, titoloEmail, corpoEmail, indirizzoEmail, filePDFVerbale);
                }

                // Aggiorna stato invio email
                if (erroreInvio)
                    repositoryCampionamenti.AggiornaStatoEmailInvioVerbale(campionamento.IdCampionamento, configurazione.StatoEmailInvioVerbale_Errore);
                else
                    repositoryCampionamenti.AggiornaStatoEmailInvioVerbale(campionamento.IdCampionamento, configurazione.StatoEmailInvioVerbale_Inviata);
            }

            logger.Debug("Invio email verbale completato");
        }

        private string TestoEmail(Campionamento campionamento, Configurazione configurazione)
        {
            logger.Debug($"{MethodBase.GetCurrentMethod().Name} - IdCampionamento: {campionamento.IdCampionamento}");

            var templateEmail = File.ReadAllText(configurazione.TemplateEmail);

            PuntoDiPrelievo puntoDiPrelievo;
            if (campionamento.PuntoDiPrelievoInseritoDaOperatore == null)
                puntoDiPrelievo = campionamento.PuntoDiPrelievoARPAL;
            else
                puntoDiPrelievo = campionamento.PuntoDiPrelievoInseritoDaOperatore;

            return templateEmail.Replace("[$SiglaVerbale$]", campionamento.SiglaVerbale)
            .Replace("[$DataCampioamento$]", campionamento.DataPrelievo.ToString("dd/MM/yyyy"))
            .Replace("[$OraCampioamento$]", campionamento.OraPrelievo.Value.ToString("HH:mm"))
            .Replace("[$CodicePuntoPrelievo$]", puntoDiPrelievo.Codice)
            .Replace("[$DenominazionePuntoPrelievo$]", puntoDiPrelievo.Denominazione)
            .Replace("[$IndirizzoPuntoPrelievo$]", puntoDiPrelievo.Indirizzo)
            .Replace("[$ComunePuntoPrelievo$]", puntoDiPrelievo.Comune.Denominazione)
            .Replace("[$Cliente$]", campionamento.Cliente.Denominazione)
            .Replace("[$Ente$]", campionamento.Ente.Denominazione);
        }

        private bool InvioSingolaEmail(Configurazione configurazione, string titoloEmail, string corpoEmail, string indirizzoEmail, string filePDFVerbale)
        {
            logger.Info($"Call {MethodBase.GetCurrentMethod().Name} - IndirizzoEmail: {indirizzoEmail}");

            try
            {
                logger.Debug("Creazione messaggio MIME...");
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse(configurazione.EmailSender));

                // Destinatari con gestione dell'override dell'email di destinazione
                string destinationEmailOverride = configurazione.DestinationEmailOverride;
                if (!string.IsNullOrEmpty(destinationEmailOverride))
                {
                    logger.Warn($"{MethodBase.GetCurrentMethod().Name} - Destination email override ({destinationEmailOverride})");
                    mimeMessage.To.Add(MailboxAddress.Parse(destinationEmailOverride));
                }
                else
                    mimeMessage.To.Add(MailboxAddress.Parse(indirizzoEmail));

                mimeMessage.Subject = titoloEmail;

                var builder = new BodyBuilder
                {
                    HtmlBody = corpoEmail
                };

                // Allegato PDF dell'IF
                builder.Attachments.Add(filePDFVerbale);

                mimeMessage.Body = builder.ToMessageBody();

                // Invio
                logger.Debug("Invio messaggio all'SMTP server...");
                using (SmtpClient client = new())
                {
                    client.SslProtocols = SslProtocols.None;
                    SecureSocketOptions secureSocketOptions = SecureSocketOptions.Auto;
                    if (configurazione.SMTPServerEnableSSL == true)
                        secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    client.Connect(configurazione.SMTPServerName, configurazione.SMTPServerPort.Value, secureSocketOptions);
                    // Usa l'autenticazione solo se la password è presente
                    if (!string.IsNullOrEmpty(configurazione.SMTPServerPassword))
                        client.Authenticate(configurazione.SMTPServerUsername, configurazione.SMTPServerPassword);
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }

                logger.Info($"{MethodBase.GetCurrentMethod().Name} - IndirizzoEmail: {indirizzoEmail} - Messaggio inviato");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"{MethodBase.GetCurrentMethod().Name} - IndirizzoEmail: {indirizzoEmail} - Errore: {ex.Message}");
                return false;
            }
        }
    }
}
