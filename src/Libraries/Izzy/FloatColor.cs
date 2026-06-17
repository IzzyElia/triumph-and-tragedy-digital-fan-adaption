using System;
using System.Globalization;
using TT2026.libraries.Izzy.UnitTesting;
using TT2026.libraries.Izzy.Utils;

namespace TT2026.libraries.Izzy;

[System.Serializable]
public struct FloatColor : IEquatable<FloatColor>
{
    public static FloatColor Red { get { return new FloatColor(1, 0, 0); } }
    public static FloatColor Green { get { return new FloatColor(0, 1, 0); } }
    public static FloatColor Blue { get { return new FloatColor(0, 0, 1); } }
    public static FloatColor Pink { get { return new FloatColor(1, 0, 1); } }
    public static FloatColor Black { get { return new FloatColor(0, 0, 0); } }
    public static FloatColor White { get { return new FloatColor(1, 1, 1); } }
    public static FloatColor Clear { get { return new FloatColor(1, 1, 1, 0); } }
    public static FloatColor RandomOpaque() => new FloatColor(RandFi.Float(), RandFi.Float(), RandFi.Float(), 1);

    public float red, green, blue, alpha;
    public FloatColor(float red, float green, float blue, float alpha)
    {
        this.red = red; this.green = green; this.blue = blue; this.alpha = alpha;
    }
    public FloatColor(float red, float green, float blue)
    {
        this.red = red; this.green = green; this.blue = blue; this.alpha = 1;
    }
    public FloatColor (string hex)
    {
        if (!TryParseHex(hex, out this))
        {
            this.red = 0; this.green = 0; this.blue = 0; this.alpha = 1;
        }
    }

