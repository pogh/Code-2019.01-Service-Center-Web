using System.Collections.Generic;
using System.Text.RegularExpressions;
using Company.BusinessObjects.Common;

namespace Company.WebService.Models.Extensions.Comparers {
	public class AddressComparer : IEqualityComparer<AddressBase> {

		public bool Equals(AddressBase x, AddressBase y) {

			if (object.ReferenceEquals(x, y)) {
				return true;
			}

			if (x == null || y == null) {
				return false;
			}

			if (!AreStringsEqual(x.Title, y.Title))
				return false;

			if (!AreStringsEqual(x.FirstName, y.FirstName))
				return false;

			if (!AreStringsEqual(x.LastName, y.LastName))
				return false;

			if (!AreStringsEqual(x.CompanyName, y.CompanyName))
				return false;

			if (!AreStringsEqual(x.Street, y.Street))
				return false;

			if (!AreStringsEqual(x.Zip, y.Zip))
				return false;

			if (!AreStringsEqual(x.Zip, y.Zip))
				return false;

			if (!AreStringsEqual(x.City, y.City))
				return false;

			if (!AreStringsEqual(x.Country, y.Country))
				return false;

			if (!AreStringsEqual(x.AdditionalLine, y.AdditionalLine))
				return false;

			return true;
		}

		private static bool AreStringsEqual(string x, string y) {

			if (x != null) {
				x = Regex.Replace(x, @"\s+", " ");
				if (string.IsNullOrWhiteSpace(x))
					x = null;
			}

			if (y != null) {
				y = Regex.Replace(y, @"\s+", " ");
				if (string.IsNullOrWhiteSpace(x))
					y = null;
			}

			if (x == null && y == null)
				return true;

			if (x == null && y != null)
				return false;

			if (x != null && y == null)
				return false;

			return x.Equals(y, System.StringComparison.InvariantCultureIgnoreCase);
		}

		public int GetHashCode(AddressBase obj) {
			return string.Concat(obj.Title, obj.FirstName, obj.LastName, obj.CompanyName, obj.Street, obj.Zip, obj.City, obj.Country, obj.AdditionalLine).GetHashCode(System.StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
