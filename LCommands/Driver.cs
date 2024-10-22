using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using LObjects;
using IllustratorManipulationLib;
using Illustrator;
using System.Text.RegularExpressions;
using System.IO;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using System.IO.MemoryMappedFiles;
//using System.Windows.Controls;

namespace AiDesignTool.LCommands
{
    public static class MainDriver
    {
        //#region DataBase Action
        //static DBConnector dBConnector;
        //public static int AddProfile(Profile profile)
        //{
        //    return dBConnector.AddProfile(profile);
        //}
        //public static bool DeleteProfile(Profile profile)
        //{
        //    return dBConnector.DeleteProfile(profile);
        //}
        //public static Profile GetProfile(int id)
        //{
        //    return dBConnector.GetProfile(id);
        //}
        //public static IEnumerable<Profile> GetAllProfiles()
        //{
        //    return dBConnector.GetAllProfiles();
        //}
        //public static bool UpdateProfile(Profile profile)
        //{
        //    return dBConnector.UpdateProfile(profile);
        //}
        //public static int AddDesignConfig(DesignConfig designconfig)
        //{
        //    return dBConnector.AddDesignConfig(designconfig);
        //}
        //public static bool DeleteDesignConfig(DesignConfig designconfig)
        //{
        //    return dBConnector.DeleteDesignConfig(designconfig);
        //}
        //public static DesignConfig GetDesignConfig(int id)
        //{
        //    return dBConnector.GetDesignConfig(id);
        //}
        //public static IEnumerable<DesignConfig> GetAllDesignConfigs()
        //{
        //    return dBConnector.GetAllDesignConfigs();
        //}
        //public static bool UpdateDesignConfig(DesignConfig designconfig)
        //{
        //    return dBConnector.UpdateDesignConfig(designconfig);
        //}
        //public static int AddItemConfig(ItemConfig itemconfig)
        //{
        //    return dBConnector.AddItemConfig(itemconfig);
        //}
        //public static bool DeleteItemConfig(ItemConfig itemconfig)
        //{
        //    return dBConnector.DeleteItemConfig(itemconfig);
        //}
        //public static ItemConfig GetItemConfig(int id)
        //{
        //    return dBConnector.GetItemConfig(id);
        //}
        //public static IEnumerable<ItemConfig> GetAllItemConfigs()
        //{
        //    return dBConnector.GetAllItemConfigs();
        //}
        //public static bool UpdateItemConfig(ItemConfig itemconfig)
        //{
        //    return dBConnector.UpdateItemConfig(itemconfig);
        //}
        //public static int AddMagic(Magic magic)
        //{
        //    return dBConnector.AddMagic(magic);
        //}
        //public static bool DeleteMagic(Magic magic)
        //{
        //    return dBConnector.DeleteMagic(magic);
        //}
        //public static Magic GetMagic(int id)
        //{
        //    return dBConnector.GetMagic(id);
        //}
        //public static IEnumerable<Magic> GetAllMagics()
        //{
        //    return dBConnector.GetAllMagics();
        //}
        //public static bool UpdateMagic(Magic magic)
        //{
        //    return dBConnector.UpdateMagic(magic);
        //}
        //#endregion
        //static Profile workingProfile;
        //static MainDriver()
        //{
        //    dBConnector = DBConnector.GetInstance();
        //    //appRef = new Application();
        //    //saveOption = new IllustratorSaveOptions();
        //    //cutSaveOption = new IllustratorSaveOptions()
        //    //{
        //    //    Compatibility = AiCompatibility.aiIllustrator8
        //    //};
        //    //noColor = new NoColor();
        //    //captureOptions = new ImageCaptureOptions()
        //    //{
        //    //    AntiAliasing = true,
        //    //    Resolution = 300,
        //    //    Transparency = true
        //    //};
        //}
        //static void SetProfile(Profile profile)
        //{
        //    workingProfile = profile;
        //}
        #region Ai Action
        static Wizard wizard;
        static Application appRef;
        static IllustratorSaveOptions saveOption;
        static IllustratorSaveOptions cutSaveOption;
        static NoColor noColor;
        static RGBColor cutColor;
        static ImageCaptureOptions captureOptions;

