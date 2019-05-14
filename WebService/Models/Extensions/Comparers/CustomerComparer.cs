using System.Collections.Generic;
using Company.BusinessObjects.Common;

namespace Company.WebService.Models.Extensions.Comparers {
	public class CustomerIdComparer : IEqualityComparer<Customer> {

		public bool Equals(Customer x, Customer y) {
			if (object.ReferenceEquals(x, y)) {
				return true;
			}

			if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) {
				return false;
			}

			return (x.Id == y.Id);
		}

		public int GetHashCode(Customer obj) {
			return obj.Id.GetHashCode();
		}
	}
}
