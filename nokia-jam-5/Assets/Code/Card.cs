using UnityEngine;

using static Solitaire.Locations;

namespace Solitaire
{
	public enum Suit	// Must be alphabetical to match sprite atlas sub-sprites being alphabetised
	{
		Clubs = 0,
		Diamonds = 1,
		Hearts = 2,
		Spades = 3
	}

	public class Card : MonoBehaviour
	{
		[SerializeField] private Sprite[] _cardSprites = null;
		[SerializeField] private Suit _suit = Suit.Spades;
		[SerializeField] private int _value = 1;
		[SerializeField] private bool _faceUp = true;

		private SpriteRenderer _sprite = null;
		private Location _location = Location.Stock;
		private Card _cardBelowThis = null;
		private Card _cardAboveThis = null;

		public int RenderOrder	// Higher number is in front
		{
			get { return _sprite.sortingOrder; }
			set { _sprite.sortingOrder = value; }
		}

		public void Init(Suit suit, int value)
		{
			_suit = suit;
			_value = value;
			_faceUp = false;
			_location = Location.Stock;
			UpdateArt();
			RenderOrder = 0;
		}

		public void SetFaceUp(bool faceUp)
		{
			if (_faceUp != faceUp)
			{
				_faceUp = faceUp;
				UpdateArt();
			}
		}

		public void SetLocation(Location location, Card cardBelow)
		{
			_location = location;
			_cardBelowThis = cardBelow;
			if (_cardBelowThis != null)
			{
				_cardBelowThis._cardAboveThis = this;
				RenderOrder = _cardBelowThis.RenderOrder + 1;
				transform.position = _cardBelowThis.transform.position + CARD_Y_OFFSET;
			}
			else	// Vacant space
			{
				RenderOrder = 0;
				switch (_location)
				{
					case Location.Stock:		transform.position = POS_STOCK;			break;
					case Location.Waste1:		transform.position = POS_WASTE1;		break;
					case Location.Waste2:		transform.position = POS_WASTE2;		break;
					case Location.Waste3:		transform.position = POS_WASTE3;		break;
					case Location.Foundation1:	transform.position = POS_FOUNDATION1;	break;
					case Location.Foundation2:	transform.position = POS_FOUNDATION2;	break;
					case Location.Foundation3:	transform.position = POS_FOUNDATION3;	break;
					case Location.Foundation4:	transform.position = POS_FOUNDATION4;	break;
					case Location.Depot1:		transform.position = POS_DEPOT1;		break;
					case Location.Depot2:		transform.position = POS_DEPOT2;		break;
					case Location.Depot3:		transform.position = POS_DEPOT3;		break;
					case Location.Depot4:		transform.position = POS_DEPOT4;		break;
					case Location.Depot5:		transform.position = POS_DEPOT5;		break;
					case Location.Depot6:		transform.position = POS_DEPOT6;		break;
					case Location.Depot7:		transform.position = POS_DEPOT7;		break;
				}
			}
		}

		public Card GetTopmost()
		{
			Card topmost = this;
			while (topmost._cardAboveThis != null)
			{
				topmost = topmost._cardAboveThis;
			}
			return topmost;
		}

		private bool InStock()
		{
			return IsStock(_location);
		}

		private bool InWaste()
		{
			return IsWaste(_location);
		}

		private bool InFoundation()
		{
			return IsFoundation(_location);
		}

		private bool InDepot()
		{
			return IsDepot(_location);
		}

		private void UpdateArt()
		{
			if (_sprite == null)
			{
				_sprite = GetComponent<SpriteRenderer>();
			}
			// The sprites array is filled from the multi-sprite texture, which orders sub-sprites alphabetically.
			// Therefore the Suit enum is also alphabetical to allow simple indexing with the following order:
			// _Back, _Empty, Clubs 1-13, Diamonds 1-13, Hearts 1-13, Spades 1-13
			int index = 0;
			if (_faceUp)
			{
				const int OFFSET = 2;	// _Back, _Empty
				const int SPRITES_PER_SUIT = 13;
				index = OFFSET + (_value - 1) + ((int)_suit * SPRITES_PER_SUIT);
			}
			_sprite.sprite = _cardSprites[index];

#if UNITY_EDITOR
			// Give the GameObject a nice name in the hierarchy
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			switch (_value)
			{
				case 1:		sb.Append("Ace");	break;
				case 11:	sb.Append("Jack");	break;
				case 12:	sb.Append("Queen");	break;
				case 13:	sb.Append("King");	break;
				default:	sb.Append(_value);	break;
			}
			sb.Append(" of ");
			sb.Append(_suit);
			gameObject.name = sb.ToString();
#endif	// UNITY_EDITOR
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			_value = Mathf.Clamp(_value, 1, 13);

			// Setting a sprite within OnValidate works perfectly fine but logs a warning,
			// so this is a workaround to avoid unnecessary logging spam
			if (gameObject.scene.rootCount > 0)		// Only update art for an instance, not the prefab
			{
				UnityEditor.EditorApplication.delayCall -= UpdateArt;
				UnityEditor.EditorApplication.delayCall += UpdateArt;
			}
		}
#endif	// UNITY_EDITOR
	}
}