    public static FloatColor operator +(FloatColor a, FloatColor b)
    {
        return new FloatColor
        (
            a.red + b.red,
            a.green + b.green,
            a.blue + b.blue,
            a.alpha + b.alpha
        );
    }
    public static FloatColor operator +(FloatColor a, float b)
    {
        return new FloatColor
        (
            a.red + b,
            a.green + b,
            a.blue + b,
            a.alpha + b
        );
    }
    public static FloatColor operator -(FloatColor a, FloatColor b)
    {
        return new FloatColor
        (
            a.red - b.red,
            a.green - b.green,
            a.blue - b.blue,
            a.alpha - b.alpha
        );
    }
    public static FloatColor operator -(FloatColor a, float b)
    {
        return new FloatColor
        (
            a.red - b,
            a.green - b,
            a.blue - b,
            a.alpha - b
        );
    }
    public static FloatColor operator *(FloatColor a, FloatColor b)
    {
        return new FloatColor
        (
            a.red * b.red,
            a.green * b.green,
            a.blue * b.blue,
            a.alpha * b.alpha
        );
    }
    public static FloatColor operator *(FloatColor a, float b)
    {
        return new FloatColor
        (
            a.red * b,
            a.green * b,
            a.blue * b,
            a.alpha * b
        );
    }
    public static FloatColor operator /(FloatColor a, FloatColor b)
    {
        return new FloatColor
        (
            a.red / b.red,
            a.green / b.green,
            a.blue / b.blue,
            a.alpha / b.alpha
        );
    }
    public static FloatColor operator /(FloatColor a, float b)
    {
        return new FloatColor
        (
            a.red / b,
            a.green / b,
            a.blue / b,
            a.alpha / b
        );
    }
    public static bool operator ==(FloatColor a, FloatColor b) => a.Equals(b);
    public static bool operator !=(FloatColor a, FloatColor b) => !a.Equals(b);
    public static FloatColor BlendAvg(FloatColor a, FloatColor b)
    {
        return BlendAvg(new FloatColor[2] { a, b });
    }
    public static FloatColor BlendAvg(FloatColor[] colors)
    {
        FloatColor average = new FloatColor(0, 0, 0);
        foreach (FloatColor color in colors)
        {
            average += color;
        }
        average /= colors.Length;
        return average;
    }
    public static FloatColor BlendAvg(FloatColor[] colors, float[] weights)
    {
        FloatColor average = new FloatColor(0, 0, 0);
        for (int i = 0; i < colors.Length; i++)
        {
            average += (colors[i] * weights[i]);
        }
        average /= ArrayUtils.SumArray(weights);
        return average;
    }
    public static FloatColor Lerp (FloatColor a, FloatColor b, float t)
    {
        return new FloatColor(
            a.red + (b.red - a.red) * t,
            a.green + (b.green - a.green) * t,
            a.blue + (b.blue - a.blue) * t,
            a.alpha + (b.alpha - a.alpha) * t
        );
    }
    /// <summary>
    /// Defaults to opaque black (0,0,0,1)
    /// </summary>
    public static bool TryParseHex(string hex, out FloatColor color)
    {
        color = Black;
        if (hex == null) { return false; }
        if (hex.Length > 0 && hex[0] == '#') hex = hex.Substring(1);
        if (!(hex.Length == 6 || hex.Length == 8))
        {
            return false;
        }
        int r, g, b, a;
        if (!int.TryParse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out r)) { return false; }
        if (!int.TryParse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out g)) { return false; }
        if (!int.TryParse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out b)) { return false; }
        if (hex.Length == 8)
        {
            if (!int.TryParse(hex.Substring(6, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out a)) { return false; }
        }
        else { a = 255; }


        color = new FloatColor
        (
            (float)r/255f,
            (float)g / 255f, 
            (float)b / 255f, 
            (float)a / 255f
        );
        return true;
    }
    public static FloatColor ParseHexOrFallback (string hex, FloatColor fallback = default)
    {
        switch (TryParseHex(hex, out FloatColor color))
        {
            case true:
                return color;
            case false:
                return fallback;
        }
    }

    //HSV Support
    public struct HSV
    {
        public float hue { get; private set; }
        public float saturation { get; private set; }
        public float value { get; private set; }
        public float alpha { get; private set; }
        public HSV(float hue, float saturation, float value, float alpha = 0)
        {
            this.hue = hue % 360;
            this.saturation = saturation;
            this.value = value;
            this.alpha = alpha;
        }
        public HSV (FloatColor color)
        {
            float max = Mathfi.Max(color.red, color.green, color.blue);
            float min = Mathfi.Min(color.red, color.green, color.blue);

            this.hue = color.GetHue();
            this.saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            this.value = max;
            this.alpha = color.alpha;
        }
        public HSV BlendHue(HSV b) => new HSV(this.hue + b.hue, this.saturation * b.saturation, this.value, this.alpha);
        public HSV BlendHue(FloatColor b) => BlendHue(b.GetHSV());
        public FloatColor RGB => new FloatColor(this);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 * hue.GetHashCode();
                hash = hash * 31 * saturation.GetHashCode();
                hash = hash * 31 * value.GetHashCode();
                hash = hash * 31 * alpha.GetHashCode();
                return hash;
            }
        }
        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(HSV) && Equals((HSV)obj);
        }
        public bool Equals(HSV obj)
        {
            return GetHashCode().Equals(obj.GetHashCode());
        }
    }
    public FloatColor(HSV hsv)
    {
        // ######################################################################
        // Derived from code by
        // T. Nathan Mundhenk
        // mundhenk@usc.edu
        // https://www.splinter.com.au/converting-hsv-to-rgb-colour-using-c/
        this.alpha = hsv.alpha;

        float hue = hsv.hue;
        float saturation = hsv.saturation;
        float value = hsv.value;


        float H = hue % 360;
        //while (H < 0) { H += 360; };
        //while (H >= 360) { H -= 360; };
        if (value <= 0)
        { red = green = blue = 0; }
        else if (saturation <= 0)
        {
            red = green = blue = value;
            return;
        }
        else
        {
            float hf = H / 60.0f;
            int i = (int)Mathfi.Floor(hf);
            float f = hf - i;
            float pv = value * (1 - saturation);
            float qv = value * (1 - saturation * f);
            float tv = value * (1 - saturation * (1 - f));
            switch (i)
            {

                // Red is the dominant color

                case 0:
                    red = value;
                    green = tv;
                    blue = pv;
                    return;

                // Green is the dominant color

                case 1:
                    red = qv;
                    green = value;
                    blue = pv;
                    return;
                case 2:
                    red = pv;
                    green = value;
                    blue = tv;
                    return;

                // Blue is the dominant color

                case 3:
                    red = pv;
                    green = qv;
                    blue = value;
                    return;
                case 4:
                    red = tv;
                    green = pv;
                    blue = value;
                    return;

                // Red is the dominant color

                case 5:
                    red = value;
                    green = pv;
                    blue = qv;
                    return;

                // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                case 6:
                    red = value;
                    green = tv;
                    blue = pv;
                    return;
                case -1:
                    red = value;
                    green = pv;
                    blue = qv;
                    return;

                // The color is not defined, we should throw an error.

                default:
                    DynamicLogger.Log($"Unknown error converting HSV [{hsv.ToString()}] to RGB");
                    red = green = blue = value; // Just pretend its black/white
                    return;
            }
        }
        //r = Clamp((int)(red * 255.0));
        //g = Clamp((int)(green * 255.0));
        //b = Clamp((int)(blue * 255.0));
    }
    public HSV GetHSV() => new HSV(this);
    public float GetHue()
    {
        float min = Mathfi.Min(red, green, blue);
        float max = Mathfi.Max(red, green, blue);
        if (red >= green && red >= blue)
        {
            return ((green - blue) / (max - min) * 60f) % 360;
        }
        else if (green > blue)
        {
            return ((blue - red) / (max - min) * 120f) % 360;
        }
        else
        {
            return ((red - green) / (max - min) * 240f) % 360;
        }
    }

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(FloatColor) && Equals((FloatColor)obj);
    }

    public bool Equals(FloatColor obj)
    {
        return GetHashCode().Equals(obj.GetHashCode());
    }

    public override string ToString()
    {
        return $"[r-{red}, g-{green}, b-{blue}, a-{alpha}]";
    }
    public int HashCode => GetHashCode();

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + red.GetHashCode();
            hash = hash * 31 + green.GetHashCode();
            hash = hash * 31 + blue.GetHashCode();
            hash = hash * 31 + alpha.GetHashCode();
            return hash;
        }
    }
}


// Unit Tests
# if DEBUG
internal class FloatColor_Tests
{
    [Test]
    static TestResult Test_TryParseHex()
    {
        string validHex1 = "a5e8b7";
        string validHex2 = "a5E8b7";
        string validHex3 = "73aBbf5F";
        string invalidHex1 = "h8bbF2";
        string invalidHex2 = "a5e8b";
        FloatColor color = default;
        if (!FloatColor.TryParseHex(validHex1, out color)) { return new TestResult(false, validHex1); }
        if (!FloatColor.TryParseHex(validHex2, out color)) { return new TestResult(false, validHex2); }
        if (!FloatColor.TryParseHex(validHex3, out color)) { return new TestResult(false, validHex3); }
        if (FloatColor.TryParseHex(invalidHex1, out color)) { return new TestResult(false, invalidHex1); }
        if (FloatColor.TryParseHex(invalidHex2, out color)) { return new TestResult(false, invalidHex2); }
        return new TestResult(true);
    }
}

#endif