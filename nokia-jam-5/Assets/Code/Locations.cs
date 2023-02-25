using UnityEngine;

namespace Solitaire
{
	public enum Location
	{
		Stock,
		Waste1,
		Waste2,
		Waste3,
		Foundation1,
		Foundation2,
		Foundation3,
		Foundation4,
		Depot1,
		Depot2,
		Depot3,
		Depot4,
		Depot5,
		Depot6,
		Depot7
	}

	public static class Locations
	{
		public static readonly Vector3 POS_STOCK = new Vector3(-42, 23, 0);

		public static readonly Vector3 POS_WASTE1 = new Vector3(-30, 23, 0);
		public static readonly Vector3 POS_WASTE2 = new Vector3(-25, 23, 0);
		public static readonly Vector3 POS_WASTE3 = new Vector3(-20, 23, 0);
		public static readonly Vector3[] POS_WASTE = new Vector3[] { POS_WASTE1, POS_WASTE2, POS_WASTE3 };
		public const int NUM_WASTES = 3;

		public static readonly Vector3 POS_FOUNDATION1 = new Vector3(-6, 23, 0);
		public static readonly Vector3 POS_FOUNDATION2 = new Vector3(6, 23, 0);
		public static readonly Vector3 POS_FOUNDATION3 = new Vector3(18, 23, 0);
		public static readonly Vector3 POS_FOUNDATION4 = new Vector3(30, 23, 0);
		public static readonly Vector3[] POS_FOUNDATION = new Vector3[] { POS_FOUNDATION1, POS_FOUNDATION2, POS_FOUNDATION3, POS_FOUNDATION4 };
		public const int NUM_FOUNDATIONS = 4;

		public static readonly Vector3 POS_DEPOT1 = new Vector3(-42, 6, 0);
		public static readonly Vector3 POS_DEPOT2 = new Vector3(-30, 6, 0);
		public static readonly Vector3 POS_DEPOT3 = new Vector3(-18, 6, 0);
		public static readonly Vector3 POS_DEPOT4 = new Vector3(-6, 6, 0);
		public static readonly Vector3 POS_DEPOT5 = new Vector3(6, 6, 0);
		public static readonly Vector3 POS_DEPOT6 = new Vector3(18, 6, 0);
		public static readonly Vector3 POS_DEPOT7 = new Vector3(30, 6, 0);
		public static readonly Vector3[] POS_DEPOT = new Vector3[] { POS_DEPOT1, POS_DEPOT2, POS_DEPOT3, POS_DEPOT4, POS_DEPOT5, POS_DEPOT6, POS_DEPOT7 };
		public const int NUM_DEPOTS = 7;

		public static readonly Vector3 CARD_X_OFFSET = new Vector3(5, 0, 0);	// Waste
		public static readonly Vector3 CARD_Y_OFFSET = new Vector3(0, -5, 0);	// Depots
		public static readonly Vector3 POINTER_OFFSET_FACING_RIGHTWARDS = new Vector3(-4, 0, 0);
		public static readonly Vector3 POINTER_OFFSET_FACING_LEFTWARDS = new Vector3(15, 0, 0);

		public static Vector3 LocationBasePosition(Location location)
		{
			Vector3 position = Vector3.zero;
			switch (location)
			{
				case Location.Stock:		position = POS_STOCK;			break;
				case Location.Waste1:		position = POS_WASTE1;			break;
				case Location.Waste2:		position = POS_WASTE2;			break;
				case Location.Waste3:		position = POS_WASTE3;			break;
				case Location.Foundation1:	position = POS_FOUNDATION1;		break;
				case Location.Foundation2:	position = POS_FOUNDATION2;		break;
				case Location.Foundation3:	position = POS_FOUNDATION3;		break;
				case Location.Foundation4:	position = POS_FOUNDATION4;		break;
				case Location.Depot1:		position = POS_DEPOT1;			break;
				case Location.Depot2:		position = POS_DEPOT2;			break;
				case Location.Depot3:		position = POS_DEPOT3;			break;
				case Location.Depot4:		position = POS_DEPOT4;			break;
				case Location.Depot5:		position = POS_DEPOT5;			break;
				case Location.Depot6:		position = POS_DEPOT6;			break;
				case Location.Depot7:		position = POS_DEPOT7;			break;
			}
			return position;
		}

		public static bool IsStock(Location location)
		{
			return location == Location.Stock;
		}

		public static bool IsWaste(Location location)
		{
			int loc = (int)location;
			return loc >= (int)Location.Waste1 && loc <= (int)Location.Waste3;
		}

		public static bool IsFoundation(Location location)
		{
			int loc = (int)location;
			return loc >= (int)Location.Foundation1 && loc <= (int)Location.Foundation4;
		}

		public static bool IsDepot(Location location)
		{
			int loc = (int)location;
			return loc >= (int)Location.Depot1 && loc <= (int)Location.Depot7;
		}

		// returns index between 0 and NUM_WASTES-1 inclusive
		public static int WasteIndex(Location location)
		{
			Debug.Assert(IsWaste(location));
			int index = (int)location - (int)Location.Waste1;
			return index;
		}

		// returns index between 0 and NUM_FOUNDATIONS-1 inclusive
		public static int FoundationIndex(Location location)
		{
			Debug.Assert(IsFoundation(location));
			int index = (int)location - (int)Location.Foundation1;
			return index;
		}

		// returns index between 0 and NUM_DEPOTS-1 inclusive
		public static int DepotIndex(Location location)
		{
			Debug.Assert(IsDepot(location));
			int index = (int)location - (int)Location.Depot1;
			return index;
		}
	}
}
