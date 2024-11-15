using LiteDB;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text;
using System.Windows.Media.Media3D;

namespace LObjects
{
    public class Profile
    {
        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FolderPath { get; set; }
        [BsonRef("panels")]
        public LPanel Panel { get; set; }
        [BsonRef("designconfigs")]
        public List<DesignConfig> DesignConfigs { get; set; }
        public Profile()
        {
            Panel = new LPanel();
            DesignConfigs = new List<DesignConfig>();
        }

    }
    public class LPanel
    {
        [BsonIgnore]
        private static int _GlobalPrintId = 0;
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
        public int PrintId { get; }
        [BsonIgnore]
        public List<BaseArt> ContainArts { get; set; }
        [BsonIgnore]
        public double[] Boundary { get; set; }
        public LPanel()
        {
            PrintId = _GlobalPrintId;
            ++_GlobalPrintId;

            ArtConfig = new ArtConfig();
            ContainArts = new List<BaseArt>();
            Boundary = new double[4] { 0, 0, 0, 0 };
        }
        public double[]? PlaceArt(BaseArt art)
        {
            double[] last = ContainArts.Count > 0 ? ContainArts.Last().Boundary : new double[] { 0, 0, 0, 0 };
            double[] pos = { double.NaN, double.NaN };
            double x = last[0] + last[2], y = last[1],
                w = x + art.Boundary[2], h = y + art.Boundary[3];

            if(w <= Width)
            {
                if (h < Height)
                {
                    art.Boundary[0] = x; art.Boundary[1] = y;
                    Boundary[3] = h > Boundary[3] ? h : Boundary[3];
                    Boundary[2] = w > Boundary[2] ? w : Boundary[2];
                    pos[0] = x; pos[1] = y;
                } else
                {
                    return null;
                }
            }else
            {
                if (art.Boundary[3] < Height)
                {
                    art.Boundary[0] = 0; art.Boundary[1] = Boundary[3];
                    pos[0] = 0; pos[1] = Boundary[3];
                    Boundary[3] = Boundary[3] + art.Boundary[3];
                } else
                {
                    return null;
                }
            }
            ContainArts.Add(art);
            return pos;
            
        }
        public double[] GetRectId()
        {
            return new double[4] { 0, 0, ArtConfig.Space / 2, ArtConfig.Space / 2 };
        }
        public LPanel CopyConfig()
        {
            LPanel lPanel = new LPanel();
            lPanel.Id = Id;
            lPanel.Width = Width;
            lPanel.Height = Height;
            lPanel.CutColor = CutColor;
            lPanel.PrintFilePath = PrintFilePath;
            lPanel.CutFilePath = CutFilePath;
            lPanel.ArtConfig = ArtConfig.Copy();
            lPanel.ContainArts = new List<BaseArt>();
            return lPanel;
        }
    }
    public class ArtConfig
    {
        [BsonId]
        public int Id { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Space { get; set; }
        public ArtConfig() { }

        public ArtConfig(int Id, double Width, double Height, double Space)
        {
            this.Id = Id;
            this.Width = Width;
            this.Height = Height;
            this.Space = Space;
        }
        public ArtConfig Copy()
        {
            return new ArtConfig(Id, Width, Height, Space);
        }
        public double[] GetTrueSide()
        {
            return new double[] { Width + Space, Height + Space };
        }

    }
    public class DesignConfig
    {
        [BsonId]
        public int Id { get; set; }
        public string Label { get; set; }
        public string FilePath { get; set; }
        [BsonRef("itemconfigs")]
        public List<ItemConfig> ItemConfigs { get; set; }
        [BsonRef("itemmappings")]
        public List<ItemMapping> ItemMappings { get; set; }
        public DesignConfig()
        {
            Label = new string("NoLabel");
            FilePath = "C\\A.ai";
            ItemConfigs = new List<ItemConfig>();
            ItemMappings = new List<ItemMapping>();
        }
        public int CountItemConfig(ItemType type)
        {
            int c = 0;
            for (int i = 0; i < ItemConfigs.Count; ++i)
                if (ItemConfigs[i].ItemType == type)
                    c++;
            return c;
        }
        public DesignConfig Copy()
        {
            DesignConfig copy = new DesignConfig();
            copy.Label = Label;
            copy.FilePath = FilePath;
            copy.ItemConfigs = ItemConfigs.Select(o => o.Copy()).ToList();
            return copy;
        }
    }
    public class BaseArt
    {
        private static int _GlobalId = 0;
        public int Id { get; }
        public string FilePath { get; set; }
        public DesignConfig DesignConfig { get; set; }
        public List<string> Values { get; set; }
        public double[] Boundary { get; set; }
        public BaseArt()
        {
            Id = _GlobalId;
            ++_GlobalId;
            DesignConfig = new DesignConfig();
            Values = new List<string>();
            Boundary = new double[] { double.NaN, double.NaN, double.NaN, double.NaN };
        }
        public BaseArt(double width, double height)
        {
            Id = _GlobalId;
            ++_GlobalId;
            DesignConfig = new DesignConfig();
            Values = new List<string>();
            Boundary = new double[] { double.NaN, double.NaN, width, height};
        }
    }
    public class Arts
    {
        public DesignConfig DesignConfig { get; set; }
        public Queue<BaseArt> ArtQueue { get; set; }
        public Arts()
        {
            DesignConfig = new DesignConfig();
            ArtQueue = new Queue<BaseArt>();
        }
    }
    public class ItemConfig
    {
        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemType ItemType { get; set; }
        [BsonRef("magics")]
        public List<Magic> Magics { get; set; }
        public ItemConfig()
        {
            Name = "NoName";
            ItemType = ItemType.TextFrame;
            Magics = new List<Magic>();
        }
        public ItemConfig(string Name)
        {
            this.Name = Name;
            Magics = new List<Magic>();
        }
        public ItemConfig Copy()
        {
            ItemConfig copy = new ItemConfig();
            copy.Name = Name;
            copy.ItemType = ItemType;
            copy.Magics = Magics.Select(o => o.Copy()).ToList();
            return copy;
        }
        
    }
    public class ItemMapping
    {
        [BsonId]
        public int Id { get; set; }
        public string Values { get; set; }
        public byte[] Polygons { get; set; }
        public ItemMapping()
        {
        }
        public ItemMapping Copy()
        {
            ItemMapping copy = new ItemMapping();
            copy.Values = Values;
            copy.Polygons = Polygons.ToArray();
            return copy;
        }
    }
    public class MappedPolygon
    {
        public string Value { get; set; }
        public Polygon Polygon { get; set; }
        public MappedPolygon()
        {
        }
        public MappedPolygon(string Value, Polygon Polygon)
        {
            this.Value = Value;
            this.Polygon = Polygon;
        }
        public MappedPolygon(ItemMapping mapping, WKBReader reader)
        {
            Value = mapping.Values;
            Polygon = (Polygon)reader.Read(mapping.Polygons);
        }
    }
    public class PolygonMaps
    {
        List<MappedPolygon> Mappeds;
        public PolygonMaps()
        {
            Mappeds = new List<MappedPolygon>();
        }
        public void Add(MappedPolygon mp)
        {
            Mappeds.Add(mp);
        }
        public Polygon Find(string value)
        {
            for (int i = 0; i < Mappeds.Count; ++i)
                if (!string.IsNullOrEmpty(Mappeds[i].Value) && Mappeds[i].Value.Equals(value))
                    return Mappeds[i].Polygon;

            return null;

        }
        public bool ForceAdd(MappedPolygon mp)
        {
            bool found = false;
            for (int i = 0; i < Mappeds.Count; ++i)
            {
                if (!string.IsNullOrEmpty(Mappeds[i].Value) && Mappeds[i].Value.Equals(mp.Value))
                {
                    Mappeds[i] = mp;
                    found = true;
                    break;
                }
            }
            if (!found)
                Add(mp);
            return true;
        }
        public IEnumerable<Polygon> Find(string[] values)
        {
            Polygon[] plgs = new Polygon[values.Length];
            for (int i = 0; i < Mappeds.Count; ++i)
            {
                for (int j = 0; j < values.Length; ++j)
                    if (Mappeds[i].Value.Equals(values[j]))
                        plgs[j] = Mappeds[i].Polygon;

            }
            return plgs;

        }
    }
    public class Magic
    {
        [BsonId]
        public int Id { get; set; }
        public Spell Spell { get; set; }
        public string Elements { get; set; }
        public Magic()
        {
        }
        public Magic Copy()
        {
            Magic copy = new Magic();
            copy.Spell = Spell;
            copy.Elements = new string(Elements);
            return copy;
        }

    }
    public enum Spell
    {
        None,
        ChangeText,
        OneLineText,
        ShowAllText,
        CreateOutlineText,
        StickTextTogether,
        FitInside,
        Rotate,
        MoveToTouch,
        StickPathTo,
        ClippingMask,
        Ungroup,
        MirrorSnowFlake,
        MoneyHolder1
    }
    public enum ItemType
    {
        TextFrame,
        PathItem,
        CompoundPathItem,
        GroupItem,
        PlacedItem,
        All
    }
    public class Messege
    {
        public string messege { get; set; }
        public MessegeInfo info { get; set; }
        public string[] status { get; set; }
        public Messege(string messege, MessegeInfo info, string[] status)
        {
            this.messege = messege;
            this.info = info;
            this.status = status;
        }
    }
    public enum MessegeInfo
    {
        None,
        Notification,
        Warning,
        Error,
        Exception,
        Complete,
        Unhandled
    }
    public class Order
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Label { get; set; }
        public string OrderNumber { get; set; }
        public string Data { get; set; }
        public Order()
        {

        }
        public Order(string[] paras)
        {
            if (paras.Length == 5)
            {
                Id = int.Parse(paras[0]);
                Count = int.Parse(paras[1]);
                Label = paras[2];
                OrderNumber = paras[3];
                Data = paras[4];
            }
            else if (paras.Length == 4)
            {
                Count = int.Parse(paras[0]);
                Label = paras[1];
                OrderNumber = paras[2];
                Data = paras[3];
            }
            else if (paras.Length == 3)
            {
                Count = int.Parse(paras[0]);
                Label = paras[1];
                OrderNumber = paras[2];
            }
            else if (paras.Length == 2)
            {
                Count = int.Parse(paras[0]);
                Label = paras[1];
            }
        }
        public string AsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Id);
            sb.Append("\t");
            sb.Append(Count);
            sb.Append("\t");
            sb.Append(Label);
            sb.Append("\t");
            sb.Append(OrderNumber);
            sb.Append("\t");
            sb.Append(Data);
            sb.Append("\t");
            return sb.ToString();
        }
    }
    public static class Constants
    {
        public const string dataFileName = "\\data.csv";
        public const string artFolderName = "\\arts\\";
        public const string printAndCutFolderName = "\\printAndCut\\";
        public const string storageFolderName = "\\storage\\";
        public const string resourceFolder = "\\resources\\";
        public const char REGEX_DATA_FILE_COLUMN = '\t';
        public const char REGEX_DATA_FILE_DATA = ';';
        public static readonly List<string> REF_LIST = new List<string>()
        {
            "REF_LEFT","REF_BOT","REF_RIGHT","REF_TOP","LINE0", "LINE1","LINE2","LINE3","LINE4","LINE5","LINE6","LINE7","LINE8","LINE9","LINE10","LINE11","LINE12","LINE13","LINE14","MERGE1","MERGE2"
        };
        public static readonly int COUNT_REF = REF_LIST.Count + 3;
        public const int REF_LIST_START_REF = 0;
        public const int REF_LIST_START_LINE = 4;
        public const int REF_LIST_END_LINE = 18;
        public const int REF_LIST_START_MERGE = 19;
        public const int REF_LIST_END_MERGE = 20;
    }
    public class GProfile
    {
        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
        public string GSheetId { get; set; }
        public string GSheetRange { get; set; }
        public string GDriveId { get; set; }
        public GProfile()
        {

        }
    }
    public enum Flag
    {
        IsAiRunning,
        SignalFlag

    }
}
