﻿#nullable enable
using Uno.Extensions;
using Uno.UI.DataBinding;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using Uno.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;

using Foundation;
using CoreGraphics;
using Uno.UI.Extensions;

namespace Windows.UI.Xaml.Controls
{
	public partial class ScrollViewer
	{
		/// <summary>
		/// The <see cref="UIScrollView"/> which will actually scroll. Mostly this will be identical to <see cref="_presenter"/>, but if we're inside a
		/// multi-line TextBox we set it to <see cref="MultilineTextBoxView"/>.
		/// </summary>
		private IUIScrollView? _scrollableContainer;

		partial void OnApplyTemplatePartial()
		{
			SetScrollableContainer();
		}

		private protected override void OnLoaded()
		{
			SetScrollableContainer();
			base.OnLoaded();
		}

		private void SetScrollableContainer()
		{
			_scrollableContainer = _presenter;

#if false // TODO Scrollable textbox https://github.com/unoplatform/uno/issues/626
			if (this.FindFirstParent<TextBox>() != null)
			{
				var multiline = this.FindFirstChild<TextBoxView>();
				if (multiline != null)
				{
					_scrollableContainer = multiline;
				}
			}
#endif
		}

		private bool ChangeViewScrollNative(double? horizontalOffset, double? verticalOffset, float? zoomFactor, bool disableAnimation)
		{
			if (_scrollableContainer != null)
			{
				// iOS doesn't limit the offset to the scrollable bounds by itself
				var point = new CGPoint(horizontalOffset ?? HorizontalOffset, verticalOffset ?? VerticalOffset);

				var newOffset = point.Clamp(CGPoint.Empty, _scrollableContainer.UpperScrollLimit);

				_scrollableContainer.SetContentOffset(newOffset, !disableAnimation);

				if (zoomFactor is { } zoom)
				{
					ChangeViewZoom(zoom, disableAnimation);
				}

				// Return true if successfully scrolled to asked offsets
				return (horizontalOffset == null || horizontalOffset == newOffset.X) &&
				       (verticalOffset == null || verticalOffset == newOffset.Y);
			}

			return false;
		}

		partial void OnZoomModeChangedPartial(ZoomMode zoomMode)
		{
			// On iOS, zooming is disabled by setting Minimum/MaximumZoomScale both to 1
			switch (zoomMode)
			{
				default:
					_presenter?.OnMinZoomFactorChanged(1f);
					_presenter?.OnMaxZoomFactorChanged(1f);
					break;
				case ZoomMode.Enabled:
					_presenter?.OnMinZoomFactorChanged(MinZoomFactor);
					_presenter?.OnMaxZoomFactorChanged(MaxZoomFactor);
					break;
			}
		}

		private void ChangeViewZoom(float zoomFactor, bool disableAnimation)
		{
			// Support for scaling https://github.com/unoplatform/uno/issues/626
		}

		private void UpdateZoomedContentAlignment()
		{
			if (ZoomFactor != 1 && Content is IFrameworkElement fe)
			{
				double insetLeft, insetTop;
				var scaledWidth = fe.ActualWidth * ZoomFactor;
				var viewportWidth = ActualWidth;

				if (viewportWidth <= scaledWidth)
				{
					insetLeft = 0;
				}
				else
				{
					switch (fe.HorizontalAlignment)
					{
						case HorizontalAlignment.Left:
							insetLeft = 0;
							break;
						case HorizontalAlignment.Right:
							insetLeft = viewportWidth - scaledWidth;
							break;
						case HorizontalAlignment.Center:
						case HorizontalAlignment.Stretch:
							insetLeft = .5 * (viewportWidth - scaledWidth);
							break;
						default:
							throw new InvalidOperationException();
					}
				}

				var scaledHeight = fe.ActualHeight * ZoomFactor;
				var viewportHeight = ActualHeight;

				if (viewportHeight <= scaledHeight)
				{
					insetTop = 0;
				}
				else
				{
					switch (fe.VerticalAlignment)
					{
						case VerticalAlignment.Top:
							insetTop = 0;
							break;
						case VerticalAlignment.Bottom:
							insetTop = viewportHeight - scaledHeight;
							break;
						case VerticalAlignment.Center:
						case VerticalAlignment.Stretch:
							insetTop = .5 * (viewportHeight - scaledHeight);
							break;
						default:
							throw new InvalidOperationException();
					}
				}

				if (_presenter != null)
				{
					_presenter.ContentInset = new AppKit.NSEdgeInsets((nfloat)insetTop, (nfloat)insetLeft, 0, 0);
				}
			}
		}

		public override void ViewWillMoveToSuperview(AppKit.NSView newsuper)
		{
			base.ViewWillMoveToSuperview(newsuper);
			UpdateSizeChangedSubscription(isCleanupRequired: newsuper == null);
		}
	}
}