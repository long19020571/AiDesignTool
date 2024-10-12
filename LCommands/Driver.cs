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
        public static bool DeleteProfile(int id)
        {
            return dBConnector.DeleteProfile(id);
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
        #region Ai Action
        static Wizard wizard;
        static Application appRef;
        static IllustratorSaveOptions saveOption;
        static IllustratorSaveOptions cutSaveOption;
        static NoColor noColor;
        static ImageCaptureOptions captureOptions;
        static MainDriver()
        {
            appRef = new Application();
            saveOption = new IllustratorSaveOptions();
            cutSaveOption = new IllustratorSaveOptions()
            {
                Compatibility = AiCompatibility.aiIllustrator8
            };
            noColor = new NoColor();
            captureOptions = new ImageCaptureOptions()
            {
                AntiAliasing = true,
                Resolution = 300,
                Transparency = true
            };
        }
        #endregion
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
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Insert(profile);
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
        public bool DeleteProfile(int id)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Delete(id);
        }
        public bool UpdateProfile(Profile profile)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Update(profile);
        }
        #endregion
    }
}
