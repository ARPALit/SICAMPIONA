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

using Microsoft.FluentUI.AspNetCore.Components;
using SICampiona.Model;
using SICampiona.Pages.Dialog;
using SICampiona.Services;

namespace SICampiona.Pages
{
    public partial class DuplicaCampionamento
    {
        #region Gestione errore
        private string messaggioErrore = null;
        private string dettagliErrore = null;
        private bool visualizzaComandoRicarica = false;
        private bool visualizzaComandoChiudi = false;
        private bool visualizzaComandoTornaHomePage = false;
        #endregion

        List<InformazioniCampionamento> campionamenti = [];

        protected override async Task OnInitializedAsync()
        {
            var result = await campionamentiStorage.Elenco();

            if (result.IsSuccess)
            {
                campionamenti = result.Data;
                campionamenti.Insert(0, new InformazioniCampionamento { IdCampionamento = 0, PuntoDiPrelievo = string.Empty });
            }
            else
            {
                messaggioErrore = "Non è stato possibile caricare l'elenco dei campionamenti";
                dettagliErrore = $"{result.ErrorMessage} - {result.ErrorDetails}";
                visualizzaComandoRicarica = true;
                visualizzaComandoChiudi = false;
                visualizzaComandoTornaHomePage = true;
            }
        }

        private async Task SelezioneCampionamento(InformazioniCampionamento campionamento)
        {
            if (campionamento.IdCampionamento == 0)
            {
                campionamentoSelezionato = null;
            }
            else
            {
                campionamentoSelezionato = campionamento;
                await CaricaTipiCampionamento(campionamento.CodiceMatrice);
            }
        }

        #region Punto di prelievo

        bool puntoInseritoDaOperatore = false; // indica se il punto è stato inserito dall'operatore oppure selezionato

        PuntoDiPrelievo puntoDiPrelievo;

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
                puntoInseritoDaOperatore = false;
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
                puntoInseritoDaOperatore = true;
                puntoDiPrelievo = inserimentoPuntoDiPrelievo;
            }
        }

        #endregion Punto di prelievo

        #region Duplica

        bool creazioneInCorso = false;

        bool creaCollegamento = false;

        InformazioniCampionamento campionamentoSelezionato = null;

        string tipoCampionamentoSelezionato = null;

        bool DuplicaDisabilitato => campionamentoSelezionato == null || puntoDiPrelievo == null;

        private async Task Duplica()
        {
            creazioneInCorso = true;
            RichiestaDuplicazioneCampionamento richiesta = new()
            {
                IdCampionamentoDaDuplicare = campionamentoSelezionato.IdCampionamento,
                CreaCollegamento = creaCollegamento,
                NuovaDataPrelievo = null,
                // Se tipoCampionamentoSelezionato è stringa vuota viene passato null
                TipoCampionamento = string.IsNullOrEmpty(tipoCampionamentoSelezionato) ? null : tipoCampionamentoSelezionato
            };
            if (puntoInseritoDaOperatore)
            {
                richiesta.PuntoDiPrelievoInseritoDaOperatore = puntoDiPrelievo;
                richiesta.PuntoDiPrelievoARPAL = null;
            }
            else
            {
                richiesta.PuntoDiPrelievoInseritoDaOperatore = null;
                richiesta.PuntoDiPrelievoARPAL = puntoDiPrelievo;
            }

            var result = await campionamentiStorage.Duplicazione(richiesta);
            if (result.IsSuccess)
            {
                Navigation.NavigateTo("/");
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
            Navigation.NavigateTo("/");
        }
    }
}
