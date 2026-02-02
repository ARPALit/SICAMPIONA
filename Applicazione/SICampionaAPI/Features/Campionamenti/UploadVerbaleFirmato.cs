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

using SICampionaAPI.Common;
using SICampionaAPI.Data;
using SICampionaAPI.Features.Operatore;
using System.Reflection;

namespace SICampionaAPI.Features.Campionamenti
{
    public class UploadVerbaleFirmato(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti)
    {
        public class Command(int IdCampionamento, IFormFile file)
        {
            public IFormFile File { get; } = file;
            public int IdCampionamento { get; } = IdCampionamento;
        }

        public class Result()
        {
        }

        public ResultDto<Result> Run(Command command)
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                // Carica campionamento
                Campionamento campionamento = repositoryCampionamenti.DettagliCampionamento(command.IdCampionamento);

                // Verifica che il verbale (non firmato) esista
                if (string.IsNullOrEmpty(campionamento.FileVerbale))
                {
                    return new ResultDto<Result>(new Error("Verbale non presente per il campionamento con id {command.IdCampionamento} ", null, ErrorType.Application));
                }

                // Verifica che il verbale firmato non esista già
                if (!string.IsNullOrEmpty(campionamento.FileVerbaleFirmato))
                {
                    return new ResultDto<Result>(new Error($"Verbale firmato già presente per il campionamento con id {command.IdCampionamento} ", null, ErrorType.Application));
                }

                // Il file non è salvato col nome originale, ma con un nome generato in base a quello del verbale originale
                string nomeFileFirmato = CreaNomeFileFirmato(campionamento.FileVerbale);

                // Determina il percorso dove salvare il file
                var percorsoFileVerbaleFirmato = Path.Combine(AppSettingsHelper.CartellaCampionamenti(), campionamento.CartellaFile, nomeFileFirmato);

                // Salva il file
                using (var stream = new FileStream(percorsoFileVerbaleFirmato, FileMode.Create))
                {
                    command.File.CopyTo(stream);
                }

                // Aggiorna il campionamento con il nome del file firmato
                repositoryCampionamenti.SalvaNomeFileVerbaleFirmato(command.IdCampionamento, nomeFileFirmato);

                return new ResultDto<Result>(new Result());
            }
            catch (ArgumentException ex)
            {
                return new ResultDto<Result>(new Error("Errore upload verbale firmato", ex.Message, ErrorType.Application));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore upload verbale firmato", errorDetails, ErrorType.Application));
            }
        }

        /// <summary>
        /// Crea il nome del file firmato aggiunge "_firmato" prima dell'estensione
        /// </summary>
        /// <param name="nomeFile"></param>
        /// <returns></returns>
        private static string CreaNomeFileFirmato(string nomeFile)
        {
            // Trova l'ultimo punto per identificare l'estensione del file
            int indicePunto = nomeFile.LastIndexOf('.');
            if (indicePunto == -1)
            {
                // Se non c'è un'estensione, aggiungi semplicemente "_firmato"
                return nomeFile + "_firmato";
            }
            else
            {
                // Inserisci "_firmato" prima dell'estensione
                return nomeFile.Substring(0, indicePunto) + "_firmato" + nomeFile.Substring(indicePunto);
            }
        }
    }
}

