﻿using IniParser;
using IniParser.Model;
using static QSP.Core.EnumConversionTools;

namespace QSP.AircraftProfiles
{
    public static class ConfigSaver
    {
        public static void Save(AircraftConfig config, string filePath)
        {
            var keys = new KeyDataCollection();
            keys.AddKey("AC", config.AC);
            keys.AddKey("Registration", config.Registration);
            keys.AddKey("TOProfile", config.TOProfile);
            keys.AddKey("LdgProfile", config.LdgProfile);
            keys.AddKey("ZfwKg", config.ZfwKg.ToString());
            keys.AddKey("MaxTOWtKg", config.MaxTOWtKg.ToString());
            keys.AddKey("MaxLdgWtKg", config.MaxLdgWtKg.ToString());
            keys.AddKey("WtUnit", WeightUnitToString(config.WtUnit));

            var secData = new SectionData("Data");
            secData.Keys = keys;

            var sections = new SectionDataCollection();
            sections.Add(secData);

            var iniData = new IniData(sections);
            new FileIniDataParser().WriteFile(filePath, iniData);
        }
    }
}