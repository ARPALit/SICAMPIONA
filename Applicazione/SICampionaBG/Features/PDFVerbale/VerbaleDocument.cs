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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SICampionaBG.Features.CreazionePDFVerbale
{
	internal class VerbaleDocument(VerbaleModel model) : IDocument
	{
		public VerbaleModel Model { get; } = model;

		public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
		public DocumentSettings GetSettings() => DocumentSettings.Default;

		public void Compose(IDocumentContainer container)
		{
			container
				.Page(page =>
				{
					page.Margin(40);

					page.Header().Element(ComposeHeader);
					page.Content().Element(ComposeContent);
					page.Footer().AlignCenter().Text(x =>
					{
						x.CurrentPageNumber();
						x.Span(" / ");
						x.TotalPages();
						x.DefaultTextStyle(TextStyle.Default.FontSize(8));
					});
				});
		}

		void ComposeHeader(IContainer container)
		{
			container.Row(row =>
			{
				row.RelativeItem().Column(column =>
				{
					column.Item().AlignCenter().Text($"VERBALE DI CAMPIONAMENTO {Model.Matrice}").Style(TextStyle.Default.Bold().FontSize(12));
					column.Item().AlignCenter().Text($"{Model.Argomento}");
					column.Item().AlignCenter().Text($"{Model.DescrizioneTipoCampionamento}");
				});
			});
		}

		void ComposeContent(IContainer container)
		{
			container
				.DefaultTextStyle(x => x.FontSize(10))
				.Column(column =>
				{
					column.Item().Element(ComposeEnteNumeroOperatoreData);
					column.Item().Element(ComposePuntoDiPrelievo);
					column.Item().Element(ComposeAnalisi);
					column.Item().Element(ComposeDescrizionePrelevatoriDataVerbaleEmailCampionamentoCollegato);
					column.Item().PaddingTop(5).AlignLeft().Text($"{Model.NoteTipoCampionamento}");
                    column.Item().Element(ComposeNote);
                });
		}

		void ComposeEnteNumeroOperatoreData(IContainer container)
		{
			container
				.PaddingTop(30)
				.PaddingBottom(20)
				.Row(row =>
				{
					row.RelativeItem().Column(column =>
					{
						column.Item().Text($"{Model.Ente}").FontSize(12).Bold();
					});

					row.RelativeItem().Column(column =>
					{
						column.Item().Text($"N° verbale: {Model.SiglaVerbale}");
						column.Item().Text($"Operatore: {Model.Operatore}");
						column.Item().Text($"Data e ora campionamento: {Model.DataOraCampionamento}");
						column.Item().Text($"Data e ora apertura campione: {Model.DateOraAperturaCampione}");
                        column.Item().Text($"Luogo apertura campione: {Model.LuogoAperturaCampione}");
                    });
				});
		}

		void ComposePuntoDiPrelievo(IContainer container)
		{
			container
				.Column(column =>
				{
					column.Item().PaddingBottom(5).AlignCenter().Text("PUNTO DI PRELIEVO").Style(TextStyle.Default.Bold());
					column.Item().Table(table =>
					{
						table.ColumnsDefinition(columns =>
						{
							columns.ConstantColumn(100);
							columns.RelativeColumn(2);
							columns.RelativeColumn(2);
							columns.RelativeColumn(1);
						});

						table.Cell().Element(TitoloCellStyle).Text("Codice");
						table.Cell().Element(TitoloCellStyle).Text("Denominazione");
						table.Cell().Element(TitoloCellStyle).Text("Indirizzo");
						table.Cell().Element(TitoloCellStyle).Text("Comune");
						table.Cell().Element(DefaultCellStyle).Text(Model.PuntoDiPrelievoCodice);
						table.Cell().Element(DefaultCellStyle).Text(Model.PuntoDiPrelievoDenominazione);
						table.Cell().Element(DefaultCellStyle).Text(Model.PuntoDiPrelievoIndirizzo);
						table.Cell().Element(DefaultCellStyle).Text(Model.PuntoDiPrelievoComune);
					});
				});
		}

		void ComposeAnalisi(IContainer container)
		{
			container
				.Column(column =>
				{
					column.Item().AlignCenter().Text("ANALISI").Style(TextStyle.Default.Bold().FontSize(12));
					column.Item().Element(ComposeMisureInLoco);
					column.Item().Element(ComposeParametri);
				});
		}

		void ComposeMisureInLoco(IContainer container)
		{
			container
				.Column(column =>
			{
				column.Item().PaddingBottom(5).AlignCenter().Text("MISURE IN LOCO").Style(TextStyle.Default.Bold());
				column.Item().Table(table =>
				{
					table.ColumnsDefinition(columns =>
					{
						columns.RelativeColumn(2);
						columns.RelativeColumn(2);
						columns.RelativeColumn(1);
						columns.RelativeColumn(2);
					});

					table.Header(header =>
					{
						header.Cell().Element(TitoloCellStyle).Text("Descrizione");
						header.Cell().Element(TitoloCellStyle).Text("Metodo");
						header.Cell().Element(TitoloCellStyle).Text("Valore/U.M.");
						header.Cell().Element(TitoloCellStyle).Text("Note");
					});

					foreach (var misuraInLoco in Model.MisureInLoco)
					{
						table.Cell().Element(DefaultCellStyle).Text(misuraInLoco.Descrizione);
						table.Cell().Element(DefaultCellStyle).Text(misuraInLoco.Metodo);
						table.Cell().Element(DefaultCellStyle).Text(misuraInLoco.ValoreConUM);
						table.Cell().Element(DefaultCellStyle).Text(misuraInLoco.Note);
					}
				});
			});
		}

		void ComposeParametri(IContainer container)
		{
			container.Column(column =>
			{
				column.Item().AlignCenter().Text("PARAMETRI RICHIESTI").Style(TextStyle.Default.Bold());
				column.Item().Table(table =>
				{
					table.ColumnsDefinition(columns =>
					{
						columns.RelativeColumn(2);
						columns.RelativeColumn(2);
						columns.RelativeColumn(2);
						columns.RelativeColumn(1);
						columns.RelativeColumn(1);
					});

					table.Header(header =>
					{
						header.Cell().Element(TitoloCellStyle).Text("Pacchetto");
						header.Cell().Element(TitoloCellStyle).Text("Descrizione");
						header.Cell().Element(TitoloCellStyle).Text("Metodo");
						header.Cell().Element(TitoloCellStyle).Text("U.M.");
						header.Cell().Element(TitoloCellStyle).Text("Limite");
					});

					foreach (var parametro in Model.Parametri)
					{
						table.Cell().Element(DefaultCellStyle).Text(parametro.Pacchetto);
						table.Cell().Element(DefaultCellStyle).Text(parametro.Descrizione);
						table.Cell().Element(DefaultCellStyle).Text(parametro.Metodo);
						table.Cell().Element(DefaultCellStyle).Text(parametro.UM);
						table.Cell().Element(DefaultCellStyle).Text(parametro.Limite);
					}
				});
			});
		}

		void ComposeDescrizionePrelevatoriDataVerbaleEmailCampionamentoCollegato(IContainer container)
		{
			container
				.PaddingTop(10)
				.Row(row =>
				{
					row.RelativeItem().Column(column =>
					{
                        column.Item().Text(x =>
                        {
                            x.Span("Descrizione attività: ").Bold();
                            x.Span($"{Model.DescrizioneAttivita}");
                        });
                        column.Item().Text(x =>
						{
							x.Span("Campione bianco: ").Bold();
							x.Span($"{Model.CampioneBianco}");
						});
						column.Item().Text(x =>
						{
							x.Span("Prelevatori: ").Bold();
							x.Span($"{Model.Prelevatori}");
						});
						column.Item().Text(x =>
						{
							x.Span("Date e ora verbale: ").Bold();
							x.Span($"{Model.DateOraVerbale}");
						});
					});
					row.RelativeItem().Column(column =>
					{
						column.Item().Text(x =>
						{
							x.Span("Email invio verbale: ").Bold();
							x.Span($"{Model.EmailInvioVerbale}");
						});
						column.Item().Text(x =>
						{
							x.Span("PEC invio verbale: ").Bold();
							x.Span($"{Model.PecInvioVerbale}");
						});
						column.Item().Text(x =>
						{
							x.Span("Campionamento collegato: ").Bold();
							x.Span($"{Model.CampionamentoCollegato}");
						});
					});
				});
		}

        void ComposeNote(IContainer container)
        {
            container
                .PaddingTop(5)
				.AlignLeft()
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(x =>
                        {
                            x.Span("Note: ").Bold();
                            x.Span($"{Model.Note}");
                        });
                    });
                });
        }

        #region Stili celle

        static IContainer DefaultCellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Lighten1)
				.PaddingVertical(3)
				.PaddingHorizontal(5)
				.AlignCenter()
				.AlignMiddle();
		}

		static IContainer TitoloCellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Lighten1)
				.PaddingVertical(3)
				.PaddingHorizontal(5)
				.AlignCenter()
				.AlignMiddle()
				.DefaultTextStyle(TextStyle.Default.Bold());
		}

		#endregion Stili celle
	}
}
