using UnityEngine;

namespace Solitaire
{
	public enum Suit	// Must be alphabetical to match sprite atlas sub-sprites being alphabetised
	{
		Clubs,
		Diamonds,
		Hearts,
		Spades
	}

	public class Card : MonoBehaviour
	{
		[SerializeField] private Sprite[] _cardSprites = null;
		[SerializeField] private Suit _suit = Suit.Spades;
		[SerializeField] private int _value = 1;
		[SerializeField] private bool _visible = true;

		private SpriteRenderer _sprite = null;

		private void OnEnable()
		{
			UpdateArt();
		}

		private void UpdateArt()
		{
			if (_sprite == null)
			{
				_sprite = GetComponent<SpriteRenderer>();
			}
			// The sprites array is filled from the multi-sprite texture, which orders sub-sprites alphabetically.
			// Therefore the Suit enum is also alphabetical to allow simple indexing with the following order:
			// Back, Clubs 1-13 + Blank, Diamonds 1-13 + Blank, Hearts 1-13 + Blank, Spades 1-13 + Blank
			int index = 0;
			if (_visible)
			{
				const int BACK_OFFSET = 1;
				const int SPRITES_PER_SUIT = 14;	// 13 values + blank
				index = BACK_OFFSET + (_value - 1) + ((int)_suit * SPRITES_PER_SUIT);
			}
			_sprite.sprite = _cardSprites[index];
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
