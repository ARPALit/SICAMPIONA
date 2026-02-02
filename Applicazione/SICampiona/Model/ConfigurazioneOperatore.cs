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

namespace SICampiona.Model
{
	public class ConfigurazioneOperatore
	{
		public string Cognome { get; set; }
		public string Nome { get; set; }
		public string CodiceFiscale { get; set; }
		public string CodiceAnagrafica { get; set; }

		public List<Ente> Enti { get; set; }

        public class Ente
        {
            public string Codice { get; set; }
            public string RagioneSociale { get; set; }
            /// <summary>
            /// Codice dell'ente nell'anagrafica clienti
            /// </summary>
            public string CodiceCliente { get; set; }
            public List<Matrice> Matrici { get; set; }

            public class Matrice
            {
                public string Codice { get; set; }
                public string Descrizione { get; set; }
                public List<Argomento> Argomenti { get; set; }

                public class Argomento
                {
                    public string Codice { get; set; }
                    public string Descrizione { get; set; }
                }
            }
        }
    }
}
