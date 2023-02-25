﻿using System.Collections.Generic;
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

		private Inputs _input = null;

		// Don't forget to reset state in Deal
		private Card[] _deck = null;
		private List<Card> _stock = null;
		private int _stockIndex = -1;
		private Card[] _depotTopmost = null;

		// Don't forget to reset state in Deal
		private Location _pointerLocation = DEFAULT_LOCATION;
		private Vector3 _pointerPosition = Vector3.zero;
		private bool _pointerRetracted = false;
		private float _timePointerAlternated = -1.0f;
		private Card _pointerCard = null;
		private Card _pointerSelection = null;

		private const int NUM_CARDS_IN_DECK = 52;
		private const Location DEFAULT_LOCATION = Location.Depot4;
		private const float POINTER_DURATION = 0.4f;

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
			Deal();

			// Enable input
			_input = new Inputs();
			_input.Enable();
		}

		private void Update()
		{
			// Navigation input. Keep it simple and just prioritise different directions
			bool right = _input.Game.Right.WasPerformedThisFrame();
			bool left = _input.Game.Left.WasPerformedThisFrame();
			bool up = _input.Game.Up.WasPerformedThisFrame();
			bool down = _input.Game.Down.WasPerformedThisFrame();
			if (right)
			{
				// Move pointer rightwards to next valid target
				if (IsStock(_pointerLocation))
				{
					PointTo(Location.Waste1);
				}
				else if (IsWaste(_pointerLocation))
				{
					PointTo(Location.Foundation1);
				}
				else if (_pointerLocation != Location.Foundation4 && _pointerLocation != Location.Depot7)
				{
					PointTo((Location)((int)_pointerLocation + 1));
				}
			}
			else if (left)
			{
				// Move pointer leftwards to next valid target
				if (IsWaste(_pointerLocation))
				{
					PointTo(Location.Stock);
				}
				else if (_pointerLocation == Location.Foundation1)
				{
					PointTo(Location.Waste1);
				}
				else if (_pointerLocation != Location.Stock && _pointerLocation != Location.Depot1)
				{
					PointTo((Location)((int)_pointerLocation - 1));
				}
			}
			else if (up)
			{
				// Move pointer upwards to next valid target
				if (IsDepot(_pointerLocation))
				{
					// Move up to earlier card in stack that can be moved
					bool handled = false;
					if (_pointerCard != null && _pointerCard.CardBehindThis != null && _pointerCard.CardBehindThis.CanBeMoved())
					{
						PointTo(_pointerCard.CardBehindThis);
						handled = true;
					}

					// Move up to stock/waste/foundation
					if (!handled)
					{
						switch (_pointerLocation)
						{
							case Location.Depot1:	PointTo(Location.Stock);		break;
							case Location.Depot2:	PointTo(Location.Waste1);		break;
							case Location.Depot3:
							case Location.Depot4:	PointTo(Location.Foundation1);	break;
							case Location.Depot5:	PointTo(Location.Foundation2);	break;
							case Location.Depot6:	PointTo(Location.Foundation3);	break;
							case Location.Depot7:	PointTo(Location.Foundation4);	break;
						}
					}
				}
			}
			else if (down)
			{
				// Move pointer downwards to next valid target
				if (IsDepot(_pointerLocation))
				{
					// Move down to later card in stack that can be moved
					if (_pointerCard != null && _pointerCard.CardInFrontOfThis != null)
					{
						PointTo(_pointerCard.CardInFrontOfThis);
					}
				}
				else
				{
					// Move down to earliest card in stack that can be moved, or vacant depot
					Location target = Location.Depot4;
					switch (_pointerLocation)
					{
						case Location.Stock:		target = Location.Depot1;	break;
						case Location.Waste1:
						case Location.Waste2:
						case Location.Waste3:		target = Location.Depot2;	break;
						case Location.Foundation1:	target = Location.Depot4;	break;
						case Location.Foundation2:	target = Location.Depot5;	break;
						case Location.Foundation3:	target = Location.Depot6;	break;
						case Location.Foundation4:	target = Location.Depot7;	break;
					}
					Card card = GetTopmostCard(target);
					if (card == null)
					{
						PointTo(target);
					}
					else
					{
						while (card.CardBehindThis != null && card.CardBehindThis.CanBeMoved())
						{
							card = card.CardBehindThis;
						}
						PointTo(card);
					}
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
					// Pick up card
					if (_pointerCard != null)
					{
						SetPointerSelection(_pointerCard);
					}
				}
				else
				{
					// Move and drop card
					if (_pointerSelection.CanBeMovedTo(_pointerLocation))
					{
						// Reveal new topmost depot card
						if (IsDepot(_pointerSelection.Location))
						{
							Card newTopmost = null;
							if (_pointerSelection.CardBehindThis != null)
							{
								_pointerSelection.CardBehindThis.SetFaceUp(true);
								newTopmost = _pointerSelection.CardBehindThis;
							}
							int index = DepotIndex(_pointerSelection.Location);
							_depotTopmost[index] = newTopmost;
						}

						// Move held card
						SetCardLocation(_pointerSelection, _pointerLocation);
					}
					// Drop held card
					SetPointerSelection(null);
				}
			}
			else if (back)
			{
				// Drop card
				if (_pointerSelection != null)
				{
					SetPointerSelection(null);
				}
			}

#if UNITY_EDITOR
			// Re-deal
			if (_input.Game.DebugRedeal.WasPerformedThisFrame())
			{
				Deal();
			}
#endif	// UNITY_EDITOR

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

		public Card GetTopmostCard(Location location)
		{
			Card topmost = null;
			if (IsDepot(location))
			{
				int depotIndex = DepotIndex(location);
				topmost = _depotTopmost[depotIndex];
			}
			else if (IsStock(location))
			{
				if (_stock.Count > 0)
				{
					topmost = _stock[_stockIndex];
				}
			}
			return topmost;
		}

		private void Deal()
		{
			// Reset internal state between deals
			for (int i = 0; i < NUM_CARDS_IN_DECK; ++i)
			{
				_deck[i].Init(this, _deck[i].Suit, _deck[i].Value);
			}
			if (_stock != null)
			{
				_stock.Clear();
			}
			_stockIndex = -1;
			if (_depotTopmost != null)
			{
				for (int i = 0; i < NUM_DEPOTS; ++i)
				{
					_depotTopmost[i] = null;
				}
			}
			_pointerLocation = DEFAULT_LOCATION;
			_pointerCard = null;
			SetPointerSelection(null);

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

			// Start the pointer in the middle
			PointTo(DEFAULT_LOCATION);
		}

		private void SetCardLocation(Card card, Location location)
		{
			// Work out what card this card will be in front of, if any
			Card topmost = null;
			bool inDepot = IsDepot(location);
			int depotIndex = -1;
			if (inDepot)
			{
				depotIndex = DepotIndex(location);
				topmost = _depotTopmost[depotIndex];
			}

#if UNITY_EDITOR
			//Debug.Log($"Placing {card.name} at location {location} atop {((topmost == null) ? "nothing" : topmost.name)}", card);
#endif

			// Place the card
			card.SetLocation(location, topmost);

			// Track topmost card
			if (inDepot)
			{
				topmost = card.GetTopmost();
				_depotTopmost[depotIndex] = topmost;
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
