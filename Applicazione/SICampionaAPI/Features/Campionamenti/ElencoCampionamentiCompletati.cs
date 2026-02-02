using SICampionaAPI.Common;
using SICampionaAPI.Data;
using SICampionaAPI.Features.Operatore;
using System.Reflection;

namespace SICampionaAPI.Features.Campionamenti
{
    public class ElencoCampionamentiCompletati(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti)
    {
        public class Query(string codiceFiscaleOperatore, RichiestaCampionamentiCompletati richiesta, string urlBase)
        {
            public string CodiceFiscaleOperatore { get; } = codiceFiscaleOperatore;
            public RichiestaCampionamentiCompletati Richiesta { get; } = richiesta;
            public string UrlBase { get; } = urlBase;
        }

        public class Result(List<InfoCampionamentoCompletato> campionamenti)
        {
            public List<InfoCampionamentoCompletato> Campionamenti { get; } = campionamenti;
        }

        public ResultDto<Result> Run(Query query)
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                var campionamenti = repositoryCampionamenti.ElencoCampionamentiCompletati(query.Richiesta);
                // Aggiunta URL file verbale e rapporto di prova
                foreach (var campionamento in campionamenti)
                {
                    var urlRadiceFileCampionamento = Utils.UrlRadiceCampionamenti(query.UrlBase) + campionamento.CartellaFile;

                    if (!string.IsNullOrEmpty(campionamento.FileVerbale))
                        campionamento.UrlPDFVerbale = $"{urlRadiceFileCampionamento}/{campionamento.FileVerbale}";

                    if (!string.IsNullOrEmpty(campionamento.FileRapportoDiProva))
                        campionamento.UrlPDFRapportoDiProva = $"{urlRadiceFileCampionamento}/{campionamento.FileRapportoDiProva}";

                    if (!string.IsNullOrEmpty(campionamento.FileVerbaleFirmato))
                        campionamento.UrlPDFVerbaleFirmato = $"{urlRadiceFileCampionamento}/{campionamento.FileVerbaleFirmato}";
                }

                return new ResultDto<Result>(new Result(campionamenti));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore Elenco campionamenti completati", errorDetails, ErrorType.Application));
            }
        }

        public class RichiestaCampionamentiCompletati
        {
            public string CodiceEnte { get; set; }
            public DateOnly? DataCampionamento { get; set; }
            public string NumeroCampione { get; set; }
            public string SiglaVerbale { get; set; }
            public string CodiceCliente { get; set; }
            public int? NumeroMassimoRisultati { get; set; }
        }

        public class InfoCampionamentoCompletato
        {
            public int IdCampionamento { get; set; }
            public string CodiceEnte { get; set; }
            public string DenominazioneEnte { get; set; }
            public string CodiceMatrice { get; set; }
            public string DenominazioneMatrice { get; set; }
            public string DenominazioneArgomento { get; set; }
            public string CodiceCliente { get; set; }
            public string DenominazioneCliente { get; set; }
            public DateOnly DataPrelievo { get; set; }
            public TimeOnly? OraPrelievo { get; set; }
            public string SiglaVerbale { get; set; }
            public string NumeroCampione { get; set; }
            public string UrlPDFVerbale { get; set; }
            public string UrlPDFRapportoDiProva { get; set; }
            public string StatoCampione { get; set; }
            public string FileVerbale { get; set; }
            public string FileRapportoDiProva { get; set; }
            public string CartellaFile { get; set; }
            public string FileVerbaleFirmato { get; set; }
            public string UrlPDFVerbaleFirmato { get; set; }
            public string TemperaturaAccettazione { get; set; }
        }
    }
}
