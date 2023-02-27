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
		[SerializeField] private Transform _cardParent = null;
		[SerializeField] private SpriteRenderer[] _emptyMarkers = null;
		[SerializeField] private SpriteRenderer _pointer = null;
		[SerializeField] private Sprite _pointerSpriteNormal = null;
		[SerializeField] private Sprite _pointerSpriteSelected = null;
		[SerializeField] private Transform _camera = null;
		[SerializeField] private Menu _menu = null;

		private Inputs _input = null;

		// Card state. Don't forget to reset in Deal
		private Card[] _deck = null;
		private List<Card> _stock = null;
		private List<Card> _waste = null;
		private Card[] _depotTopmost = null;
		private Card[] _foundationTopmost = null;

		// Pointer state. Don't forget to reset in Deal
		private Location _pointerLocation = DEFAULT_LOCATION;
		private Vector3 _pointerPosition = Vector3.zero;
		private bool _pointerRetracted = false;
		private float _timePointerAlternated = -1.0f;
		private Card _pointerCard = null;
		private Card _pointerSelection = null;

		// Options
		private int _numCardsToDrawFromStock = 3;
		private bool _canOnlyPlaceKingsInVacancies = true;
		private bool _canTakeFromFoundation = true;
		private bool _tableauFaceDown = true;
		private bool _canOnlyStackAlternatingColors = true;

		private const int NUM_CARDS_IN_DECK = 52;
		private const Location DEFAULT_LOCATION = Location.Depot4;
		private const float POINTER_DURATION = 0.4f;

		public Inputs Input { get { return _input; } }
		public int NumCardsToDrawFromStock
		{
			get { return _numCardsToDrawFromStock; }
			set
			{
				if (value < 1)
				{
					value = NUM_WASTES;
				}
				else if (value > NUM_WASTES)
				{
					value = 1;
				}
				_numCardsToDrawFromStock = value;
			}
		}
		public bool CanOnlyPlaceKingsInVacancies
		{
			get { return _canOnlyPlaceKingsInVacancies; }
			set { _canOnlyPlaceKingsInVacancies = value; }
		}
		public bool CanTakeFromFoundation
		{
			get { return _canTakeFromFoundation; }
			set { _canTakeFromFoundation = value; }
		}
		public bool TableauFaceDown
		{
			get { return _tableauFaceDown; }
			set
			{
				_tableauFaceDown = value;
				UpdateTableauFaceDown();
			}
		}
		public bool CanOnlyStackAlternatingColors
		{
			get { return _canOnlyStackAlternatingColors; }
			set { _canOnlyStackAlternatingColors = value; }
		}

		private void Awake()
		{
			// Mark possible card locations
			_emptyMarkers[0].transform.position = POS_STOCK;
			_emptyMarkers[1].transform.position = POS_WASTE1;
			_emptyMarkers[2].transform.position = POS_FOUNDATION1;
			_emptyMarkers[3].transform.position = POS_FOUNDATION2;
			_emptyMarkers[4].transform.position = POS_FOUNDATION3;
			_emptyMarkers[5].transform.position = POS_FOUNDATION4;
			_emptyMarkers[6].transform.position = POS_DEPOT1;
			_emptyMarkers[7].transform.position = POS_DEPOT2;
			_emptyMarkers[8].transform.position = POS_DEPOT3;
			_emptyMarkers[9].transform.position = POS_DEPOT4;
			_emptyMarkers[10].transform.position = POS_DEPOT5;
			_emptyMarkers[11].transform.position = POS_DEPOT6;
			_emptyMarkers[12].transform.position = POS_DEPOT7;

			// Make all the cards
			_deck = new Card[NUM_CARDS_IN_DECK];
			int cardIndex = 0;
			for (int suitIndex = 0; suitIndex <= 3; ++suitIndex)
			{
				Suit suit = (Suit)suitIndex;
				for (int value = 1; value <= 13; ++value)
				{
					Card card = Instantiate(_cardPrefab, _cardParent);
					card.Init(this, suit, value);
					_deck[cardIndex++] = card;
				}
			}

			// Make the tableau
			_depotTopmost = new Card[NUM_DEPOTS];
			_foundationTopmost = new Card[NUM_FOUNDATIONS];
			Deal();

			// Enable input
			_input = new Inputs();
			_input.Enable();
		}

		private void Update()
		{
			if (_menu.State == MenuState.Closed)
			{
				ProcessInput();
			}

			// Pointer animation
			float time = Time.unscaledTime;
			if (_timePointerAlternated <= 0.0f)
			{
				_timePointerAlternated = time;	// Skip over startup lag
			}
			else if (time - _timePointerAlternated >= POINTER_DURATION)
			{
				_timePointerAlternated = time + time - _timePointerAlternated - POINTER_DURATION;	// Preserve overflow
				_pointerRetracted = !_pointerRetracted;
				UpdatePointerPosition();
			}
		}

		private void ProcessInput()
		{
			// Navigation input. Keep it simple and just prioritise different directions
			bool right = _input.Game.Right.WasPerformedThisFrame();
			bool left = _input.Game.Left.WasPerformedThisFrame();
			bool up = _input.Game.Up.WasPerformedThisFrame();
			bool down = _input.Game.Down.WasPerformedThisFrame();
			if (right)
			{
				// Move pointer rightwards to next valid target, wrapping around from right to left
				if (IsDepot(_pointerLocation))
				{
					Location location = Location.Depot1;
					if (_pointerLocation != Location.Depot7)
					{
						location = (Location)((int)_pointerLocation + 1);
					}
					NavigateToDepot(location);
				}
				else if (IsFoundation(_pointerLocation))
				{
					Location location = Location.Stock;
					if (_pointerLocation != Location.Foundation4)
					{
						location = (Location)((int)_pointerLocation + 1);
					}
					PointTo(location);
				}
				else if (IsStock(_pointerLocation))
				{
					PointTo(Location.Waste1);
				}
				else if (IsWaste(_pointerLocation))
				{
					PointTo(Location.Foundation1);
				}
			}
			else if (left)
			{
				// Move pointer leftwards to next valid target, wrapping around from left to right
				if (IsDepot(_pointerLocation))
				{
					Location location = Location.Depot7;
					if (_pointerLocation != Location.Depot1)
					{
						location = (Location)((int)_pointerLocation - 1);
					}
					NavigateToDepot(location);
				}
				else if (IsFoundation(_pointerLocation))
				{
					Location location = Location.Waste1;
					if (_pointerLocation != Location.Foundation1)
					{
						location = (Location)((int)_pointerLocation - 1);
					}
					PointTo(location);
				}
				else if (IsStock(_pointerLocation))
				{
					PointTo(Location.Foundation4);
				}
				else if (IsWaste(_pointerLocation))
				{
					PointTo(Location.Stock);
				}
			}
			else if (up)
			{
				// Move pointer upwards to next valid target, wrapping around from top to bottom
				if (IsDepot(_pointerLocation))
				{
					// Move up to earlier card in stack that can be moved
					if (_pointerCard != null && _pointerCard.CardBehindThis != null
						&& _pointerCard.CardBehindThis.CanBeMoved(_canOnlyStackAlternatingColors))
					{
						PointTo(_pointerCard.CardBehindThis);
					}
					// Move up to stock/waste/foundation
					else
					{
						Location location = GetVerticalDestination(_pointerLocation);
						PointTo(location);
					}
				}
				else
				{
					// Wrap around to topmost card or vacant depot
					Location location = GetVerticalDestination(_pointerLocation);
					PointTo(location);
				}
			}
			else if (down)
			{
				// Move pointer downwards to next valid target, wrapping around from bottom to top
				if (IsDepot(_pointerLocation))
				{
					// Move down to later card in stack that can be moved
					if (_pointerCard != null && _pointerCard.CardInFrontOfThis != null)
					{
						PointTo(_pointerCard.CardInFrontOfThis);
					}
					// Wrap around to stock/waste/foundation
					else
					{
						Location location = GetVerticalDestination(_pointerLocation);
						PointTo(location);
					}
				}
				else
				{
					// Move down to bottommost card in stack that can be moved, or vacant depot
					Location location = GetVerticalDestination(_pointerLocation);
					NavigateToDepot(location);
				}
			}

			// Action inputs
			bool select = _input.Game.Select.WasPerformedThisFrame();
			bool back = _input.Game.Back.WasPerformedThisFrame();
			bool draw = _input.Game.Draw.WasPerformedThisFrame();
			if (select)
			{
				if (_pointerSelection == null)
				{
					if (IsStock(_pointerLocation))
					{
						DrawFromStock();
					}
					else if (_pointerCard != null)
					{
						// Pick up card
						bool foundation = IsFoundation(_pointerLocation);
						if (!foundation || _canTakeFromFoundation)
						{
							SetPointerSelection(_pointerCard);
						}
					}
				}
				else
				{
					// Move and drop card
					if (_pointerSelection.CanBeMovedTo(_pointerLocation, _canOnlyStackAlternatingColors, _canOnlyPlaceKingsInVacancies))
					{
						// Track/reveal new topmost depot card
						if (IsDepot(_pointerSelection.Location))
						{
							Card newTopmost = null;
							if (_pointerSelection.CardBehindThis != null)
							{
								_pointerSelection.CardBehindThis.SetFaceUp(true, natural: true);
								newTopmost = _pointerSelection.CardBehindThis;
							}
							int index = DepotIndex(_pointerSelection.Location);
							_depotTopmost[index] = newTopmost;
						}
						// Track new topmost foundation card
						else if (IsFoundation(_pointerSelection.Location))
						{
							Card newTopmost = null;
							if (_pointerSelection.CardBehindThis != null)
							{
								newTopmost = _pointerSelection.CardBehindThis;
							}
							int index = FoundationIndex(_pointerSelection.Location);
							_foundationTopmost[index] = newTopmost;
						}
						// Track new topmost waste card
						else if (IsWaste(_pointerSelection.Location))
						{
							int numCardsInWaste = _waste.Count;
							_waste.RemoveAt(numCardsInWaste - 1);
							--numCardsInWaste;

							// Cards atop the waste are revealed in sets, with the previous set being disabled so
							// that it isn't visible behind the current set. If the new topmost waste card is
							// disabled, that means the whole topmost set has been removed, so the next set should
							// be enabled again to make it visible. We also want to reposition them in case the
							// option for how many cards to draw from the waste per set has been changed since the
							// cards were first positioned in the waste
							if (numCardsInWaste > 0 && !_waste[numCardsInWaste - 1].isActiveAndEnabled)
							{
								int numCardsToEnable = Mathf.Min(_numCardsToDrawFromStock, numCardsInWaste);
								Location wasteLocation = Location.Waste3;
								if (numCardsToEnable == 2)
								{
									wasteLocation = Location.Waste2;
								}
								else if (numCardsToEnable == 1)
								{
									wasteLocation = Location.Waste1;
								}
								for (int i = 0; i < numCardsToEnable; ++i)
								{
									Card card = _waste[numCardsInWaste - 1 - i];
									card.gameObject.SetActive(true);
									SetCardLocation(card, (Location)((int)wasteLocation - i));
									card.RenderOrder = i;
								}
							}
						}

						// Move held card
						SetCardLocation(_pointerSelection, _pointerLocation);

						// Point to new topmost foundation
						if (IsFoundation(_pointerLocation))
						{
							PointTo(_pointerSelection);
						}

						// Check win condition: Kings on all foundations
						bool won = true;
						for (int i = 0; i < NUM_FOUNDATIONS; ++i)
						{
							if (_foundationTopmost[i] == null || _foundationTopmost[i].Value != 13)
							{
								won = false;
								break;
							}
						}
						if (won)
						{
							_menu.SetState(MenuState.Win);
						}
					}
					// Drop held card
					SetPointerSelection(null);
				}
			}
			else if (back)
			{
				if (_pointerSelection != null)
				{
					// Drop card
					SetPointerSelection(null);
				}
				else
				{
					// Open the menu
					_menu.SetState(MenuState.Pause);
				}
			}
			else if (draw)
			{
				DrawFromStock();
			}
		}

		private Location GetVerticalDestination(Location location)
		{
			switch (location)
			{
				case Location.Stock:		return Location.Depot1;
				case Location.Waste1:
				case Location.Waste2:		return Location.Depot2;
				case Location.Waste3:		return Location.Depot3;
				case Location.Foundation1:	return Location.Depot4;
				case Location.Foundation2:	return Location.Depot5;
				case Location.Foundation3:	return Location.Depot6;
				case Location.Foundation4:	return Location.Depot7;
				case Location.Depot1:		return Location.Stock;
				case Location.Depot2:
				case Location.Depot3:		return Location.Waste1;
				case Location.Depot4:		return Location.Foundation1;
				case Location.Depot5:		return Location.Foundation2;
				case Location.Depot6:		return Location.Foundation3;
				case Location.Depot7:		return Location.Foundation4;
			}
			return location;
		}

		private void NavigateToDepot(Location location)
		{
			Debug.Assert(IsDepot(location));
			Card topmost = GetTopmostCard(location);
			if (topmost == null)
			{
				// Point to base of vacant depot
				PointTo(location);
			}
			else if (_pointerSelection != null)
			{
				// If you have a card selected, you probably want to place it on the topmost card of the depot
				PointTo(topmost);
			}
			else
			{
				Card bottommostMovable = topmost.GetBottommostMovable(_canOnlyStackAlternatingColors);
				if (bottommostMovable.Value == 13 && bottommostMovable.CardBehindThis == null)
				{
					// If you don't have a card selected, and the depot is stacked with a King at the root, you probably
					// want to move the topmost card to a foundation, or something near the top to a different depot
					PointTo(topmost);
				}
				else
				{
					// If you don't have a card selected, you probably want to move the largest possible stack from the depot
					PointTo(bottommostMovable);
				}
			}
		}

		public Card GetTopmostCard(Location location)
		{
			Card topmost = null;
			if (IsDepot(location))
			{
				int depotIndex = DepotIndex(location);
				topmost = _depotTopmost[depotIndex];
			}
			else if (IsFoundation(location))
			{
				int foundationIndex = FoundationIndex(location);
				topmost = _foundationTopmost[foundationIndex];
			}
			else if (IsStock(location))
			{
				if (_stock.Count > 0)
				{
					topmost = _stock[0];
				}
			}
			else if (IsWaste(location))
			{
				if (_waste.Count > 0)
				{
					topmost = _waste[_waste.Count - 1];
				}
			}
			return topmost;
		}

		public void Deal()
		{
			// Reset internal state between deals
			for (int i = 0; i < NUM_CARDS_IN_DECK; ++i)
			{
				_deck[i].Init(this, _deck[i].Suit, _deck[i].Value);
				_deck[i].gameObject.SetActive(true);
			}
			if (_stock != null)
			{
				_stock.Clear();
			}
			if (_waste != null)
			{
				_waste.Clear();
			}
			if (_depotTopmost != null)
			{
				for (int i = 0; i < NUM_DEPOTS; ++i)
				{
					_depotTopmost[i] = null;
				}
			}
			if (_foundationTopmost != null)
			{
				for (int i = 0; i < NUM_FOUNDATIONS; ++i)
				{
					_foundationTopmost[i] = null;
				}
			}
			_pointerLocation = DEFAULT_LOCATION;
			_pointerCard = null;
			SetPointerSelection(null);
			_camera.transform.position = new Vector3(_camera.transform.position.x, 0, _camera.transform.position.z);

			// Fisher-Yates in-place shuffle
			for (int i = 0; i <= NUM_CARDS_IN_DECK - 2; ++i)
			{
				int j = Random.Range(i, NUM_CARDS_IN_DECK);		// Min inclusive, max exclusive. [min, max)
				Card temp = _deck[i];
				_deck[i] = _deck[j];
				_deck[j] = temp;
			}

			// Deal the tableau
			int nextCardIndex = 0;
			for (int row = 0; row < NUM_DEPOTS; ++row)
			{
				for (int depot = row; depot < NUM_DEPOTS; ++depot)
				{
					Card card = _deck[nextCardIndex++];
					SetCardLocation(card, (Location)((int)Location.Depot1 + depot));
					if (depot == row || !_tableauFaceDown)
					{
						card.SetFaceUp(true, natural: (depot == row));
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
				card.SetFaceUp(false, natural: true);
				_stock.Add(card);
			}
			if (_waste == null)
			{
				_waste = new List<Card>(numRemaining);
			}

			// Start the pointer in the middle
			PointTo(DEFAULT_LOCATION);
		}

		private void UpdateTableauFaceDown()
		{
			if (_tableauFaceDown)
			{
				// Mark cards in all depots that haven't naturally been turned over as face down
				for (int depotIndex = 0; depotIndex < NUM_DEPOTS; ++depotIndex)
				{
					Card next = _depotTopmost[depotIndex];
					while (next != null)
					{
						if (!next.RevealedNaturally)
						{
							next.SetFaceUp(false, natural: false);
						}
						next = next.CardBehindThis;
					}
				}
			}
			else
			{
				// Mark all cards in all depots as face up
				for (int depotIndex = 0; depotIndex < NUM_DEPOTS; ++depotIndex)
				{
					Card next = _depotTopmost[depotIndex];
					while (next != null)
					{
						next.SetFaceUp(true, natural: false);
						next = next.CardBehindThis;
					}
				}
			}
		}

		private void DrawFromStock()
		{
			// Refill the stock from the waste if the stock is empty
			int numCardsInStock = _stock.Count;
			int numCardsInWaste = _waste.Count;
			if (numCardsInStock == 0)
			{
				if (numCardsInWaste == 0)
				{
					return;
				}
				RefillStock();
				numCardsInStock = numCardsInWaste;
				numCardsInWaste = 0;
			}

			// Hide any currently visible waste cards
			for (int i = numCardsInWaste - 1; i >= 0; --i)
			{
				Card card = _waste[i];
				if (!card.isActiveAndEnabled)
				{
					break;
				}
				card.gameObject.SetActive(false);
			}

			// Draw new cards from stock to waste
			int numCardsToDraw = Mathf.Min(_numCardsToDrawFromStock, numCardsInStock);
			for (int i = 0; i < numCardsToDraw; ++i)
			{
				Card card = _stock[0];
				_stock.RemoveAt(0);
				_waste.Add(card);
				card.SetFaceUp(true, natural: true);
				SetCardLocation(card, (Location)((int)Location.Waste1 + i));
				card.RenderOrder = i;
			}
		}

		private void RefillStock()
		{
			int numCards = _waste.Count;
			for (int i = 0; i < numCards; ++i)
			{
				Card card = _waste[i];
				_stock.Add(card);
				card.gameObject.SetActive(true);
				card.SetFaceUp(false, natural: true);
				SetCardLocation(card, Location.Stock);
				card.RenderOrder = 0;
			}
			_waste.Clear();
		}

		private void SetCardLocation(Card card, Location location)
		{
			// Work out what card this card will be in front of, if any
			Card topmost = null;
			bool inDepot = IsDepot(location);
			bool inFoundation = IsFoundation(location);
			int index = -1;
			if (inDepot)
			{
				index = DepotIndex(location);
				topmost = _depotTopmost[index];
			}
			else if (inFoundation)
			{
				index = FoundationIndex(location);
				topmost = _foundationTopmost[index];
			}

			// Place the card
			card.SetLocation(location, topmost);

			// Track topmost card
			if (inDepot)
			{
				topmost = card.GetTopmost();
				_depotTopmost[index] = topmost;
			}
			else if (inFoundation)
			{
				_foundationTopmost[index] = card;
			}
		}

		private void PointTo(Location location)
		{
			_pointerLocation = location;
			Card topmost = GetTopmostCard(location);
			if (topmost != null)
			{
				PointTo(topmost);
				return;
			}
			_pointerCard = null;
			bool pointRightwards = ShouldPointRightwards(location);
			Vector3 position = LocationBasePosition(location);
			PointTo(position, pointRightwards);
		}

		private void PointTo(Card card)
		{
			_pointerLocation = card.Location;
			_pointerCard = card;
			bool pointRightwards = ShouldPointRightwards(card.Location);
			PointTo(card.transform.position, pointRightwards);
		}

		private void PointTo(Vector3 position, bool pointRightwards)
		{
			// Configure pointer
			_pointer.flipX = !pointRightwards;
			_pointerPosition = position;
			UpdatePointerPosition();

			// Scroll camera to frame selection
			const int HALF_SCREEN_HEIGHT = SCREEN_HEIGHT / 2;
			bool aboveViewport = _pointerPosition.y > _camera.position.y + HALF_SCREEN_HEIGHT;
			bool belowViewport = _pointerPosition.y - CARD_HEIGHT < _camera.position.y - HALF_SCREEN_HEIGHT;
			if (aboveViewport || belowViewport)
			{
				float y = Mathf.Min(0.0f, _pointerPosition.y + HALF_SCREEN_HEIGHT - CARD_HEIGHT);
				_camera.position = new Vector3(_camera.position.x, y, _camera.position.z);
			}
		}

		private void UpdatePointerPosition()
		{
			bool facingRightwards = !_pointer.flipX;
			Vector3 pos = _pointerPosition;

			// Offset pointer from stock position so that it doesn't block the waste from view
			if (_pointerLocation == Location.Stock)
			{
				pos += 2 * Vector3.left;
			}

			// Animate retraction forward and back to create pointing motion
			if (facingRightwards)
			{
				pos += POINTER_OFFSET_FACING_RIGHTWARDS;
				if (_pointerRetracted)
				{
					pos += Vector3.left;
				}
			}
			else
			{
				pos += POINTER_OFFSET_FACING_LEFTWARDS;
				if (_pointerRetracted)
				{
					pos += Vector3.right;
				}
			}
			_pointer.transform.position = pos;
		}

		private bool ShouldPointRightwards(Location location)
		{
			bool leftEdge = (location == Location.Stock || location == Location.Depot1);
			return !leftEdge;
		}

		private void SetPointerSelection(Card card)
		{
			_pointerSelection = card;
			_pointer.sprite = (_pointerSelection == null) ? _pointerSpriteNormal : _pointerSpriteSelected;
		}
	}
}
