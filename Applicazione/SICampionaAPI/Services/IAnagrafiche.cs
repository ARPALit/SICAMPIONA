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

namespace SICampionaAPI.Services
{
    public interface IAnagrafiche
    {
        /// <summary>
        /// Elenco analiti per matrice, argomento, nome dell'analita e linea (laboratorio o territorio)
        /// </summary>
        /// <param name="codiceMatrice"></param>
        /// <param name="codiceArgomento"></param>
        /// <param name="codicePacchetto"></param>
        /// <param name="parteDelNome"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        List<Analita> Analiti([Required] string codiceMatrice, string codiceArgomento = null, string codicePacchetto = null,
            string parteDelNome = null, string linea = null);

        /// <summary>
        /// Misure in loco per pacchetto
        /// </summary>
        /// <param name="codiceMatrice"></param>
        /// <param name="codiceArgomento"></param>
        /// <param name="codiciPacchetto"></param>
        /// <returns></returns>
        List<MisuraInLoco> MisureInLoco([Required] string codiceMatrice, [Required] string codiceArgomento, [Required] List<string> codiciPacchetto);

        /// <summary>
        /// Prelevatori per ente e argomento
        /// </summary>
        /// <param name="codiceEnte"></param>
        /// <param name="codiceArgomento"></param>
        /// <returns></returns>
        List<Prelevatore> Prelevatori([Required] string codiceEnte, [Required] string codiceArgomento);

        /// <summary>
        /// Ricerca cliente per codice o denominazione 
        /// </summary>
        /// <param name="parteDenominazioneCodice"></param>
        /// <returns></returns>
        List<ClienteConIndirizzo> Clienti([Required] string parteDenominazioneCodice = null);

        /// <summary>
        /// Ricerca punto di prelievo per denominazione e codice istat comune
        /// </summary>
        /// <param name="codiceIstatComune"></param>
        /// <param name="parteDenominazioneCodice"></param>
        /// <returns></returns>
        List<PuntoDiPrelievo> PuntiDiPrelievo([Required] string codiceIstatComune, string parteDenominazioneCodice = null);

        /// <summary>
        /// Elenco pacchetti per matrice
        /// </summary>
        /// <param name="codiceMatrice"></param>
        /// <param name="codiceArgomento"></param>
        /// <param name="codiceSedeAccettazione"></param>
        /// <returns></returns>
        List<Pacchetto> Pacchetti([Required] string codiceMatrice, string codiceArgomento, string codiceSedeAccettazione);

        /// <summary>
        /// Configurazione operatore per codice fiscale
        /// </summary>
        /// <param name="codiceFiscale"></param>
        /// <returns></returns>
        ConfigurazioneOperatore ConfigurazioneOperatore([Required] string codiceFiscale);

        /// <summary>
        /// Contentitori per pacchetto
        /// </summary>
        /// <param name="codicePacchetto"></param>
        /// <returns></returns>
        List<Contenitore> Contenitori([Required] string codicePacchetto);

        /// <summary>
        /// Matrici
        /// </summary>		
        /// <returns></returns>
        List<MatriceNomeDescrizione> Matrici();
    }
}
