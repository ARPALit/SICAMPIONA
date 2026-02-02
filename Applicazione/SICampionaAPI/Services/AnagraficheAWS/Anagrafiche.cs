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

using SICampionaAPI.Features.Analiti;
using SICampionaAPI.Features.Campionamenti;
using SICampionaAPI.Features.Clienti;
using SICampionaAPI.Features.Operatore;
using System.ComponentModel.DataAnnotations;

namespace SICampionaAPI.Services.AnagraficheAWS
{
    public class Anagrafiche(ILogger<Anagrafiche> logger, IConfiguration configuration, IHttpClientFactory clientFactory) : IAnagrafiche
    {
        public List<Analita> Analiti([Required] string codiceMatrice, string codiceArgomento = null, string codicePacchetto = null, string parteDelNome = null, string linea = null)
        {
            var anagraficaAnaliti = new AnagraficaAnaliti(logger, configuration, clientFactory);
            return anagraficaAnaliti.Analiti(codiceMatrice, codiceArgomento, codicePacchetto, parteDelNome, linea);
        }

        public List<ClienteConIndirizzo> Clienti(string parteDenominazioneCodice = null)
        {
            var anagraficaClienti = new AnagraficaClienti(logger, configuration, clientFactory);
            return anagraficaClienti.Clienti(parteDenominazioneCodice);
        }

        public ConfigurazioneOperatore ConfigurazioneOperatore(string codiceFiscale)
        {
            var generatoreConfigurazioneOperatore = new GeneraConfigurazioneOperatore(logger, configuration, clientFactory);
            return generatoreConfigurazioneOperatore.Genera(codiceFiscale);
        }

        public List<Contenitore> Contenitori([Required] string codicePacchetto)
        {
            var anagraficaContenitori = new AnagraficaContenitori(logger, configuration, clientFactory);
            return anagraficaContenitori.Contenitori(codicePacchetto);
        }

        public List<MatriceNomeDescrizione> Matrici()
        {
            var anagraficaMatrici = new AnagraficaMatrici(logger, configuration, clientFactory);
            return anagraficaMatrici.Matrici();
        }

        public List<MisuraInLoco> MisureInLoco(string codiceMatrice, string codiceArgomento, List<string> codiciPacchetto)
        {
            var anagraficaMisureInLoco = new AnagraficaMisureInLoco(logger, configuration, clientFactory);
            return anagraficaMisureInLoco.MisureInLoco(codiceMatrice, codiceArgomento, codiciPacchetto);
        }

        public List<Pacchetto> Pacchetti(string codiceMatrice, string codiceArgomento, string codiceSedeAccettazione)
        {
            var anagraficaPacchetti = new AnagraficaPacchetti(logger, configuration, clientFactory);
            return anagraficaPacchetti.Pacchetti(codiceMatrice, codiceArgomento, codiceSedeAccettazione);
        }

        public List<Prelevatore> Prelevatori(string codiceEnte, string codiceArgomento)
        {
            var anagraficaPrelevatori = new AnagraficaPrelevatori(logger, configuration, clientFactory);
            return anagraficaPrelevatori.Prelevatori(codiceEnte, codiceArgomento);
        }

        public List<PuntoDiPrelievo> PuntiDiPrelievo(string codiceIstatComune, string parteDenominazioneCodice = null)
        {
            var anagraficaPuntiDiPrelievo = new AnagraficaPuntiDiPrelievo(logger, configuration, clientFactory);
            return anagraficaPuntiDiPrelievo.PuntiDiPrelievo(codiceIstatComune, parteDenominazioneCodice);
        }
    }
}
