using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace GoodbyeDiplom.ViewModels
{
    public class FunctionData
    {
        public List<FunctionModel> Functions { get; set; }
        public ColorScene ColorsScene { get; set; }
        public bool ShowAxes { get; set; }
        public bool ShowLabels { get; set; }
        public bool ShowCube { get; set; }
        public bool ShowGrid { get; set; }
        public bool ShowGraphic { get; set; }
        public double GridSize { get; set; }
        public double AngleX { get; set; }
        public double AngleY { get; set; }
    }
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return Color.Parse(value);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class ColorSceneJsonConverter : JsonConverter<ColorScene>
    {
        public override ColorScene Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                return new ColorScene(
                    Color.Parse(root.GetProperty("ColorX").GetString()),
                    Color.Parse(root.GetProperty("ColorY").GetString()),
                    Color.Parse(root.GetProperty("ColorZ").GetString()),
                    Color.Parse(root.GetProperty("ColorCube").GetString()),
                    Color.Parse(root.GetProperty("ColorBG").GetString()),
                    Color.Parse(root.GetProperty("ColorGrid").GetString()));
            }
        }

        public override void Write(Utf8JsonWriter writer, ColorScene value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("ColorX", value.ColorX.ToString());
            writer.WriteString("ColorY", value.ColorY.ToString());
            writer.WriteString("ColorZ", value.ColorZ.ToString());  
            writer.WriteString("ColorCube", value.ColorCube.ToString());
            writer.WriteString("ColorBG", value.ColorBG.ToString());
            writer.WriteString("ColorGrid", value.ColorGrid.ToString());
            writer.WriteEndObject();
        }
    }
}