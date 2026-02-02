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

using SICampionaAPI.Features.Campionamenti;
using static SICampionaAPI.Features.Campionamenti.ElencoCampionamentiCompletati;

namespace SICampionaAPI.Data
{
    public interface IRepositoryCampionamenti
    {
        Campionamento DettagliCampionamento(int idCampionamento);
        int AggiuntaCampionamento(Campionamento campionamento);
        void AggiornamentoCampionamento(Campionamento campionamento);
        List<Campionamento> ElencoCampionamentiAperti(List<string> codiciEnte);
        List<InfoCampionamentoCompletato> ElencoCampionamentiCompletati(RichiestaCampionamentiCompletati richiesta);
        /// <summary>
        /// Elenco dei campionamenti completati che non hanno il numero di campione ma hanno il file del verbale
        /// </summary>
        /// <returns></returns>
        List<Campionamento> ElencoCampionamentiSenzaNumeroCampione();
        List<Cliente> ClientiCampionamentiCompletati(string codiceEnte);
        List<string> ElencoTipiCampionamento(string codiceMatrice);
        void SalvaNomeFileVerbaleFirmato(int idCampionamento, string nomeFileFirmato);
    }
}
