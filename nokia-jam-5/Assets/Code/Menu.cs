using TMPro;
using UnityEngine;

using static Solitaire.Locations;

namespace Solitaire
{
	public enum MenuState
	{
		Closed,
		Pause,
		Options,
		Title,
		Win
	}

	public class Menu : MonoBehaviour
	{
		[SerializeField] private Solitaire _solitaire = null;

		[Header("Pause menu")]
		[SerializeField] private GameObject _pauseRoot = null;
		[SerializeField] private Transform _pauseHighlight = null;
		[SerializeField] private TextMeshProUGUI _pauseTextRedeal = null;
		[SerializeField] private TextMeshProUGUI _pauseTextOptions = null;
		[SerializeField] private TextMeshProUGUI _pauseTextQuit = null;

		[Header("Options menu")]
		[SerializeField] private GameObject _optionsRoot = null;
		[SerializeField] private Transform _optionsHighlight = null;
		[SerializeField] private TextMeshProUGUI _optionsTextDraw = null;
		[SerializeField] private TextMeshProUGUI _optionsTextVacant = null;
		[SerializeField] private TextMeshProUGUI _optionsTextTakeBack = null;
		[SerializeField] private TextMeshProUGUI _optionsTextTableau = null;
		[SerializeField] private TextMeshProUGUI _optionsTextStack = null;

		[Header("Title menu")]
		[SerializeField] private GameObject _titleRoot = null;

		[Header("Win")]
		[SerializeField] private GameObject _winRoot = null;

		private MenuState _state = MenuState.Closed;
		private int _frameStateEntered = -1;
		private int _highlightIndex = -1;
		private TextMeshProUGUI[] _pauseText = null;
		private TextMeshProUGUI[] _optionsText = null;

		public MenuState State { get { return _state; } }

		private void OnEnable()
		{
			_pauseText = new TextMeshProUGUI[] { _pauseTextRedeal, _pauseTextOptions, _pauseTextQuit };
			_optionsText = new TextMeshProUGUI[] { _optionsTextDraw, _optionsTextVacant, _optionsTextTakeBack, _optionsTextTableau, _optionsTextStack };
			SetState(MenuState.Title);
		}

		private void LateUpdate()
		{
			if (_state == MenuState.Closed || _frameStateEntered == Time.frameCount)
			{
				return;
			}

			Inputs input = _solitaire.Input;
			bool right = input.Game.Right.WasPerformedThisFrame();
			bool left = input.Game.Left.WasPerformedThisFrame();
			bool up = input.Game.Up.WasPerformedThisFrame();
			bool down = input.Game.Down.WasPerformedThisFrame();
			bool select = input.Game.Select.WasPerformedThisFrame();
			bool back = input.Game.Back.WasPerformedThisFrame();

			if (_state == MenuState.Title)
			{
				if (select || back)
				{
					SetState(MenuState.Closed);
				}
			}
			else if (_state == MenuState.Win)
			{
				if (select || back)
				{
					SetState(MenuState.Title);
					_solitaire.Deal();
				}
			}
			else if (_state == MenuState.Options)
			{
				if (down)
				{
					SetHighlight((_highlightIndex == _optionsText.Length - 1) ? 0 : _highlightIndex + 1);
				}
				else if (up)
				{
					SetHighlight((_highlightIndex == 0) ? _optionsText.Length - 1 : _highlightIndex - 1);
				}
				if (back)
				{
					SetState(MenuState.Pause);
				}
				else if (select || right || left)
				{
					if (_highlightIndex == 0)
					{
						// How many cards are drawn from stock: 1/2/3
						_solitaire.NumCardsToDrawFromStock += (left) ? -1 : 1;
					}
					else if (_highlightIndex == 1)
					{
						// What can be placed in vacant depots: Kings/Any
						_solitaire.CanOnlyPlaceKingsInVacancies = !_solitaire.CanOnlyPlaceKingsInVacancies;
					}
					else if (_highlightIndex == 2)
					{
						// Whether you can take cards back after putting them in the foundations: Yes/No
						_solitaire.CanTakeFromFoundation = !_solitaire.CanTakeFromFoundation;
					}
					else if (_highlightIndex == 3)
					{
						// Whether the cards in the tableau are face up or face down: Hidden/Shown
						_solitaire.TableauFaceDown = !_solitaire.TableauFaceDown;
					}
					else if (_highlightIndex == 4)
					{
						// What color cards you can place descending values on: Opposites/Any
						_solitaire.CanOnlyStackAlternatingColors = !_solitaire.CanOnlyStackAlternatingColors;
					}
					UpdateOptionsText();
				}
			}
			else if (_state == MenuState.Pause)
			{
				if (down)
				{
					SetHighlight((_highlightIndex == _pauseText.Length - 1) ? 0 : _highlightIndex + 1);
				}
				else if (up)
				{
					SetHighlight((_highlightIndex == 0) ? _pauseText.Length - 1 : _highlightIndex - 1);
				}
				if (back)
				{
					SetState(MenuState.Closed);
				}
				else if (select)
				{
					if (_highlightIndex == 0)
					{
						// Re-deal
						SetState(MenuState.Closed);
						_solitaire.Deal();
					}
					else if (_highlightIndex == 1)
					{
						// Open the options menu
						SetState(MenuState.Options);
					}
					else if (_highlightIndex == 2)
					{
						// Quit (open title menu)
						SetState(MenuState.Title);
					}
				}
			}
		}

