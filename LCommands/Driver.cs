using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using LObjects;
using static System.Runtime.InteropServices.JavaScript.JSType;
using IllustratorManipulationLib;
using Illustrator;
using Microsoft.VisualBasic;

namespace AiDesignTool.LCommands
{
    public static class MainDriver
    {
        #region DataBase Action
        static DBConnector dBConnector;
        public static int AddProfile(Profile profile)
        {
            return dBConnector.AddProfile(profile);
        }
        public static bool DeleteProfile(Profile profile)
        {
            return dBConnector.DeleteProfile(profile);
        }
        public static Profile GetProfile(int id)
        {
            return dBConnector.GetProfile(id);
        }
        public static IEnumerable<Profile> GetAllProfiles()
        {
            return dBConnector.GetAllProfiles();
        }
        public static bool UpdateProfile(Profile profile)
        {
            return dBConnector.UpdateProfile(profile);
        }
        #endregion
        static Profile workingProfile;
        static MainDriver()
        {
            dBConnector = DBConnector.GetInstance();
            //appRef = new Application();
            //saveOption = new IllustratorSaveOptions();
            //cutSaveOption = new IllustratorSaveOptions()
            //{
            //    Compatibility = AiCompatibility.aiIllustrator8
            //};
            //noColor = new NoColor();
            //captureOptions = new ImageCaptureOptions()
            //{
            //    AntiAliasing = true,
            //    Resolution = 300,
            //    Transparency = true
            //};
        }
        static void SetProfile(Profile profile)
        {
            workingProfile = profile;
        }
        #region Ai Action
        static Wizard wizard;
        static Application appRef;
        static IllustratorSaveOptions saveOption;
        static IllustratorSaveOptions cutSaveOption;
        static NoColor noColor;
        static ImageCaptureOptions captureOptions;
        #endregion

