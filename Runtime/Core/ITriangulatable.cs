namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Interface for types that are decomposable to triangle arrays.
	/// </summary>
	public interface ITriangulatable
	{
		/// <summary>
		/// Return a new array of triangle indices.
		/// </summary>
		/// <returns></returns>
		int[] ToTriangles();
	}
}
