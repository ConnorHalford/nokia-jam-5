using System.Collections.Generic;
using UnityEngine;

using static Solitaire.Locations;

namespace Solitaire
{
	// https://en.wikipedia.org/wiki/Klondike_(solitaire)
	// https://en.wikipedia.org/wiki/Glossary_of_patience_terms
	public class Solitaire : MonoBehaviour
	{
		[SerializeField] private Card _cardPrefab = null;

		private Card[] _deck = null;
		private List<Card> _stock = null;
		private int _stockIndex = -1;
		private Card[] _depotTopCards = null;

		private const int NUM_CARDS_IN_DECK = 52;

		private void Awake()
		{
			// Make all the cards
			_deck = new Card[NUM_CARDS_IN_DECK];
			int cardIndex = 0;
			for (int suitIndex = 0; suitIndex <= 3; ++suitIndex)
			{
				Suit suit = (Suit)suitIndex;
				for (int value = 1; value <= 13; ++value)
				{
					Card card = Instantiate(_cardPrefab);
					card.Init(suit, value);
					_deck[cardIndex++] = card;
				}
			}

			_depotTopCards = new Card[NUM_DEPOTS];

			Deal();
		}

		private void Deal()
		{
			Shuffle();

			// Deal the tableau
			int nextCardIndex = 0;
			for (int row = 0; row < NUM_DEPOTS; ++row)
			{
				for (int depot = row; depot < NUM_DEPOTS; ++depot)
				{
					Card card = _deck[nextCardIndex++];
					SetCardLocation(card, (Location)((int)Location.Depot1 + depot));
					if (depot == row)
					{
						card.SetFaceUp(true);
					}
				}
			}

			// Fill the stock
			int numRemaining = NUM_CARDS_IN_DECK - nextCardIndex;
			if (_stock == null)
			{
				_stock = new List<Card>(numRemaining);
			}
			else
			{
				_stock.Clear();
				_stock.Capacity = numRemaining;
			}
			while (nextCardIndex < NUM_CARDS_IN_DECK)
			{
				Card card = _deck[nextCardIndex++];
				SetCardLocation(card, Location.Stock);
				card.SetFaceUp(false);
				_stock.Add(card);
			}
			_stockIndex = 0;
		}

		private void SetCardLocation(Card card, Location location)
		{
			// Work out what card this card will be on top of, if any
			Card topCard = null;
			bool inDepot = IsDepot(location);
			int depotIndex = -1;
			if (inDepot)
			{
				depotIndex = DepotIndex(location);
				topCard = _depotTopCards[depotIndex];
			}

#if UNITY_EDITOR
			//Debug.Log($"Placing {card.name} at location {location} atop {((topCard == null) ? "nothing" : topCard.name)}", card);
#endif

			// Place the card
			card.SetLocation(location, cardBelow: topCard);

			// Track topmost card
			if (inDepot)
			{
				topCard = card.GetTopmost();
				_depotTopCards[depotIndex] = topCard;
			}
		}

		private void Shuffle()
		{
			// Fisher-Yates in-place shuffle
			for (int i = 0; i <= NUM_CARDS_IN_DECK - 2; ++i)
			{
				int j = Random.Range(i, NUM_CARDS_IN_DECK);		// Min inclusive, max exclusive. [min, max)
				Card temp = _deck[i];
				_deck[i] = _deck[j];
				_deck[j] = temp;
			}
		}
	}
}
