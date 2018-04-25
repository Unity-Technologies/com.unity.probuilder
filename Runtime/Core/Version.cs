namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// The versioning information for this ProBuilder install.
	/// </summary>
	public static class Version
	{
		internal static readonly VersionInfo current = new VersionInfo("4.0.0-preview.1", "en-US: 03/29/2018");

		/// <summary>
		/// Get the current version.
		/// </summary>
		/// <returns></returns>
		public static string GetString()
		{
			return current.ToString();
		}
	}
}
