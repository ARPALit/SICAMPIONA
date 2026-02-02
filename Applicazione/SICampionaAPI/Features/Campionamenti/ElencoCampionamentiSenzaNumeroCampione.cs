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
using System.Text.Json.Serialization;


namespace SICampionaAPI.Features.Campionamenti
{
    public class ElencoCampionamentiSenzaNumeroCampione(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti)
    {
        public class Result(List<CampionamentoExport> campionamenti)
        {
            public List<CampionamentoExport> Campionamenti { get; } = campionamenti;
        }

        public ResultDto<Result> Run()
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                var campionamenti = repositoryCampionamenti.ElencoCampionamentiSenzaNumeroCampione();

                // Sono restituiti solo i campionamenti che hanno il file PDF del verbale 
                // che è convertito Base64 e inserito nel campo PDFVerbale
                var campionamentiExport = new List<CampionamentoExport>();

                foreach (var campionamento in campionamenti)
                {
                    var campionamentoExport = new CampionamentoExport(campionamento);
                    // Aggiunta PDF verbale
                    string percorsoFileVerbale = Path.Combine(AppSettingsHelper.CartellaCampionamenti(), campionamento.CartellaFile, campionamento.FileVerbale);
                    if (File.Exists(percorsoFileVerbale))
                    {
                        byte[] fileBytes = File.ReadAllBytes(percorsoFileVerbale);
                        campionamentoExport.PDFVerbale = Convert.ToBase64String(fileBytes);
                    }
                    else
                    {
                        logger.LogWarning("Campionamento {IdCampionamento} - File verbale non trovato: {PercorsoFileVerbale} - Campionamento non aggiunto ai risultati",
                            campionamento.IdCampionamento, percorsoFileVerbale);
                        continue;
                    }

                    // Se è presente il punto di prelievo ARPAL ma la denominazione è vuota
                    // viene impostata a "Campione derivante da SICampiona"
                    if (campionamentoExport.PuntoDiPrelievoARPAL != null && string.IsNullOrWhiteSpace(campionamentoExport.PuntoDiPrelievoARPAL.Denominazione))
                        campionamentoExport.PuntoDiPrelievoARPAL.Denominazione = "Campione derivante da SICampiona";

                    campionamentiExport.Add(campionamentoExport);
                }

                return new ResultDto<Result>(new Result(campionamentiExport));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore Elenco campionamenti", errorDetails, ErrorType.Application));
            }
        }
    }

    public class CampionamentoExport : Campionamento
    {
        [JsonPropertyOrder(100)]
        public string PDFVerbale { get; set; }
        [JsonPropertyOrder(101)]
        public string PDFRapportoDiProva { get; set; }
        public CampionamentoExport(Campionamento campionamento)
        {
            foreach (PropertyInfo propertyInfo in campionamento.GetType().GetProperties())
            {
                if (!propertyInfo.CanWrite)
                    continue;

                // Sono restituiti solo gli analiti non rimossi dall'operatore
                if (propertyInfo.Name.Equals("Analiti"))
                {
                    // Gli analiti sono ordinati per codice pacchetto e ordine
                    var analitiNonRimossiDaOperatore = campionamento.Analiti
                        .Where(a => !a.RimossoDaOperatore)
                        .OrderBy(a => a.CodicePacchetto)
                        .ThenBy(a => a.Ordinamento)
                        .ToList();
                    propertyInfo.SetValue(this, analitiNonRimossiDaOperatore, null);
                    continue;
                }

                propertyInfo.SetValue(this, propertyInfo.GetValue(campionamento, null), null);
            }
        }
    }
}
