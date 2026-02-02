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
using System.Globalization;

namespace SICampiona.Pages
{
    public partial class ModificaCampionamento
	{
		[Parameter]
		public Campionamento Campionamento { get; set; }

		private string EsitoComando = null;

		private string DatiMancanti = null;

		#region Gestione errore
		private string messaggioErrore = null;
		private string dettagliErrore;
		private bool visualizzaComandoRicarica = false;
		private bool visualizzaComandoChiudi = false;
		private bool visualizzaComandoTornaHomePage = false;
		#endregion

		PuntoDiPrelievo puntoDiPrelievo = new PuntoDiPrelievo();

		private async Task RicercaPuntoDiPrelievoDialog()
		{
			PuntoDiPrelievo selezionePuntoDiPrelievo = new();

			DialogParameters parametriDialog = new()
			{
				Title = "Selezione punto di prelievo",
				DismissTitle = "Annulla",
				Width = "60%"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<SelezionePuntoDiPrelievo>(selezionePuntoDiPrelievo, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.PuntoDiPrelievoARPAL = selezionePuntoDiPrelievo;
				Campionamento.PuntoDiPrelievoInseritoDaOperatore = null;
				await AggiornaCampionamento();
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

			if (Campionamento.PuntoDiPrelievoInseritoDaOperatore != null)
			{
				inserimentoPuntoDiPrelievo = new()
				{
					Codice = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Codice,
					Denominazione = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Denominazione,
					Indirizzo = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Indirizzo,
					Comune = new Comune()
					{
						CodiceIstat = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Comune.CodiceIstat,
						Denominazione = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Comune.Denominazione
					},
					Coordinate = new Coordinate()
					{
						Latitudine = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Coordinate.Latitudine,
						Longitudine = Campionamento.PuntoDiPrelievoInseritoDaOperatore.Coordinate.Longitudine
					}
				};
			}
			DialogParameters parametriDialog = new()
			{
				Title = "Punto di prelievo",
				DismissTitle = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoModificaPuntoDiPrelievo>(inserimentoPuntoDiPrelievo, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.PuntoDiPrelievoInseritoDaOperatore = inserimentoPuntoDiPrelievo;
				Campionamento.PuntoDiPrelievoARPAL = null;
				await AggiornaCampionamento();
				puntoDiPrelievo = inserimentoPuntoDiPrelievo;
			}
		}

		private async Task PrelevatoriDialog()
		{
			DialogParameters parametriDialog = new()
			{
				Title = "Prelevatori",
				DismissTitle = "Annulla",
				PrimaryAction = "Salva",
				SecondaryAction = "Annulla"
			};

			ModificaPrelevatoriContent modificaPrelevatoriContent = new()
			{
				Prelevatori = Campionamento.Prelevatori,
				PrelevatoriSelezionabili = Campionamento.PrelevatoriSelezionabili
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<ModificaPrelevatori>(modificaPrelevatoriContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.Prelevatori = modificaPrelevatoriContent.Prelevatori;
				await AggiornaCampionamento();
			}
		}

		private async Task InserimentoDescrizioneAttivitaDialog()
		{
			InserimentoTestoContent inserimentoDescrizioneAttivitaContent = new()
			{
				Etichetta = string.Empty,
				ValoreInserito = Campionamento.DescrizioneAttivita,
				LunghezzaMassimaTesto = 1000, // Opzionale
				Multilinea = true, // Opzionale predefinito: false
				LarghezzaCasella = 100, // Opzionale
				Obbligatorio = false // Predefinito: false
			};
			DialogParameters parametriDialog = new()
			{
				Title = "Descrizione attività",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoTesto>(inserimentoDescrizioneAttivitaContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.DescrizioneAttivita = inserimentoDescrizioneAttivitaContent.ValoreInserito;
				await AggiornaCampionamento();
			}
		}

		#region Data e ora prelievo

		private async Task InserimentoDataPrelievoDialog()
		{
			InserimentoDataOraContent inserimentoDataPrelievoContent = new()
			{
				Etichetta = "Data prelievo:",
				ValoreDataInserito = Campionamento.DataPrelievo.ToDateTime(TimeOnly.Parse("00:00", CultureInfo.InvariantCulture)),
				VisualizzaData = true,
				VisualizzaOra = false,
				Obbligatorio = true // Predefinito: false
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Data prelievo",
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
					Campionamento.DataPrelievo = DateOnly.FromDateTime(inserimentoDataPrelievoContent.ValoreDataInserito.Value);
				await AggiornaCampionamento();
			}
		}

		private async Task InserimentoOraPrelievoDialog()
		{
			InserimentoDataOraContent inserimentoOraPrelievoContent = new()
			{
				Etichetta = "Ora prelievo:",
				ValoreOraInserito = Campionamento.OraPrelievo.HasValue ? (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local) + Campionamento.OraPrelievo.Value.ToTimeSpan()) : null,
				VisualizzaData = false,
				VisualizzaOra = true,
				Obbligatorio = false // Predefinito: false
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Ora prelievo",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla",
				TrapFocus = true,
				Modal = true,
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoDataOra>(inserimentoOraPrelievoContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				if (inserimentoOraPrelievoContent.ValoreOraInserito.HasValue)
					Campionamento.OraPrelievo = TimeOnly.FromDateTime(inserimentoOraPrelievoContent.ValoreOraInserito.Value);
				else
					Campionamento.OraPrelievo = null;
				await AggiornaCampionamento();
			}
		}

		#endregion  Data e ora prelievo

		#region Data, ora e luogo apertura campione

		private async Task InserimentoDataAperturaCampioneDialog()
		{
			DateTime? valoreDataInserito = null;
			if (Campionamento.DataAperturaCampione.HasValue)
				valoreDataInserito = Campionamento.DataAperturaCampione.Value.ToDateTime(TimeOnly.Parse("00:00", CultureInfo.InvariantCulture));

			InserimentoDataOraContent inserimentoDataAperturaCampioneContent = new()
			{
				Etichetta = "Data apertura campione:",
				ValoreDataInserito = valoreDataInserito,
				VisualizzaData = true,
				VisualizzaOra = false,
				Obbligatorio = false // Predefinito: false
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Data apertura campione",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla",
				TrapFocus = true,
				Modal = true,
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoDataOra>(inserimentoDataAperturaCampioneContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				if (inserimentoDataAperturaCampioneContent.ValoreDataInserito.HasValue)
					Campionamento.DataAperturaCampione = DateOnly.FromDateTime(inserimentoDataAperturaCampioneContent.ValoreDataInserito.Value);
				else
					Campionamento.DataAperturaCampione = null;
				await AggiornaCampionamento();
			}
		}

		private async Task InserimentoOraAperturaCampioneDialog()
		{
			InserimentoDataOraContent inserimentoOraAperturaCampioneContent = new()
			{
				Etichetta = "Ora prelievo:",
				ValoreOraInserito = Campionamento.OraAperturaCampione.HasValue ? (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local) + Campionamento.OraAperturaCampione.Value.ToTimeSpan()) : null,
				VisualizzaData = false,
				VisualizzaOra = true,
				Obbligatorio = false // Predefinito: false
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Ora apertura campione",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla",
				TrapFocus = true,
				Modal = true,
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoDataOra>(inserimentoOraAperturaCampioneContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				if (inserimentoOraAperturaCampioneContent.ValoreOraInserito.HasValue)
					Campionamento.OraAperturaCampione = TimeOnly.FromDateTime(inserimentoOraAperturaCampioneContent.ValoreOraInserito.Value);
				else
					Campionamento.OraAperturaCampione = null;
				await AggiornaCampionamento();
			}
		}

        private async Task LuogoAperturaCampioneDialog()
        {
            InserimentoTestoContent luogoAperturaCampioneContent = new()
            {
                Etichetta = string.Empty,
                ValoreInserito = Campionamento.LuogoAperturaCampione,
                LunghezzaMassimaTesto = 250, // Opzionale
                Multilinea = true, // Opzionale predefinito: false
                LarghezzaCasella = 200, // Opzionale
                Obbligatorio = false // Predefinito: false
            };
            DialogParameters parametriDialog = new()
            {
                Title = "Luogo apertura campione",
                DismissTitle = "Annulla",
                SecondaryAction = "Annulla"
            };

            IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoTesto>(luogoAperturaCampioneContent, parametriDialog);
            DialogResult result = await dialog.Result;

            if (!result.Cancelled)
            {
                Campionamento.LuogoAperturaCampione = luogoAperturaCampioneContent.ValoreInserito;
                await AggiornaCampionamento();
            }
        }

        #endregion  Data e ora apertura campione

        private async Task InserimentoEmailInvioVerbaleDialog()
		{
			InserimentoEmailContent inserimentoEmailInvioVerbaleContent = new()
			{
				Etichetta = string.Empty,
				ValoreInserito = Campionamento.EmailInvioVerbale,
				LunghezzaMassimaTesto = 100, // Opzionale
				LarghezzaCasella = 100, // Opzionale
				Obbligatorio = false // Predefinito: false
			};
			DialogParameters parametriDialog = new()
			{
				Title = "Email a cui inviare il verbale",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoEmail>(inserimentoEmailInvioVerbaleContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.EmailInvioVerbale = inserimentoEmailInvioVerbaleContent.ValoreInserito;
				await AggiornaCampionamento();
			}
		}

		private async Task InserimentoPECInvioVerbaleDialog()
		{
			InserimentoTestoContent inserimentoPECInvioVerbaleContent = new()
			{
				Etichetta = string.Empty,
				ValoreInserito = Campionamento.PECInvioVerbale,
				LunghezzaMassimaTesto = 100, // Opzionale
				Multilinea = false, // Opzionale predefinito: false
				LarghezzaCasella = 100, // Opzionale
				Obbligatorio = false // Predefinito: false
			};
			DialogParameters parametriDialog = new()
			{
				Title = "PEC a cui inviare il verbale",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoTesto>(inserimentoPECInvioVerbaleContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.PECInvioVerbale = inserimentoPECInvioVerbaleContent.ValoreInserito;
				await AggiornaCampionamento();
			}
		}

		private async Task InserimentoCampionamentoCollegatoDialog()
		{
			InserimentoTestoContent inserimentoCampionamentoCollegatoContent = new()
			{
				Etichetta = string.Empty,
				ValoreInserito = Campionamento.CampionamentoCollegato,
				LunghezzaMassimaTesto = 100, // Opzionale
				Multilinea = false, // Opzionale predefinito: false
				LarghezzaCasella = 100, // Opzionale
				Obbligatorio = false // Predefinito: false
			};
			DialogParameters parametriDialog = new()
			{
				Title = "Campionamento collegato",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoTesto>(inserimentoCampionamentoCollegatoContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.CampionamentoCollegato = inserimentoCampionamentoCollegatoContent.ValoreInserito;
				await AggiornaCampionamento();
			}
		}

		private async Task CampioneBiancoDialog()
		{
			InserimentoTestoContent campioneBiancoContent = new()
			{
				Etichetta = string.Empty,
				ValoreInserito = Campionamento.CampioneBianco,
				LunghezzaMassimaTesto = 500, // Opzionale
				Multilinea = true, // Opzionale predefinito: false
				LarghezzaCasella = 200, // Opzionale
				Obbligatorio = false // Predefinito: false
			};
			DialogParameters parametriDialog = new()
			{
				Title = "Campione bianco (serigrafia, lotto e scadenza)",
				DismissTitle = "Annulla",
				SecondaryAction = "Annulla"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoTesto>(campioneBiancoContent, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				Campionamento.CampioneBianco = campioneBiancoContent.ValoreInserito;
				await AggiornaCampionamento();
			}
		}

        private async Task NoteDialog()
        {
            InserimentoTestoContent noteContent = new()
            {
                Etichetta = string.Empty,
                ValoreInserito = Campionamento.Note,
                LunghezzaMassimaTesto = 500, // Opzionale
                Multilinea = true, // Opzionale predefinito: false
                LarghezzaCasella = 200, // Opzionale
                Obbligatorio = false // Predefinito: false
            };
            DialogParameters parametriDialog = new()
            {
                Title = "Note",
                DismissTitle = "Annulla",
                SecondaryAction = "Annulla"
            };

            IDialogReference dialog = await DialogService.ShowDialogAsync<InserimentoTesto>(noteContent, parametriDialog);
            DialogResult result = await dialog.Result;

            if (!result.Cancelled)
            {
                Campionamento.Note = noteContent.ValoreInserito;
                await AggiornaCampionamento();
            }
        }

        private async Task MisureInLocoDialog()
		{
			DialogParameters parametriDialog = new()
			{
				Title = "Misure in loco",
				DismissTitle = "Annulla",
				PrimaryAction = "Salva",
				SecondaryAction = "Annulla",
				Width = "80%"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<ModificaMisureInLoco>(Campionamento.MisureInLoco, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				await AggiornaCampionamento();
			}
		}

		#region Analiti

		private async Task RicercaAnalitaDialog()
		{
			SelezioneAnalitaContent selezioneAnalita = new()
			{
				CodiceMatrice = Campionamento.Matrice.Codice
			};

			DialogParameters parametriDialog = new()
			{
				Title = "Selezione analita",
				DismissTitle = "Annulla",
				Width = "60%"
			};

			IDialogReference dialog = await DialogService.ShowDialogAsync<SelezioneAnalita>(selezioneAnalita, parametriDialog);
			DialogResult result = await dialog.Result;

			if (!result.Cancelled)
			{
				await AggiuntaAnalita(selezioneAnalita.Analita);
			}
		}

		private async Task AggiuntaAnalita(Analita analita)
		{
			// Se l'analita è già presente e non è stato rimosso da operatore, chiede conferma dell'aggiunta
			if (Campionamento.Analiti.Exists(a => a.Codice == analita.Codice && !a.RimossoDaOperatore))
			{
				IDialogReference dialog = await DialogService.ShowDialogAsync<Conferma>("Analita già presente tra quelli selezionati. Aggiungerlo comunque?", new() { DismissTitle = "Annulla" });
				DialogResult result = await dialog.Result;
				if (result.Cancelled)
					return;
			}

			// Se l'analita è presente ma ha il flag rimosso impostato a false, il flag viene impostato a true.
			// Se non è presente è aggiunto.
			if (Campionamento.Analiti.Exists(a => a.Codice == analita.Codice))
			{
				var analitaPresente = Campionamento.Analiti.First(a => a.Codice == analita.Codice);
				if (analitaPresente.RimossoDaOperatore)
					analitaPresente.RimossoDaOperatore = false;
				else
				{
					// E' presente ma non è stato rimosso
					analita.AggiuntoDaOperatore = true;
					Campionamento.Analiti.Add(analita);
				}
			}
			else
			{
				// Non è presente, viene aggiunto
				analita.AggiuntoDaOperatore = true;
				Campionamento.Analiti.Add(analita);
			}

			await AggiornaCampionamento();
		}

		private async Task EliminaAnalita(Analita analita)
		{
			IDialogReference dialog = await DialogService.ShowDialogAsync<Conferma>("Eliminare l'analita?", new() { DismissTitle = "Annulla" });
			DialogResult result = await dialog.Result;
			if (result.Cancelled)
				return;

			// Se l'analita è stato aggiunto da operatore, viene eliminato.
			// altrimenti viene indicato come rimosso da operatore.
			if (analita.AggiuntoDaOperatore)
				Campionamento.Analiti.Remove(analita);
			else
				analita.RimossoDaOperatore = true;

			await AggiornaCampionamento();
		}

		#endregion Analiti

		#region Elimina campionamento

		private async Task RichiestaConfermaEliminaCampionamento()
		{
			IDialogReference dialog = await DialogService.ShowDialogAsync<Conferma>("Eliminare il campionamento?", new() { DismissTitle = "Annulla" });
			DialogResult result = await dialog.Result;
			if (!result.Cancelled)
			{
				await EliminaCampionamento();
			}
		}

		private async Task EliminaCampionamento()
		{
			var result = await campionamentiStorage.Eliminazione(Campionamento.IdCampionamento);
			if (result.IsSuccess)
			{
				Navigation.NavigateTo("./");
			}
			else
				EsitoComando = result.ErrorMessage;
		}

		#endregion Elimina campionamento

		#region Completa campionamento
		private async Task RichiestaConfermaCompletaCampionamento()
		{
			// Verifica dati obbligatori
			if (
				!Campionamento.OraPrelievo.HasValue
				||
				Campionamento.Prelevatori.Count == 0
				)
			{
				DatiMancanti = "Per concludere il campionamento inserire l'ora del prelievo e almeno un prelevatore.";
			}
			else
			{
				IDialogReference dialog = await DialogService.ShowDialogAsync<Conferma>("Completare il campionamento?", new() { DismissTitle = "Annulla" });
				DialogResult result = await dialog.Result;

				if (!result.Cancelled)
				{
					await CompletaCampionamento();
				}
			}
		}

		private async Task CompletaCampionamento()
		{
			var result = await campionamentiStorage.Dettagli(Campionamento.IdCampionamento);
			if (result.IsSuccess)
			{
				var campionamento = result.Data;
				campionamento.DataOraChiusuraCampionamento = DateTime.Now;
				result = await campionamentiStorage.Aggiornamento(campionamento);
				if (result.IsSuccess)
				{
					Navigation.NavigateTo("./");
				}
				else
					EsitoComando = result.ErrorMessage;
			}
			else
				EsitoComando = result.ErrorMessage;
		}

		#endregion Completa campionamento

		private async Task AggiornaCampionamento()
		{
			var result = await campionamentiStorage.Dettagli(Campionamento.IdCampionamento);
			if (result.IsSuccess)
			{
				var campionamento = result.Data;
				campionamento.PuntoDiPrelievoARPAL = Campionamento.PuntoDiPrelievoARPAL;
				campionamento.PuntoDiPrelievoInseritoDaOperatore = Campionamento.PuntoDiPrelievoInseritoDaOperatore;
				campionamento.DescrizioneAttivita = Campionamento.DescrizioneAttivita;
				campionamento.DataPrelievo = Campionamento.DataPrelievo;
				campionamento.OraPrelievo = Campionamento.OraPrelievo;
				campionamento.DataAperturaCampione = Campionamento.DataAperturaCampione;
				campionamento.OraAperturaCampione = Campionamento.OraAperturaCampione;
				campionamento.LuogoAperturaCampione = Campionamento.LuogoAperturaCampione;
                campionamento.EmailInvioVerbale = Campionamento.EmailInvioVerbale;
				campionamento.PECInvioVerbale = Campionamento.PECInvioVerbale;
				campionamento.CampionamentoCollegato = Campionamento.CampionamentoCollegato;
				campionamento.Prelevatori = Campionamento.Prelevatori;
				campionamento.Analiti = Campionamento.Analiti;
				campionamento.MisureInLoco = Campionamento.MisureInLoco;
				campionamento.CampioneBianco = Campionamento.CampioneBianco;
                campionamento.Note = Campionamento.Note;

                result = await campionamentiStorage.Aggiornamento(campionamento);
				if (!result.IsSuccess)
				{
					messaggioErrore = "Non è stato possibile aggiornare il campionamento";
					dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
					visualizzaComandoRicarica = true;
					visualizzaComandoChiudi = false;
					visualizzaComandoTornaHomePage = true;
				}
			}
			else
			{
				messaggioErrore = "Non è stato possibile caricare il campionamento";
				dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
				visualizzaComandoRicarica = true;
				visualizzaComandoChiudi = false;
				visualizzaComandoTornaHomePage = true;
			}
		}

		private void Chiudi()
		{
			Navigation.NavigateTo("/");
		}
	}
}
