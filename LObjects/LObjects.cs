using LiteDB;
using System;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LObjects
{
	public class Profile
	{
		[BsonId]
		public int Id { get; set; }
		public string Name { get; set; }
		public string FilePath { get; set; }
		public Panel Panel { get; set; }
		public List<DesignConfig> DesignConfigs { get; set; }

	}
	public class Panel
	{
		public int Id { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public string CutColor { get; set; }
		public string PrintFilePath { get; set; }
		public string CutFilePath { get; set; }
		public ArtConfig ArtConfig { get; set; }
		public List<BaseArt> ContainArts { get; set; }
	}
    public class ArtConfig
	{
		public int Id { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public double Space { get; set; }

	}
    public class DesignConfig
    {
        public int Id { get; set; }
		public string Label { get; set; }
		public string FilePath { get; set; }
		public List<ItemConfig> ItemConfigs { get; set; }
    } 
    public class BaseArt
    {
        public int Id { get; set; }
		public string FilePath { get; set; }
		public DesignConfig DesignConfig { get; set; }
		public List<string> Values { get; set; }
    }
    public class Arts
	{
		public DesignConfig DesignConfig { get; set; }
		public Queue<BaseArt> QueueArt { get; set; }
    }
    public class ItemConfig
	{
		public string Name { get; set; }
		public bool IsMagic { get; set; }
        public ItemMapping Mapping { get; set; }
    }
	public class TextConfig : ItemConfig
	{
		public bool IsOneLine { get; set; }
		public double FontSize { get; set; }
		public Polygon GetMapped(char c)
		{
			for (int i = 0; i < Mapping.Values.Length; i++)
				if (Mapping.Values[i] == c)
					return Mapping.Polygon[i];
			return null;
		}
		public Polygon[] GetMapped(string s)
		{
			Polygon[] pls = new Polygon[s.Length];
			for (int i = 0; i < s.Length; i++)
				pls[i] = GetMapped(s[i]);
			return pls;
        }
	}
    public class PathConfig : ItemConfig
    {
        public bool IsFit { get; set; }
		public double FitWidth { get; set; }
		public double FitHeight { get; set; }
		public Polygon GetMapped()
		{
			return Mapping.Polygon[0];
		}
    }
	public class ItemMapping
	{
		public string Values { get; set; }
		public Polygon[] Polygon { get; set; }
	}
}
