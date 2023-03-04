﻿using System;
using System.Globalization;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Avalonia.Skia.UnitTests.Media
{
    public class CustomFontManagerImpl : IFontManagerImpl
    {
        private readonly Typeface[] _customTypefaces;
        private readonly string _defaultFamilyName;

        private readonly Typeface _defaultTypeface =
            new Typeface("resm:Avalonia.Skia.UnitTests.Assets?assembly=Avalonia.Skia.UnitTests#Noto Mono");
        private readonly Typeface _arabicTypeface =
           new Typeface("resm:Avalonia.Skia.UnitTests.Assets?assembly=Avalonia.Skia.UnitTests#Noto Sans Arabic");
        private readonly Typeface _hebrewTypeface =
         new Typeface("resm:Avalonia.Skia.UnitTests.Assets?assembly=Avalonia.Skia.UnitTests#Noto Sans Hebrew");
        private readonly Typeface _italicTypeface =
            new Typeface("resm:Avalonia.Skia.UnitTests.Assets?assembly=Avalonia.Skia.UnitTests#Noto Sans", FontStyle.Italic);
        private readonly Typeface _emojiTypeface =
            new Typeface("resm:Avalonia.Skia.UnitTests.Assets?assembly=Avalonia.Skia.UnitTests#Twitter Color Emoji");

        public CustomFontManagerImpl()
        {
            _customTypefaces = new[] { _emojiTypeface, _italicTypeface, _arabicTypeface, _hebrewTypeface, _defaultTypeface };
            _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
        }

        public string GetDefaultFontFamilyName()
        {
            return _defaultFamilyName;
        }

        public string[] GetInstalledFontFamilyNames(bool checkForUpdates = false)
        {
            return _customTypefaces.Select(x => x.FontFamily.Name).ToArray();
        }

        private readonly string[] _bcp47 = { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch,
            FontFamily fontFamily,
            CultureInfo culture, out Typeface typeface)
        {
            foreach (var customTypeface in _customTypefaces)
            {
                if (customTypeface.GlyphTypeface.GetGlyph((uint)codepoint) == 0)
                {
                    continue;
                }

                typeface = new Typeface(customTypeface.FontFamily, fontStyle, fontWeight);

                return true;
            }

            var fallback = SKFontManager.Default.MatchCharacter(fontFamily?.Name, (SKFontStyleWeight)fontWeight,
                (SKFontStyleWidth)fontStretch, (SKFontStyleSlant)fontStyle, _bcp47, codepoint);

            typeface = new Typeface(fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

            return true;
        }

        public IGlyphTypeface CreateGlyphTypeface(Typeface typeface)
        {
            SKTypeface skTypeface;

            Uri source = null;

            switch (typeface.FontFamily.Name)
            {
                case "Twitter Color Emoji":
                    {
                        source = _emojiTypeface.FontFamily.Key.Source;
                        break;
                    }
                case "Noto Sans":
                    {
                        source = _italicTypeface.FontFamily.Key.Source;
                        break;
                    }
                case "Noto Sans Arabic":
                    {
                        source = _arabicTypeface.FontFamily.Key.Source;
                        break;
                    }
                case "Noto Sans Hebrew":
                    {
                        source = _hebrewTypeface.FontFamily.Key.Source;
                        break;
                    }
                case FontFamily.DefaultFontFamilyName:
                case "Noto Mono":
                    {
                        source = _defaultTypeface.FontFamily.Key.Source;
                        break;
                    }
                default:
                    {

                        break;
                    }
            }

            if (source is null)
            {
                skTypeface = SKTypeface.FromFamilyName(typeface.FontFamily.Name,
                           (SKFontStyleWeight)typeface.Weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)typeface.Style);
            }
            else
            {
                var assetLoader = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();

                var assetUri = FontFamilyLoader.LoadFontAssets(source).First();

                var stream = assetLoader.Open(assetUri);

                skTypeface = SKTypeface.FromStream(stream);
            }

            return new GlyphTypefaceImpl(skTypeface, FontSimulations.None);
        }

        public bool TryCreateGlyphTypeface(string familyName, FontStyle style, FontWeight weight,
            FontStretch stretch, [NotNullWhen(true)] out IGlyphTypeface glyphTypeface)
        {
            SKTypeface skTypeface;

            Uri source = null;

            switch (familyName)
            {
                case "Twitter Color Emoji":
                    {
                        source = _emojiTypeface.FontFamily.Key.Source;
                        break;
                    }
                case "Noto Sans":
                    {
                        source = _italicTypeface.FontFamily.Key.Source;
                        break;
                    }
                case "Noto Sans Arabic":
                    {
                        source = _arabicTypeface.FontFamily.Key.Source;
                        break;
                    }
                case "Noto Sans Hebrew":
                    {
                        source = _hebrewTypeface.FontFamily.Key.Source;
                        break;
                    }
                case FontFamily.DefaultFontFamilyName:
                case "Noto Mono":
                    {
                        source = _defaultTypeface.FontFamily.Key.Source;
                        break;
                    }
                default:
                    {

                        break;
                    }
            }

            if (source is null)
            {
                skTypeface = SKTypeface.FromFamilyName(familyName,
                           (SKFontStyleWeight)weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)style);
            }
            else
            {
                var assetLoader = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();

                var assetUri = FontFamilyLoader.LoadFontAssets(source).First();

                var stream = assetLoader.Open(assetUri);

                skTypeface = SKTypeface.FromStream(stream);
            }

            glyphTypeface = new GlyphTypefaceImpl(skTypeface, FontSimulations.None);

            return true;
        }

        public bool TryCreateGlyphTypeface(Stream stream, [NotNullWhen(true)] out IGlyphTypeface glyphTypeface)
        {
            var skTypeface = SKTypeface.FromStream(stream);

            glyphTypeface = new GlyphTypefaceImpl(skTypeface, FontSimulations.None);

            return true;
        }
    }
}
