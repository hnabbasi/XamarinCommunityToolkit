﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.CommunityToolkit.iOS.UI.Views;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using System.Threading.Tasks;
using System.Threading;
using Foundation;
using Xamarin.Forms.Internals;
using Xamarin.CommunityToolkit.Extensions.iOS;

[assembly: ExportRenderer(typeof(SegmentedView), typeof(SegmentedViewRenderer))]

namespace Xamarin.CommunityToolkit.iOS.UI.Views
{
	public class SegmentedViewRenderer : ViewRenderer<SegmentedView, UISegmentedControl>
	{
		const string TAG = "SegmentedView";

		protected override async void OnElementChanged(ElementChangedEventArgs<SegmentedView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control == null)
				SetNativeControl(new UISegmentedControl());

			if (e.OldElement != null && Control != null)
				InvalidateControl();

			if (e.NewElement != null)
			{
				try
				{
					await InitializeControl();
				}
				catch (Exception ex)
				{
					Log.Warning(TAG, ex.Message);
				}
			}
		}

		async Task InitializeControl()
		{
			await PopulateSegments(Element.Items);
			Control.ClipsToBounds = true;
			Control.SelectedSegment = Element.SelectedIndex;
			Control.BackgroundColor = Element.BackgroundColor.ToUIColor();
			Control.Layer.MasksToBounds = true;
			UpdateSelectedSegment(Element.SelectedIndex);

			Control.ValueChanged += OnSelectedIndexChanged;
			((INotifyCollectionChanged)Element.Items).CollectionChanged += SegmentsCollectionChanged;

			if (Element.IsColorSet)
			{
				Control.SelectedSegmentTintColor = Element.Color.ToUIColor();
				Control.TintColor = Element.Color.ToUIColor();
			}
		}

		async Task PopulateSegments(IEnumerable<string> segments)
		{
			for (var i = 0; i < segments.Count(); i++)
			{
				await InsertSegment(segments.ElementAt(i), i);
			}
		}

		async Task InsertSegment(string segment, int position)
		{
			switch (Element.DisplayMode)
			{
				case SegmentMode.Image:
					var img = await ((ImageSource)segment).GetNativeImageAsync();
					if (img != null)
						Control.InsertSegment(img, position, false);
					else
						Log.Warning(TAG, "ImageSource is null");
					break;
				default:
				case SegmentMode.Text:
					Control.InsertSegment(segment, position, false);
					break;
			}
		}

		void InvalidateControl()
		{
			Control.ValueChanged -= OnSelectedIndexChanged;
			((INotifyCollectionChanged)Element.Items).CollectionChanged -= SegmentsCollectionChanged;
		}

		async void SegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var startIndex = e.NewStartingIndex;
					foreach (var item in e.NewItems)
					{
						try
						{
							await InsertSegment((string)item, startIndex++);
						}
						catch (Exception ex)
						{
							Log.Warning(TAG, ex.Message);
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						Control.RemoveSegmentAtIndex(e.OldStartingIndex, false);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					Control.RemoveAllSegments();
					break;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (Control == null || Element == null)
				return;

			if (e.PropertyName == SegmentedView.SelectedItemProperty.PropertyName || e.PropertyName == SegmentedView.SelectedIndexProperty.PropertyName)
				UpdateSelectedSegment(Element.SelectedIndex);

			if (e.PropertyName == SegmentedView.ColorProperty.PropertyName)
				Control.SelectedSegmentTintColor = Element.Color.ToUIColor();
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			Element.SelectedIndex = (int)Control.SelectedSegment;
		}

		void UpdateSelectedSegment(int index)
		{
			Control.SelectedSegment = index;
		}

		protected override void Dispose(bool disposing)
		{
			Control.ValueChanged -= OnSelectedIndexChanged;
			Control.Dispose();
			base.Dispose(disposing);
		}

		public async Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			UIImage image = null;
			var streamsource = imagesource as StreamImageSource;
			if (streamsource?.Stream != null)
			{
				using (var streamImage = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					if (streamImage != null)
						image = UIImage.LoadFromData(NSData.FromStream(streamImage), scale);
				}
			}

			if (image == null)
			{
				Log.Warning(TAG, "Could not load image: {0}", streamsource);
			}

			return image;
		}
	}
}