		public void SetState(MenuState newState)
		{
			MenuState oldState = _state;
			_state = newState;
			_frameStateEntered = Time.frameCount;
			_pauseRoot.SetActive(_state == MenuState.Pause);
			_optionsRoot.SetActive(_state == MenuState.Options);
			_titleRoot.SetActive(_state == MenuState.Title);
			_winRoot.SetActive(_state == MenuState.Win);
			_highlightIndex = -1;

			if (newState == MenuState.Pause || newState == MenuState.Options)
			{
				TextMeshProUGUI[] entries = (_state == MenuState.Pause) ? _pauseText : _optionsText;
				int numEntries = entries.Length;
				for (int i = 0; i < numEntries; ++i)
				{
					entries[i].color = COLOR_DARK;
				}
			}

			if (oldState == MenuState.Options && newState == MenuState.Pause)
			{
				SetHighlight(1);	// Restore selection to Options
			}
			else if (newState == MenuState.Pause || newState == MenuState.Options)
			{
				SetHighlight(0);
			}

			if (newState == MenuState.Options)
			{
				UpdateOptionsText();
			}
		}

		private void SetHighlight(int newHighlightIndex)
		{
			TextMeshProUGUI[] entries = (_state == MenuState.Pause) ? _pauseText : _optionsText;

			if (_highlightIndex != -1)
			{
				TextMeshProUGUI oldText = entries[_highlightIndex];
				oldText.color = COLOR_DARK;
			}

			_highlightIndex = newHighlightIndex;
			if (_highlightIndex != -1)
			{
				TextMeshProUGUI newText = entries[_highlightIndex];
				newText.color = COLOR_LIGHT;
				Transform highlight = (_state == MenuState.Pause) ? _pauseHighlight : _optionsHighlight;
				highlight.position = newText.transform.position;
			}
		}

		private void UpdateOptionsText()
		{
			_optionsTextDraw.text = $"Draw: {_solitaire.NumCardsToDrawFromStock}";
			_optionsTextVacant.text = $"Vacant: {(_solitaire.CanOnlyPlaceKingsInVacancies ? "Kings" : "Any")}";
			_optionsTextTakeBack.text = $"Take back: {(_solitaire.CanTakeFromFoundation ? "Yes" : "No")}";
			_optionsTextTableau.text = $"Tableau: {(_solitaire.TableauFaceDown ? "Hidden" : "Shown")}";
			_optionsTextStack.text = $"Stack: {(_solitaire.CanOnlyStackAlternatingColors ? "Opposites" : "Any")}";
		}
	}
}
