using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
	/// <summary>
	/// Contains list helper methods
	/// </summary>
	public static class ListHelpers
	{
		/// <summary>
		/// Adds the source to the destination
		/// </summary>
		/// <typeparam name="T">The type of the list item</typeparam>
		/// <param name="destination">The destination list</param>
		/// <param name="source">The source</param>
		public static void AddRange<T>(this IList<T> destination, IEnumerable<T> source)
		{
			foreach (var item in source)
			{
				destination.Add(item);
			}
		}
	}
}
