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

using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using SICampiona.Model;
using SICampiona.Pages.Dialog;
using System.Text;

namespace SICampiona.Pages
{
	public partial class AggiuntaCampionamento
	{
		#region Gestione errore
		private string messaggioErrore = null;
		private string dettagliErrore = null;
		private bool visualizzaComandoRicarica = false;
		private bool visualizzaComandoChiudi = false;
		private bool visualizzaComandoTornaHomePage = false;
		#endregion

		protected override async Task OnInitializedAsync()
		{
			if (infoOperatore.Autorizzato)
			{
				PopolaElencoEnti();
				await PopolaElencoSediAccettazione();
				PacchettiEmptyContentFragment = SelezionareLaSedeFragment;
			}
			else
			{
				messaggioErrore = "Per accedere a questa funzione occorre fare logon.";
			}
		}
        private async Task ModificaSelezioneMatrice()
		{
            argomenti = infoOperatore.Argomenti(codiceEnteSelezionato, codiceMatriceSelezionata);
            codiceArgomentoSelezionato = argomenti.FirstOrDefault()?.Codice;

            InizializzaPacchetti();
			await CaricaTipiCampionamento(codiceMatriceSelezionata);
		}

		#region Cliente

		string codiceClienteSelezionato = null;
		string denominazioneClienteSelezionato = null;
		private readonly ClienteConIndirizzo datiCliente = new();

