using System.Runtime.CompilerServices;
using UnityEngine;

namespace ODev.Util
{
	public static class Colour
	{
		public static Color FromHex(string pHEX)
		{
			if (string.IsNullOrEmpty(pHEX))
			{

				return Color.white;
			}
			if (pHEX[0] == '#')
			{
				Debug.LogError("ColorConst.FromHex: Colour hex code doesnt start with a \'#\'.", typeof(Colour));
			}
			ColorUtility.TryParseHtmlString(pHEX, out Color colour);
			return colour;
		}

		public static Color Yellow
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(1f, 47f / 51f, 0.015686275f, 1f);
		}
		public static Color White
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(1f, 1f, 1f, 1f);
		}
		public static Color Magenta
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(1f, 0f, 1f, 1f);
		}
		public static Color Green
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0f, 1f, 0f, 1f);
		}
		public static Color Grey
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0.5f, 0.5f, 0.5f, 1f);
		}
		public static Color Cyan
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0f, 1f, 1f, 1f);
		}
		public static Color Clear
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0f, 0f, 0f, 0f);
		}
		public static Color Red
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(1f, 0f, 0f, 1f);
		}
		public static Color Black
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0f, 0f, 0f, 1f);
		}
		public static Color Blue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0f, 0f, 1f, 1f);
		}

		public static Color DarkRed
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0.4f, 0.0f, 0.0f, 1.0f);
		}
		public static Color DarkGreen
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0.0f, 0.4f, 0.0f, 0.0f);
		}
		public static Color DarkBlue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0.0f, 0.0f, 0.4f, 1.0f);
		}
		public static Color Purple
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0.4f, 0.0f, 0.6f, 1.0f);
		}
		public static Color Orange
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Color32(255, 120, 0, 255);
		}
		public static Color Brown
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(0.5377358f, 0.191421f, 0.0f, 1.0f);
		}
		public static Color Merlot
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Color32(127, 0, 44, 255);
		}
		public static Color BlueGreen
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Color32(0, 80, 80, 255);
		}
		public static Color SeaFoam
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Color32(120, 220, 160, 255);
		}
		public static Color Pink
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Color32(255, 174, 201, 255);
		}
	}
}
