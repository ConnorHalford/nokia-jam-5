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
		public static readonly Vector3 POS_STOCK = new Vector3(-7, 4, 0);

		public static readonly Vector3 POS_WASTE1 = new Vector3(-5, 4, 0);
		public static readonly Vector3 POS_WASTE2 = new Vector3(-4.25f, 4, 0);
		public static readonly Vector3 POS_WASTE3 = new Vector3(-3.5f, 4, 0);
		public static readonly Vector3[] POS_WASTE = new Vector3[] { POS_WASTE1, POS_WASTE2, POS_WASTE3 };
		public const int NUM_WASTES = 3;

		public static readonly Vector3 POS_FOUNDATION1 = new Vector3(-1, 4, 0);
		public static readonly Vector3 POS_FOUNDATION2 = new Vector3(1, 4, 0);
		public static readonly Vector3 POS_FOUNDATION3 = new Vector3(3, 4, 0);
		public static readonly Vector3 POS_FOUNDATION4 = new Vector3(5, 4, 0);
		public static readonly Vector3[] POS_FOUNDATION = new Vector3[] { POS_FOUNDATION1, POS_FOUNDATION2, POS_FOUNDATION3, POS_FOUNDATION4 };
		public const int NUM_FOUNDATIONS = 4;

		public static readonly Vector3 POS_DEPOT1 = new Vector3(-7, 1, 0);
		public static readonly Vector3 POS_DEPOT2 = new Vector3(-5, 1, 0);
		public static readonly Vector3 POS_DEPOT3 = new Vector3(-3, 1, 0);
		public static readonly Vector3 POS_DEPOT4 = new Vector3(-1, 1, 0);
		public static readonly Vector3 POS_DEPOT5 = new Vector3(1, 1, 0);
		public static readonly Vector3 POS_DEPOT6 = new Vector3(3, 1, 0);
		public static readonly Vector3 POS_DEPOT7 = new Vector3(5, 1, 0);
		public static readonly Vector3[] POS_DEPOT = new Vector3[] { POS_DEPOT1, POS_DEPOT2, POS_DEPOT3, POS_DEPOT4, POS_DEPOT5, POS_DEPOT6, POS_DEPOT7 };
		public const int NUM_DEPOTS = 7;

		public static readonly Vector3 CARD_X_OFFSET = new Vector3(1.25f, 0, 0);
		public static readonly Vector3 CARD_Y_OFFSET = new Vector3(0, -0.75f, 0);

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
