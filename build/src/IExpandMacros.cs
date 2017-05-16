
namespace ProBuilder.BuildSystem
{
	/**
	 *	Interface for classes that run string replacement on macro dictionaries.
	 */
	interface IExpandMacros
	{
		/**
		 *	Classes implementing IExpandMacros should perform key/value replacement
		 *	on all expandable strings in this function.
		 */
		void Replace(string key, string value);
	}
}
