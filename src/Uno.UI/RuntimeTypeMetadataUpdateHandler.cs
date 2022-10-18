using System;
using Uno.UI.Xaml;
using Microsoft.UI.Xaml;

namespace Uno.UI
{
	/// <summary>
	/// Metadata update handler used to reset caches when changes are applied
	/// by the hot reload engine.
	/// </summary>
	internal class RuntimeTypeMetadataUpdateHandler
	{
		public static void ClearCache(Type[] types)
		{
			Microsoft.UI.Xaml.DependencyProperty.ClearRegistry();
			DataBinding.BindingPropertyHelper.ClearCaches();
		}

		public static void UpdateApplication(Type[] types)
		{
			// Keep empty, both methods (ClearCache, UpdateApplication) are
			// invoked in sequence.
		}
	}
}
