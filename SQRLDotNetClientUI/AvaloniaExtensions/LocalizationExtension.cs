﻿using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Data.Converters;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace SQRLDotNetClientUI.AvaloniaExtensions
{
    /// <summary>
    /// This extension allows you to have strings in different languages based on
    /// localization culture on the current machine
    /// This expects an Asset located in Assets/Localization/localization.json
    /// 
    /// The Format of the JSON in that file is as follows
    ///
    /*
    {
        "default":
        [
          {
            "SQRLTag":"Secure Quick Reliable Login"
          }
        ],
        "en-US":
        [
          {
            "SQRLTag":"Secure Quick Reliable Login"
          }
        ]
    } */
    /// The extension will use the default node if the current specific culture 
    /// isn't found in the file.
    /// </summary>
    public class LocalizationExtension : MarkupExtension
    {
        /// <summary>
        /// Magic string for marking the default localization.
        /// </summary>
        public static readonly string DEFAULT_LOC = "default";

        private string _resourceId { get; set; }
        private IAssetLoader _assets { get; set; }
        private static string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static JObject _localizationStrings { get; set; } = null;
        private static string _currentLocalization = DEFAULT_LOC;
        private static bool _initialized = false;

        /// <summary>
        /// Gets or sets the currently active localization using the
        /// format languagecode2-country/regioncode2 (e.g. "en-US").
        /// </summary>
        public static string CurrentLocalization
        {
            get { return _currentLocalization; }
            set
            {
                if (Localizations.ContainsKey(value))
                {
                    _currentLocalization = value;
                }
                else
                {
                    throw new ArgumentException("The specified localization is not yet supported!");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <c>IValueConverter</c> to use.
        /// </summary>
        public IValueConverter Converter { get; set; }

        /// <summary>
        /// Provides a list of available localizations and their resources.
        /// </summary>
        public static Dictionary<string, LocalizationInfo> Localizations { get; } = new Dictionary<string, LocalizationInfo>();

        /// <summary>
        /// Creates a new <c>LocalizationExtension</c> instance.
        /// </summary>
        public LocalizationExtension()
        {
            _assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            if (!_initialized)
            {
                RegisterLocalizations();
                GetLocalization();
            }

            _initialized = true;
        }

        /// <summary>
        /// Registers all the available localizations.
        /// If a new translation language is added, it has to be added here.
        /// </summary>
        private void RegisterLocalizations()
        {
            string uriPath = $"resm:{_assemblyName}.Assets.Localization.Flags.";

            // Register new localizations in this list!
            List<LocalizationInfo> localizations = new List<LocalizationInfo>()
            {
                new LocalizationInfo()
                {
                    CultureInfo = CultureInfo.CreateSpecificCulture("en-US"),
                    Image = new Bitmap(_assets.Open(new Uri(uriPath + "united_states_16.png")))
                },

                new LocalizationInfo()
                {
                    CultureInfo = CultureInfo.CreateSpecificCulture("de-DE"),
                    Image = new Bitmap(_assets.Open(new Uri(uriPath + "germany_16.png")))
                }

            };

            // Create the default localization and add id
            var defaultLoc = new LocalizationInfo()
            {
                CultureInfo = CultureInfo.CurrentCulture,
                Image = new Bitmap(_assets.Open(new Uri(uriPath + "default_16.png")))
            };

            Localizations.Add(DEFAULT_LOC, defaultLoc);

            // Now add the registered localizations
            foreach (var localization in localizations)
            {
                Localizations.Add(localization.CultureInfo.Name, localization);
            }
        }

        /// <summary>
        /// Creates a new <c>LocalizationExtension</c> instance and sets the 
        /// resource id to the given <paramref name="resourceID"/>.
        /// </summary>
        public LocalizationExtension(string resourceID) : this()
        {
            this._resourceId = resourceID;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return GetLocalizationValue(this._resourceId);
        }

        /// <summary>
        /// Returns the localized string value for the given <paramref name="resourceID"/>
        /// </summary>
        public string GetLocalizationValue(string resourceID)
        {
            var activeCulture = Localizations[_currentLocalization].CultureInfo;
            string localizedString = null;

            if (_localizationStrings.ContainsKey(activeCulture.Name))
            {
                try
                {
                    localizedString = ResolveFormatting(
                        _localizationStrings[activeCulture.Name].Children()[resourceID].First().ToString());
                }
                catch { }
            }

            if (localizedString == null)
            {
                try
                {
                    localizedString = ResolveFormatting(
                        _localizationStrings[DEFAULT_LOC].Children()[resourceID].First().ToString());
                }
                catch
                {
                    return "Missing translation: " + resourceID;
                }
            }

            if (Converter != null)
                localizedString = (string)Converter.Convert(localizedString, typeof(string), null, activeCulture);

            return localizedString;
        }

        /// <summary>
        /// Reads the project's localization .json file into a <c>JObject</c>.
        /// </summary>
        private void GetLocalization()
        {
            _localizationStrings = (JObject)JsonConvert.DeserializeObject(new StreamReader(
                _assets.Open(new Uri($"resm:{_assemblyName}.Assets.Localization.localization.json"))).ReadToEnd());
        }

        /// <summary>
        /// Finds any escaped control sequences like "\n" in the given <paramref name="input"/>
        /// string and returns a string where any of such sequences are converted.
        /// </summary>
        /// <param name="input">The input string containing control sequences such as "\n".</param>
        private string ResolveFormatting(string input)
        {
            return input
                .Replace("\\r\\n", Environment.NewLine)
                .Replace("\\n", Environment.NewLine)
                .Replace("\\t", "\t");
        }
    }

    /// <summary>
    /// Holds information about a particular localization.
    /// </summary>
    public class LocalizationInfo
    {
        public CultureInfo CultureInfo;
        public Bitmap Image;
    }
}
