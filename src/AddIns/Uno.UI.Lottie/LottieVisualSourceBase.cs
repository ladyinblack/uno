using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Uno;
using Uno.Disposables;
using Uno.Foundation.Logging;
using Uno.Extensions;
using Uno.Helpers;

namespace Microsoft.Toolkit.Uwp.UI.Lottie
{
	public abstract partial class LottieVisualSourceBase : DependencyObject, IAnimatedVisualSource
	{
		public delegate void UpdatedAnimation(string animationJson, string cacheKey);

		private AnimatedVisualPlayer? _player;

		public static DependencyProperty UriSourceProperty { get ; } = DependencyProperty.Register(
			"UriSource",
			typeof(Uri),
			typeof(LottieVisualSourceBase),
			new FrameworkPropertyMetadata(
				default(Uri),
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
				OnUriSourceChanged));

		public Uri UriSource
		{
			get => (Uri)GetValue(UriSourceProperty);
			set => SetValue(UriSourceProperty, value);
		}

		public static DependencyProperty OptionsProperty { get ; } = DependencyProperty.Register(
			"Options", typeof(LottieVisualOptions), typeof(LottieVisualSourceBase), new FrameworkPropertyMetadata(LottieVisualOptions.None));

		[NotImplemented]
		public LottieVisualOptions Options
		{
			get => (LottieVisualOptions)GetValue(OptionsProperty);
			set => SetValue(OptionsProperty, value);
		}

		[NotImplemented]
		public static LottieVisualSource CreateFromString(string uri)
		{
			throw new NotImplementedException();
		}

#if HAS_UNO_WINUI
		[NotImplemented]
		public IAnimatedVisual TryCreateAnimatedVisual(Microsoft.UI.Composition.Compositor compositor, out object diagnostics)
		{
			throw new NotImplementedException();
		}
#endif

		private static void OnUriSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			if (sender is LottieVisualSource source)
			{
				source.Update(source._player);
			}
		}

		public Task SetSourceAsync(Uri sourceUri)
		{
			UriSource = sourceUri;

			// TODO: this method should not return before the animation is ready.

			return Task.CompletedTask;
		}


#if !(__WASM__ || (__ANDROID__ && !NET6_0) || (__IOS__ && !NET6_0) || (__MACOS__ && !NET6_0) || HAS_SKOTTIE)
		public void Play(double fromProgress, double toProgress, bool looped)
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public void Pause()
		{
			throw new NotImplementedException();
		}

		public void Resume()
		{
			throw new NotImplementedException();
		}

		public void SetProgress(double progress)
		{
			throw new NotImplementedException();
		}

		public void Load()
		{
			throw new NotImplementedException();
		}

		public void Unload()
		{
			throw new NotImplementedException();
		}

		public Size Measure(Size availableSize)
		{
			throw new NotImplementedException();
		}

#pragma warning disable CA1805 // Do not initialize unnecessarily
		private readonly Size CompositionSize = default;
#pragma warning restore CA1805 // Do not initialize unnecessarily

		private Task InnerUpdate(CancellationToken ct)
		{
			throw new NotSupportedException("Lottie on this platform is not supported yet.");
		}
#endif

		private readonly SerialDisposable _updateDisposable = new SerialDisposable();

		public void Update(AnimatedVisualPlayer? player)
		{
			_updateDisposable.Disposable = null;

			_player = player;
			if (_player != null)
			{
				var cts = new CancellationDisposable();
				_updateDisposable.Disposable = cts;
				var t = InnerUpdate(cts.Token);
			}
		}

		/// <summary>
		/// If the payload needs to be altered before being feed to the player
		/// </summary>
		protected abstract bool IsPayloadNeedsToBeUpdated { get; }

		/// <summary>
		/// Load the animation json payload
		/// </summary>
		protected virtual IDisposable? LoadAndObserveAnimationData(
			IInputStream sourceJson,
			string sourceCacheKey,
			UpdatedAnimation updateCallback)
		{
			var cts = new CancellationTokenSource();

			async Task Load(CancellationToken ct)
			{
				string json;
				using (var reader = new StreamReader(sourceJson.AsStreamForRead(0)))
				{
					json = await reader.ReadToEndAsync();
				}

				// close the input stream
				sourceJson.Dispose();

				// load the stream (not dynamic: won't produce another version)
				updateCallback(json, sourceCacheKey);
			}

			var t = Load(cts.Token);

			return Disposable.Create(() =>
			{
				cts.Cancel();
				cts.Dispose();
			});
		}

		private void SetIsPlaying(bool isPlaying) => _player?.SetValue(AnimatedVisualPlayer.IsPlayingProperty, isPlaying);

		Size IAnimatedVisualSource.Measure(Size availableSize)
		{
			if (_player == null)
			{
				return default;
			}

			var compositionSize = CompositionSize;
			if (compositionSize == default)
			{
				return default;
			}

			var stretch = _player.Stretch;

			if (stretch == Stretch.None)
			{
				return compositionSize;
			}

			var availableWidth = availableSize.Width;
			var availableHeight = availableSize.Height;

			var resultSize = availableSize;

			if (double.IsInfinity(availableWidth))
			{
				if (double.IsInfinity(availableHeight))
				{
					return compositionSize;
				}

				resultSize = new Size(availableHeight * compositionSize.Width / compositionSize.Height, availableHeight);
			}

			if (double.IsInfinity(availableHeight))
			{
				resultSize = new Size(availableWidth, availableWidth * compositionSize.Height / compositionSize.Width);
			}

			InnerMeasure(resultSize);

			return resultSize;
		}

		partial void InnerMeasure(Size size);

		private async Task<IInputStream?> TryLoadDownloadJson(Uri uri, CancellationToken ct)
		{
			if (TryLoadEmbeddedJson(uri, ct) is { } json)
			{
				return json;
			}

			if (uri.IsLocalResource())
			{
				var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask(ct);
				var value = await file.OpenAsync(FileAccessMode.Read).AsTask(ct);

				return value;
			}
			else if (uri.IsAppData())
			{
				var fileStream = File.OpenRead(AppDataUriEvaluator.ToPath(uri));

				return fileStream.AsInputStream();
			}

			return IsPayloadNeedsToBeUpdated
				? await DownloadJsonFromUri(uri, ct)
				: null;
		}

		private IInputStream? TryLoadEmbeddedJson(Uri uri, CancellationToken ct)
		{
			if (uri.Scheme != "embedded")
			{
				return null;
			}

			var assemblyName = uri.Host;

			var assembly = assemblyName == "."
				? Application.Current.GetType().Assembly
				: Assembly.Load(assemblyName);

			if (assembly == null)
			{
				return null;
			}

			var resourceName = uri.AbsolutePath.Substring(1).Replace("(assembly)", assembly.GetName().Name);
			var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				if (this.Log().IsEnabled(LogLevel.Warning))
				{
					this.Log().Warn($"Unable to find embedded resource named '{resourceName}' to load.");
				}
				return null;
			}

			return stream.AsInputStream();
		}

		private async Task<IInputStream?> DownloadJsonFromUri(Uri uri, CancellationToken ct)
		{
			using var client = new HttpClient();

			using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);

			if (!response.IsSuccessStatusCode)
			{
				return null;
			}

			if (response.Content.Headers.ContentLength is { } length && length < 2)
			{
				return null;
			}

			var stream = await response.Content.ReadAsStreamAsync();

			return stream.AsInputStream();
		}
	}
}
