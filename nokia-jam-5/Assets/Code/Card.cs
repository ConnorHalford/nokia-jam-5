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

	public enum SuitColor
	{
		Black,
		Red
	}

	public class Card : MonoBehaviour
	{
		[SerializeField] private Sprite _backSprite = null;
		[SerializeField] private Sprite[] _cardSprites = null;
		[SerializeField] private Suit _suit = Suit.Spades;
		[SerializeField] private int _value = 1;
		[SerializeField] private bool _faceUp = true;

		// Don't forget to reset state in Init
		private Solitaire _solitaire = null;
		private SpriteRenderer _sprite = null;
		private Location _location = Location.Stock;
		private Card _cardBehindThis = null;
		private Card _cardInFrontOfThis = null;

		public Suit Suit				{ get { return _suit; } }
		public int Value				{ get { return _value; } }
		public bool IsFaceUp			{ get { return _faceUp; } }
		public Location Location		{ get { return _location; } }
		public Card CardBehindThis		{ get { return _cardBehindThis; } }
		public Card CardInFrontOfThis	{ get { return _cardInFrontOfThis; } }

		public int RenderOrder	// Higher number is in front
		{
			get { return _sprite.sortingOrder; }
			set { _sprite.sortingOrder = value; }
		}

		public SuitColor SuitColor
		{
			get { return (_suit == Suit.Spades || _suit == Suit.Clubs) ? SuitColor.Black : SuitColor.Red; }
		}

		public void Init(Solitaire solitaire, Suit suit, int value)
		{
			_solitaire = solitaire;
			_suit = suit;
			_value = value;
			_faceUp = false;
			_location = Location.Stock;
			_cardBehindThis = null;
			_cardInFrontOfThis = null;
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

		public void SetLocation(Location location, Card currentTopmost)
		{
			// Clear previous connection
			if (_cardBehindThis != null)
			{
				_cardBehindThis._cardInFrontOfThis = null;
				_cardBehindThis = null;
			}

			// Move to location
			_location = location;
			_cardBehindThis = currentTopmost;
			if (_cardBehindThis != null)
			{
				_cardBehindThis._cardInFrontOfThis = this;
				RenderOrder = _cardBehindThis.RenderOrder + 1;
				transform.position = _cardBehindThis.transform.position;
				if (IsDepot(location))
				{
					transform.position += CARD_Y_OFFSET;
				}
				else if (IsWaste(location))
				{
					transform.position += CARD_X_OFFSET;
				}
			}
			else	// Vacant space
			{
				RenderOrder = 0;
				transform.position = LocationBasePosition(_location);
			}

			// Move cards in front of this
			Card next = _cardInFrontOfThis;
			Vector3 position = transform.position;
			int renderOrder = RenderOrder + 1;
			while (next != null)
			{
				position += CARD_Y_OFFSET;
				next._location = _location;
				next.RenderOrder = renderOrder++;
				next.transform.position = position;
				next = next._cardInFrontOfThis;
			}
		}

		public Card GetTopmost()
		{
			Card topmost = this;
			while (topmost._cardInFrontOfThis != null)
			{
				topmost = topmost._cardInFrontOfThis;
			}
			return topmost;
		}

		public Card GetBottommostMovable()
		{
			Card bottommostMovable = this;
			while (bottommostMovable.CardBehindThis != null && bottommostMovable.CardBehindThis.CanBeMoved())
			{
				bottommostMovable = bottommostMovable.CardBehindThis;
			}
			return bottommostMovable;
		}

		public bool CanBeMoved()
		{
			bool canBeMoved = false;
			if (InDepot())
			{
				if (_faceUp)
				{
					// Can be moved if all cards in front of it are alternating colors and descending value
					Card current = this;
					Card next = _cardInFrontOfThis;
					canBeMoved = true;
					while (next != null)
					{
						if (next.SuitColor == current.SuitColor || next.Value != current.Value - 1)
						{
							canBeMoved = false;
							break;
						}
						current = next;
						next = current._cardInFrontOfThis;
					}
				}
			}
			else
			{
				Card topmost = _solitaire.GetTopmostCard(_location);
				canBeMoved = (this == topmost);
			}
			return canBeMoved;
		}

		// Assumes that CanBeMoved is true, that should be checked first
		public bool CanBeMovedTo(Location location)
		{
			if (IsDepot(location))
			{
				Card topmost = _solitaire.GetTopmostCard(location);
				if (topmost == null)
				{
					// Only Kings can be placed in vacant depots
					return _value == 13;
				}
				return CanBeMovedTo(topmost);
			}
			if (IsFoundation(location))
			{
				Card topmost = _solitaire.GetTopmostCard(location);
				if (topmost == null)
				{
					// Only Aces can be placed in vacant foundations
					return _value == 1;
				}
				return CanBeMovedTo(topmost);
			}
			return false;
		}

		// Assumes that CanBeMoved is true, that should be checked first
		public bool CanBeMovedTo(Card other)
		{
			if (other.InDepot())
			{
				// Alternating colors and descending value (can move stacks)
				return other.SuitColor != this.SuitColor && other.Value == this.Value + 1;
			}
			if (other.InFoundation())
			{
				// Matching suit and ascending value (only 1 card at a time)
				return other.Suit == this.Suit && other.Value == this.Value - 1 && _cardInFrontOfThis == null;
			}
			return false;
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
			// Clubs 1-13, Diamonds 1-13, Hearts 1-13, Spades 1-13
			if (_faceUp)
			{
				const int SPRITES_PER_SUIT = 13;
				int index = (_value - 1) + ((int)_suit * SPRITES_PER_SUIT);
				_sprite.sprite = _cardSprites[index];
			}
			else
			{
				_sprite.sprite = _backSprite;
			}

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
