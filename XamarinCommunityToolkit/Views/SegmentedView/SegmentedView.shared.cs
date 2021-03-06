﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Xamarin.CommunityToolkit.UI.Views
{
	public class SegmentedView : View
	{
		public event EventHandler<SelectedItemChangedEventArgs> SelectedIndexChanged;

		public static BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(SegmentedView));
		public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SegmentedView), 0, propertyChanged: OnSegmentSelected);
		public static BindableProperty DisplayModeProperty = BindableProperty.Create(nameof(DisplayMode), typeof(SegmentMode), typeof(SegmentedView));

		public IList<string> Items { get; } = new LockableObservableListWrapper();

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(SegmentedView), default(IList),
									propertyChanged: OnItemsSourceChanged);

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SegmentedView), null, BindingMode.TwoWay,
									propertyChanged: OnSelectedItemChanged);

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public SegmentMode DisplayMode
		{
			get { return (SegmentMode)GetValue(DisplayModeProperty); }
			set { SetValue(DisplayModeProperty, value); }
		}

		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(SegmentedView), new CornerRadius(6.0));

		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		BindingBase itemDisplayBinding;

		public BindingBase ItemDisplayBinding
		{
			get => itemDisplayBinding;

			set
			{
				if (itemDisplayBinding == value)
					return;

				OnPropertyChanging();
				itemDisplayBinding = value;
				ResetItems();
				OnPropertyChanged();
			}
		}

		static readonly BindableProperty DisplayProperty =
			BindableProperty.Create("Display", typeof(string), typeof(SegmentedView), default(string));

		string GetDisplayMember(object item)
		{
			if (ItemDisplayBinding == null)
				return item.ToString();

			return (string)GetValue(DisplayProperty);
		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((SegmentedView)bindable)?.OnItemsSourceChanged((IList)oldValue, (IList)newValue);
		}

		void OnItemsSourceChanged(IList oldValue, IList newValue)
		{
			if (oldValue is INotifyCollectionChanged oldObservable)
				oldObservable.CollectionChanged -= CollectionChanged;

			if (newValue is INotifyCollectionChanged newObservable)
				newObservable.CollectionChanged += CollectionChanged;

			if (newValue != null)
				ResetItems();
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(e);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems(e);
					break;
				default:
					ResetItems();
					break;
			}
		}

		void AddItems(NotifyCollectionChangedEventArgs e)
		{
			var index = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
			foreach (var newItem in e.NewItems)
				((LockableObservableListWrapper)Items).InternalInsert(index++, GetDisplayMember(newItem));
		}

		void RemoveItems(NotifyCollectionChangedEventArgs e)
		{
			var index = e.OldStartingIndex < Items.Count ? e.OldStartingIndex : Items.Count;
			foreach (var _ in e.OldItems)
				((LockableObservableListWrapper)Items).InternalRemoveAt(index--);
		}

		void ResetItems()
		{
			if (ItemsSource == null)
				return;

			((LockableObservableListWrapper)Items).InternalClear();

			foreach (var item in ItemsSource)
				((LockableObservableListWrapper)Items).InternalAdd(GetDisplayMember(item));

			UpdateSelectedItem(SelectedIndex);
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var segments = (SegmentedView)bindable;
			segments.SelectedItem = newValue;
		}

		void UpdateSelectedItem(int index)
		{
			if (index == -1)
			{
				SelectedItem = null;
				return;
			}

			if (ItemsSource != null)
			{
				SelectedItem = ItemsSource[index];
				return;
			}

			SelectedItem = Items[index];
		}

		public int SelectedIndex
		{
			get => (int)GetValue(SelectedIndexProperty);
			set => SetValue(SelectedIndexProperty, value);
		}

		static void OnSegmentSelected(BindableObject bindable, object oldValue, object newValue)
		{
			if (!(bindable is SegmentedView segment))
				return;

			var index = 0;

			if (!int.TryParse(newValue?.ToString(), out index))
				index = 0;

			segment.SelectedIndexChanged?.Invoke(segment, new SelectedItemChangedEventArgs(segment?.Items[index], index));
			segment.SelectedItem = segment?.Items[index];
		}

		// IColorElement
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set { SetValue(ColorProperty, value); }
		}

		public bool IsColorSet => IsSet(ColorProperty);
	}
}
