// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//------------------------------------------------------------------------
//
//  Abstract:
//
//      XAML types.
//      NOTE: This file was generated by a tool.
//
//------------------------------------------------------------------------

using Windows.UI.Core;
using DateTime = System.DateTimeOffset;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
	partial class CalendarPanel
	{
		private void CheckThread() => CoreDispatcher.CheckThreadAccess();

		internal static readonly DependencyProperty CacheLengthProperty = DependencyProperty.Register(
			"CacheLength", typeof(double), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(double)));

		internal double CacheLength
		{
			get { return (double)GetValue(CacheLengthProperty); }
			set { SetValue(CacheLengthProperty, value); }
		}

		internal static readonly DependencyProperty ColsProperty = DependencyProperty.Register(
			"Cols", typeof(int), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(int)));

		internal int Cols
		{
			get { return (int)GetValue(ColsProperty); }
			set { SetValue(ColsProperty, value); }
		}

		internal int FirstCacheIndex
		{
			get
			{
				CheckThread();

				return ((CalendarPanel)(this)).FirstCacheIndexImpl;
			}
		}

		internal int FirstVisibleIndex
		{
			get
			{
				CheckThread();

				return ((CalendarPanel)(this)).FirstVisibleIndexImpl;
			}
		}


		internal static readonly DependencyProperty ItemMinHeightProperty = DependencyProperty.Register(
			"ItemMinHeight", typeof(double), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(double)));

		internal double ItemMinHeight
		{
			get { return (double)GetValue(ItemMinHeightProperty); }
			set { SetValue(ItemMinHeightProperty, value); }
		}

		internal static readonly DependencyProperty ItemMinWidthProperty = DependencyProperty.Register(
			"ItemMinWidth", typeof(double), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(double)));

		internal double ItemMinWidth
		{
			get { return (double)GetValue(ItemMinWidthProperty); }
			set { SetValue(ItemMinWidthProperty, value); }
		}

		internal int LastCacheIndex
		{
			get
			{
				CheckThread();

				return ((CalendarPanel)(this)).LastCacheIndexImpl;
			}
		}

		internal int LastVisibleIndex
		{
			get
			{
				CheckThread();

				return ((CalendarPanel)(this)).LastVisibleIndexImpl;
			}
		}

		internal static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
			"Orientation", typeof(Orientation), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(Orientation)));

		internal Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		internal static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
			"Rows", typeof(int), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(int)));

		internal int Rows
		{
			get { return (int)GetValue(RowsProperty); }
			set { SetValue(RowsProperty, value); }
		}

		private PanelScrollingDirection ScrollingDirection
		{
			get
			{
				CheckThread();

				return ((CalendarPanel)(this)).ScrollingDirectionImpl;
			}
		}

		internal static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register(
			"StartIndex", typeof(int), typeof(CalendarPanel), new FrameworkPropertyMetadata(default(int)));

		internal int StartIndex
		{
			get { return (int)GetValue(StartIndexProperty); }
			set { SetValue(StartIndexProperty, value); }
		}
	}
}
