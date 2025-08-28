using System.Collections.Generic;
using UnityEngine;
using static ColorBlindFilter.ColorblindFeature;

namespace ColorBlindFilter;

public static class ColorBlindnessValues
{
    public static Material Material { get; internal set; }
    
    public static bool Boost { get; set; }
    
    public static float GlobalMultiplier { get; set; } = 1f;
    
    public static readonly Dictionary<ColorBlindMode, Color[]> Colors = new() {
        { ColorBlindMode.Normal, [ new(1, 0, 0), new(0, 1, 0), new(0, 0, 1) ] },
        { ColorBlindMode.Protanopia, [ new(.56667f, .43333f, 0f), new(.55833f, .44167f, 0f), new(0f, .25167f, .75833f) ] },
        { ColorBlindMode.Protanomaly, [ new(.81667f, .18333f, 0f), new(.33333f, .66667f, 0f), new(0f, .125f, .875f) ] },
        { ColorBlindMode.Deuteranopia, [ new(.625f, .375f, 0f), new(.7f, .3f, 0f), new(0f, .3f, .7f) ] },
        { ColorBlindMode.Deuteranomaly, [ new(.8f, .2f, 0f), new(0f, .25833f, .74167f), new(0f, .14167f, .85833f) ] },
        { ColorBlindMode.Tritanopia, [ new(.95f, .05f, 0f), new(0f, .43333f, .56667f), new(0f, .475f, .525f) ] },
        { ColorBlindMode.Tritanomaly, [ new(.96667f, .03333f, 0f), new(0f, .73333f, .26667f), new(0f, .18333f, .81667f) ] },
        { ColorBlindMode.Achromatopsia, [ new(.299f, .587f, .114f), new(.299f, .587f, .114f), new(.299f, .587f, .114f) ] },
        { ColorBlindMode.Achromatomaly, [ new(.618f, .32f, .062f), new(.163f, .775f, .062f), new(.163f, .32f, .516f) ] },
    };

    public static readonly Dictionary<ColorBlindMode, float> Multipliers = new()
    {
        { ColorBlindMode.Normal, 1f },
        { ColorBlindMode.Protanopia, 1.4f },
        { ColorBlindMode.Protanomaly, 1.2f },
        { ColorBlindMode.Deuteranopia,1.4f },
        { ColorBlindMode.Deuteranomaly, 1.2f },
        { ColorBlindMode.Tritanopia, 1.025f },
        { ColorBlindMode.Tritanomaly, 1.0125f },
        { ColorBlindMode.Achromatopsia, 1.55f },
        { ColorBlindMode.Achromatomaly, 1.3f },
    };

    public static void ApplyMode(ColorBlindMode mode)
    {
        if (!Colors.TryGetValue(mode, out var colors)
            && !Colors.TryGetValue(ColorBlindMode.Normal, out colors))
            return;

        if (!Multipliers.TryGetValue(mode, out var multiplier)
            && !Multipliers.TryGetValue(ColorBlindMode.Normal, out multiplier))
            return;
        
        if (!Boost)
            multiplier = 1f;
        
        var mat = Material;
        
        mat.SetColor("_R", colors[0] * multiplier * GlobalMultiplier);
        mat.SetColor("_G", colors[1] * multiplier * GlobalMultiplier);
        mat.SetColor("_B", colors[2] * multiplier * GlobalMultiplier);
    }
}