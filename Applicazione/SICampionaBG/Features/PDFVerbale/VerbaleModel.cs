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
namespace SICampionaBG.Features.CreazionePDFVerbale
{
	/// <summary>
	/// Contenuto del verbale
	/// </summary>
	public class VerbaleModel
	{
		public string Matrice { get; set; }
		public string Argomento { get; set; }
		public string Ente { get; set; }
		public string SiglaVerbale { get; set; }
		public string Operatore { get; set; }
		public string DataOraCampionamento { get; set; }
		public string PuntoDiPrelievoCodice { get; set; }
		public string PuntoDiPrelievoDenominazione { get; set; }
		public string PuntoDiPrelievoIndirizzo { get; set; }
		public string PuntoDiPrelievoComune { get; set; }
		public List<MisuraInLoco> MisureInLoco { get; set; }
		public List<Parametro> Parametri { get; set; }
		public string DescrizioneAttivita { get; set; }
		public string Prelevatori { get; set; }
		public string DateOraVerbale { get; set; }
		public string DateOraAperturaCampione { get; set; }
		public string EmailInvioVerbale { get; set; }
		public string PecInvioVerbale { get; set; }
		public string CampionamentoCollegato { get; set; }
		public string DescrizioneTipoCampionamento { get; set; }
		public string NoteTipoCampionamento { get; set; }
        public string CampioneBianco { get; set; }
        public string LuogoAperturaCampione { get; set; }
        public string Note { get; set; }

        public class MisuraInLoco
		{
			public string Descrizione { get; set; }
			public string Metodo { get; set; }
			public string ValoreConUM { get; set; }
			public string Note { get; set; }
		}

		public class Parametro
		{
			public string Pacchetto { get; set; }
			public string Descrizione { get; set; }
			public string Metodo { get; set; }
			public string UM { get; set; }
			public string Limite { get; set; }
		}
	}
}
