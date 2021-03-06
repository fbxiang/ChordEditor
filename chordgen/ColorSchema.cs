﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chordgen
{
    class ColorSchema
    {
        static Dictionary<string, string> RawColor_Light = new Dictionary<string, string>
        {
            { "IV","#FF6347"},
            {"iv","#C87E7E"},
            {"V","#FFFF66"},
            {"I","#FFDD99"},
            {"vi","#99BBFF"},
            {"VI","#8888FF"},
            {"ii","#CCBBFF"},
            {"II","#8A2DFB"},
            {"iii","#99FF99"},
            {"III","#33FF33"},
            {"N","#DDDDDD"}
        };
        static Dictionary<string, string> RawColor_Dark = new Dictionary<string, string>
        {
            {"IV","#8B0000"},
            {"iv","#5E2B2B"},
            {"V","#8B8B00"},
            {"I","#8B5A00"},
            {"vi","#00008B"},
            {"ii","#8B1C62"},
            {"II","#3E0387"},
            {"iii","#006400"},
            {"III","#008B00"},
            {"N","#1C1C1C"}
        };
        static Dictionary<string, string> RawColor = RawColor_Light;
        public static Color GetColorByChordName(string chordName)
        {
            string result = "";
            if(!RawColor.TryGetValue(chordName,out result))
            {
                result = RawColor["N"];
            }
            return ColorTranslator.FromHtml(result);
        }
        public static Color GetTransparentColorByChordName(string chordName,int transparency=50)
        {
            string result = "";
            if (!RawColor.TryGetValue(chordName, out result))
            {
                result = RawColor["N"];
            }
            return Color.FromArgb(transparency, ColorTranslator.FromHtml(result));
        }
    }
}