        //static List<Arts> sArts;
        static List<BaseArt> allArts;
        static List<Order> allOrders;
        //static Dictionary<int, DesignConfig> designConfigs;
        static List<Arts> siArts;
        static ArtConfig artConfig;
        static List<LPanel> allPanels;
        static LPanel TemplatePanel;
        static List<PolygonMaps> allMappedPolygons;


        static Action<Messege> AddMessege;
        static Action<bool, int> ProgressMessege;

        public static ManualResetEvent ControlSignal;

        static string WorkingFolder;
        static MainDriver()
        {
            wizard = new Wizard();
            //List<Arts> arts = new List<Arts>();
            allArts = new List<BaseArt>();
            allOrders = new List<Order>();
            allPanels = new List<LPanel>();

        }
        public static void ClearSession()
        {
        }
        public static void SetDesignConfigs(List<DesignConfig> dcfs)
        {
            int c = dcfs.Count;
            siArts = new List<Arts>();
            allMappedPolygons = new List<PolygonMaps>();
            for (int i = 0; i < c; ++i)
            {
                //designConfigs.Add(i, dcfs[i]);
                //sArts[i].DesignConfig = dcfs[i];
                Arts arts = new Arts();
                arts.DesignConfig = dcfs[i];
                siArts.Add(arts);
                allMappedPolygons.Add(LoadMapping(dcfs[i].ItemMappings));
            }
        }
        public static void SetSignaling(ManualResetEvent mre)
        {
            ControlSignal = mre;
        }
        public static void SetTemplatePanel(LPanel lPanel)
        {
            TemplatePanel = lPanel.CopyConfig();
        }
        public static void SetMessegeTunel(Action<Messege> action)
        {
            AddMessege = action;
        }
        public static void SetProgressMessege(Action<bool, int> action)
        {
            ProgressMessege = action;
        }
        public static void SetWorkingFolder(string workingFolder)
        {
            WorkingFolder = workingFolder;
        }
        public static void SetCutColor(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            // Đảm bảo mã hex có độ dài hợp lệ (6 ký tự)
            if (hex.Length != 6)
            {
                return;
            }

            // Chuyển đổi từng cặp ký tự của hex thành giá trị số
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            cutColor = new RGBColor();
            cutColor.Red = r;
            cutColor.Green = g;
            cutColor.Blue = b;
        }
        public static bool StartDriver()
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
            return true;
        }
        #endregion
        public static IEnumerable<Order> LoadOrder()
        {
            List<Order> ordes = new List<Order>();
            string[] lines = File.ReadAllLines(WorkingFolder + Constants.dataFileName);
            
            AddMessege(new Messege("There are " + lines.Length + " lines in Data File", MessegeInfo.Notification, null));
            ProgressMessege(false, lines.Length);
            for(int i = 0; i < lines.Length; ++i)
            {
                string[] paras = lines[i].Split(Constants.REGEX_DATA_FILE_COLUMN);
                if (paras.Length != 4)
                {
                    AddMessege(new Messege("Wrong format data at line :" + i, MessegeInfo.Error, null));
                    return null;
                } else
                {
                    ordes.Add(new Order(paras));
                    ProgressMessege(true, i +1);
                }
            }
            allOrders = ordes;
            return ordes; 
        }
        private static bool LoadArt(Order order)
        {
            int key;
            Arts arts = siArts.FirstOrDefault(c => c.DesignConfig.Label == order.Label);
            DesignConfig dc = arts.DesignConfig;
            if (dc.Label != order.Label)
                return false;
            BaseArt art = new BaseArt();
            art.DesignConfig = dc;
            art.Values.Add(order.OrderNumber);
            foreach (string ss in order.Data.Split(Constants.REGEX_DATA_FILE_DATA))
                art.Values.Add(ss);
            if (art.Values.Count != dc.CountItemConfig(ItemType.TextFrame))
            {
                return false;
            }
            arts.ArtQueue.Enqueue(art);
            return true;
        }
        public static bool LoadArts()
        {
            ProgressMessege(false, allOrders.Count -1);
            for(int i = 0; i < allOrders.Count; ++i)
            {
                if(!LoadArt(allOrders[i]))
                {
                    AddMessege(new Messege("Data and Config are mismatch :" + allOrders[i].AsString(), MessegeInfo.Error, null));
                    return false;
                }
                ProgressMessege(true, i);
            }
            return true;
        }
        private static IEnumerable<dynamic> merge(DesignConfig dc, Document docRef)
        {
            int c = dc.ItemConfigs.Count, count = 0;
            dynamic[] objs = new dynamic[c];
            for (int j = 1; j <= docRef.PageItems.Count && count < c; j++)
            {
                dynamic item = docRef.PageItems[j];
                string name = item.Name;
                if(name == string.Empty)
                    continue;
                for (int i = 0; i < dc.ItemConfigs.Count; i++)
                {
                    if(name.Equals(dc.ItemConfigs[i].Name))
                    {
                        ++count;
                        objs[i] = item;
                        break;
                    }
                }
            }
            if (count != dc.ItemConfigs.Count)
                return null;
            return objs;
        }
        public static bool CreateArts()
        {
            AddMessege(new Messege("Start creating all arts.", MessegeInfo.Notification, null));

            ProgressMessege(false, siArts.Select(o => o.ArtQueue.Count).Sum() -1);
            int p = 0;
            for (int iii = 0; iii < siArts.Count; ++iii)
            {
                Arts ats = siArts[iii];
                DesignConfig dc = ats.DesignConfig;
                PolygonMaps plgMaps = allMappedPolygons[iii];
                if (ats.ArtQueue.Count == 0)
                    continue;
                AddMessege(new Messege("Start Create Art : " + dc.Label, MessegeInfo.Notification, null));
                

                List<dynamic> mergeObjs = null;
                Document docRef = null;

                docRef = appRef.Open(dc.FilePath);
                mergeObjs = new List<dynamic>(merge(dc, docRef));

                while (ats.ArtQueue.Count > 0)
                {
                    BaseArt art = ats.ArtQueue.Dequeue();
                    dynamic[] copyI = new dynamic[mergeObjs.Count];
                    for (int i = 0; i < mergeObjs.Count; ++i)
                    {
                        copyI[i] = mergeObjs[i].Duplicate();
                        mergeObjs[i].Hidden = true;
                    }
                    for (int i = 0; i < copyI.Length; ++i)
                    {
                        dynamic item = copyI[i];
                        item.Hidden = false;
                        ItemConfig ic = dc.ItemConfigs[i];
                        for (int j = 0; j < ic.Magics.Count; ++j)
                        {
                            item = DoMagic(item, art.Values[i], plgMaps, ic.Magics[j]);
                        }
                    }
                    string savePath = WorkingFolder + Constants.artFolderName + "\\" + art.Id.ToString("D8") + "_" + dc.Label + ".ai";
                    art.FilePath = savePath;
                    docRef.SaveAs(savePath, saveOption);

                    for (int i = 0; i < copyI.Length; ++i)
                    {
                        copyI[i].Delete();
                    }

                    allArts.Add(art);
                    p++;
                    ProgressMessege(true, p);

                }
                docRef.Close(AiSaveOptions.aiDoNotSaveChanges);
            }
            return true;
        }
        public static bool CreatePrintAndCut()
        {
            AddMessege(new Messege("Start Create print and cut files.", MessegeInfo.Notification, null));
            ProgressMessege(false, allArts.Count -1);
            allArts.Sort(delegate (BaseArt f1, BaseArt f2)
            {
                return f1.Id.CompareTo(f2.Id);
            });

            int artCount = allArts.Count,
            label = 0, i = 0;

            int maxCount = TemplatePanel.CountMaxArt();

            while (i < artCount)
            {
                DocumentPreset preset = new DocumentPreset();
                preset.DocumentColorSpace = AiDocumentColorSpace.aiDocumentRGBColor;
                preset.DocumentUnits = AiRulerUnits.aiUnitsPoints;
                Document docRef = appRef.Documents.AddDocument("preset", preset);

                LPanel panel = TemplatePanel.CopyConfig();
                int j = 0;
                List<GroupItem> groups = new List<GroupItem>();
                while (j < maxCount && i < artCount)
                {
                    panel.ContainArts.Add(allArts[i]);
                    GroupItem pItem = docRef.GroupItems.CreateFromFile(allArts[i].FilePath);
                    groups.Add(pItem);
                    double[] pos = panel.GetPosition(j);
                    pItem.Position = new object[] { pos[0], pos[1] };
                    ++j; ++i;
                    ProgressMessege(true, j);
                }
                List<BaseArt> errors = CheckPlacedItem(groups, panel.ContainArts);
                if(errors.Count > 0)
                {
                    string[] errorss = errors.Select(e => string.Join(",", e.Values)).ToArray();
                    AddMessege(new Messege("Try to fix Errors Art: ", MessegeInfo.Unhandled, errors));
                    ControlSignal.WaitOne();
                }

                double[] ll = panel.GetRectId(), bb = panel.GetRectBoundary();
                TextFrame sign = docRef.TextFrames.Add();
                sign.Contents = panel.PrintId.ToString();
                GroupItem signP = sign.CreateOutline();
                signP.Width = ll[2]; signP.Height = ll[3];
                signP.Position = new object[] { ll[0], ll[1] };
                PathItem border = docRef.PathItems.Rectangle(bb[0], bb[1], bb[2], bb[3]);
                border.Position = new object[] { bb[0], bb[1] };

                border.Filled = false;
                border.StrokeColor = cutColor;


                docRef.SaveAs(WorkingFolder + Constants.storageFolderName + "File_All_" + panel.PrintId.ToString() + ".ai", saveOption);

                appRef.ExecuteMenuCommand("deselectall");
                border.Selected = true;
                appRef.ExecuteMenuCommand("Find Stroke Color menu item");
                border.Selected = false;

                for (int ij = 1; ij <= docRef.PathItems.Count; ++ij)
                {
                    try
                    {
                        if (docRef.PathItems[ij].Selected)
                        {
                            docRef.PathItems[ij].StrokeColor = noColor;
                        }
                    }
                    catch { continue; }
                }

                appRef.ExecuteMenuCommand("Fit Artboard to artwork bounds");

                docRef.ImageCapture(WorkingFolder + Constants.printAndCutFolderName + panel.PrintId.ToString() + "_In.PNG", null, captureOptions);

                border.Selected = true;
                signP.Selected = true;

                docRef.Copy();
                docRef.PageItems.RemoveAll();
                docRef.Paste();

                for (int ij = 1; ij <= docRef.PathItems.Count; ++ij)
                {
                    try
                    {
                        docRef.PathItems[ij].StrokeColor = noColor;
                        docRef.PathItems[ij].FillColor = noColor;

                    }
                    catch { continue; }
                }
                allPanels.Add(panel);
                docRef.SaveAs(WorkingFolder + Constants.printAndCutFolderName + "File_Cut_" + label.ToString() + ".ai", cutSaveOption);
                docRef.Close();

                allArts.RemoveRange(0, i);

            }
            return true;
        }
        public static bool Verify()
            { return true; }
        public static List<BaseArt> CheckPlacedItem(List<GroupItem> groupItems, List<BaseArt> arts)
        {
            List<BaseArt> errors = new List<BaseArt>();
            for (int i = 0; i < arts.Count; ++i)
            {
                StringBuilder griBuild = new StringBuilder();
                StringBuilder artBuild = new StringBuilder();
                wizard.getAllText(groupItems[i], ref griBuild);
                foreach (string s in arts[i].Values)
                    artBuild.Append(s);

                string griS = griBuild.ToString(), artS = artBuild.ToString();
                Regex.Replace(griS, @"\s+", "");
                Regex.Replace(artS, @"\s+", "");

                if (griS.Length != artS.Length)
                {
                    errors.Add(arts[i]);
                    continue;
                }
                char[] griChars = griS.ToCharArray(), artChars = artS.ToCharArray();
                Array.Sort(griChars); Array.Sort(artChars);

                for (int j = 0; j < griChars.Length; ++j)
                {
                    if (griChars[j] != artChars[j])
                    {
                        errors.Add(arts[i]);
                        break;
                    }
                }

            }
            return errors;
        }
        public static dynamic DoMagic(dynamic input, string values , PolygonMaps maps, Magic magic)
        {
            string elements = magic.Elements;
            switch(magic.Spell)
            {
                case Spell.None:
                    {
                        break;
                    }
                case Spell.ShowAllText:
                    {
                        if(input is TextFrame tf)
                        {
                            TextRange tr = tf.TextRange;
                            int l = values.Length;
                            if (values == string.Empty)
                                break;
                            double k = tr.CharacterAttributes.Size;
                            while (wizard.CountTextAppear(tr) < l)
                            {
                                k = k - 0.5;
                                tr.CharacterAttributes.Size = k;
                            }
                        }
                        break;
                    }
                case Spell.OneLineText:
                    {
                        if (input is TextFrame tf)
                        {
                            TextRange tr = tf.TextRange;
                            int l = values.Length;
                            if (values == string.Empty)
                                break;
                            double k = tr.CharacterAttributes.Size;
                            while (tf.Lines.Count > 1)
                            {
                                k = k - 0.5;
                                tr.CharacterAttributes.Size = k;
                            }
                        }
                        break;
                    }
                case Spell.ChangeText:
                    {
                        if (input is TextFrame tf)
                        {
                            tf.Contents = values;
                        }
                        break;
                    }
                case Spell.StickTextTogether:
                    {
                        if (input is TextFrame tf)
                        {
                            double _move = double.Parse(elements);
                            double[] pos =
                                {
                                    tf.Position[0] + tf.Width/2,
                                    tf.Position[1] - tf.Height/2
                                };
                            string s = null, tfname = tf.Name;
                            while (s is null)
                                s = tf.Contents;

                            GroupItem gi = tf.CreateOutline();

                            int c = s.Length;
                            double scl;
                            List<CompoundPathItem> cpis = new List<CompoundPathItem>();
                            List<Polygon> plgs = new List<Polygon>();
                            for (int i = c; i > 0; --i)
                            {
                                CompoundPathItem item = gi.CompoundPathItems[i];
                                cpis.Add(item);
                                Polygon tmpplg = maps.Find(s[c - i].ToString());
                                scl = item.Width / tmpplg.EnvelopeInternal.Width;
                                AffineTransformation scaler = AffineTransformation.ScaleInstance(scl, scl);
                                plgs.Add(wizard.moveTo((Polygon)scaler.Transform(tmpplg), item.Position[0], item.Position[1], PositionReference.TopLeft));
                            }
                            AffineTransformation mover = AffineTransformation.TranslationInstance(-_move, 0);
                            for (int i = 0; i < c - 1; ++i)
                            {
                                double delta = 0;
                                while (!plgs[i].Intersects(plgs[i + 1]))
                                {
                                    plgs[i + 1] = (Polygon)mover.Transform(plgs[i + 1]);
                                    delta -= _move;
                                }
                                object[] ps =
                                {
                                    cpis[i +1].Position[0] + delta, cpis[i +1].Position[1]
                                };
                                cpis[i + 1].Position = ps;
                            }
                            tf.Application.ExecuteMenuCommand("deselectall");
                            object[] pos2 =
                            {
                                pos[0] - gi.Width/2,
                                pos[1] + gi.Height/2
                            };
                            gi.Position = pos2;
                            gi.Selected = true;
                            gi.Name = tfname;
                            Thread.Sleep(100);
                            tf.Application.ExecuteMenuCommand("Live Pathfinder Add");
                            tf.Application.ExecuteMenuCommand("expandStyle");
                            Polygon groupedPlg = (Polygon)wizard.moveTo((Polygon)CascadedPolygonUnion.Union(plgs.Cast<Geometry>().ToArray()), pos[0], pos[1], PositionReference.MiddleCenter);

                            maps.ForceAdd(new MappedPolygon(tfname, groupedPlg));

                            for (int i = 0; i < tf.Application.Selection.Length; ++i)
                            {
                                dynamic a = tf.Application.Selection[i];
                                if (a is GroupItem groupItem)
                                {
                                    return groupItem;
                                }
                            }
                            return null;
                        }
                        break;
                    }
                case Spell.FitPathInside:
                    {
                        break;
                    }
                case Spell.StickPathTo:
                    {
                        break;
                    }
            }
            return input;
        }
        public static List<ItemMapping> MakeMapping()
        {
            Document docRef = appRef.ActiveDocument;
            List<ItemMapping> items = new List<ItemMapping>();

            for (int i = 1; i <= docRef.PathItems.Count; ++i)
            {
                PathItem pathItem = docRef.PathItems[i];
                ItemMapping im = new ItemMapping();
                im.Values = pathItem.Name;
                im.Polygons = wizard.Localize(pathItem);
                items.Add(im);
            }
            for (int i = 1; i <= docRef.CompoundPathItems.Count; ++i)
            {
                CompoundPathItem pathItem = docRef.CompoundPathItems[i];
                ItemMapping im = new ItemMapping();
                im.Values = pathItem.Name;
                im.Polygons = wizard.Localize(pathItem);
                items.Add(im);
            }
            for (int i = 1; i <= docRef.TextFrames.Count; ++i)
            {
                TextFrame tf = docRef.TextFrames[i];
                string s = tf.Contents;
                GroupItem gi = tf.CreateOutline();

                int c = s.Length;
                for (int j = c; j > 0; --j)
                {
                    CompoundPathItem pathItem = gi.CompoundPathItems[j];
                    ItemMapping im = new ItemMapping();
                    im.Values = s[c - j].ToString();
                    im.Polygons = wizard.Localize(pathItem);
                    items.Add(im);
                }
            }
            return items;
        }
        public static PolygonMaps LoadMapping(List<ItemMapping> mappings)
        {
            PolygonMaps mppeds = new PolygonMaps();
            WKBReader wkbReader = new WKBReader();
            for (int i = 0; i < mappings.Count; ++i)
            {
                mppeds.Add(new MappedPolygon(mappings[i], wkbReader));
            }
            return mppeds;
        }
    }
    public class DBConnector
    {
        public static readonly LiteDatabase db;
        static DBConnector() {
            db = new LiteDatabase(@"Data.db");
        }
        #region Profile Action
        public static int AddProfile(Profile profile)
        {
            AddDesignConfig(profile.DesignConfigs);
            AddPanel(profile.Panel);
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Insert(profile);
        }
        public static int AddProfile(IEnumerable<Profile> profiles)
        {
            AddDesignConfig(profiles.SelectMany(i => i.DesignConfigs));
            AddPanel(profiles.Select(i => i.Panel));
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Insert(profiles);
        }
        public static Profile GetProfile(int id)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.
                Include(i => i.Panel).
                Include(i => i.Panel.ArtConfig).
                Include(i => i.DesignConfigs).
                Include(i => i.DesignConfigs.Select(m => m.ItemConfigs)).
                Include(i => i.DesignConfigs.Select(m => m.ItemConfigs.SelectMany(n => n.Magics))).
                Include(i => i.DesignConfigs.Select(m => m.ItemMappings)).
                FindById(id);
        }
        public static IEnumerable<Profile> GetProfile(IEnumerable<int> ids)
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.
                Include(i => i.Panel).
                Include(i => i.Panel.ArtConfig).
                Include(i => i.DesignConfigs).
                Include(i => i.DesignConfigs.Select(m => m.ItemConfigs)).
                Include(i => i.DesignConfigs.Select(m => m.ItemConfigs.Select(n => n.Magics))).
                Include(i => i.DesignConfigs.Select(m => m.ItemMappings)).
                Find(i => ids.Contains(i.Id));
        }
        public static IEnumerable<Profile> GetAllProfiles()
        {
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.
                Include(i => i.Panel).
                Include(i => i.Panel.ArtConfig).
                Include(i => i.DesignConfigs).
                Include(i => i.DesignConfigs.Select(m => m.ItemConfigs)).
                Include(i => i.DesignConfigs.Select(m => m.ItemConfigs.Select(n => n.Magics))).
                Include(i => i.DesignConfigs.Select(m => m.ItemMappings)).
                FindAll();
        }
        public static bool DeleteProfile(Profile profile)
        {
            DeleteDesignConfig(profile.DesignConfigs);
            DeletePanel(profile.Panel);
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Delete(profile.Id);
        }
        public static int DeleteProfile(IEnumerable<Profile> profiles)
        {
            DeleteDesignConfig(profiles.SelectMany(i => i.DesignConfigs));
            DeletePanel(profiles.Select(i => i.Panel));
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.DeleteMany(item => profiles.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdateProfile(Profile profile)
        {
            UpdateDesignConfig(profile.DesignConfigs);
            UpdatePanel(profile.Panel);
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Update(profile);
        }
        public static int UpdateProfile(IEnumerable<Profile> profiles)
        {
            UpdateDesignConfig(profiles.SelectMany(i => i.DesignConfigs));
            UpdatePanel(profiles.Select(i => i.Panel));
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Update(profiles);
        }
        #endregion
        #region Panel Action
        public static int AddPanel(LPanel panel)
        {
            AddArtConfig(panel.ArtConfig);
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Insert(panel);
        }
        public static int AddPanel(IEnumerable<LPanel> panels)
        {
            AddArtConfig(panels.Select(i => i.ArtConfig));
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Insert(panels);
        }
        public static LPanel GetPanel(int id)
        {
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Include(i => i.ArtConfig).FindById(id);
        }
        public static IEnumerable<LPanel> GetPanel(IEnumerable<int> ids)
        {
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Include(i => i.ArtConfig).Find(i => ids.Contains(i.Id));
        }
        public static IEnumerable<LPanel> GetAllPanels()
        {
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Include(i => i.ArtConfig).FindAll();
        }
        public static bool DeletePanel(LPanel panel)
        {
            DeleteArtConfig(panel.ArtConfig);
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Delete(panel.Id);
        }
        public static int DeletePanel(IEnumerable<LPanel> panel)
        {
            DeleteArtConfig(panel.Select(i => i.ArtConfig));
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.DeleteMany(item => panel.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdatePanel(LPanel panel)
        {
            UpdateArtConfig(panel.ArtConfig);
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Update(panel);
        }
        public static int UpdatePanel(IEnumerable<LPanel> panels)
        {
            UpdateArtConfig(panels.Select(i => i.ArtConfig));
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Update(panels);
        }
        #endregion
        #region DesignConfig Action
        public static int AddDesignConfig(DesignConfig designconfigs)
        {
            AddItemMapping(designconfigs.ItemMappings);
            AddItemConfig(designconfigs.ItemConfigs);
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Insert(designconfigs);
        }
        public static int AddDesignConfig(IEnumerable<DesignConfig> designconfigs)
        {
            AddItemMapping(designconfigs.SelectMany(i => i.ItemMappings));
            AddItemConfig(designconfigs.SelectMany(i => i.ItemConfigs));
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Insert(designconfigs);
        }
        public static DesignConfig GetDesignConfig(int id)
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.
                Include(i => i.ItemConfigs).
                Include(i => i.ItemConfigs.SelectMany(m => m.Magics)).
                Include(i => i.ItemMappings).
                FindById(id);
        }
        public static IEnumerable<DesignConfig> GetDesignConfig(IEnumerable<int> ids)
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.
                Include(i => i.ItemConfigs).
                Include(i => i.ItemConfigs.SelectMany(m => m.Magics)).
                Include(i => i.ItemMappings).
                Find(i => ids.Contains(i.Id));
        }
        public static IEnumerable<DesignConfig> GetAllDesignConfigs()
        {
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.
                Include(i => i.ItemConfigs).
                Include(i => i.ItemMappings).
                FindAll();
        }
        public static bool DeleteDesignConfig(DesignConfig designconfig)
        {
            DeleteItemMapping(designconfig.ItemMappings);
            DeleteItemConfig(designconfig.ItemConfigs);
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Delete(designconfig.Id);
        }
        public static int DeleteDesignConfig(IEnumerable<DesignConfig> designconfigs)
        {
            DeleteItemMapping(designconfigs.SelectMany(i => i.ItemMappings));
            DeleteItemConfig(designconfigs.SelectMany(i => i.ItemConfigs));
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.DeleteMany(item => designconfigs.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdateDesignConfig(DesignConfig designconfig)
        {
            UpdateItemMapping(designconfig.ItemMappings);
            UpdateItemConfig(designconfig.ItemConfigs);
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Update(designconfig);
        }
        public static int UpdateDesignConfig(IEnumerable<DesignConfig> designconfigs)
        {
            UpdateItemMapping(designconfigs.SelectMany(i => i.ItemMappings));
            UpdateItemConfig(designconfigs.SelectMany(i => i.ItemConfigs));
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Update(designconfigs);
        }
        #endregion
        #region ArtConfig Action OK
        public static int AddArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Insert(artconfig);
        }
        public static int AddArtConfig(IEnumerable<ArtConfig> artconfigs)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Insert(artconfigs);
        }
        public static ArtConfig GetArtConfig(int id)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.FindById(id);
        }
        public static IEnumerable<ArtConfig> GetArtConfig(IEnumerable<int> ids)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Find(m => ids.Contains(m.Id));
        }
        public static IEnumerable<ArtConfig> GetAllArtConfigs()
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.FindAll();
        }
        public static bool DeleteArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Delete(artconfig.Id);
        }
        public static int DeleteArtConfig(IEnumerable<ArtConfig> artconfigs)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.DeleteMany(item => artconfigs.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdateArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Update(artconfig);
        }
        public static int UpdateArtConfig(IEnumerable<ArtConfig> artconfigs)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Update(artconfigs);
        }
        #endregion
        #region ItemConfig Action OK
        public static int AddItemConfig(ItemConfig itemconfig)
        {
            AddMagic(itemconfig.Magics);
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Insert(itemconfig);
        }
        public static int AddItemConfig(IEnumerable<ItemConfig> itemconfigs)
        {
            AddMagic(itemconfigs.SelectMany(i => i.Magics));
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Insert(itemconfigs);
        }
        public static ItemConfig GetItemConfig(int id)
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Include(i => i.Magics).FindById(id);
        }
        public static IEnumerable<ItemConfig> GetItemConfig(IEnumerable<int> ids)
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Include(i => i.Magics).Find(i => ids.Contains(i.Id));
        }
        public static IEnumerable<ItemConfig> GetAllItemConfigs()
        {
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Include(i => i.Magics).FindAll();
        }
        public static bool DeleteItemConfig(ItemConfig itemconfig)
        {
            DeleteMagic(itemconfig.Magics);
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Delete(itemconfig.Id);
        }
        public static int DeleteItemConfig(IEnumerable<ItemConfig> itemconfigs)
        {
            DeleteMagic(itemconfigs.SelectMany(i => i.Magics));
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.DeleteMany(item => itemconfigs.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdateItemConfig(ItemConfig itemconfig)
        {
            UpdateMagic(itemconfig.Magics);
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Update(itemconfig);
        }
        public static int UpdateItemConfig(IEnumerable<ItemConfig> itemconfigs)
        {
            UpdateMagic(itemconfigs.SelectMany(i => i.Magics));
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Update(itemconfigs);
        }
        #endregion
        #region ItemMapping Action OK
        public static int AddItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Insert(itemmapping);
        }
        public static int AddItemMapping(IEnumerable<ItemMapping> itemmappings)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Insert(itemmappings);
        }
        public static ItemMapping GetItemMapping(int id)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.FindById(id);
        }
        public static IEnumerable<ItemMapping> GetItemMapping(IEnumerable<int> ids)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Find(m => ids.Contains(m.Id));
        }
        public static IEnumerable<ItemMapping> GetAllItemMappings()
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.FindAll();
        }
        public static bool DeleteItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Delete(itemmapping.Id);
        }
        public static int DeleteItemMapping(IEnumerable<ItemMapping> itemmappings)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.DeleteMany(item => itemmappings.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdateItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Update(itemmapping);
        }
        public static int UpdateItemMapping(IEnumerable<ItemMapping> itemmappings)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Update(itemmappings);
        }
        #endregion
        
        #region Magic Action OK
        public static int AddMagic(Magic magic)
        {
            ILiteCollection<Magic> magics = db.GetCollection<Magic>("magics");
            return magics.Insert(magic);
        }
        public static int AddMagic(IEnumerable<Magic> magic)
        {
            ILiteCollection<Magic> magics = db.GetCollection<Magic>("magics");
            return magics.Insert(magic);
        }
        public static Magic GetMagic(int id)
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.FindById(id);
        }
        public static IEnumerable<Magic> GetMagic(IEnumerable<int> ids)
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.Find(o => ids.Contains(o.Id));
        }
        public static IEnumerable<Magic> GetAllMagics()
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.FindAll();
        }
        public static bool DeleteMagic(Magic magic)
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.Delete(magic.Id);
        }
        public static int DeleteMagic(IEnumerable<Magic> magics)
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.DeleteMany(item => magics.Select(lp => lp.Id).Contains(item.Id));
        }
        public static bool UpdateMagic(Magic magic)
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.Update(magic);
        }
        public static int UpdateMagic(IEnumerable<Magic> magics)
        {
            ILiteCollection<Magic> collection = db.GetCollection<Magic>("magics");
            return collection.Update(magics);
        }
        #endregion
    }
    public enum DataInteraction
    {
        Current,
        Recursive
    }
}