		private async Task RicercaClienteDialog()
		{
			DialogParameters parametriDialog = new()
			{
				Title = "Selezione cliente",
				DismissTitle = "Annulla",
				Width = "60%"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<SelezioneCliente>(datiCliente, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				codiceClienteSelezionato = datiCliente.Codice;
				denominazioneClienteSelezionato = datiCliente.Denominazione;
			}
		}

		/// <summary>
		/// Se all'ente è associato un codice cliente, lo usa per impostare il cliente.
		/// La denominazione è la ragione sociale dell'ente.
		/// </summary>
		private void SelezioneEnteComeCliente()
		{
			if (codiceEnteSelezionato != null)
			{
				var ente = enti.First(i => i.Codice == codiceEnteSelezionato);
				if (!string.IsNullOrWhiteSpace(ente.CodiceCliente))
				{
					codiceClienteSelezionato = ente.CodiceCliente;
					denominazioneClienteSelezionato = ente.RagioneSociale;
					datiCliente.Codice = codiceClienteSelezionato;
					datiCliente.Denominazione = denominazioneClienteSelezionato;
				}
			}
		}

		#endregion Cliente

		#region Ente

		List<ConfigurazioneOperatore.Ente> enti = [];
		List<ConfigurazioneOperatore.Ente.Matrice.Argomento> argomenti = [];
		List<ConfigurazioneOperatore.Ente.Matrice> matrici = [];

		private void PopolaElencoEnti()
		{
			enti = infoOperatore.Enti();
			codiceEnteSelezionato = enti.FirstOrDefault()?.Codice;
			SelezioneEnteComeCliente();
		}

		private void SelezioneEnte(ConfigurazioneOperatore.Ente ente)
		{
			matrici = infoOperatore.Matrici(ente.Codice);
			codiceMatriceSelezionata = matrici.FirstOrDefault()?.Codice;

			argomenti = infoOperatore.Argomenti(ente.Codice, codiceMatriceSelezionata);
			codiceArgomentoSelezionato = argomenti.FirstOrDefault()?.Codice;
		}

		#endregion Ente

		#region Punto di prelievo

		bool puntoInserito = false; // indica se il punto è stato inserito dall'operatore oppure selezionato
		PuntoDiPrelievo puntoDiPrelievo = new();

		private async Task RicercaPuntoDiPrelievoDialog()
		{
			PuntoDiPrelievo selezionePuntoDiPrelievo = new();

			DialogParameters parametriDialog = new()
			{
				Title = "Selezione punto di prelievo",
				DismissTitle = "Annulla",
				Width = "60%"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<SelezionePuntoDiPrelievo>
				(selezionePuntoDiPrelievo, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				puntoInserito = false;
				puntoDiPrelievo = selezionePuntoDiPrelievo;
			}
		}

		private async Task PuntoDiPrelievoDialog()
		{
			PuntoDiPrelievo inserimentoPuntoDiPrelievo = new()
			{
				Codice = null,
				Denominazione = null,
				Indirizzo = null,
				Comune = new Comune()
				{
					CodiceIstat = null,
					Denominazione = null
				},
				Coordinate = new Coordinate()
				{
					Latitudine = null,
					Longitudine = null
				}
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Punto di prelievo",
				DismissTitle = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoModificaPuntoDiPrelievo>
				(inserimentoPuntoDiPrelievo, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				puntoInserito = true;
				puntoDiPrelievo = inserimentoPuntoDiPrelievo;
			}
		}

		#endregion Punto di prelievo

		#region Sede di accettazione

		List<SedeAccettazione> sediAccettazione = [];
		string codiceSedeAccettazioneSelezionata = null;

		private async Task PopolaElencoSediAccettazione()
		{
			var result = await anagrafiche.SediAccettazione();
			if (result.IsSuccess)
			{
				sediAccettazione = result.Data;
				sediAccettazione.Insert(0, new SedeAccettazione { Codice = null, Denominazione = string.Empty });
				StateHasChanged();
				codiceSedeAccettazioneSelezionata = null;
			}
			else
			{
				messaggioErrore = "Non è stato possibile recuperare l'elenco delle sedi di accettazione";
				dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
				visualizzaComandoRicarica = true;
				visualizzaComandoChiudi = false;
				visualizzaComandoTornaHomePage = true;
			}
		}

		#endregion Sede di accettazione

		#region Pacchetti

		List<PacchettoSelezionabile> pacchettiSelezionabili = [];
		private bool aggiornamentoPacchettiInCorso = false;
		private RenderFragment PacchettiEmptyContentFragment;
		bool nessunPacchetto = true;

		private void InizializzaPacchetti()
		{
			pacchettiSelezionabili = [];
			codiceSedeAccettazioneSelezionata = null;
			PacchettiEmptyContentFragment = SelezionareLaSedeFragment;
		}

		private async Task CaricaPacchetti(SedeAccettazione sede)
		{
			selezionaTutti = true;
			if (sede == null || sede.Codice == null)
			{
				selezionaTutti = false;
				pacchettiSelezionabili = [];
				nessunPacchetto = true;
				return;
			}

			aggiornamentoPacchettiInCorso = true;
			var result = await anagrafiche.Pacchetti(codiceMatriceSelezionata, codiceArgomentoSelezionato, sede.Codice);
			if (result.IsSuccess)
			{
				pacchettiSelezionabili = [];
				foreach (var p in result.Data)
				{
					pacchettiSelezionabili.Add(new PacchettoSelezionabile()
					{
						Codice = p.Codice,
						Descrizione = p.Descrizione,
						Selezionato = true
					});
				}
				if (pacchettiSelezionabili.Count == 0)
				{
					PacchettiEmptyContentFragment = NessunPacchettoTrovatoFragment;
					nessunPacchetto = true;
				}
				else
				{
					nessunPacchetto = false;
				}
			}
			else
			{
				messaggioErrore = "Non è stato possibile recuperare l'elenco dei pacchetti";
				dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
				visualizzaComandoRicarica = true;
				visualizzaComandoChiudi = false;
				visualizzaComandoTornaHomePage = true;
			}
			aggiornamentoPacchettiInCorso = false;
		}

		private sealed class PacchettoSelezionabile()
		{
			public string Codice { get; set; }
			public string Descrizione { get; set; }
			public string CodiceDescrizione
			{
				get
				{
					return $"{Descrizione} ({Codice})";
				}
			}
			public bool Selezionato { get; set; }
		}

		#endregion Pacchetti

		#region Tipo campionamento

		List<string> tipiCampionamento = [];
		private async Task CaricaTipiCampionamento(string codiceMatrice)
		{
			var result = await anagrafiche.TipiCampionamento(codiceMatrice);
			if (result.IsSuccess)
			{
				tipiCampionamento = result.Data;
				tipiCampionamento.Insert(0, string.Empty);
				tipoCampionamentoSelezionato = null;
			}
			else
			{
				messaggioErrore = "Non è stato possibile recuperare l'elenco dei tipi campionamento";
				dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
				visualizzaComandoRicarica = true;
				visualizzaComandoChiudi = false;
				visualizzaComandoTornaHomePage = true;
			}
		}

		#endregion Tipo campionamento

		#region Nuovo

		string codiceMatriceSelezionata = null;
		string codiceArgomentoSelezionato = null;
		string codiceEnteSelezionato = null;
		string tipoCampionamentoSelezionato = null;
		private DateTime? dataPrelievoSelezionata = DateTime.Now;

		bool creazioneInCorso = false;
		string messaggioErroreDatiIncompleti = null;
		private async Task Nuovo()
		{
			// Verifica campi obbligatori
			StringBuilder sb = new();
			if (string.IsNullOrEmpty(denominazioneClienteSelezionato))
			{
				sb.Append("cliente");
			}
			if (string.IsNullOrEmpty(puntoDiPrelievo.Codice))
			{
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append("punto di prelievo");
			}
			if (string.IsNullOrEmpty(codiceMatriceSelezionata))
			{
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append("matrice");
			}
			if (string.IsNullOrEmpty(codiceArgomentoSelezionato))
			{
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append("argomento");
			}
			if (!pacchettiSelezionabili.Exists(i => i.Selezionato))
			{
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append("almeno un pacchetto");
			}
			if (sb.Length > 0)
			{
				messaggioErroreDatiIncompleti = "Per aggiungere il campionamento devono essere inseriti questi dati: " + sb.ToString();
				return;
			}
			if (string.IsNullOrEmpty(codiceSedeAccettazioneSelezionata))
			{
				if (sb.Length > 0)
					sb.Append(", ");
				sb.Append("sede accettazione");
			}

			creazioneInCorso = true;

			var nuovo = new NuovoCampionamento
			{
				Ente = new Model.Ente()
				{
					Denominazione = enti.First(i => i.Codice == codiceEnteSelezionato).RagioneSociale,
					Codice = codiceEnteSelezionato
				},
				Cliente = new Cliente()
				{
					Denominazione = denominazioneClienteSelezionato,
					Codice = codiceClienteSelezionato
				},
				Matrice = new Matrice()
				{
					Denominazione = matrici.First(i => i.Codice == codiceMatriceSelezionata).Descrizione,
					Codice = codiceMatriceSelezionata
				},
				Argomento = new Argomento()
				{
					Denominazione = argomenti.First(i => i.Codice == codiceArgomentoSelezionato).Descrizione,
					Codice = codiceArgomentoSelezionato
				},
				CodiceSedeAccettazione = codiceSedeAccettazioneSelezionata,
				TipoCampionamento = string.IsNullOrEmpty(tipoCampionamentoSelezionato) ? null : tipoCampionamentoSelezionato,
			};

			string comuneCodiceIstat = null;
			string comuneDenominazione = null;
			string coordinateLatitudine = null;
			string coordinateLongitudine = null;

			if (puntoDiPrelievo.Comune != null)
			{
				comuneCodiceIstat = puntoDiPrelievo.Comune.CodiceIstat;
				comuneDenominazione = puntoDiPrelievo.Comune.Denominazione;
			}
			if (puntoDiPrelievo.Coordinate != null)
			{
				coordinateLatitudine = puntoDiPrelievo.Coordinate.Latitudine;
				coordinateLongitudine = puntoDiPrelievo.Coordinate.Longitudine;
			}

			// Decide che cosa creare (punto arpal o operatore) se il punto è stato selezionato o inswrito
			if (puntoInserito)
			{
				nuovo.PuntoDiPrelievoInseritoDaOperatore = new()
				{
					Codice = puntoDiPrelievo.Codice,
					Denominazione = puntoDiPrelievo.Denominazione,
					Indirizzo = puntoDiPrelievo.Indirizzo,
					Comune = new Comune()
					{
						CodiceIstat = comuneCodiceIstat,
						Denominazione = comuneDenominazione
					},
					Coordinate = new Coordinate()
					{
						Latitudine = coordinateLatitudine,
						Longitudine = coordinateLongitudine
					}
				};
			}
			else
			{
				nuovo.PuntoDiPrelievoARPAL = new PuntoDiPrelievo()
				{
					Codice = puntoDiPrelievo.Codice,
					Denominazione = puntoDiPrelievo.Denominazione,
					Indirizzo = puntoDiPrelievo.Indirizzo,
					Comune = new Comune()
					{
						CodiceIstat = comuneCodiceIstat,
						Denominazione = comuneDenominazione
					},
					Coordinate = new Coordinate()
					{
						Latitudine = coordinateLatitudine,
						Longitudine = coordinateLongitudine
					}
				};
			}
			nuovo.DataPrelievo = DateOnly.FromDateTime(dataPrelievoSelezionata.Value);

			nuovo.CodiciPacchetto = [];
			foreach (var p in pacchettiSelezionabili.Where(i => i.Selezionato))
			{
				nuovo.CodiciPacchetto.Add(p.Codice);
			}

			var result = await campionamentiStorage.Aggiunta(nuovo);
			if (result.IsSuccess)
			{
				Navigation.NavigateTo("/");
			}
			else
			{
				messaggioErrore = "Non è stato possibile creare il campionamento";
				dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
				visualizzaComandoRicarica = true;
				visualizzaComandoChiudi = false;
				visualizzaComandoTornaHomePage = true;
			}
		}

		#endregion Nuovo

		private void Annulla()
		{
			Navigation.NavigateTo("/");
		}
	}
}
