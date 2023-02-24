using UnityEngine;

namespace Solitaire
{
	public class Solitaire : MonoBehaviour
	{
		[SerializeField] private Card _cardPrefab = null;

		private Card[] _deck = null;

		private void Awake()
		{
			// Make all the cards
			_deck = new Card[52];
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

			Shuffle();
		}

		private void Shuffle()
		{
			// Fisher-Yates in-place shuffle
			for (int i = 0; i <= 50; ++i)
			{
				int j = Random.Range(i, 52);
				Card temp = _deck[i];
				_deck[i] = _deck[j];
				_deck[j] = temp;
			}
		}
	}
}
