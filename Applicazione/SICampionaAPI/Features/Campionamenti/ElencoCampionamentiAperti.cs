using SICampionaAPI.Common;
using SICampionaAPI.Data;
using SICampionaAPI.Features.Operatore;
using SICampionaAPI.Services;
using System.Reflection;

namespace SICampionaAPI.Features.Campionamenti
{
    public class ElencoCampionamentiAperti(ILogger<OperatoriController> logger, IRepositoryCampionamenti repositoryCampionamenti, IAnagrafiche anagrafiche)
    {
        public class Query(string codiceFiscaleOperatore, string urlBase)
        {
            public string CodiceFiscaleOperatore { get; } = codiceFiscaleOperatore;
            public string UrlBase { get; } = urlBase;
        }

        public class Result(List<Campionamento> campionamenti)
        {
            public List<Campionamento> Campionamenti { get; } = campionamenti;
        }

        public ResultDto<Result> Run(Query query)
        {
            try
            {
                logger.LogDebug("{Metodo}", MethodBase.GetCurrentMethod().Name);

                // Lettura configurazione operatore
                var caricaConfigurazione = new CaricaConfigurazione(logger, anagrafiche);
                var resultConfigurazioneOperatore = caricaConfigurazione.Run(new CaricaConfigurazione.Query(query.CodiceFiscaleOperatore));
                if (resultConfigurazioneOperatore.Failed)
                    return new ResultDto<Result>(new Error($"Errore elenco campionamenti: {resultConfigurazioneOperatore.Error.Message}", resultConfigurazioneOperatore.Error.Details));

                // elenco codici degli enti dell'operatore
                var codiciEnte = resultConfigurazioneOperatore.Data.Configurazione.Enti.Select(i => i.Codice).ToList();

                var campionamenti = repositoryCampionamenti.ElencoCampionamentiAperti(codiciEnte);
                // Aggiunta del percorso dei verbali
                foreach (var campionamento in campionamenti)
                {
                    var urlRadiceFileCampionamento = Utils.UrlRadiceCampionamenti(query.UrlBase) + campionamento.CartellaFile;
                    if (!string.IsNullOrEmpty(campionamento.FileVerbale))
                    {
                        campionamento.UrlPDFVerbale = $"{urlRadiceFileCampionamento}/{campionamento.FileVerbale}";
                    }
                    if (!string.IsNullOrEmpty(campionamento.FileVerbaleFirmato))
                    {
                        campionamento.UrlPDFVerbaleFirmato = $"{urlRadiceFileCampionamento}/{campionamento.FileVerbaleFirmato}";
                    }
                }

                return new ResultDto<Result>(new Result(campionamenti));
            }
            catch (Exception ex)
            {
                string errorDetails = Utils.GetExceptionDetails(ex);
                return new ResultDto<Result>(new Error("Errore elenco campionamenti", errorDetails, ErrorType.Application));
            }
        }
    }
}
