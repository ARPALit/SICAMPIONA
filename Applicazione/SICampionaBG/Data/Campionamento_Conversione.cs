

using SICampionaBG.Model;
using System.Globalization;

namespace SICampionaBG.Data
{
	public partial class CampionamentoDB
	{
		public static explicit operator Campionamento(CampionamentoDB campionamentoDB)
		{
			var campionamento = new Campionamento
			{
				IdCampionamento = (int)campionamentoDB.ID_CAMPIONAMENTO,
				Ente = new Ente()
				{
					Codice = campionamentoDB.ENTE_CODICE,
					Denominazione = campionamentoDB.ENTE_DENOMINAZIONE
				},
				Matrice = new Matrice()
				{
					Codice = campionamentoDB.MATRICE_CODICE,
					Denominazione = campionamentoDB.MATRICE_DENOMINAZIONE
				},
				Argomento = new Argomento()
				{
					Codice = campionamentoDB.ARGOMENTO_CODICE,
					Denominazione = campionamentoDB.ARGOMENTO_DENOMINAZIONE
				},
				Cliente = new Cliente()
				{
					Codice = campionamentoDB.CLIENTE_CODICE,
					Denominazione = campionamentoDB.CLIENTE_DENOMINAZIONE,
					CodiceFiscale = campionamentoDB.CLIENTE_CODICE_FISCALE,
					PartitaIVA = campionamentoDB.CLIENTE_PARTITA_IVA
				},
				DescrizioneAttivita = campionamentoDB.DESCRIZIONE_ATTIVITA,
				DataOraCreazioneCampionamento = campionamentoDB.DATA_ORA_CREAZIONE,
				DataPrelievo = DateOnly.FromDateTime(campionamentoDB.DATA_PRELIEVO),
				OraPrelievo = !string.IsNullOrEmpty(campionamentoDB.ORA_PRELIEVO) ? TimeOnly.ParseExact(campionamentoDB.ORA_PRELIEVO, "HH:mm", CultureInfo.CurrentCulture) : null,
				DataOraChiusuraCampionamento = campionamentoDB.DATA_ORA_CHIUSURA,
				UltimoAggiornamento = new UltimoAggiornamento()
				{
					DataOra = campionamentoDB.ULTIMO_AGG_DATA_ORA,
					Operatore = new Operatore()
					{
						Codice = campionamentoDB.ULTIMO_AGG_OPER_CODICE,
						Cognome = campionamentoDB.ULTIMO_AGG_OPER_COGNOME,
						Nome = campionamentoDB.ULTIMO_AGG_OPER_NOME
					},
				},
				SiglaVerbale = campionamentoDB.SIGLA_VERBALE,
				NumeroCampione = campionamentoDB.NUMERO_CAMPIONE,
				Eliminato = campionamentoDB.ELIMINATO,
				FileVerbale = campionamentoDB.FILE_VERBALE,
				FileRapportoDiProva = campionamentoDB.FILE_RAPPORTO_DI_PROVA,
				StatoCampione = campionamentoDB.STATO_CAMPIONE,
				SuffissoCartella = campionamentoDB.SUFFISSO_CARTELLA,
				DataAperturaCampione = campionamentoDB.DATA_APERTURA_CAMPIONE.HasValue ? DateOnly.FromDateTime(campionamentoDB.DATA_APERTURA_CAMPIONE.Value) : null,
				OraAperturaCampione = !string.IsNullOrEmpty(campionamentoDB.ORA_APERTURA_CAMPIONE) ? TimeOnly.ParseExact(campionamentoDB.ORA_APERTURA_CAMPIONE, "HH:mm", CultureInfo.InvariantCulture) : null,
				EmailInvioVerbale = campionamentoDB.EMAIL_INVIO_VERBALE,
				PecInvioVerbale = campionamentoDB.PEC_INVIO_VERBALE,
				CampionamentoCollegato = campionamentoDB.CAMPIONAMENTO_COLLEGATO,
				TipoCampionamento = campionamentoDB.TIPO_CAMPIONAMENTO,
                StatoEmailInvioVerbale = campionamentoDB.STATO_EMAIL_INVIO_VERBALE,
                TemperaturaAccettazione = campionamentoDB.TEMPERATURA_ACCETTAZIONE,
				CampioneBianco = campionamentoDB.CAMPIONE_BIANCO,
                LuogoAperturaCampione = campionamentoDB.LUOGO_APERTURA_CAMPIONE,
                Note = campionamentoDB.NOTE
            };

			if (campionamentoDB.PUNTO_ARPAL_CODICE != null)
			{
				campionamento.PuntoDiPrelievoARPAL = new PuntoDiPrelievo()
				{
					Codice = campionamentoDB.PUNTO_ARPAL_CODICE,
					Denominazione = campionamentoDB.PUNTO_ARPAL_DENOMINAZIONE,
					Indirizzo = campionamentoDB.PUNTO_ARPAL_INDIRIZZO,
					Comune = new Comune()
					{
						CodiceIstat = campionamentoDB.PUNTO_ARPAL_COMUNE_CODICE,
						Denominazione = campionamentoDB.PUNTO_ARPAL_COMUNE_DENOMINAZ
					}
				};
				if (campionamentoDB.PUNTO_ARPAL_LATITUDINE != null || campionamentoDB.PUNTO_ARPAL_LONGITUDINE != null)
				{
					campionamento.PuntoDiPrelievoARPAL.Coordinate = new Coordinate()
					{
						Latitudine = campionamentoDB.PUNTO_ARPAL_LATITUDINE,
						Longitudine = campionamentoDB.PUNTO_ARPAL_LONGITUDINE
					};
				}
			}

			if (campionamentoDB.PUNTO_OPER_CODICE != null)
			{
				campionamento.PuntoDiPrelievoInseritoDaOperatore = new PuntoDiPrelievo()
				{
					Codice = campionamentoDB.PUNTO_OPER_CODICE,
					Denominazione = campionamentoDB.PUNTO_OPER_DENOMINAZ,
					Indirizzo = campionamentoDB.PUNTO_OPER_INDIRIZZO,
					Comune = new Comune()
					{
						CodiceIstat = campionamentoDB.PUNTO_OPER_COMUNE_CODICE,
						Denominazione = campionamentoDB.PUNTO_OPER_COMUNE_DENOMINAZ
					}
				};
				if (campionamentoDB.PUNTO_OPER_LATITUDINE != null || campionamentoDB.PUNTO_OPER_LONGITUDINE != null)
				{
					campionamento.PuntoDiPrelievoInseritoDaOperatore.Coordinate = new Coordinate()
					{
						Latitudine = campionamentoDB.PUNTO_OPER_LATITUDINE,
						Longitudine = campionamentoDB.PUNTO_OPER_LONGITUDINE
					};
				}
			}

			campionamento.Prelevatori = [];
			foreach (var prelevatore in campionamentoDB.PRELEVATORI)
			{
				campionamento.Prelevatori.Add(new Model.Prelevatore()
				{
					Codice = prelevatore.CODICE,
					Cognome = prelevatore.COGNOME,
					Nome = prelevatore.NOME
				});

			}

			campionamento.MisureInLoco = [];
			foreach (var misuraInLoco in campionamentoDB.MISURE_IN_LOCO)
			{
				campionamento.MisureInLoco.Add(new Model.MisuraInLoco()
				{
					Codice = misuraInLoco.CODICE,
					Descrizione = misuraInLoco.DESCRIZIONE,
					CodiceMetodo = misuraInLoco.METODO_CODICE,
					DescrizioneMetodo = misuraInLoco.METODO_DESCRIZIONE,
					UnitaMisura = misuraInLoco.UNITA_MISURA,
					Valore = misuraInLoco.VALORE
				});
			}

			campionamento.Analiti = [];
			foreach (var analita in campionamentoDB.ANALITI)
			{
				var contenitori = new List<Model.Contenitore>();
				foreach (var contenitore in analita.CONTENITORI)
				{
					contenitori.Add(new Model.Contenitore()
					{
						Tipo = contenitore.TIPO,
						Quantita = (int)contenitore.QUANTITA
					});
				}
				campionamento.Analiti.Add(new Model.Analita()
				{
					Codice = analita.CODICE,
					Descrizione = analita.DESCRIZIONE,
					CodiceMetodo = analita.METODO_CODICE,
					DescrizioneMetodo = analita.METODO_DESCRIZIONE,
					CodicePacchetto = analita.PACCHETTO_CODICE,
					DescrizionePacchetto = analita.PACCHETTO_DESCRIZIONE,
					ValoreLimite = analita.VALORE_LIMITE,
					UnitaMisura = analita.UNITA_MISURA,
					Contenitori = contenitori,
					RimossoDaOperatore = analita.RIMOSSO_DA_OPER ?? false
				});
			}

			if (campionamentoDB.NOTA_ACCETTAZIONE != null)
			{
				campionamento.NotaAccettazione = new NotaAccettazione()
				{
					Testo = campionamentoDB.NOTA_ACCETTAZIONE,
					UltimoAggiornamento = new UltimoAggiornamento()
					{
						DataOra = campionamentoDB.NOTA_ACCETTAZIONE_DATA_ORA.Value,
						Operatore = new Operatore()
						{
							Codice = campionamentoDB.NOTA_ACCETTAZIONE_OPER_CODICE,
							Cognome = campionamentoDB.NOTA_ACCETTAZIONE_OPER_COGNOME,
							Nome = campionamentoDB.NOTA_ACCETTAZIONE_OPER_NOME
						}
					}
				};
			}

			// PDF verbale e rapporto di prova non sono valorizzati
			// perché salvati su file e aggiunti al compionamento come
			// stringhe base64 solo nelle API che espongono i campionamenti
			// a sistemi esterni

			return campionamento;
		}
	}
}
