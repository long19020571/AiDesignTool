using LiteDB;
using System;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using static LObjects.DesignConfig;

namespace LObjects
{
	public class Profile
	{
		[BsonId]
		public int Id { get; set; }
		public string Name { get; set; }
		public string FilePath { get; set; }
		[BsonRef("panels")]
		public Panel Panel { get; set; }
		[BsonRef("designconfigs")]
		public List<DesignConfig> DesignConfigs { get; set; }
		public Profile()
		{
			Panel = new Panel();
			DesignConfigs = new List<DesignConfig>();
		}

	}
	public class Panel
	{
        [BsonId]
        public int Id { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public string CutColor { get; set; }
		public string PrintFilePath { get; set; }
		public string CutFilePath { get; set; }
        [BsonRef("artconfigs")]
        public ArtConfig ArtConfig { get; set; }
		[BsonIgnore]
        public List<BaseArt> ContainArts { get; set; }
		public Panel()
		{
			ArtConfig = new ArtConfig();
			ContainArts = new List<BaseArt>();
		}
	}
    public class ArtConfig
	{
        [BsonId]
        public int Id { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public double Space { get; set; }

	}
    public class DesignConfig
    {
        [BsonId]
        public int Id { get; set; }
		public string Label { get; set; }
		public string FilePath { get; set; }
        [BsonRef("itemconfigs")]
        public List<ItemConfig> ItemConfigs { get; set; }
		public DesignConfig()
		{
			ItemConfigs = new List<ItemConfig>();
		}
    } 
    public class BaseArt
    {
        public int Id { get; set; }
		public string FilePath { get; set; }
		public DesignConfig DesignConfig { get; set; }
		public List<string> Values { get; set; }
		public BaseArt()
		{
			DesignConfig = new DesignConfig();
				Values = new List<string>();
		}
    }
    public class Arts
	{
		public DesignConfig DesignConfig { get; set; }
		public Queue<BaseArt> QueueArt { get; set; }
		public Arts()
			{
				DesignConfig = new DesignConfig();
				QueueArt = new Queue<BaseArt>();
			}
    }
    public class ItemConfig
	{
        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
		public bool IsMagic { get; set; }

        [BsonRef("itemmappings")]
        public ItemMapping Mapping { get; set; }
		public ItemConfig()
			{
				Mapping = new ItemMapping();
			}
    }
	public class TextConfig : ItemConfig
	{
		public bool IsOneLine { get; set; }
		public double FontSize { get; set; }
		public TextConfig() : base()
			{

			}
		public LPolygon GetMapped(char c)
		{
			for (int i = 0; i < Mapping.Values.Length; i++)
				if (Mapping.Values[i] == c)
					return Mapping.Polygon[i];
			return null;
		}
		public LPolygon[] GetMapped(string s)
		{
            LPolygon[] pls = new LPolygon[s.Length];
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
			public PathConfig() : base()
			{ }
		public LPolygon GetMapped()
		{
			return Mapping.Polygon[0];
		}
    }
	public class ItemMapping
	{
        [BsonId]
        public int Id { get; set; }
        public string Values { get; set; }
        public LPolygon[] Polygon { get; set; }
	}
    public class LPolygon
    {
        [BsonId]
        public int Id { get; set; }
		public byte[] Polygon;
    }
}
