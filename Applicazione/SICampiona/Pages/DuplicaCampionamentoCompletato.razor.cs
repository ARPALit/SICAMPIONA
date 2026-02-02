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
using SICampiona.Pages.Dialog;
using SICampiona.Services;
using System.Globalization;

namespace SICampiona.Pages
{
    public partial class DuplicaCampionamentoCompletato
	{
		#region Gestione errore
		private string messaggioErrore = null;
		private string dettagliErrore = null;
		private bool visualizzaComandoRicarica = false;
		private bool visualizzaComandoChiudi = false;
		private bool visualizzaComandoTornaHomePage = false;
		#endregion

		[Parameter]
		public int IdCampionamento { get; set; }
		[Parameter]
		public string CodiceMatrice { get; set; }


		#region Data prelievo

		DateOnly nuovaDataPrelievo = DateOnly.FromDateTime(DateTime.Now);

		private async Task InserimentoDataPrelievoDialog()
		{
			InserimentoDataOraContent inserimentoDataPrelievoContent = new()
			{
				Etichetta = "Data prelievo:",
				ValoreDataInserito = nuovaDataPrelievo.ToDateTime(TimeOnly.Parse("00:00", CultureInfo.InvariantCulture)),
				VisualizzaData = true,
				VisualizzaOra = false,
				Obbligatorio = true // Predefinito: false
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Nuova data prelievo",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla",
				TrapFocus = true,
				Modal = true,
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoDataOra>(inserimentoDataPrelievoContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				if (inserimentoDataPrelievoContent.ValoreDataInserito.HasValue)
					nuovaDataPrelievo = DateOnly.FromDateTime(inserimentoDataPrelievoContent.ValoreDataInserito.Value);
			}
		}

		#endregion Data prelievo

		#region Duplica

		bool creazioneInCorso = false;

		bool creaCollegamento = false;

		string tipoCampionamentoSelezionato = null;

		protected override async Task OnInitializedAsync()
		{
			await CaricaTipiCampionamento(CodiceMatrice);
		}

		private async Task Duplica()
		{
			creazioneInCorso = true;

			RichiestaDuplicazioneCampionamentoCompletato richiesta = new()
			{
				IdCampionamentoDaDuplicare = IdCampionamento,
				CreaCollegamento = creaCollegamento,
				NuovaDataPrelievo = nuovaDataPrelievo,
                // Se tipoCampionamentoSelezionato è stringa vuota viene passato null
                TipoCampionamento = string.IsNullOrEmpty(tipoCampionamentoSelezionato) ? null : tipoCampionamentoSelezionato
            };

			var result = await campionamentiStorage.DuplicazioneCampionamentoCompletato(richiesta);
			if (result.IsSuccess)
			{
				Navigation.NavigateTo($"/campionamento/{result.Data.IdCampionamento}");
			}
			else
			{
				messaggioErrore = "Non è stato possibile duplicare il campionamento";
				dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
				visualizzaComandoRicarica = true;
				visualizzaComandoChiudi = false;
				visualizzaComandoTornaHomePage = true;
			}
		}

		#endregion Duplica

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

		private void Annulla()
		{
			Navigation.NavigateTo("/archivio");
		}
	}
}
