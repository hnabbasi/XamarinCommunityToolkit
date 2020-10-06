﻿using Xamarin.CommunityToolkit.UI.Views;

namespace Xamarin.CommunityToolkit.Sample.ViewModels.Views
{
	public class GravatarImageViewModel : BaseViewModel
	{
		bool enableCache;
		string email = "dsiegel@avantipoint.com";
		int size = 50;
		DefaultGravatar defaultGravatar = DefaultGravatar.MysteryPerson;

		public DefaultGravatar[] Defaults { get; } = new[]
		{
			DefaultGravatar.Blank,
			DefaultGravatar.FileNotFound,
			DefaultGravatar.Identicon,
			DefaultGravatar.MonsterId,
			DefaultGravatar.MysteryPerson,
			DefaultGravatar.Retro,
			DefaultGravatar.Robohash,
			DefaultGravatar.Wavatar
		};

		public bool EnableCache
		{
			get => enableCache;
			set => Set(ref enableCache, value);
		}

		public string Email
		{
			get => email;
			set => Set(ref email, value);
		}

		public int Size
		{
			get => size;
			set => Set(ref size, value);
		}

		public DefaultGravatar DefaultGravatar
		{
			get => defaultGravatar;
			set => Set(ref defaultGravatar, value);
		}
	}
}