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

using SICampiona.Model;

namespace SICampiona.Services
{
	public interface ICampionamentiStorage
	{
		/// <summary>
		/// Elenco campionamenti memorizzati in locale
		/// </summary>
		/// <returns></returns>
		public Task<Result<List<InformazioniCampionamento>>> Elenco();
		public Task<Result<Campionamento>> Dettagli(int idCampionamento);
		public Task<Result<Campionamento>> Aggiornamento(Campionamento campionamento);
		public Task<Result> Eliminazione(int idCampionamento);
		public Task<Result<Campionamento>> Aggiunta(NuovoCampionamento nuovoCampionamento);
		public Task<Result> Sincronizzazione();
		public Task<Result<bool>> SeModificati();
		public Task<Result<Campionamento>> Duplicazione(RichiestaDuplicazioneCampionamento richiestaDuplicazioneCampionamento);
		public Task<Result<Campionamento>> DuplicazioneCampionamentoCompletato(RichiestaDuplicazioneCampionamentoCompletato richiestaDuplicazioneCampionamentoCompletato);        
		public Task<Result> SegnaComeModificato(int idCampionamento);
    }
}