        static bool LoadConfigs()
        {
            return true;
        }
        static bool LoadData()
        { return true; }
        static bool CreateArts()
        {  return true; }
        static bool CreatePrintAndCut()
            { return true; }
        static bool Verify()
            { return true; }
    }
    public class DBConnector
    {
        private readonly LiteDatabase db = new LiteDatabase(@"Data.db");
        private DBConnector() { }
        private static DBConnector Instance;
        public static DBConnector GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DBConnector();
            }
            return Instance;
        }
        #region Profile Action
        public int AddProfile(Profile profile)
        {
            ILiteCollection<Profile> profiles = db.GetCollection<Profile>("profiles");
            return profiles.Insert(profile);
        }
        public int AddProfile(IEnumerable<Profile> profile)
        {
            ILiteCollection<Profile> profiles = db.GetCollection<Profile>("profiles");
            return profiles.Insert(profile);
        }
        public Profile GetProfile(int id)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.FindById(id);
        }
        public IEnumerable<Profile> GetAllProfiles()
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.FindAll();
        }
        public bool DeleteProfile(Profile profile)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Delete(profile.Id);
        }
        public int DeleteProfile(IEnumerable<Profile> profile)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.DeleteMany(item => profile.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdateProfile(Profile profile)
        {
            ILiteCollection<Profile> profiles = db.GetCollection<Profile>("profiles");
            return profiles.Update(profile);
        }
        public int UpdateProfile(IEnumerable<Profile> profile)
        {
            ILiteCollection<Profile> profiles = db.GetCollection<Profile>("profiles");
            return profiles.Update(profile);
        }
        #endregion
        #region Panel Action
        public int AddPanel(Panel panel)
        {
            ILiteCollection<Panel> panels = db.GetCollection<Panel>("panels");
            return panels.Insert(panel);
        }
        public int AddPanel(IEnumerable<Panel> panel)
        {
            ILiteCollection<Panel> panels = db.GetCollection<Panel>("panels");
            return panels.Insert(panel);
        }
        public Panel GetPanel(int id)
        {
            ILiteCollection<Panel> collection = db.GetCollection<Panel>("panels");
            return collection.FindById(id);
        }
        public IEnumerable<Panel> GetAllPanels()
        {
            ILiteCollection<Panel> collection = db.GetCollection<Panel>("panels");
            return collection.FindAll();
        }
        public bool DeletePanel(Panel panel)
        {
            ILiteCollection<Panel> collection = db.GetCollection<Panel>("panels");
            return collection.Delete(panel.Id);
        }
        public int DeletePanel(IEnumerable<Panel> panel)
        {
            ILiteCollection<Panel> collection = db.GetCollection<Panel>("panels");
            return collection.DeleteMany(item => panel.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdatePanel(Panel panel)
        {
            ILiteCollection<Panel> panels = db.GetCollection<Panel>("panels");
            return panels.Update(panel);
        }
        public int UpdatePanel(IEnumerable<Panel> panel)
        {
            ILiteCollection<Panel> panels = db.GetCollection<Panel>("panels");
            return panels.Update(panel);
        }
        #endregion
        #region DesignConfig Action
        public int AddDesignConfig(DesignConfig designconfig)
        {
            ILiteCollection<DesignConfig> designconfigs = db.GetCollection<DesignConfig>("designconfigs");
            return designconfigs.Insert(designconfig);
        }
        public int AddDesignConfig(IEnumerable<DesignConfig> designconfig)
        {
            ILiteCollection<DesignConfig> designconfigs = db.GetCollection<DesignConfig>("designconfigs");
            return designconfigs.Insert(designconfig);
        }
        public DesignConfig GetDesignConfig(int id)
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.FindById(id);
        }
        public IEnumerable<DesignConfig> GetAllDesignConfigs()
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.FindAll();
        }
        public bool DeleteDesignConfig(DesignConfig designconfig)
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Delete(designconfig.Id);
        }
        public int DeleteDesignConfig(IEnumerable<DesignConfig> designconfig)
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.DeleteMany(item => designconfig.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdateDesignConfig(DesignConfig designconfig)
        {
            ILiteCollection<DesignConfig> designconfigs = db.GetCollection<DesignConfig>("designconfigs");
            return designconfigs.Update(designconfig);
        }
        public int UpdateDesignConfig(IEnumerable<DesignConfig> designconfig)
        {
            ILiteCollection<DesignConfig> designconfigs = db.GetCollection<DesignConfig>("designconfigs");
            return designconfigs.Update(designconfig);
        }
        #endregion
        #region ArtConfig Action
        public int AddArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> artconfigs = db.GetCollection<ArtConfig>("artconfigs");
            return artconfigs.Insert(artconfig);
        }
        public int AddArtConfig(IEnumerable<ArtConfig> artconfig)
        {
            ILiteCollection<ArtConfig> artconfigs = db.GetCollection<ArtConfig>("artconfigs");
            return artconfigs.Insert(artconfig);
        }
        public ArtConfig GetArtConfig(int id)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.FindById(id);
        }
        public IEnumerable<ArtConfig> GetAllArtConfigs()
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.FindAll();
        }
        public bool DeleteArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Delete(artconfig.Id);
        }
        public int DeleteArtConfig(IEnumerable<ArtConfig> artconfig)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.DeleteMany(item => artconfig.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdateArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> artconfigs = db.GetCollection<ArtConfig>("artconfigs");
            return artconfigs.Update(artconfig);
        }
        public int UpdateArtConfig(IEnumerable<ArtConfig> artconfig)
        {
            ILiteCollection<ArtConfig> artconfigs = db.GetCollection<ArtConfig>("artconfigs");
            return artconfigs.Update(artconfig);
        }
        #endregion
        #region ItemConfig Action
        public int AddItemConfig(ItemConfig itemconfig)
        {
            ILiteCollection<ItemConfig> itemconfigs = db.GetCollection<ItemConfig>("itemconfigs");
            return itemconfigs.Insert(itemconfig);
        }
        public int AddItemConfig(IEnumerable<ItemConfig> itemconfig)
        {
            ILiteCollection<ItemConfig> itemconfigs = db.GetCollection<ItemConfig>("itemconfigs");
            return itemconfigs.Insert(itemconfig);
        }
        public ItemConfig GetItemConfig(int id)
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.FindById(id);
        }
        public IEnumerable<ItemConfig> GetAllItemConfigs()
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.FindAll();
        }
        public bool DeleteItemConfig(ItemConfig itemconfig)
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Delete(itemconfig.Id);
        }
        public int DeleteItemConfig(IEnumerable<ItemConfig> itemconfig)
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.DeleteMany(item => itemconfig.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdateItemConfig(ItemConfig itemconfig)
        {
            ILiteCollection<ItemConfig> itemconfigs = db.GetCollection<ItemConfig>("itemconfigs");
            return itemconfigs.Update(itemconfig);
        }
        public int UpdateItemConfig(IEnumerable<ItemConfig> itemconfig)
        {
            ILiteCollection<ItemConfig> itemconfigs = db.GetCollection<ItemConfig>("itemconfigs");
            return itemconfigs.Update(itemconfig);
        }
        #endregion
        #region ItemMapping Action
        public int AddItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> itemmappings = db.GetCollection<ItemMapping>("itemmappings");
            return itemmappings.Insert(itemmapping);
        }
        public int AddItemMapping(IEnumerable<ItemMapping> itemmapping)
        {
            ILiteCollection<ItemMapping> itemmappings = db.GetCollection<ItemMapping>("itemmappings");
            return itemmappings.Insert(itemmapping);
        }
        public ItemMapping GetItemMapping(int id)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.FindById(id);
        }
        public IEnumerable<ItemMapping> GetAllItemMappings()
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.FindAll();
        }
        public bool DeleteItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Delete(itemmapping.Id);
        }
        public int DeleteItemMapping(IEnumerable<ItemMapping> itemmapping)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.DeleteMany(item => itemmapping.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdateItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> itemmappings = db.GetCollection<ItemMapping>("itemmappings");
            return itemmappings.Update(itemmapping);
        }
        public int UpdateItemMapping(IEnumerable<ItemMapping> itemmapping)
        {
            ILiteCollection<ItemMapping> itemmappings = db.GetCollection<ItemMapping>("itemmappings");

            return itemmappings.Update(itemmapping);
        }
        #endregion
        #region LPolygon Action
        public bool AddLPolygon(LPolygon lpolygon)
        {
            ILiteCollection<LPolygon> lpolygons = db.GetCollection<LPolygon>("lpolygons");
            return lpolygons.Upsert(lpolygon);
        }
        public int AddLPolygon(IEnumerable<LPolygon> lpolygon)
        {
            ILiteCollection<LPolygon> lpolygons = db.GetCollection<LPolygon>("lpolygons");
            return lpolygons.Upsert(lpolygon);
        }
        public LPolygon GetLPolygon(int id)
        {
            ILiteCollection<LPolygon> collection = db.GetCollection<LPolygon>("lpolygons");
            return collection.FindById(id);
        }
        public IEnumerable<LPolygon> GetAllLPolygons()
        {
            ILiteCollection<LPolygon> collection = db.GetCollection<LPolygon>("lpolygons");
            return collection.FindAll();
        }
        public bool DeleteLPolygon(LPolygon lpolygon)
        {
            ILiteCollection<LPolygon> collection = db.GetCollection<LPolygon>("lpolygons");
            return collection.Delete(lpolygon.Id);
        }
        public int DeleteLPolygon(IEnumerable<LPolygon> lpolygon)
        {
            ILiteCollection<LPolygon> collection = db.GetCollection<LPolygon>("lpolygons");
            return collection.DeleteMany(item => lpolygon.Select(lp => lp.Id).Contains(item.Id));
        }
        public bool UpdateLPolygon(LPolygon lpolygon)
        {
            ILiteCollection<LPolygon> lpolygons = db.GetCollection<LPolygon>("lpolygons");
            return lpolygons.Update(lpolygon);
        }
        public int UpdateLPolygon(IEnumerable<LPolygon> lpolygon)
        {
            ILiteCollection<LPolygon> lpolygons = db.GetCollection<LPolygon>("lpolygons");
            return lpolygons.Update(lpolygon);
        }
        #endregion
    }
}
