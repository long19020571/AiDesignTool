using Illustrator;
using IllustratorManipulationLib;
using LiteDB;
using LObjects;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;
using SkiaSharp;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
//using System.Windows.Controls;

namespace AiDesignTool.LCommands
{
    public static class MainDriver
    {
        #region Ai Action
        static Wizard wizard;
        static Illustrator.Application appRef;
        static IllustratorSaveOptions saveOption;
        static IllustratorSaveOptions cutSaveOption;
        static NoColor noColor;
        static RGBColor cutColor;
        static ImageCaptureOptions captureOptions;
        static DocumentPreset preset;

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
        static Action<bool> WavingFlags;

        public static ManualResetEvent ControlSignal;

        static string WorkingFolder;
        static MainDriver()
        {
            wizard = new Wizard();
            allArts = new List<BaseArt>();
            allOrders = new List<Order>();
            allPanels = new List<LPanel>();
        }
        public static void ClearSession()
        {
            allArts.Clear();
            allOrders.Clear();
            siArts.ForEach(o => o.ArtQueue.Clear());
            artConfig = null;
            allPanels.Clear();
            allMappedPolygons.Clear();
            AddMessege = null;
            ProgressMessege = null;
            ControlSignal = null;
            WorkingFolder = null;
            WavingFlags = null;
        }
        public static void SetDesignConfigs(List<DesignConfig> dcfs)
        {
            int c = dcfs.Count;
            siArts = new List<Arts>();
            allMappedPolygons = new List<PolygonMaps>();
            for (int i = 0; i < c; ++i)
            {
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
        public static void SetWavingFlags(Action<bool> action)
        {
            WavingFlags = action;
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
            appRef = new Illustrator.Application();
            appRef.UserInteractionLevel = AiUserInteractionLevel.aiDontDisplayAlerts;
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
            preset = new DocumentPreset();
            preset.DocumentColorSpace = AiDocumentColorSpace.aiDocumentRGBColor;
            preset.DocumentUnits = AiRulerUnits.aiUnitsPoints;
            return true;
        }
        public static bool EndDriver()
        {
            Marshal.FinalReleaseComObject(appRef);
            Marshal.FinalReleaseComObject(saveOption);
            Marshal.FinalReleaseComObject(cutSaveOption);
            Marshal.FinalReleaseComObject(noColor);
            Marshal.FinalReleaseComObject(cutColor);
            Marshal.FinalReleaseComObject(captureOptions);
            Marshal.FinalReleaseComObject(preset);

            ClearSession();
            return true;
        }
        #endregion
        public static IEnumerable<Order> LoadOrder()
        {
            List<Order> ordes = new List<Order>();
            string[] lines = File.ReadAllLines(WorkingFolder + Constants.dataFileName);

            AddMessege(new Messege("There are " + lines.Length + " lines in Data File", MessegeInfo.Notification, null));
            ProgressMessege(false, lines.Length);
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] paras = lines[i].Split(Constants.REGEX_DATA_FILE_COLUMN);
                if (paras.Length != 4)
                {
                    AddMessege(new Messege("Wrong format data at line :" + i, MessegeInfo.Error, null));
                    return null;
                }
                else
                {
                    ordes.Add(new Order(paras));
                }
                
                ProgressMessege(true, i + 1);
            }
            allOrders = ordes;
            return ordes;
        }
        private static bool LoadArt(Order order)
        {
            Arts arts = siArts.FirstOrDefault(c => c.DesignConfig.Label == order.Label);
            if (arts == null)
                return false;
            DesignConfig dc = arts.DesignConfig;
            if (dc.Label != order.Label)
                return false;
            for (int i = 0; i < order.Count; ++i)
            {
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
            }
            return true;
        }
        public static bool LoadArts()
        {
            ProgressMessege(false, allOrders.Count);
            for (int i = 0; i < allOrders.Count; ++i)
            {
                if (!LoadArt(allOrders[i]))
                {
                    AddMessege(new Messege("Data and Config are mismatch : " + allOrders[i].AsString(), MessegeInfo.Error, null));
                    return false;
                }
                ProgressMessege(true, i + 1);
            }
            AddMessege(new Messege("Number of arts : " + allOrders.Count, MessegeInfo.Notification, null));
            return true;
        }
        private static IEnumerable<dynamic> merge(DesignConfig dc, Document docRef)
        {
            int c = dc.ItemConfigs.Count, count = 0;
            dynamic[] objs = new dynamic[c];
            dynamic item = null;
            for (int j = 1; j <= docRef.PageItems.Count && count < c; j++)
            {
                item = docRef.PageItems[j];
                string name = item.Name;
                if (name == string.Empty)
                    continue;
                for (int i = 0; i < dc.ItemConfigs.Count; i++)
                {
                    if (name.Equals(dc.ItemConfigs[i].Name))
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

            ProgressMessege(false, siArts.Select(o => o.ArtQueue.Count).Sum());
            int p = 0;
            for (int iii = 0; iii < siArts.Count; ++iii)
            {
                Arts ats = siArts[iii];
                DesignConfig dc = ats.DesignConfig;
                PolygonMaps plgMaps = allMappedPolygons[iii];
                if (ats.ArtQueue.Count == 0)
                    continue;
                AddMessege(new Messege("Start Create Art : " + dc.Label, MessegeInfo.Notification, null));


                Document docRef = null;
                docRef = appRef.Open(dc.FilePath);
                List<dynamic> mergeObjs = new List<dynamic>(merge(dc, docRef));
                //List<string> mergeNames = mergeObjs.Select(o => (string)o.Name).ToList();
                List<string> mergeNames = new List<string>();
                for (int i = 0; i < mergeObjs.Count; ++i)
                    mergeNames.Add((string)mergeObjs[i].Name);

                dynamic[] refI = new dynamic[Constants.COUNT_REF];
                dynamic[] originI = new dynamic[mergeObjs.Count];
                for (int i = 1; i <= docRef.PageItems.Count; ++i)
                {
                    dynamic tmp = docRef.PageItems[i];
                    string tmpName;
                    try
                    {
                        tmpName = tmp.Name;
                    } catch
                    {
                        continue;
                    }
                    int index1 = Constants.REF_LIST.IndexOf(tmpName),
                        index2 = mergeNames.IndexOf(tmpName);
                    if (index1 > -1)
                    {
                        tmp.Hidden = true;
                        refI[index1] = tmp;
                    }
                    if (index2 > -1)
                    {
                        originI[index2] = tmp;
                        tmp.Hidden = true;
                    }
                    tmp = null;
                }
                while (ats.ArtQueue.Count > 0)
                {
                    BaseArt art = ats.ArtQueue.ElementAt(0);
                    dynamic[] copyI = new dynamic[mergeObjs.Count];
                    for (int i = 0; i < mergeObjs.Count; ++i)
                    {
                        copyI[i] = originI[i].Duplicate();
                        copyI[i].Hidden = false;
                    }
                    for (int i = 0; i < copyI.Length; ++i)
                    {
                        ItemConfig ic = dc.ItemConfigs[i];
                        for (int j = 0; j < ic.Magics.Count; ++j)
                        {
                            copyI[i] = DoMagic(docRef , copyI[i], art.Values[i], plgMaps, ic.Magics[j], refI);
                        }
                        //Marshal.ReleaseComObject(item);
                        //item = null;
                    }
                    string savePath = WorkingFolder + Constants.artFolderName + "\\" + art.Id.ToString("D8") + "_" + dc.Label + ".ai";
                    art.FilePath = savePath;
                    docRef.SaveAs(savePath, saveOption);


                    for (int i = 0; i < copyI.Length; ++i)
                    {
                        try
                        {
                            copyI[i].Delete();
                        } catch { }
                    }

                    ats.ArtQueue.Dequeue();
                    allArts.Add(art);
                    p++;
                    ProgressMessege(true, p);

                }
                docRef.Close(AiSaveOptions.aiDoNotSaveChanges);

                Marshal.CleanupUnusedObjectsInCurrentContext();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                AddMessege(new Messege("Finish Create Art : " + dc.Label, MessegeInfo.Notification, null));
            }
            return true;
        }
        public static bool CreatePrintAndCut(string sessionFolder, bool AutoExportPC)
        {
            string storage = WorkingFolder + Constants.storageFolderName + sessionFolder + "\\";
            if (!Directory.Exists(storage))
                Directory.CreateDirectory(storage);

            AddMessege(new Messege("Start Create print and cut files.", MessegeInfo.Notification, null));
            ProgressMessege(false, allArts.Count);
            allArts.Sort(delegate (BaseArt f1, BaseArt f2)
            {
                return f1.Id.CompareTo(f2.Id);
            });

            int label = 0;

            int maxCount = TemplatePanel.CountMaxArt();

            while (true)
            {
                int artCount = allArts.Count;
                if (artCount <= 0)
                    break;
                int i = 0;
                Document docRef = appRef.Documents.AddDocument("preset", preset);

                LPanel panel = TemplatePanel.CopyConfig();
                List<GroupItem> groups = new List<GroupItem>();
                while (i < maxCount && i < artCount)
                {
                    panel.ContainArts.Add(allArts[i]);
                    GroupItem pItem = docRef.GroupItems.CreateFromFile(allArts[i].FilePath);
                    groups.Add(pItem);
                    double[] pos = panel.GetPosition(i);
                    pItem.Position = new object[] { pos[0], -pos[1] };
                    ++i;
                    ProgressMessege(true, i);
                }
                List<BaseArt> errors = CheckPlacedItem(groups, panel.ContainArts);
                if (errors.Count > 0)
                {
                    string[] errorss = errors.Select(e => string.Join(",", e.Values)).ToArray();
                    AddMessege(new Messege("Try to fix Errors Art: ", MessegeInfo.Unhandled, errors));
                    WavingFlags(true);
                    ControlSignal.WaitOne();
                }

                double[] ll = panel.GetRectId(), bb = panel.GetRectBoundary();
                TextFrame sign = docRef.TextFrames.Add();
                sign.Contents = label.ToString();
                GroupItem signP = sign.CreateOutline();
                signP.Width = ll[2]; signP.Height = ll[3];
                signP.Position = new object[] { ll[0], ll[1] };
                PathItem border = docRef.PathItems.Rectangle(bb[0], bb[1], bb[2], bb[3]);
                border.Position = new object[] { bb[0], bb[1] };

                border.Filled = false;
                border.StrokeColor = cutColor;


                docRef.SaveAs(storage + "File_All_" + label.ToString() + ".ai", saveOption);

                //
                allPanels.Add(panel);
                allArts.RemoveRange(0, i);

                AddMessege(new Messege("Created panel : " + label, MessegeInfo.Notification, null));
                label++;
                if (!AutoExportPC)
                    continue;
                //
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

                docRef.ImageCapture(storage + label.ToString() + "_In.PNG", null, captureOptions);

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
                docRef.SaveAs(storage + "File_Cut_" + label.ToString() + ".ai", cutSaveOption);
                docRef.Close();

                Marshal.CleanupUnusedObjectsInCurrentContext();
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
        public static dynamic DoMagic(Document docRef, dynamic input, string values, PolygonMaps maps, Magic magic, dynamic[] refItem)
        {
            string elements = magic.Elements;
            switch (magic.Spell)
            {
                case Spell.None:
                    {
                        break;
                    }
                case Spell.ShowAllText:
                    {
                        if (input is TextFrame tf)
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
                case Spell.CreateOutlineText:
                    {
                        if (input is TextFrame tf)
                        {
                            input = tf.CreateOutline();
                        }
                        break;
                    }
                case Spell.Rotate:
                    {
                        break;
                    }
                case Spell.MoveToTouch:
                    {
                        break;
                    }
                case Spell.StickTextTogether:
                    {
                        if (input is TextFrame tf)
                        {
                            double _move = double.Parse(elements);
                            string s = null, tfname = tf.Name;
                            while (s is null)
                                s = tf.Contents;

                            GroupItem gi = tf.CreateOutline();

                            double[] pos =
                                {
                                    gi.Position[0] + gi.Width/2,
                                    gi.Position[1] - gi.Height/2
                                };
                            int c = s.Length;
                            double sclw, sclh, minArea = 10;

                            CompoundPathItem[] cpis = new CompoundPathItem[c];
                            Polygon[] plgs = new Polygon[c];
                            double[] distances = new double[c];
                            Array.Fill(distances, 0.0);
                            for (int i = 0, t = c; i < c; ++i)
                            {
                                if (s[i] == ' ')
                                    continue;
                                CompoundPathItem item = gi.CompoundPathItems[t];
                                cpis[i] = item;
                                Polygon tmpplg = maps.Find(s[i].ToString());
                                sclw = item.Width / tmpplg.EnvelopeInternal.Width;
                                sclh = item.Height / tmpplg.EnvelopeInternal.Height;
                                AffineTransformation scaler = AffineTransformation.ScaleInstance(sclw, sclh);
                                plgs[i] = (wizard.moveTo((Polygon)scaler.Transform(tmpplg), item.Position[0], item.Position[1], PositionReference.TopLeft));
                                --t;
                            }
                            AffineTransformation mover = AffineTransformation.TranslationInstance(-_move, 0);
                            for (int i = 0; i < c - 1; ++i)
                            {
                                if (plgs[i] == null || plgs[i + 1] == null)
                                    continue;
                                plgs[i] = (Polygon)plgs[i];
                                plgs[i + 1] = (Polygon)plgs[i + 1];
                                while (plgs[i].Intersection(plgs[i + 1]).Area < minArea)
                                {
                                    for (int u = i + 1; u < c; ++u)
                                    {
                                        plgs[u] = (Polygon)mover.Transform(plgs[u]);
                                        distances[u] -= _move;
                                    }
                                }
                            }
                            for (int i = 0; i < c; ++i)
                            {
                                if (distances[i] == 0)
                                    continue;
                                object[] ps =
                                {
                                    cpis[i].Position[0] + distances[i], cpis[i ].Position[1]
                                };
                                cpis[i].Position = ps;
                            }
                            docRef.Application.ExecuteMenuCommand("deselectall");
                            object[] pos2 =
                            {
                                pos[0] - gi.Width/2,
                                pos[1] + gi.Height/2
                            };
                            gi.Position = pos2;
                            gi.Selected = true;
                            gi.Name = tfname;
                            Thread.Sleep(100);
                            docRef.Application.ExecuteMenuCommand("Live Pathfinder Add");
                            docRef.Application.ExecuteMenuCommand("expandStyle");
                            Polygon groupedPlg = (Polygon)wizard.moveTo((Polygon)CascadedPolygonUnion.Union(plgs.Cast<Geometry>().ToArray()), pos[0], pos[1], PositionReference.MiddleCenter);

                            maps.ForceAdd(new MappedPolygon(tfname, groupedPlg));


                            for (int i = 0; i < tf.Application.Selection.Length; ++i)
                            {
                                if (tf.Application.Selection[i] is object item)
                                {
                                    return item;
                                }
                            }
                        }
                        break;
                    }
                case Spell.FitPathInsize:
                    {

                        break;
                    }
                case Spell.StickPathTo:
                    {
                        string[] es = elements.Split(Constants.REGEX_DATA_FILE_DATA);
                        double _move = double.Parse(es[0]), minWidth = double.Parse(es[1]);
                        double scl00 = 0.25, scl01 = 0.25, scl10 = 1.05, scl11 = 1.05;
                        double sK1 = 0.25, sK2 = 1.025;
                        if (es[2].Equals("00"))
                        {
                            scl00 = sK1; scl01 = sK1; scl10 = sK2; scl11 = sK2;
                        }
                        else if (es[2].Equals("01"))
                        {
                            scl00 = sK1; scl01 = 1; scl10 = sK2; scl11 = 1;
                        }
                        else if (es[2].Equals("10"))
                        {
                            scl00 = 1; scl01 = sK1; scl10 = 1; scl11 = sK2;
                        }
                        Polygon lRef, bRef, rRef, plgItem;
                        string itemName = input.Name;
                        lRef = maps.Find(Constants.REF_LIST[0]);
                        bRef = maps.Find(Constants.REF_LIST[1]);
                        rRef = maps.Find(Constants.REF_LIST[2]);
                        plgItem = maps.Find(itemName);

                        lRef = wizard.moveTo(lRef, refItem[0].Position[0], refItem[0].Position[1], PositionReference.TopLeft);
                        bRef = wizard.moveTo(bRef, refItem[1].Position[0], refItem[1].Position[1], PositionReference.TopLeft);
                        rRef = wizard.moveTo(rRef, refItem[2].Position[0], refItem[2].Position[1], PositionReference.TopLeft);
                        plgItem = wizard.moveTo(plgItem, input.Position[0], input.Position[1], PositionReference.TopLeft);

                        double dLeft, dRight, dBottom, itemWidth = input.Width;
                        double scl = 1, minArea = 10;
                        if (itemWidth > minWidth)
                        {
                            AffineTransformation scaler1 = AffineTransformation.ScaleInstance(scl00, scl01);
                            double[] oldP1 = wizard.getPosition(plgItem, PositionReference.MiddleCenter);
                            plgItem = (Polygon)scaler1.Transform(plgItem);
                            plgItem = wizard.moveTo(plgItem, oldP1[0], oldP1[1], PositionReference.MiddleCenter);

                            AffineTransformation scaler = AffineTransformation.ScaleInstance(scl10, scl11);
                            while (true)
                            {
                                //dLeft = plgItem.Distance(lRef);
                                //dRight = plgItem.Distance(rRef);

                                //if (dLeft == double.NegativeZero && dRight == double.NegativeZero)
                                //{
                                //    break;
                                //}
                                //double[] oldP = wizard.getPosition(plgItem, PositionReference.MiddleCenter);
                                //plgItem = (Polygon)scaler.Transform(plgItem);
                                //plgItem = wizard.moveTo(plgItem, oldP[0], oldP[1], PositionReference.MiddleCenter);
                                //plgItem = wizard.move(plgItem, (dRight - dLeft) / 2, 0);
                                ////
                                dLeft = plgItem.Distance(lRef);
                                dRight = plgItem.Distance(rRef);

                                if (dLeft == double.NegativeZero && dRight == double.NegativeZero)
                                {
                                    //while(true)
                                    //{
                                    //    dLeft = plgItem.Intersection(lRef).Area;
                                    //    dRight = plgItem.Intersection(rRef).Area;
                                    //    if (dLeft > minArea && dRight > minArea)
                                    //    {
                                    //        break;
                                    //    }
                                    //    double[] _oldP = wizard.getPosition(plgItem, PositionReference.MiddleCenter);
                                    //    plgItem = (Polygon)scaler.Transform(plgItem);
                                    //    plgItem = wizard.moveTo(plgItem, _oldP[0], _oldP[1], PositionReference.MiddleCenter);
                                    //    plgItem = wizard.move(plgItem, _move * (dLeft - dRight) / (dLeft > dRight ? dLeft : dRight), 0);

                                    //}
                                    double[] _oldP = wizard.getPosition(plgItem, PositionReference.MiddleCenter);
                                    plgItem = (Polygon)scaler.Transform(plgItem);
                                    plgItem = wizard.moveTo(plgItem, _oldP[0], _oldP[1], PositionReference.MiddleCenter);

                                    break;
                                }
                                double[] oldP = wizard.getPosition(plgItem, PositionReference.MiddleCenter);
                                plgItem = (Polygon)scaler.Transform(plgItem);
                                plgItem = wizard.moveTo(plgItem, oldP[0], oldP[1], PositionReference.MiddleCenter);
                                plgItem = wizard.move(plgItem, (dRight - dLeft) / 2, 0);

                                
                            }
                        }
                        else if (itemWidth <= minWidth)
                        {
                            while (true)
                            {
                                dBottom = plgItem.Distance(bRef);

                                if (dBottom == double.NegativeZero)
                                {
                                    break;
                                }
                                plgItem = wizard.move(plgItem, 0, -_move);
                            }
                        }
                        input.Width = plgItem.EnvelopeInternal.Width;
                        input.Height = plgItem.EnvelopeInternal.Height;
                        double[] pos = wizard.getPosition(plgItem, PositionReference.TopLeft);
                        object[] poso =
                        {
                            pos[0],pos[1]
                        };
                        input.Position = poso;

                        
                        break;
                    }
                case Spell.MirrorSnowFlake:
                    {

                        Polygon lRef, bRef, rRef, tRef, plgItem;
                        string itemName = input.Name;
                        lRef = maps.Find(Constants.REF_LIST[0]);
                        bRef = maps.Find(Constants.REF_LIST[1]);
                        rRef = maps.Find(Constants.REF_LIST[2]);
                        tRef = maps.Find(Constants.REF_LIST[3]);
                        plgItem = maps.Find(itemName);

                        dynamic ilRef = refItem[0], ibRel = refItem[1], irRef = refItem[2].Duplicate(), itRef = refItem[3];

                        lRef = wizard.moveTo(lRef, ilRef.Position[0], ilRef.Position[1], PositionReference.TopLeft);
                        bRef = wizard.moveTo(bRef, ibRel.Position[0], ibRel.Position[1], PositionReference.TopLeft);
                        rRef = wizard.moveTo(rRef, irRef.Position[0], irRef.Position[1], PositionReference.TopLeft);
                        tRef = wizard.moveTo(tRef, itRef.Position[0], itRef.Position[1], PositionReference.TopLeft);
                        plgItem = wizard.moveTo(plgItem, input.Position[0], input.Position[1], PositionReference.TopLeft);

                        double dBottom, dLeft, dTop, dRight, _move = 2;
                        while (true)
                        {
                            dBottom = plgItem.Distance(bRef);

                            if (dBottom == double.NegativeZero)
                            {
                                break;
                            }
                            plgItem = wizard.move(plgItem, 0, -_move);
                        }
                        while (true)
                        {
                            dLeft = plgItem.Distance(lRef);

                            if (dLeft == double.NegativeZero)
                            {
                                break;
                            }
                            plgItem = wizard.move(plgItem, -_move, 0);
                        }
                        double[] pos1 = wizard.getPosition(plgItem, PositionReference.BottomLeft);
                        AffineTransformation scaler = AffineTransformation.ScaleInstance(1.05, 1.05);
                        while (true)
                        {
                            dTop = plgItem.Distance(tRef);

                            if (dTop == double.NegativeZero)
                            {
                                break;
                            }
                            plgItem = (Polygon)scaler.Transform(plgItem);
                            plgItem = wizard.moveTo(plgItem, pos1[0], pos1[1], PositionReference.BottomLeft);
                        }
                        input.Width = plgItem.EnvelopeInternal.Width;
                        input.Height = plgItem.EnvelopeInternal.Height;
                        double[] pos2 = wizard.getPosition(plgItem, PositionReference.TopLeft);
                        object[] poso =
                        {
                            pos2[0],pos2[1]
                        };
                        input.Position = poso;
                        while (true)
                        {
                            dRight = plgItem.Distance(rRef);

                            if (dRight == double.NegativeZero)
                            {
                                break;
                            }
                            rRef = wizard.move(rRef, -_move, 0);
                        }

                        irRef.Width = rRef.EnvelopeInternal.Width;
                        irRef.Height = rRef.EnvelopeInternal.Height;
                        double[] pos22 = wizard.getPosition(rRef, PositionReference.TopLeft);
                        object[] poso2 =
                        {
                            pos22[0],pos22[1]
                        };
                        irRef.Position = poso2;
                        dynamic main = input, main2 = irRef;
                        GroupItem group = appRef.ActiveDocument.GroupItems.Add();
                        main.Move(group, AiElementPlacement.aiPlaceInside);
                        main2.Move(group, AiElementPlacement.aiPlaceInside);
                        for (int i = Constants.REF_LIST_START_LINE; i <= Constants.REF_LIST_END_LINE; i++)
                        {
                            dynamic tmp = refItem[i];
                            if (tmp == null)
                                continue;
                            Illustrator.Matrix km = wizard.getMatrix(refItem[i]);
                            main = main.Duplicate();
                            main.Transform(km, true, true, true, true, 1, AiTransformation.aiTransformDocumentOrigin);
                            main.Move(group, AiElementPlacement.aiPlaceInside);
                            main2 = main2.Duplicate();
                            main2.Transform(km, true, true, true, true, 1, AiTransformation.aiTransformDocumentOrigin);
                            main2.Move(group, AiElementPlacement.aiPlaceInside);
                        }
                        input = group;
                        break;
                    }
                case Spell.MoneyHolder1:
                    {
                        if (input is TextFrame tf)
                        {
                            GroupItem gr = tf.CreateOutline();
                            double iW = gr.Width, iH = gr.Height;
                            double phi = Math.Asin(iH / (3 * iW)) * (180/Math.PI);

                            gr.Rotate(phi, true, false, false, false, AiTransformation.aiTransformCenter);
                            wizard.Align(refItem[0], gr, Alignment.HorizontalLeft);
                            wizard.Align(refItem[3], gr, Alignment.VerticalTop);

                            GroupItem grD = gr.Duplicate();
                            dynamic mer1 = refItem[Constants.REF_LIST_START_MERGE].Duplicate();
                            mer1.Hidden = false;
                            for (int i = 1; i <= gr.CompoundPathItems.Count; ++i)
                            {
                                CompoundPathItem cpi = gr.CompoundPathItems[i];
                                for (int ii = 1; ii <= cpi.PathItems.Count; ++ii)
                                {
                                    PathItem p = cpi.PathItems[ii];

                                            p.StrokeColor = cutColor;
                                            //p.FillColor = noColor;
                                            p.StrokeWidth = 16;
                                            p.StrokeJoin = AiStrokeJoin.aiRoundEndJoin;

                                }
                            }

                            gr.Selected = true;
                            docRef.Application.ExecuteMenuCommand("OffsetPath v22");
                            mer1.Move(gr, AiElementPlacement.aiPlaceAtBeginning);
                            gr.Selected = true;
                            //mer1.Selected = true;
                            //docRef.Application.ExecuteMenuCommand("group");

                            docRef.Application.ExecuteMenuCommand("Live Pathfinder Add");
                            docRef.Application.ExecuteMenuCommand("expandStyle");


                            grD.Selected = true;
                            docRef.Application.ExecuteMenuCommand("ungroup");
                            docRef.Application.ExecuteMenuCommand("group");
                            for (int i = 1; i <= docRef.GroupItems.Count; ++i)
                            {
                                if (docRef.GroupItems[i].Selected)
                                {
                                    input = docRef.GroupItems[i];
                                    break;
                                }
                            }
                        }
                        break;
                    }
            }
            return input;
        }
        public static List<ItemMapping> MakeMapping()
        {
            Document docRef = appRef.ActiveDocument;
            List<ItemMapping> items = new List<ItemMapping>();
            AddMessege(new Messege("Start Make Mapping\nMaking PathItem ...", MessegeInfo.Notification, null));
            int c = docRef.PathItems.Count;
            ProgressMessege(false, c);
            for (int i = 1; i <= c; ++i)
            {
                PathItem pathItem = docRef.PathItems[i];
                ItemMapping im = new ItemMapping();
                im.Values = pathItem.Name;
                im.Polygons = wizard.Localize(pathItem);
                items.Add(im);
                ProgressMessege(true, i);
            }
            c = docRef.CompoundPathItems.Count;
            AddMessege(new Messege("Making CompoundPathItem ...", MessegeInfo.Notification, null));
            ProgressMessege(false, c);
            for (int i = 1; i <= c; ++i)
            {
                CompoundPathItem pathItem = docRef.CompoundPathItems[i];
                ItemMapping im = new ItemMapping();
                im.Values = pathItem.Name;
                im.Polygons = wizard.Localize(pathItem);
                items.Add(im);
                ProgressMessege(true, i);
            }
            AddMessege(new Messege("Making TextFrame ...", MessegeInfo.Notification, null));
            for (int i = 1; i <= docRef.TextFrames.Count; ++i)
            {

                TextFrame tf = docRef.TextFrames[1];
                string s = tf.Contents;
                GroupItem gi = tf.CreateOutline();


                docRef.Application.ExecuteMenuCommand("deselectall");
                gi.Selected = true;
                Thread.Sleep(100);
                docRef.Application.ExecuteMenuCommand("Live Pathfinder Add");
                docRef.Application.ExecuteMenuCommand("expandStyle");

                c = s.Length;
                ProgressMessege(false, c - 1);
                for (int j = c; j > 0; --j)
                {
                    CompoundPathItem pathItem = gi.CompoundPathItems[j];
                    ItemMapping im = new ItemMapping();
                    im.Values = s[c - j].ToString();
                    im.Polygons = wizard.Localize(pathItem);
                    items.Add(im);
                    ProgressMessege(true, c - j);
                }
            }
            return items;
        }
        public static List<ItemMapping> MakeMapping(string fontPath)
        {
            SKTypeface typeface = SKTypeface.FromFile(fontPath);
            SKFont font = new SKFont(typeface);

            List<ItemMapping> maps = new List<ItemMapping>();
            for (char c = (char)0; c < (char)0xFFFF; c++)
            {
                if (typeface.ContainsGlyph(c))
                {
                    SKPath path = font.GetGlyphPath(typeface.GetGlyph(c));
                    if (path != null && !path.IsEmpty)
                    {
                        ItemMapping im = new ItemMapping();
                        im.Polygons = wizard.Localize(path);
                        im.Values = c.ToString();
                        maps.Add(im);
                    }
                }
            }
            return maps;
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
        static DBConnector()
        {
            db = new LiteDatabase(@"Data.db");
        }
        static void Dispose()
        {
            db?.Dispose();
        }
        #region Profile Action
        public static bool AddProfile(Profile profile)
        {
            AddDesignConfig(profile.DesignConfigs);
            AddPanel(profile.Panel);
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Upsert(profile);
        }
        public static int AddProfile(IEnumerable<Profile> profiles)
        {
            AddDesignConfig(profiles.SelectMany(i => i.DesignConfigs));
            AddPanel(profiles.Select(i => i.Panel));
            ILiteCollection<Profile> collection = db.GetCollection<Profile>("profiles");
            return collection.Upsert(profiles);
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
        public static bool AddPanel(LPanel panel)
        {
            AddArtConfig(panel.ArtConfig);
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Upsert(panel);
        }
        public static int AddPanel(IEnumerable<LPanel> panels)
        {
            AddArtConfig(panels.Select(i => i.ArtConfig));
            ILiteCollection<LPanel> collection = db.GetCollection<LPanel>("panels");
            return collection.Upsert(panels);
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
        public static bool AddDesignConfig(DesignConfig designconfigs)
        {
            AddItemMapping(designconfigs.ItemMappings);
            AddItemConfig(designconfigs.ItemConfigs);
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Upsert(designconfigs);
        }
        public static int AddDesignConfig(IEnumerable<DesignConfig> designconfigs)
        {
            AddItemMapping(designconfigs.SelectMany(i => i.ItemMappings));
            AddItemConfig(designconfigs.SelectMany(i => i.ItemConfigs));
            ILiteCollection<DesignConfig> collection = db.GetCollection<DesignConfig>("designconfigs");
            return collection.Upsert(designconfigs);
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
        public static bool AddArtConfig(ArtConfig artconfig)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Upsert(artconfig);
        }
        public static int AddArtConfig(IEnumerable<ArtConfig> artconfigs)
        {
            ILiteCollection<ArtConfig> collection = db.GetCollection<ArtConfig>("artconfigs");
            return collection.Upsert(artconfigs);
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
        public static bool AddItemConfig(ItemConfig itemconfig)
        {
            AddMagic(itemconfig.Magics);
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Upsert(itemconfig);
        }
        public static int AddItemConfig(IEnumerable<ItemConfig> itemconfigs)
        {
            AddMagic(itemconfigs.SelectMany(i => i.Magics));
            ILiteCollection<ItemConfig> collection = db.GetCollection<ItemConfig>("itemconfigs");
            return collection.Upsert(itemconfigs);
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
        public static bool AddItemMapping(ItemMapping itemmapping)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Upsert(itemmapping);
        }
        public static int AddItemMapping(IEnumerable<ItemMapping> itemmappings)
        {
            ILiteCollection<ItemMapping> collection = db.GetCollection<ItemMapping>("itemmappings");
            return collection.Upsert(itemmappings);
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
        public static bool AddMagic(Magic magic)
        {
            ILiteCollection<Magic> magics = db.GetCollection<Magic>("magics");
            return magics.Upsert(magic);
        }
        public static int AddMagic(IEnumerable<Magic> magic)
        {
            ILiteCollection<Magic> magics = db.GetCollection<Magic>("magics");
            return magics.Upsert(magic);
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
}
