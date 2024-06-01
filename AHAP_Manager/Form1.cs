using BrightIdeasSoftware;
using Octokit;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using Label = System.Windows.Forms.Label;

namespace AHAP_Manager
{
    public partial class Form1 : Form
    {
        string trtExtension = ".trt";
        string tgaExtension = ".tga";


        //bool isAllNONUpdated;
        string live_VersionNumber = "";
        string live_NextObjectID = "";
        int live_NON = -1;
        int live_verNum = -1;

        static Bitmap empty = ResizeImage(new Bitmap(64, 64), new Size(64, 64));
        static Bitmap stopWatch = Properties.Resources.stopwatch;
        static Bitmap hand = Properties.Resources.hand1;

        //Color colBlueDarkSelectioned = Color.RoyalBlue;//Color.FromArgb(80, 117, 191); //75%
        Color colBlueDarkChecked = Color.FromArgb(69, 102, 163); //64%
        Color colBlueDarkPanelHeader = Color.FromArgb(64, 94, 153); //60%
        Color colBlueDarkPanel = Color.FromArgb(72, 105, 171); //67%
        Color colBlueNormal = Color.FromArgb(75, 111, 179); //70%
        Color colBlueLightPanel = Color.FromArgb(78, 114, 186); //73%
        Color colBlueLightPanelHeader = Color.FromArgb(84, 122, 199); //78% 

        #region Export
        /// <summary>
        /// Default = 0
        /// </summary>
        int e_actual_oldNON
        {
            get
            {
                if (e_actualVersion <= 0) return 0;
                if (Settings.Default.list_Old_NextObjNum.Count >= e_actualVersion)
                {
                    if (int.TryParse(Settings.Default.list_Old_NextObjNum[e_actualVersion - 1], out var nb)) return nb;
                    return 0;
                }
                return 0;
            }
        }

        int e_actualVersion = 0;
        int e_actual_nextObjNum = 0;
        int e_actual_nextSprNum = 0;
        int e_actual_nextSoundNum = 0;
        int e_newObj_Count;
        bool e_isLiveVersion
        {
            get { return live_verNum == e_actualVersion; }
        }
        bool e_LoadSucess = false;
        bool fastLoaded = false;
        int e_file_copy_number;
        //int e_file_skip_number;
        string e_oxzFilePath = "";
        string e_oxzFileName = "";
        string e_TransitionPath = "";
        string e_ExportPath = "";
        string e_ObjectPath = "";
        string e_SpritesPath = "";

        List<Transition> e_transitionsList = new List<Transition>();

        List<ObjectSettings> e_listObjSettingsClear; //We remove the "null" value (was the ID missing)

        CustomObjectListView e_ObjListView;
        ViewListObject e_ObjSelListView;
        CustomTransitionsListView e_TransListView;
        #endregion


        Color colGreenDarkSelection = Color.SeaGreen; //Color.FromArgb(121, 189, 113);
        Color colGreenDarkHeader = Color.FromArgb(43, 128, 81);
        Color colGreenDark = Color.FromArgb(52, 153, 97);
        Color colGreenNormal = Color.FromArgb(61, 179, 114);
        Color colGreenLight = Color.FromArgb(69, 204, 130);
        Color colGreenLightHeader = Color.FromArgb(65, 191, 122);

        #region Import
        bool deleteOldFile;
        int i_newTr_num;
        int i_replaceTr_num;
        int i_PassedTr_num;
        string i_TransitionPath = "";
        //string import_ExportPath = "";
        string i_ObjectFolderPath = "";
        string i_SprFolderPath = "";
        string i_TransitionsFolderPath = "";
        string i_Import_AddFolderPath = "";
        string i_Import_ReplaceFolderPath = "";
        List<int> i_listNewIDs = new List<int>();
        List<int> i_listOldIDs = new List<int>();
        List<Transition> i_transList = new List<Transition>();
        List<Transition> i_transList_IdMiss = new List<Transition>();
        List<Transition> i_transList_AlreadyExist = new List<Transition>();
        List<Transition> i_transList_AlreadyExist_Selected = new List<Transition>();

        List<ObjectSettings> i_listObjSettingsClear; //We remove the "null" value (was the ID missing)

        CustomTransitionsListView i_TransListView = new CustomTransitionsListView();
        //CustomTransitionsListView i_TransLV = new CustomTransitionsListView();
        #endregion

        RowBorderDecoration rbd_AlreadyExist;
        RowBorderDecoration rbd_TransitionError; // when transition have any of his IDs > highestID or < lowestID an > 
        RowBorderDecoration rbd_Selected;
        private int e_NbTransition_Error;

        public Form1()
        {
            InitializeComponent();
            //Settings.Default.Reset();
            if (Settings.Default.list_Old_NextObjNum == null)
                Settings.Default.list_Old_NextObjNum = new System.Collections.Specialized.StringCollection();
            if (Settings.Default.list_RepoTagName == null)
                Settings.Default.list_RepoTagName = new System.Collections.Specialized.StringCollection();
            i_checkBox_DeleteOldFiles.Checked = Settings.Default.i_DeleteAllFile;

            Debug.WriteLine("list NON count : " + Settings.Default.list_Old_NextObjNum.Count);
            Debug.WriteLine("list RepoTags count : " + Settings.Default.list_RepoTagName.Count);
            rbd_AlreadyExist = new RowBorderDecoration();
            rbd_AlreadyExist.BorderPen = new Pen(Color.FromArgb(128, Color.Yellow), 2);
            rbd_AlreadyExist.FillBrush = new SolidBrush(Color.FromArgb(48, Color.Yellow));
            rbd_AlreadyExist.BoundsPadding = new Size(1, 1);
            rbd_AlreadyExist.CornerRounding = 6.0f;
            rbd_TransitionError = new RowBorderDecoration();
            //rbd_TransitionError.BorderPen = new Pen(Color.FromArgb(200, Color.DarkRed), 1);
            rbd_TransitionError.FillBrush = new SolidBrush(Color.FromArgb(100, Color.DarkRed));
            rbd_TransitionError.BoundsPadding = new Size(1, 1);
            rbd_TransitionError.CornerRounding = 6.0f;
            rbd_Selected = new RowBorderDecoration();
            rbd_Selected.BorderPen = new Pen(Color.FromArgb(128, Color.Black), 1);
            rbd_Selected.FillBrush = new SolidBrush(Color.FromArgb(50, Color.Black));
            rbd_Selected.BoundsPadding = new Size(1, 1);
            rbd_Selected.CornerRounding = 6.0f;

            e_label_Info_OnBrowserButton.ForeColor = Color.DarkRed;
            e_label_Info_OnExportButtonClick.ForeColor = Color.DarkGreen;
            i_label_Info_OnBrowserButtonClick.ForeColor = Color.DarkRed;
            i_label_Info_OnImportButtonClick.ForeColor = Color.DarkGreen;

            deleteOldFile = i_checkBox_DeleteOldFiles.Checked;

        }
        private void Form1_Load_1(object sender, EventArgs e)
        {



            panel1.BackColor = colBlueNormal;
            panel_exp.BackColor = colBlueDarkPanel;
            panel5.BackColor = colBlueDarkPanel;
            panel2.BackColor = colBlueDarkPanel;
            panel9.BackColor = colBlueDarkPanel;
            panel3.BackColor = colBlueDarkPanelHeader;
            panel6.BackColor = colBlueDarkPanelHeader;
            panel4.BackColor = colBlueDarkPanelHeader;
            panel11.BackColor = colBlueLightPanel;
            panel10.BackColor = colBlueLightPanelHeader;
            e_panel_TransOverview.BackColor = Color.FromArgb(80, colBlueDarkPanel);
            //Import
            panel13.BackColor = colGreenDarkHeader;
            panel14.BackColor = colGreenDarkHeader;
            panel7.BackColor = colGreenDark;
            panel8.BackColor = colGreenDark;

            GetLiveGameInformation();
            e_CreateCustomListsView();
            i_CreateCustomListsView();

            i_TransListView.MouseLeave += I_TransListView_MouseLeave;
            e_TransListView.MouseLeave += E_TransListView_MouseLeave;
        }

        private void E_TransListView_MouseLeave(object? sender, EventArgs e)
        {
            e_panel_TransOverview.Visible = false;
        }

        private void I_TransListView_MouseLeave(object? sender, EventArgs e)
        {
            i_panel_TransOverview.Visible = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
        }

        static async Task<string[]> FindInfoLiveAsync()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string[] info = new string[2];
                string versionNumber_url = "https://raw.githubusercontent.com/jasonrohrer/AnotherPlanetData/master/dataVersionNumber.txt";
                string nextObjectID_url = "https://raw.githubusercontent.com/jasonrohrer/AnotherPlanetData/master/objects/nextObjectNumber.txt";
                string versionNumber = await httpClient.GetStringAsync(versionNumber_url);
                string nextObjectID_Live = await httpClient.GetStringAsync(nextObjectID_url);
                return new string[] { versionNumber, nextObjectID_Live };
            }
            catch
            {
                var confirmIDs = MessageBox.Show("Problem getting infomations through http request, check internet connection and retry. (if you skip => no 'Fast Export' option available)", "HTTP request problem", MessageBoxButtons.RetryCancel);
                if (confirmIDs == DialogResult.Retry)
                {
                    await FindInfoLiveAsync();
                }
                return null;
            }

        }
        private async void GetLiveGameInformation()
        {

            string[] info = await FindInfoLiveAsync();
            if (info == null || info.Length < 2) return;
            live_VersionNumber = info[0];
            live_NextObjectID = info[1];
            // We set text without check if they can be parse ( so we can debug easily)
            labelLiveVersion.Text = live_VersionNumber;
            labelLiveNextObjectID.Text = live_NextObjectID;

            // test if that a number
            bool isVersion_Valid = int.TryParse(info[0], out live_verNum);
            bool isLiveNON_Valid = int.TryParse(info[1], out live_NON);
            if (isLiveNON_Valid) e_checkBox_FastLoad.Enabled = true;
            if (isLiveNON_Valid && isVersion_Valid) button_SearchLiveInfo.Visible = false;

            if (isVersion_Valid) // Dont search old NON if we dont have valid version (int)
                RequestAllNON();
        }
        public async Task<string> getOne_NextObjNum(int version)
        {
            int maxRetry = 6; // To ensure people dont use all request by spaming the button
            try
            {
                string username = "jasonrohrer";
                string repo = "AnotherPlanetData";
                //string versionNumber_name = "dataVersionNumber.txt";
                string nextObjectNumber_name = "objects/nextObjectNumber.txt";

                var github = new GitHubClient(new ProductHeaderValue("SearchOldNextObjectNumber"));
                //If we dont have tags update
                if (Settings.Default.list_RepoTagName.Count < version)
                {
                    var tags = await github.Repository.GetAllTags(username, repo);
                    Settings.Default.list_RepoTagName.Clear();
                    for (int i = tags.Count - 1; i >= 0; i--) { Settings.Default.list_RepoTagName.Add(tags[i].Name); }// the Tags saved need update
                    Settings.Default.Save();
                    Debug.WriteLine("OK version-1 = " + (version - 1) + " / " + Settings.Default.list_RepoTagName[version - 1]);
                    var tmpNON = await github.Repository.Content.GetRawContentByRef(username, repo, nextObjectNumber_name, Settings.Default.list_RepoTagName[version - 1]);
                    return Encoding.Default.GetString(tmpNON);
                }
                else
                {
                    Debug.WriteLine("version-1 = " + (version - 1) + " / " + Settings.Default.list_RepoTagName[version - 1]);
                    var tmpNON = await github.Repository.Content.GetRawContentByRef(username, repo, nextObjectNumber_name, Settings.Default.list_RepoTagName[version - 1]);
                    return Encoding.Default.GetString(tmpNON);
                }
            }
            catch (RateLimitExceededException eLimit)
            {
                var confirmIDs = MessageBox.Show("Git Limit acess (" + eLimit.Limit + " request per hour) \n retry in one hour \n\n" +
                    " (skip = don't show wrong IDs reference)"
                    , "Git request Limit", MessageBoxButtons.OK);
                return "";
            }
            catch
            {
                maxRetry--;
                if (maxRetry > 0)
                {
                    var probDialog = MessageBox.Show("Problem when getting old version (" + version + ") of 'nextObjectNumber.txt' from github \n" +
                        "check internet connection and retry (" + maxRetry + ") \n\n (skip = don't show wrong IDs reference)"
                            , "Git request problem", MessageBoxButtons.RetryCancel);
                    if (probDialog == DialogResult.Retry)
                    {
                        await getOne_NextObjNum(version);
                    }
                }
                else
                {
                    var probDialog = MessageBox.Show("Still Problem when getting old version of 'nextObjectNumber.txt' from github \nNo more retry \n\n (skip = don't show wrong IDs reference)"
                            , "Git request problem", MessageBoxButtons.OK);
                }
                return "";
            }
        }
        private async void RequestAllNON()
        {
            if (Settings.Default.list_Old_NextObjNum.Count == live_verNum)
                return;
            else if (Settings.Default.list_Old_NextObjNum.Count < live_verNum)
            {
                List<string> tmplist = new List<string>();
                //TODO: Maybe update a file with all version on github to have only one request / or find a way to request all in one request
                // We need read in backware to get the last version first ( imagine install this app when the version is over 60 (max 60 req per hour))
                for (int i = live_verNum - 1; i >= Settings.Default.list_Old_NextObjNum.Count; i--)
                {
                    var NON = await getOne_NextObjNum(i + 1); // +1 cause version start at 1
                    if (NON == "") return; // we return and dont ask for each missing version
                    tmplist.Add(NON);
                }
                int nbtoadd = live_verNum - Settings.Default.list_Old_NextObjNum.Count;
                // we add empty value to have acess to the last index (ps: need do this after cause tags is init only if list_Old_NextObjNum.count != live_version)
                for (int n = 0; n < nbtoadd; n++)
                {
                    Settings.Default.list_Old_NextObjNum.Add("");
                }
                //16/15/14/13/12
                int lastIndex = Settings.Default.list_Old_NextObjNum.Count-1;
                foreach (var NON in tmplist)
                {
                    Settings.Default.list_Old_NextObjNum[lastIndex] = NON;
                    lastIndex--;
                }
                Settings.Default.Save();
            }
        }


        #region LivePanel
        private void button_SearchLiveInfo_Click(object sender, EventArgs e)
        {
            GetLiveGameInformation();
        }
        #endregion

        #region Export
        public void e_SetErrorDecoration_OnTransitionListView()
        {
            if (e_actual_oldNON == 0) return;
            #region Put Decorations On the wrong transitions
            for (int i = 0; i < e_transitionsList.Count; i++)
            {
                e_transitionsList[i].SetWillCauseProb(e_actual_oldNON, e_ObjListView);

            }
            e_UpdateAllDecorations_Transition();
            #endregion
        }
        private void GetTransitionsFast(ref List<string> listPath, ref List<string> IDsSelected)
        {
            foreach (var file in Directory.GetFiles(e_TransitionPath))
            {
                //string fileName = Path.GetFileName(trans_list[i]);
                if (HasID(file, IDsSelected))
                {
                    e_file_copy_number++;
                    listPath.Add(file);
                }
            }
        }
        #region Buttons
        private void e_button_Browser_Click(object sender, EventArgs e)
        {

            e_button_Export.Enabled = false; fastLoaded = false; e_LoadSucess = false;
            e_label_Info_OnExportButtonClick.Text = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "oxz files (*.oxz)|*.oxz|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxEditorGameFolderPath_export.Text = openFileDialog.FileName;

                }
                else
                {
                    return;
                }
            }

            FormLoading.ShowSplashScreen(this.Location, this.Size);
            e_ObjListView.ClearObjects();
            e_TransListView.ClearObjects();
            e_ObjSelListView.ClearObjects();
            e_oxzFilePath = textBoxEditorGameFolderPath_export.Text;
            e_oxzFileName = Path.GetFileNameWithoutExtension(e_oxzFilePath);
            string[] oxzSeparateName = e_oxzFileName.Split('_');
            e_newObj_Count = 0;
            if (oxzSeparateName.Length > 1 && !int.TryParse(oxzSeparateName[1], out e_newObj_Count))
            {
                CreateFatalErrorMessage("Oxz file name problem"); return;
            }

            #region Check Folder/Files Acess
            string gameFolderPath = Directory.GetParent(e_oxzFilePath).Parent.FullName;
            string dataVersNumPath = gameFolderPath + "/dataVersionNumber.txt";
            e_TransitionPath = gameFolderPath + "/transitions/";
            e_ExportPath = gameFolderPath + "/exports/";
            e_ObjectPath = gameFolderPath + "/objects/";
            e_SpritesPath = gameFolderPath + "/sprites/";
            string e_SoundsPath = gameFolderPath + "/sounds/";
            string actualNextObjNumPath = e_ObjectPath + "nextObjectNumber.txt";
            string actualNextSprNumPath = e_SpritesPath + "nextSpriteNumber.txt";
            string actualNextSoundNumPath = e_SoundsPath + "nextSoundNumber.txt";

            if (!Directory.Exists(e_TransitionPath))
            {
                CreateFatalErrorMessage("transitions folder do not exist, make sure to select the oxz file in the export folder"); return;
            }
            if (!Directory.Exists(e_ExportPath))
            {
                CreateFatalErrorMessage("exports folder do not exist, make sure to select the oxz file in the export folder"); return;
            }
            if (!Directory.Exists(e_ObjectPath))
            {
                CreateFatalErrorMessage("objects folder do not exist, make sure to select the oxz file in the export folder"); return;
            }
            if (!Directory.Exists(e_SpritesPath))
            {
                CreateFatalErrorMessage("sprites folder do not exist, make sure to select the oxz file in the export folder"); return;
            }
            if (!Directory.Exists(e_SoundsPath))
            {
                CreateFatalErrorMessage("sounds folder do not exist, make sure to select the oxz file in the export folder"); return;
            }
            if (!File.Exists(dataVersNumPath))
            {
                CreateFatalErrorMessage("dataVersionNumber.txt do not exist in this game folder"); return;
            }
            if (!File.Exists(actualNextObjNumPath))
            {
                CreateFatalErrorMessage("nextObjectNumber.txt do not exist in this game folder"); return;
            }
            if (!File.Exists(actualNextSprNumPath))
            {
                CreateFatalErrorMessage("nextSpriteNumber.txt do not exist in this game folder"); return;
            }
            if (!File.Exists(actualNextSoundNumPath))
            {
                CreateFatalErrorMessage("nextSoundNumber.txt do not exist in this game folder"); return;
            }


            if (!int.TryParse(File.ReadAllText(dataVersNumPath), out e_actualVersion))
            {
                CreateFatalErrorMessage("Cannot parse dataVersionNumber.txt"); return;
            }
            if (!int.TryParse(File.ReadAllText(actualNextObjNumPath), out e_actual_nextObjNum))
            {
                CreateFatalErrorMessage("Cannot parse nextObjectNumber.txt"); return;
            }
            if (!int.TryParse(File.ReadAllText(actualNextSprNumPath), out e_actual_nextSprNum))
            {
                CreateFatalErrorMessage("Cannot parse nextSpriteNumber.txt"); return;
            }
            if (!int.TryParse(File.ReadAllText(actualNextSoundNumPath), out e_actual_nextSoundNum))
            {
                CreateFatalErrorMessage("Cannot parse nextSoundNumber.txt"); return;
            }
            #endregion

            if (!e_isLiveVersion) e_checkBox_FastLoad.Checked = false;


            e_listObjSettingsClear = new List<ObjectSettings>();
            ObjectSettings[] e_listObjectsSettings = LoadAllObjects(e_ObjectPath, e_SpritesPath, true);
            if (e_listObjectsSettings == null) return;

            #region Assign objects to ListViews / Clear null value
            for (int x = 0; x < e_listObjectsSettings.Length; x++)
            {

                if (e_listObjectsSettings[x] == null)
                {
                    
                    continue;
                }
                e_listObjSettingsClear.Add(e_listObjectsSettings[x]);
                e_ObjListView.AddObject(e_listObjectsSettings[x]);

            }
            e_ObjListView.listObj = e_listObjSettingsClear;
            e_TransListView.listObj = e_listObjSettingsClear;
            #endregion


            if (e_checkBox_AutoSelect.Checked)
                e_AutoSelectObject();

            if (e_checkBox_FastLoad.Checked) fastLoaded = true;
            e_button_Export.Enabled = true;
            e_label_SelectedDataVersion.Text = e_actualVersion.ToString();
            e_label_OldNextObjNum.Text = e_actual_oldNON.ToString();
            e_label_NextObjNum.Text = e_actual_nextObjNum.ToString();
            e_label_NextSprNum.Text = e_actual_nextSprNum.ToString();
            e_label_NextSoundNum.Text = e_actual_nextSoundNum.ToString();
            e_SetLabel_NbObject(e_listObjSettingsClear.Count);

            e_SetErrorDecoration_OnTransitionListView(); // Need call this after transition list and transition list view are setup and after auto select

            e_LoadSucess = true;
            FormLoading.CloseForm();
        }
        private void e_button_Export_Click(object sender, EventArgs e)
        {
            e_file_copy_number = 0;

            string transitionExportFileName = e_oxzFileName + trtExtension;

            List<string> selectedTraListPath = new List<string>();
            List<string> selectedTraListIDs = new List<string>();
            foreach (ObjectSettings o in e_ObjSelListView.Objects)
            {
                selectedTraListIDs.Add(o.id.ToString());
            }

            if (fastLoaded)
            {
                GetTransitionsFast(ref selectedTraListPath, ref selectedTraListIDs);
            }
            else
            {
                foreach (Transition t in e_TransListView.SelectedObjects)
                {
                    selectedTraListPath.Add(t.filePath);
                }
            }

            CreateTrtFile(selectedTraListPath, Path.Combine(e_ExportPath + transitionExportFileName), selectedTraListIDs);

            e_label_Info_OnExportButtonClick.Text = "Sucess : " + e_file_copy_number + " transitions copy ";

        }
        #endregion

        #region CheckBox
        private void e_checkBox_NoImage_CheckedChanged(object sender, EventArgs e)
        {
            e_Refresh_ObjListView_ImageColumns();
        }
        private void e_checkBox_Overview_CheckedChanged(object sender, EventArgs e)
        {
            if (e_checkBox_Overview.Checked == true)
            {
                panel3.Visible = true;
                panel_exp.Size = new Size(panel_exp.Width, panel_exp.Size.Height - panel3.Size.Height);
                e_ObjListView.Size = new Size(e_ObjListView.Width, e_ObjListView.Size.Height - panel3.Size.Height);
            }
            else
            {
                panel_exp.Size = new Size(panel_exp.Width, panel_exp.Size.Height + panel3.Size.Height);
                e_ObjListView.Size = new Size(e_ObjListView.Width, e_ObjListView.Size.Height + panel3.Size.Height);
                panel3.Visible = false;
            }
        }
        private void e_checkBox_NoImage_Selected_CheckedChanged(object sender, EventArgs e)
        {
            e_ObjSelListView.AllColumns[2].IsVisible = !e_checkBox_NoImage_Selected.Checked;
            e_ObjSelListView.RowHeight = e_checkBox_NoImage_Selected.Checked ? 16 : 32;
            e_ObjSelListView.RebuildColumns();
        }
        private void e_checkBox_FastLoad_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void e_checkBox_AutoSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (e_checkBox_AutoSelect.Checked && e_LoadSucess) e_AutoSelectObject();
        }
        private void e_checkBox_TransOverview_CheckedChanged(object sender, EventArgs e)
        {
            if (!e_checkBox_TransOverview.Checked) e_panel_TransOverview.Visible = false;
        }
        #endregion

        private void e_AutoSelectObject()
        {
            if (e_actual_oldNON == 0) // If we dont have acess to the old NON, we take the last X obj ( witch can not select all good objects ) 
            {
                if (e_ObjListView.Items.Count >= e_newObj_Count)
                {
                    for (int i = 0; i < e_newObj_Count; i++)
                    {
                        e_ObjListView.Items[i].Selected = true;
                    }
                    e_ObjListView.RefreshSelectedList();
                    e_UpdateSelectedListView();
                    if (!e_checkBox_FastLoad.Checked) e_UpdateTransitions();
                }
            }
            else
            {

                for (int i = 0; i <= e_GetIndexOf(e_actual_oldNON); i++) // cause list is reverse
                {
                    e_ObjListView.Items[i].Selected = true;
                }
                e_ObjListView.RefreshSelectedList();
                e_UpdateSelectedListView();
                if (!e_checkBox_FastLoad.Checked) e_UpdateTransitions();
            }
        }
        public int e_GetIndexOf(int ID)
        {

            for (int i = 0; i < e_listObjSettingsClear.Count; i++)
            {
                if (e_listObjSettingsClear[i].id == ID)
                {
                    return i;
                }
            }
            return 0;
        }
        private void e_Refresh_ObjListView_ImageColumns()
        {
            e_ObjListView.AllColumns[2].IsVisible = e_checkBox_NoImage.Checked;
            e_ObjListView.RowHeight = e_checkBox_NoImage.Checked ? 64 : 16;
            e_ObjListView.RebuildColumns();
            e_ObjListView.RefreshSelectedList();
        }
        private void e_UpdateAllDecorations_Transition()
        {
            int tmpCount = 0;
            for (int i = 0; i < e_TransListView.GetItemCount(); i++)
            {
                Transition t = (Transition)e_TransListView.GetModelObject(i);
                if (t.willAnyCauseProb)
                {
                    e_TransListView.GetItem(i).Decoration = rbd_TransitionError;
                    tmpCount++;
                }
            }
            e_NbTransition_Error = tmpCount;
        }
        private string[] GetListPathsOrdered(string objectPath, int startID)
        {

            string nextObNbPath = objectPath + "nextObjectNumber.txt";
            int nextObjectNumberLocal;
            if (!int.TryParse(File.ReadAllText(nextObNbPath), out nextObjectNumberLocal))
            {
                Debug.WriteLine("COULDNT PARSE NEXT OBJ NUM");
                return null;
            }
            List<string> pathToSearch_tmp = new List<string>();
            List<int> ids = new List<int>();
            //var fileList = Directory.GetFiles(objectPath).OrderBy(p => Path.GetFileNameWithoutExtension(p), new ComparerN()).Select(fi => fi).ToArray();
            foreach (string path in Directory.GetFiles(objectPath, "*.txt"))
            {
                if (Path.GetFileNameWithoutExtension(path).Any(x => char.IsLetter(x)))
                    continue;
                int id;
                if (int.TryParse(Path.GetFileNameWithoutExtension(path), out id))
                {
                    ids.Add(id);
                    pathToSearch_tmp.Add(path);
                }
                else
                {
                    Debug.WriteLine("couldnt parse : " + Path.GetFileNameWithoutExtension(path));
                }
            }
            string[] pathToSearch = new string[nextObjectNumberLocal];

            for (int i = 0; i < ids.Count; i++)
            {

                if (ids[i] - 1 < pathToSearch.Length)
                {
                    pathToSearch[ids[i] - 1] = pathToSearch_tmp[i];
                }
                else
                {
                    Debug.WriteLine("SHOULD NOT ids[i]-1 : " + (ids[i] - 1));
                }
            }

            List<string> paths = pathToSearch.Where(c => c != null).ToList();

            if (startID < 1) return paths.ToArray(); // not 0 cause 0 mean we take all the list witch is useless to do
            else
            {
                int index = paths.IndexOf(objectPath + startID + ".txt");
                return paths.GetRange(index, paths.Count - index).ToArray();
            }
        }
        private void CreateFatalErrorMessage(string text, bool isExportTab = true)
        {
            if (isExportTab) e_label_Info_OnBrowserButton.Text = text;
            else i_label_Info_OnBrowserButtonClick.Text = text;
            FormLoading.CloseForm();
        }
        private void CreateTrtFile(List<string> fileList, string newFilePath, List<string> IDList)
        {
            List<string> dataList = new List<string>
            {
                GetStringFromList(IDList, ',')
            };
            foreach (var file in fileList)
            {
                dataList.Add(Path.GetFileName(file));
                foreach (var line in File.ReadLines(file))
                {
                    dataList.Add(line);
                }
                dataList.Add(";;"); // To separate every files (needed when reading)
            }
            File.WriteAllLines(newFilePath, dataList);
        }

        public void e_CreateCustomListsView()
        {
            #region ObjectListView
            //e_ObjListView = new CustomObjectListView();
            e_ObjListView = CreateObjectsListView(panel_exp, 16, colBlueDarkPanel);//, colBlueDarkSelectioned);

            OLVColumn columnID = CreateColumn("ID", "ObjID", 80, false, 0);
            OLVColumn columnName = CreateColumn("Name", "ObjName", 200, false, 1, 100);
            OLVColumn columnImage = CreateColumn("Image", "", 64, false, 2);
            columnImage.IsVisible = false;// cause Image is uncheck

            columnImage.ImageGetter = new ImageGetterDelegate(this.ObjectSettingsImageGetter64);
            columnID.Sortable = true;
            e_ObjListView.AllColumns.Add(columnID);
            e_ObjListView.AllColumns.Add(columnName);
            e_ObjListView.AllColumns.Add(columnImage);
            e_ObjListView.RebuildColumns();

            e_ObjListView.form1 = this;
            e_ObjListView.pictureBox = pictureBoxOverview;
            e_ObjListView.labelName = labelExportOverviewName;
            e_ObjListView.labelSubName = labelExportOverviewSubName;
            e_ObjListView.labelID = labelExportOverviewID;
            e_ObjListView.export = true;

            #endregion

            #region ObjectSelected ListView

            e_ObjSelListView = CreateViewListObject(panel11, 32, colBlueLightPanel, colBlueLightPanelHeader);
            OLVColumn columnID2 = CreateColumn("ID", "ObjID", 80, false, 0);
            OLVColumn columnName2 = CreateColumn("Name", "ObjName", 200, false, 1, 100);
            OLVColumn columnImage2 = CreateColumn("Image", "", 32, false, 2);

            columnImage2.ImageGetter = new ImageGetterDelegate(this.ObjectSettingsImageGetter32);
            columnID2.Sortable = true;
            columnName2.FreeSpaceProportion = 100;
            e_ObjSelListView.AllColumns.Add(columnID2);
            e_ObjSelListView.AllColumns.Add(columnName2);
            e_ObjSelListView.AllColumns.Add(columnImage2);
            e_ObjSelListView.RebuildColumns();
            #endregion

            #region TransitionsListView
            e_TransListView = CreateTransitionListView(panel5, 64, colBlueDarkPanel);//, colBlueDarkSelectioned);

            OLVColumn columnHand1 = CreateColumn("Hand1", "", 64, false, 0);
            OLVColumn columnMore1 = CreateColumn("More1", "", 20, false, 1);
            OLVColumn columnTarget1 = CreateColumn("Target1", "", 64, false, 2);
            OLVColumn columnEgal = CreateColumn("Egal", "", 20, false, 3);
            OLVColumn columnHand2 = CreateColumn("Hand2", "", 64, false, 4);
            OLVColumn columnMore2 = CreateColumn("More2", "", 20, false, 5);
            OLVColumn columnTarget2 = CreateColumn("Target2", "", 64, false, 6);

            columnHand1.ImageGetter = new ImageGetterDelegate(this.TransitionsHand1ImageGetter);
            columnMore1.AspectGetter = new AspectGetterDelegate(this.TransitionsMore1Getter);
            columnTarget1.ImageGetter = new ImageGetterDelegate(this.TransitionsTarget1ImageGetter);
            columnEgal.AspectGetter = new AspectGetterDelegate(this.TransitionsEgalGetter);
            columnHand2.ImageGetter = new ImageGetterDelegate(this.TransitionsHand2ImageGetter);
            columnMore2.AspectGetter = new AspectGetterDelegate(this.TransitionsMore2Getter);
            columnTarget2.ImageGetter = new ImageGetterDelegate(this.TransitionsTarget2ImageGetter);

            columnHand1.FillsFreeSpace = true;
            columnTarget1.FillsFreeSpace = true;
            columnHand2.FillsFreeSpace = true;
            columnTarget2.FillsFreeSpace = true;
            e_TransListView.AllColumns.Add(columnHand1);
            e_TransListView.AllColumns.Add(columnMore1);
            e_TransListView.AllColumns.Add(columnTarget1);
            e_TransListView.AllColumns.Add(columnEgal);
            e_TransListView.AllColumns.Add(columnHand2);
            e_TransListView.AllColumns.Add(columnMore2);
            e_TransListView.AllColumns.Add(columnTarget2);
            e_TransListView.RebuildColumns();

            e_TransListView.checkBox_TransOverview = e_checkBox_TransOverview;
            e_TransListView.labelName = e_label_TrOverviewName;
            e_TransListView.labelSubName = e_label_TrOverviewSubName;
            e_TransListView.labelID = e_label_TrOverviewID;
            e_TransListView.panelOverview = e_panel_TransOverview;
            e_TransListView.form1 = this;
            e_TransListView.export = true;
            #endregion
        }
        public void e_UpdateSelectedListView()
        {
            e_ObjSelListView.ClearObjects();
            e_ObjSelListView.AddObjects(e_ObjListView.SelectedObjects);
            e_SetLabel_NbObjSelected(e_ObjListView.SelectedObjects.Count);
        }
        public void e_SetLabelsTransitions(int count, int count_error = -1) //TODO : do same than i_UpdateLabelsTransitions
        {
            e_labelNbTrExported.Text = count + "/" + e_transitionsList.Count;
            if (count_error == 0)
            {
                e_labelNbTr_Error.ForeColor = Color.Green;
            }
            else if (count_error > 0) e_labelNbTr_Error.ForeColor = Color.Red;
            if (count_error > -1)
            {
                e_labelNbTr_Error.Text = count_error + "";
            }
        }
        public void e_SetLabel_NbObjSelected(int count)
        {
            e_labelNbObjSelected.Text = count + "";// + "/" + e_listObjSettingsClear.Count;
        }
        public void e_SetLabel_NbObject(int count)
        {
            e_labelNbObj.Text = count + "";// + "/" + e_listObjSettingsClear.Count;
        }
        public void e_UpdateTransitions()
        {
            if (fastLoaded) return;

            e_transitionsList.Clear();
            List<string> listIDs = new List<string>();
            foreach (ObjectSettings obj in e_ObjListView.SelectedObjects)
            {
                listIDs.Add(obj.id.ToString());
            }
            e_transitionsList = GetTransitions(listIDs, e_TransitionPath);
            e_TransListView.ClearObjects();
            e_TransListView.AddObjects(e_transitionsList);
            e_SetErrorDecoration_OnTransitionListView();
            e_TransListView.e_SelectAllGoodTransitions();
            e_TransListView.RefreshObjects(e_transitionsList);

            e_SetLabelsTransitions(e_TransListView.SelectedItems.Count, e_NbTransition_Error);
        }

        #endregion

        #region Import
        private bool OpenFile(string filePath, string transitionFolderPath)
        {
            i_listOldIDs.Clear();
            i_transList.Clear();
            i_transList_IdMiss.Clear();
            i_transList_AlreadyExist.Clear();
            i_transList_AlreadyExist_Selected.Clear();
            bool firstOfTheFile = true;
            int count = 0; string tmpTransitionFilePath = "";
            List<string> tmpFileContent = new List<string>();
            Transition tmpTransition = new Transition();

            foreach (var line in File.ReadLines(filePath))
            {
                if (firstOfTheFile)
                {
                    var tmp = line.Split(',');
                    #region Get Old IDs
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        int tmpInt;
                        if (!int.TryParse(tmp[i], out tmpInt))
                        {
                            CreateFatalErrorMessage(".trt File, first line problem", false);
                            return false;
                        }
                        i_listOldIDs.Add(tmpInt);
                    }
                    #endregion
                    firstOfTheFile = false;
                    continue;
                }
                if (count == 0) //Name of the file
                {
                    tmpTransition = new Transition();
                    string lineTmp = line.Remove(line.Length - 4); // remove '.txt'
                    string[] listTmp = lineTmp.Split('_');
                    string firstID = listTmp[0];
                    string secondID = listTmp[1];
                    string rest = "";

                    bool first = false; bool second = false; // to prevent erase an already modify ID
                    for (int i = 0; i < i_listOldIDs.Count; i++)
                    {
                        if (!first && firstID == i_listOldIDs[i].ToString())
                        {
                            firstID = i_listNewIDs[i].ToString(); first = true;
                        }
                        if (!second && secondID == i_listOldIDs[i].ToString())
                        {
                            secondID = i_listNewIDs[i].ToString(); second = true;
                        }
                        if (first && second) break; // break the loop if we already find both
                    }
                    if (listTmp.Length > 2) //put the _LT etc..
                        rest = "_" + listTmp[2];
                    tmpTransitionFilePath = transitionFolderPath + firstID + "_" + secondID + rest + ".txt";
                    tmpTransition.filePath = tmpTransitionFilePath;
                    tmpTransition.SetFileNameIDs(firstID, secondID, i_listObjSettingsClear);
                }
                else if (count == 1) // first line of the file (where there is Third and Fourth ID)
                {

                    string[] listTmp = line.Split(' ');
                    string thirdID = listTmp[0];
                    string fourthID = listTmp[1];
                    int time; int.TryParse(listTmp[2], out time);
                    bool third = false; bool fourth = false; // to prevent erase an already modify ID
                    for (int i = 0; i < i_listOldIDs.Count; i++)
                    {
                        if (!third && thirdID == i_listOldIDs[i].ToString())
                        {
                            thirdID = i_listNewIDs[i].ToString(); third = true;
                        }
                        if (!fourth && fourthID == i_listOldIDs[i].ToString())
                        {
                            fourthID = i_listNewIDs[i].ToString(); fourth = true;
                        }
                        if (third && fourth) break; // break the loop if we already find both
                    }
                    string modifyLine = thirdID + " " + fourthID;
                    for (int a = 2; a < listTmp.Length; a++) // we start at 2 cause third and fourth modify
                    {
                        modifyLine += " " + listTmp[a];
                    }

                    tmpFileContent.Add(modifyLine);
                    tmpTransition.SetFileContentIDs(thirdID, fourthID, i_listObjSettingsClear);
                    tmpTransition.SetTimer(time);
                }
                else // other lines ( description + authors )
                {
                    if (line != ";;") tmpFileContent.Add(line); //if needed to not add the ";;" line into the file
                }

                if (line == ";;") // at the end of a file we create the new file
                {
                    //File.WriteAllLines(tmpTransitionFilePath, tmpLineList);
                    tmpTransition.fileContent = new List<string>(tmpFileContent);
                    i_transList.Add(tmpTransition);
                    count = 0;
                    tmpFileContent.Clear();
                }
                else
                {
                    count++;
                }

            }
            //return i_transitionsList;
            return true;
        }
        public void i_SetErrorDecoration_OnTransitionListView()
        {
            if (e_actual_oldNON == 0) return;
            #region Put Decorations On the wrong transitions
            for (int i = 0; i < e_transitionsList.Count; i++)
            {
                e_transitionsList[i].SetWillCauseProb(e_actual_oldNON, e_ObjListView);

            }
            e_UpdateAllDecorations_Transition();
            #endregion
        }
        private string GetStringFromList(List<string> list, char separator)
        {
            string result = "";
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i];
                if (i != list.Count - 1) result += separator;
            }
            return result;
        }
        private bool HasID(string filePath, List<string> IDs)
        {
            string[] fileName = Path.GetFileNameWithoutExtension(filePath).Split('_');
            if (fileName.Length < 2) return false;
            for (int i = 0; i < IDs.Count; i++)
            {
                if (fileName[0] == IDs[i] || fileName[1] == IDs[i])
                {
                    return true;
                }
                foreach (string s in File.ReadLines(filePath))
                {
                    string[] tmp = s.Split(' ');
                    string tmp1 = tmp[0];
                    string tmp2 = tmp[1];
                    if (IDs.Contains(tmp1) || IDs.Contains(tmp2))
                    {
                        return true;
                    }
                    break; // Only read first line
                }
            }
            return false;
        }
        #region Buttons
        private void i_button_Browser_Click(object sender, EventArgs e)
        {
            i_button_Import.Enabled = false;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "trt files (*.trt)|*.trt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxGameFolderPath.Text = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            FormLoading.ShowSplashScreen(this.Location, this.Size);

            string trtFilePath = textBoxGameFolderPath.Text;

            if (trtFilePath == "")
            {
                CreateFatalErrorMessage("Empty Field", false); return;
            }
            string trtFileName = Path.GetFileNameWithoutExtension(trtFilePath);

            #region Check Folder Acess
            string gameFolderPath = Directory.GetParent(trtFilePath).Parent.FullName;
            i_TransitionPath = gameFolderPath + "/transitions/";
            i_ObjectFolderPath = gameFolderPath + "/objects/";
            i_SprFolderPath = gameFolderPath + "/sprites/";
            i_Import_AddFolderPath = gameFolderPath + "/import_add/";
            i_Import_ReplaceFolderPath = gameFolderPath + "/import_replace/";
            //i_ImportedFolderPath = gameFolderPath + "/imported/";
            if (!Directory.Exists(i_TransitionPath))
            {
                CreateFatalErrorMessage("transitions Folder dont exist"); return;
            }
            if (!Directory.Exists(i_ObjectFolderPath))
            {
                CreateFatalErrorMessage("objects Folder dont exist"); return;
            }
            if (!Directory.Exists(i_SprFolderPath))
            {
                CreateFatalErrorMessage("sprites Folder dont exist"); return;
            }
            if (!Directory.Exists(i_Import_AddFolderPath))
            {
                CreateFatalErrorMessage("import_add Folder dont exist"); return;
            }
            if (!Directory.Exists(i_Import_ReplaceFolderPath))
            {
                CreateFatalErrorMessage("import_replace Folder dont exist"); return;
            }
            #endregion

            #region Shearch the nextObjectNumber.txt file and return nextID
            string[] list_objects = Directory.GetFiles(i_ObjectFolderPath);
            int nextID = 0; bool nextObjNumfind = false;

            for (int i = list_objects.Length - 1; i >= 0; i--)
            {
                if (Path.GetFileName(list_objects[i]) == "nextObjectNumber.txt")
                {
                    nextObjNumfind = true;
                    if (!int.TryParse(File.ReadAllLines(list_objects[i])[0], out nextID))
                    {
                        i_label_Info_OnBrowserButtonClick.ForeColor = Color.Red; i_label_Info_OnBrowserButtonClick.Text = "cannot parse 'nextObjectNumber.txt' file"; return;
                    }
                }
            }
            if (!nextObjNumfind)
            {
                i_label_Info_OnBrowserButtonClick.ForeColor = Color.Red; i_label_Info_OnBrowserButtonClick.Text = "'nextObjectNumber.txt' file not find"; return;
            }
            #endregion

            #region Set new IDs List of objects
            i_listNewIDs = new List<int>();
            string[] trtFileNameSeparated = trtFileName.Split('_');
            int nbObject = 0;
            if (!int.TryParse(trtFileNameSeparated[trtFileNameSeparated.Length - 2], out nbObject)) // lenght -2 in case there is '_' in the name
            {
                i_label_Info_OnBrowserButtonClick.ForeColor = Color.Red; i_label_Info_OnBrowserButtonClick.Text = "trt file name problem (parse)"; return;
            }

            int startID = nextID - nbObject; // -1 but +1  
            int tmp_num = startID;
            for (int i = startID; i < nextID; i++)
            {
                i_listNewIDs.Add(tmp_num);
                tmp_num++;
            }
            i_listNewIDs.Reverse();
            #endregion

            i_listObjSettingsClear = new List<ObjectSettings>();
            ObjectSettings[] i_listObjectsSettings = LoadAllObjects(i_ObjectFolderPath, i_SprFolderPath, false);
            if (i_listObjectsSettings == null) return;

            #region Clear null value
            for (int x = 0; x < i_listObjectsSettings.Length; x++)
            {

                if (i_listObjectsSettings[x] == null)
                {
                    continue;
                }
                i_listObjSettingsClear.Add(i_listObjectsSettings[x]);
            }
            //i_ObjListView.listObj = i_listObjSettingsClear;
            i_TransListView.listObj = i_listObjSettingsClear;

            #endregion


            // i_listOldIDs / i_transitionsList / i_transitionsList_Error / i_transitionsList_Error_Selected are clear in OpenFile method
            // we modify old IDs directly when we open file
            if (!OpenFile(trtFilePath, i_TransitionPath)) return;

            //i_TransLV.SetObjects(i_transitionsList);
            i_TransListView.SetObjects(i_transList);

            //i_TransListView.SelectAll();
            i_label_NbTrImported.Text = i_transList.Count + "/" + i_transList.Count; //all selected

            //check if any transition has same file name that an already existing one ( no need check same file content)
            for (int i = 0; i < i_transList.Count; i++)
            {
                if (i_transList[i].haveAnyIDMiss)
                {
                    i_transList_IdMiss.Add(i_transList[i]);
                    continue; // we dont add if list already exist (ID miss = not selectable)
                }
                if (File.Exists(i_transList[i].filePath))
                {
                    //i_TransListView.GetItem(i).Selected = false;
                    i_transList[i].pathAlreadyExist = true;
                    i_transList_AlreadyExist.Add(i_transList[i]);
                }
            }
            i_TransListView.i_SelectAllGoodTransitions();
            i_UpdateLabelsTransitions();
            i_UpdateAllDecorations_Transition(i_TransListView);
            i_button_Import.Enabled = true;

            FormLoading.CloseForm();
        }
        private void i_button_Import_Click(object sender, EventArgs e)
        {

            i_newTr_num = 0; i_replaceTr_num = 0; i_PassedTr_num = 0;

            foreach (Transition t in i_TransListView.SelectedObjects)
            {
                File.WriteAllLines(t.filePath, t.fileContent);
                if (t.pathAlreadyExist) i_replaceTr_num++;
                else i_newTr_num++;
            }
            i_PassedTr_num = i_transList.Count - i_TransListView.SelectedObjects.Count;
            //Should copy .trt file into 'imported' folder but EditOneLife already copy it same time than .oxz file
            #region Delete Old .trt/.oxz Files
            if (i_checkBox_DeleteOldFiles.Checked == true)
            {
                foreach (var a in Directory.GetFiles(i_Import_AddFolderPath))
                {
                    File.Delete(a);
                }
                foreach (var b in Directory.GetFiles(i_Import_ReplaceFolderPath))
                {
                    File.Delete(b);
                }
            }
            #endregion

            File.Delete(i_TransitionPath + "cache.fcz"); // to make sure that reload transitions

            i_label_Info_OnImportButtonClick.ForeColor = Color.GreenYellow;
            i_label_Info_OnImportButtonClick.Text = i_newTr_num + " new / "
                + i_replaceTr_num + " replace / "
                + i_PassedTr_num + " passed";
        }
        #endregion
        #region Checkbox
        private void i_checkBox_DeleteOldFiles_CheckedChanged(object sender, EventArgs e)
        {
            //Save in datasettings *NM
        }
        private void i_checkBox_TransOverview_CheckedChanged(object sender, EventArgs e)
        {
            if (!i_checkBox_TransOverview.Checked) i_panel_TransOverview.Visible = false;
        }
        #endregion
        public void i_UpdateAllDecorations_Transition(CustomTransitionsListView list)
        {
            for (int i = 0; i < list.GetItemCount(); i++)
            {
                Transition t = (Transition)list.GetModelObject(i);
                if (t.haveAnyIDMiss)
                    list.GetItem(i).Decoration = rbd_TransitionError;
                else if (t.pathAlreadyExist) //TODO : Remove 'else' when personal rbd => rbd cannot overlaps each other (weird looking) 
                    list.GetItem(i).Decoration = rbd_AlreadyExist;
                //if(t.pathAlreadyExist)
            }
        }
        public void i_CreateCustomListsView()
        {
            #region TransitionsListView
            i_TransListView = CreateTransitionListView(i_panel_TranImported_ListView, 64, colGreenDark);//,colGreenDarkSelection);

            OLVColumn columnHand1 = CreateColumn("Hand1", "", 64, false, 0);
            OLVColumn columnMore1 = CreateColumn("+", "", 20, false, 1);
            OLVColumn columnTarget1 = CreateColumn("Target1", "", 64, false, 2);
            OLVColumn columnEgal = CreateColumn(">", "", 20, false, 3);
            OLVColumn columnHand2 = CreateColumn("Hand2", "", 64, false, 4);
            OLVColumn columnMore2 = CreateColumn("+", "", 20, false, 5);
            OLVColumn columnTarget2 = CreateColumn("Target2", "", 64, false, 6);

            columnHand1.ImageGetter = new ImageGetterDelegate(this.TransitionsHand1ImageGetter);
            columnMore1.AspectGetter = new AspectGetterDelegate(this.TransitionsMore1Getter);
            columnTarget1.ImageGetter = new ImageGetterDelegate(this.TransitionsTarget1ImageGetter);
            columnEgal.AspectGetter = new AspectGetterDelegate(this.TransitionsEgalGetter);
            columnHand2.ImageGetter = new ImageGetterDelegate(this.TransitionsHand2ImageGetter);
            columnMore2.AspectGetter = new AspectGetterDelegate(this.TransitionsMore2Getter);
            columnTarget2.ImageGetter = new ImageGetterDelegate(this.TransitionsTarget2ImageGetter);

            columnHand1.FillsFreeSpace = true;
            columnTarget1.FillsFreeSpace = true;
            columnHand2.FillsFreeSpace = true;
            columnTarget2.FillsFreeSpace = true;
            i_TransListView.AllColumns.Add(columnHand1);
            i_TransListView.AllColumns.Add(columnMore1);
            i_TransListView.AllColumns.Add(columnTarget1);
            i_TransListView.AllColumns.Add(columnEgal);
            i_TransListView.AllColumns.Add(columnHand2);
            i_TransListView.AllColumns.Add(columnMore2);
            i_TransListView.AllColumns.Add(columnTarget2);
            i_TransListView.RebuildColumns();

            i_TransListView.checkBox_TransOverview = i_checkBox_TransOverview;
            i_TransListView.labelName = i_label_TrOverviewName;
            i_TransListView.labelSubName = i_label_TrOverviewSubName;
            i_TransListView.labelID = i_label_TrOverviewID;
            i_TransListView.panelOverview = i_panel_TransOverview;
            i_TransListView.form1 = this;
            i_TransListView.export = false;

            //i_TransListView.FormatRow += AlreadyExistTransition_FormatRow;

            #endregion

            /*
            #region TransitionsListView
            i_TransLV = CreateTransitionListView(panel15, 64, colGreenDark);//, colGreenDarkSelection);

            OLVColumn _columnHand1 = CreateColumn("Hand1","", 64, false, 0);
            OLVColumn _columnMore1 = CreateColumn("+", "", 16, false, 1);
            OLVColumn _columnTarget1 = CreateColumn("Target1", "", 64, false, 2);
            OLVColumn _columnEgal = CreateColumn(">", "", 16, false, 3);
            OLVColumn _columnHand2 = CreateColumn("Hand2", "", 64, false, 4);
            OLVColumn _columnMore2 = CreateColumn("+", "", 16, false, 5);
            OLVColumn _columnTarget2 = CreateColumn("Target2", "", 64, false, 6);

            _columnHand1.ImageGetter = new ImageGetterDelegate(this.TransitionsHand1ImageGetter);
            _columnMore1.AspectGetter = new AspectGetterDelegate(this.TransitionsMore1Getter);
            _columnTarget1.ImageGetter = new ImageGetterDelegate(this.TransitionsTarget1ImageGetter);
            _columnEgal.AspectGetter = new AspectGetterDelegate(this.TransitionsEgalGetter);
            _columnHand2.ImageGetter = new ImageGetterDelegate(this.TransitionsHand2ImageGetter);
            _columnMore2.AspectGetter = new AspectGetterDelegate(this.TransitionsMore2Getter);
            _columnTarget2.ImageGetter = new ImageGetterDelegate(this.TransitionsTarget2ImageGetter);

            _columnHand1.FillsFreeSpace = true;
            _columnTarget1.FillsFreeSpace = true;
            _columnHand2.FillsFreeSpace = true;
            _columnTarget2.FillsFreeSpace = true;
            i_TransLV.AllColumns.Add(_columnHand1);
            i_TransLV.AllColumns.Add(_columnMore1);
            i_TransLV.AllColumns.Add(_columnTarget1);
            i_TransLV.AllColumns.Add(_columnEgal);
            i_TransLV.AllColumns.Add(_columnHand2);
            i_TransLV.AllColumns.Add(_columnMore2);
            i_TransLV.AllColumns.Add(_columnTarget2);
            i_TransLV.RebuildColumns();
            i_TransLV.transitions = i_transitionsList;
            i_TransLV.form1 = this;
            i_TransLV.export = false;
            //i_TransLV.ItemChecked += OnItemCheck;
            //i_TransLV.FormatRow += AlreadyExistTransition_FormatRow;
            

            #endregion
            */
        }
        public void i_UpdateLabelsTransitions()
        {
            if (i_transList_AlreadyExist_Selected.Count > 0) i_label_NbTrImported.ForeColor = Color.Yellow;
            else i_label_NbTrImported.ForeColor = Color.Black;

            i_label_NbTrImported.Text = i_TransListView.list_Selected.Count + "/" + i_transList.Count;
            i_label_NbTr_AlrExist.Text = i_transList_AlreadyExist_Selected.Count + "/" + i_transList_AlreadyExist.Count;
            i_label_NbTran_IdMiss.Text = i_transList_IdMiss.Count + "";

        }
        /*
        public void i_SetLabelsTransitions(int count, int count_error = -1)
        {
            Debug.WriteLine("Set label : "+count);
            Debug.WriteLine("Set label error : "+count_error);
            if (i_transitionsList_Error_Selected.Count > 0) i_label_NbTrImported.ForeColor = Color.Yellow;
            else i_label_NbTrImported.ForeColor = Color.Black;
            if (count_error == 0)
                i_labelNbTr_Error.ForeColor = Color.Green;
            else if (count_error > 0) i_labelNbTr_Error.ForeColor = Color.Yellow;

            i_label_NbTrImported.Text = count + "/" + i_transitionsList.Count;
            
            if (count_error > -1)
            {
                i_labelNbTr_Error.Text = count_error + "";
            }
        }
        */

        #endregion

        #region GETTERS
        public object? ObjectSettingsImageGetter64(object rowObject)
        {
            ObjectSettings objSet = (ObjectSettings)rowObject;
            if (objSet.objBmp64 != null)
            {
                return objSet.objBmp64;
            }
            return null;
        }
        public object? ObjectSettingsImageGetter32(object rowObject)
        {
            ObjectSettings objSet = (ObjectSettings)rowObject;
            if (objSet.objBmp32 != null)
            {
                return objSet.objBmp32;
            }
            return null;
        }
        public object TransitionsMore1Getter(object rowObject)
        {
            return "+";
        }
        public object TransitionsEgalGetter(object rowObject)
        {
            return ">";
        }
        public object TransitionsMore2Getter(object rowObject)
        {
            Transition traSet = (Transition)rowObject;
            if (traSet.isObjHand1Timer)
                return "";
            return "+";
        }
        public object? TransitionsHand1ImageGetter(object rowObject)
        {
            Transition traSet = (Transition)rowObject;
            if (traSet.objHand1 != null)
            {
                return traSet.GetObjHand1Bitmap();
            }
            return null;
        }
        public object? TransitionsTarget1ImageGetter(object rowObject)
        {
            Transition traSet = (Transition)rowObject;
            if (traSet.objTarget1 != null)
            {
                return traSet.GetObjTarget1Bitmap();
            }
            return null;
        }
        public object? TransitionsHand2ImageGetter(object rowObject)
        {
            Transition traSet = (Transition)rowObject;
            if (traSet.objHand2 != null)
            {
                return traSet.GetObjHand2Bitmap();
            }
            return null;
        }
        public object? TransitionsTarget2ImageGetter(object rowObject)
        {
            Transition traSet = (Transition)rowObject;
            if (traSet.objTarget2 != null)
            {
                return traSet.GetObjTarget2Bitmap();
            }
            return null;
        }
        #endregion

        #region Global Method
        private ObjectSettings[]? LoadAllObjects(string ObjFolderPath, string sprFolderPath, bool isExportTab)//, List<ObjectSettings> listObjSettingClear)
        {
            string nextObjectNumberPath = ObjFolderPath + "nextObjectNumber.txt";
            int nextObjectNumber; int.TryParse(File.ReadAllText(nextObjectNumberPath), out nextObjectNumber);
            ObjectSettings[] e_listObjectsSettings = new ObjectSettings[nextObjectNumber - 1]; // With null value (the missing IDs)
            //listObjSettingClear = new List<ObjectSettings>();
            ObjectSettings objectSettings = new ObjectSettings();
            SpriteSettings spriteSettings = new SpriteSettings();
            string[] pathsList;
            if (isExportTab && e_checkBox_FastLoad.Checked)
            {
                pathsList = GetListPathsOrdered(ObjFolderPath, live_NON);
            }
            else
            {
                pathsList = Directory.GetFiles(ObjFolderPath, "*.txt");
                //pathsList = Directory.GetFiles(ObjFolderPath,"*.txt").Where(file => Regex.IsMatch(Path.GetFileNameWithoutExtension(file), "^[0-9]+");
            }
            foreach (string path in pathsList) // For every file in objects folder
            {
                if (Path.GetFileNameWithoutExtension(path).Any(x => char.IsLetter(x)))
                {
                    continue;
                }
                int countLine = 0;
                #region Read Lines
                foreach (var line in File.ReadAllLines(path)) // For every line in this object
                {
                    countLine++;
                    if (countLine == 1)
                    {
                        int id = -1;
                        if (!int.TryParse(line.Split('=')[1], out id))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the ID at line : " + line, isExportTab);
                            return null;
                        }
                        objectSettings = new ObjectSettings();
                        objectSettings.id = id;
                        e_listObjectsSettings[id - 1] = objectSettings; //-1 cause ID start at 1 ( array start at 0 )
                        continue;
                    }
                    if (countLine == 2)
                    {
                        objectSettings.fullName = line;
                        continue;
                    }
                    if (line.StartsWith("numSprites="))
                    {
                        int numSprite;
                        if (!int.TryParse(line.Split('=')[1], out numSprite))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the numSprites at line : " + line, isExportTab);
                            return null;
                        }
                        objectSettings.numSprite = numSprite;
                        objectSettings.listSprites = new List<SpriteSettings>(numSprite);
                        continue;
                    }
                    if (line.StartsWith("spriteID="))
                    {
                        spriteSettings = new SpriteSettings();
                        int sprID;
                        if (!int.TryParse(line.Split('=')[1], out sprID))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the ID at line : " + line, isExportTab);
                            return null;
                        }
                        spriteSettings.ID = sprID;
                        objectSettings.listSprites.Add(spriteSettings);
                        continue;
                    }
                    if (line.StartsWith("pos="))
                    {
                        string[] p = line.Split('=')[1].Split(',');
                        float x; float y;
                        if (!float.TryParse(p[0], NumberStyles.Number, CultureInfo.InvariantCulture, out x))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the x pos at line : " + line, isExportTab);
                            return null;
                        }
                        if (!float.TryParse(p[1], NumberStyles.Number, CultureInfo.InvariantCulture, out y))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the y pos at line : " + line, isExportTab);
                            return null;
                        }
                        spriteSettings.pos = new Point((int)x, (int)y);
                        continue;
                    }
                    if (line.StartsWith("rot="))
                    {
                        float r;
                        if (!float.TryParse(line.Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the rot at line : " + line, isExportTab);
                            return null;
                        }
                        spriteSettings.rot = r;
                        continue;
                    }
                    if (line.StartsWith("hFlip="))
                    {
                        if (line.Split('=')[1] == "0") spriteSettings.hFlip = false;
                        else spriteSettings.hFlip = true;
                        continue;
                    }
                    if (line.StartsWith("color="))
                    {
                        string[] c = line.Split('=')[1].Split(',');
                        float h; float s; float v;
                        if (!float.TryParse(c[0], NumberStyles.Number, CultureInfo.InvariantCulture, out v))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the color hue at line : " + line, isExportTab);
                            return null;
                        }
                        if (!float.TryParse(c[1], NumberStyles.Number, CultureInfo.InvariantCulture, out s))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the color saturation at line : " + line, isExportTab);
                            return null;
                        }
                        if (!float.TryParse(c[2], NumberStyles.Number, CultureInfo.InvariantCulture, out h))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the color value at line : " + line, isExportTab);
                            return null;
                        }

                        spriteSettings.color = ColorFromHSV(h, s, v);
                        continue;
                    }
                    if (line.StartsWith("pixHeight="))
                    {
                        int pH;
                        if (!int.TryParse(line.Split('=')[1], out pH))
                        {
                            CreateFatalErrorMessage("During objects loading : Couldnt parse the pixHeight at line : " + line, isExportTab);
                            return null;
                        }
                        objectSettings.pixHeight = pH;
                        continue;
                    }
                }
                #endregion

                #region Load TGA file
                foreach (var sprSetting in objectSettings.listSprites)
                {
                    string sprPath = sprFolderPath + sprSetting.ID + tgaExtension;
                    if (!File.Exists(sprPath))
                    {
                        CreateFatalErrorMessage("Can not find file at : " + sprPath, isExportTab);
                        return null;
                    }
                    Bitmap bm = Paloma.TargaImage.LoadTargaImage(sprFolderPath + sprSetting.ID + tgaExtension);
                    string[] lines = File.ReadAllLines(sprFolderPath + sprSetting.ID + ".txt");
                    string[] separate = lines[0].Split(' ');
                    int xAnch; int.TryParse(separate[2], out xAnch);
                    int yAnch; int.TryParse(separate[3], out yAnch);
                    sprSetting.anchor = new Point(xAnch, yAnch);
                    sprSetting.bm = new Bitmap(bm);

                    bm.Dispose();
                }
                #endregion

                objectSettings.MergeListOfSprites();
            }
            Array.Reverse(e_listObjectsSettings); // Reverse cause people will mostly work on last IDs

            return e_listObjectsSettings;

        }
        public List<Transition> GetTransitions(List<string> ids, string transitionFolderPath)
        {
            string[] trans_list = Directory.GetFiles(transitionFolderPath);
            List<Transition> trans_list_choosed = new List<Transition>();

            for (int i = 0; i < trans_list.Length; i++)
            {
                string fileName = Path.GetFileName(trans_list[i]);
                if (HasID(trans_list[i], ids))
                {
                    trans_list_choosed.Add(GetTransition(trans_list[i]));
                }
            }
            return trans_list_choosed;
        }
        public Transition GetTransition(string transitionPath)
        {
            Transition t = new Transition();
            t.filePath = transitionPath;
            List<string> list = new List<string>();

            //IDs on file name :
            string[] IDsOnName = Path.GetFileNameWithoutExtension(transitionPath).Split('_');
            list.Add(IDsOnName[0]); list.Add(IDsOnName[1]);
            //IDs in file content :
            string[] IDsLine = null;
            foreach (string s in File.ReadLines(transitionPath))
            {
                IDsLine = s.Split(' ');
                list.Add(IDsLine[0]);
                list.Add(IDsLine[1]);
                break; // Only read first line
            }

            if (IDsOnName.Length > 2) //Last
            {
                if (IDsOnName[2] == "LA")
                    t.isObjHand1Last = true;
                else if (IDsOnName[2] == "LT")
                    t.isObjTarget1Last = true;
            }

            if (list[0] == "-1") //Timer
            {
                int time = -1;
                int.TryParse(IDsLine[2], out time);
                t.time = time;
            }
            int.TryParse(IDsLine[7], out t.chase);

            ObjectSettings[] tmpList = GetObjectsSettings(list, e_listObjSettingsClear);
            if (tmpList == null) return null;
            t.objHand1 = tmpList[0];
            t.objTarget1 = tmpList[1];
            t.objHand2 = tmpList[2];
            t.objTarget2 = tmpList[3];
            return t;
        }
        public ObjectSettings[] GetObjectsSettings(List<string> listStrID, List<ObjectSettings> listToSearch)
        {

            ObjectSettings[] list = new ObjectSettings[4];
            if (listStrID.Count != 4)
            {
                e_label_Info_OnBrowserButton.ForeColor = Color.Red; e_label_Info_OnBrowserButton.Text = "ListStrID.count != 4"; return null;
            }
            int[] ids = new int[4];

            //Debug.WriteLine("We search : ");
            //Debug.WriteLine("" + listStrID[0] +" / "+ listStrID[1] + " / " + listStrID[2] + " / " + listStrID[3]);

            for (int i = 0; i < listStrID.Count; i++)
            {
                if (!int.TryParse(listStrID[i], out ids[i]))
                {
                    e_label_Info_OnBrowserButton.ForeColor = Color.Red; e_label_Info_OnBrowserButton.Text = "During Transition Get : cannot parse : " + listStrID[i]; return list;
                }
            }

            for (int i = 0; i < ids.Length; i++) // IDs can be the same, so we search 4 time in the list instead of search 1 time for each IDs
            {
                if (ids[i] <= 0) // Object is NULL on purpose
                { // if = -1 or 0
                    list[i] = new ObjectSettings();
                    list[i].isNULL = true;
                    continue;
                }
                bool find = false;
                foreach (var obj in listToSearch) // Object ID find in the list
                {
                    if (obj.id == ids[i])
                    {
                        list[i] = obj;
                        find = true;
                        //break; // NOT SURE NEED TO TEST
                    }
                }
                if (!find) // Object is NULL not on purpose (we show ID missing)
                {
                    //Should be null if we do nothing *TO MODIFY
                    list[i] = new ObjectSettings();
                    list[i].idNotFind = true;
                    list[i].id = ids[i];
                }
            }
            return list;
        }
        public ViewListObject CreateViewListObject(Panel parent, int rowHeight, Color backColor, Color selectedBackColor)
        {

            ViewListObject ObjLV = new ViewListObject();
            ObjLV.SelectedRowDecoration = rbd_Selected;
            ObjLV.BorderStyle = BorderStyle.None;
            ObjLV.View = View.Details;
            ObjLV.Bounds = new Rectangle(new Point(0, 0), parent.Size);
            ObjLV.HeaderStyle = ColumnHeaderStyle.None;
            parent.Controls.Add(ObjLV);
            //e_CustomObjectsListView.HasCollapsibleGroups = false;
            ObjLV.Sorting = SortOrder.None;
            ObjLV.RowHeight = rowHeight;
            ObjLV.SelectAllOnControlA = false;
            ObjLV.FullRowSelect = true;
            ObjLV.HideSelection = false;

            ObjLV.BackColor = backColor;
            //ObjLV.SelectedBackColor = selectedBackColor;
            ObjLV.SelectedBackColor = backColor;
            ObjLV.SelectedForeColor = Color.Black;
            //ObjLV.UnfocusedSelectedBackColor = ObjLV.SelectedBackColor;
            ObjLV.UnfocusedSelectedBackColor = backColor;
            ObjLV.UnfocusedSelectedForeColor = Color.Black;
            return ObjLV;
        }
        public CustomTransitionsListView CreateTransitionListView(Panel parent, int rowHeight, Color backColor)//, Color selectedBackColor)
        {

            CustomTransitionsListView ObjLV = new CustomTransitionsListView();
            ObjLV.SelectedRowDecoration = rbd_Selected;
            ObjLV.BorderStyle = BorderStyle.None;
            ObjLV.View = View.Details;
            ObjLV.Bounds = new Rectangle(new Point(0, 0), parent.Size);
            ObjLV.HeaderStyle = ColumnHeaderStyle.None;
            parent.Controls.Add(ObjLV);
            ObjLV.Sorting = SortOrder.None;
            ObjLV.RowHeight = rowHeight;
            ObjLV.SelectAllOnControlA = false;
            ObjLV.FullRowSelect = true;
            ObjLV.HideSelection = false;

            ObjLV.BackColor = backColor;
            ObjLV.SelectedBackColor = backColor;
            ObjLV.SelectedForeColor = Color.Black;
            ObjLV.UnfocusedSelectedBackColor = backColor;
            ObjLV.UnfocusedSelectedForeColor = Color.Black;

            return ObjLV;
        }
        public CustomObjectListView CreateObjectsListView(Panel parent, int rowHeight, Color backColor)//, Color selectedBackColor)
        {

            CustomObjectListView ObjLV = new CustomObjectListView();
            ObjLV.SelectedRowDecoration = rbd_Selected;
            ObjLV.BorderStyle = BorderStyle.None;
            ObjLV.View = View.Details;
            ObjLV.Bounds = new Rectangle(new Point(0, 0), parent.Size);
            ObjLV.HeaderStyle = ColumnHeaderStyle.None;
            parent.Controls.Add(ObjLV);
            ObjLV.Sorting = SortOrder.None;
            ObjLV.RowHeight = rowHeight;
            ObjLV.SelectAllOnControlA = false;
            ObjLV.FullRowSelect = true;
            ObjLV.HideSelection = false;

            ObjLV.BackColor = backColor;
            ObjLV.SelectedBackColor = backColor;
            ObjLV.SelectedForeColor = Color.Black;
            ObjLV.UnfocusedSelectedBackColor = backColor;
            ObjLV.UnfocusedSelectedForeColor = Color.Black;

            return ObjLV;
        }
        public OLVColumn CreateColumn(string title, string aspectName, int width, bool groupable, int dispayIndex, int freeSpace = 0)
        {
            var column = new OLVColumn();
            column.Name = title;
            column.AspectName = aspectName;
            column.Width = width;
            column.Groupable = groupable;
            column.DisplayIndex = dispayIndex;
            column.TextAlign = HorizontalAlignment.Center;
            column.HeaderTextAlign = HorizontalAlignment.Center;
            column.FreeSpaceProportion = freeSpace;
            return column;
        }
        #endregion

        #region Global Static Method
        public static void ColorToHSV(Color color, out float hue, out float saturation, out float value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            value = max / 255f;
        }
        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
        public static Color Colorize(Color baseColor, Color colorizeColor, float amount)
        {
            byte r = (byte)(baseColor.R + colorizeColor.R * (1 - amount));
            byte g = (byte)(baseColor.G + colorizeColor.G * (1 - amount));
            byte b = (byte)(baseColor.B + colorizeColor.B * (1 - amount));
            return Color.FromArgb(baseColor.A, r, g, b);
        }
        public static string TimeConverter(int seconds)
        {
            Debug.WriteLine("time converter : " + seconds);
            string result = "";
            int h = 0, m = 0, s = 0;
            if (seconds < 0) return "-1";
            else if (seconds < 60) s = seconds;
            else if (seconds < 3600) { m = seconds / 60; s = seconds % 60; }
            else { h = seconds / 3600; m = seconds % 3600 % 60; s = seconds % 60; }

            if (h != 0)
                result += h + "h";
            if (m != 0)
                result += m + "m";
            if (s != 0)
                result += s + "s";
            Debug.WriteLine("result : " + result);
            return result;
        }
        private static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercentW = ((float)size.Width / (float)sourceWidth);
            float nPercentH = ((float)size.Height / (float)sourceHeight);
            float nPercent = Math.Min(nPercentW, nPercentH);

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            using (var g = Graphics.FromImage(b))
            {
                //Graphics g = Graphics.FromImage(b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Rectangle r = new Rectangle(0, 0, b.Width, b.Height);
                SolidBrush brush = new SolidBrush(Color.FromArgb(50, 20, 20, 20));
                g.FillPath(brush, RoundedRect(r, 10));
                g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
                //g.Dispose();
            }
            return b;
        }
        public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        #endregion

        public class ObjectSettings
        {
            #region ListViewSettings
            [OLVColumn(ImageAspectName = "ObjName")]
            public string ObjName
            {
                get { return fullName; }
                set { fullName = value; }
            }
            [OLVColumn(ImageAspectName = "ObjID")]
            public string ObjID
            {
                get { return id.ToString(); }
                set { int.TryParse(value, out id); }
            }

            /// <summary>
            /// If set up to true => No overview panel
            /// </summary>
            public bool isNULL = false;
            public bool idNotFind = false;
            #endregion

            public int id;
            public string fullName;
            /*public string subName
            {
                get
                {
                    if (fullName.Split('-')[0].Length >= fullName.Split('#')[0].Length)
                        return ;
                }
            }*/
            public int numSprite;
            public List<SpriteSettings> listSprites;
            public int pixHeight;
            public Bitmap objBmp;
            public Bitmap objBmp64;
            public Bitmap objBmp32;
            public void MergeListOfSprites()
            {
                if (listSprites == null)
                {
                    return;
                }

                #region Color sprites
                /*
                for (int i = 0; i < listSprites.Count; i++)
                {
                    if (listSprites[i].color == Color.Black) continue;
                    using (var g = Graphics.FromImage(listSprites[i].bm))
                    {
                        for (int a = 0; a < listSprites[i].bm.Width;a++)
                        {
                            for (int b = 0; b < listSprites[i].bm.Height; b++)
                            {
                                listSprites[i].bm.SetPixel(a,b, Colorize(listSprites[i].bm.GetPixel(a, b), listSprites[i].color, 0.5f));
                            }
                        }
                        g.DrawImage(listSprites[i].bm, 0, 0);
                    }
                }
                */
                #endregion

                #region Rotate each sprites
                for (int i = 0; i < listSprites.Count; i++)
                {

                    float tmpRot = listSprites[i].rot % 1f;
                    Bitmap bmRotated;
                    //int newWidth=0;
                    //int newHeight=0;
                    /*if (tmpRot!=0f) {
                        
                        /*if ((tmpRot > 0f && tmpRot < 0.25f) || (tmpRot > 0.5f && tmpRot < 0.75f))
                        {
                            newWidth = (int)Math.Abs((listSprites[i].bm.Width * Math.Cos(tmpRot)) + (int)(listSprites[i].bm.Height * Math.Sin(tmpRot)));
                            newHeight = (int)Math.Abs((listSprites[i].bm.Width * Math.Sin(tmpRot)) + (int)(listSprites[i].bm.Height * Math.Cos(tmpRot)));
                        }
                        else
                        {
                            newWidth = (int)Math.Abs((listSprites[i].bm.Height * Math.Cos(tmpRot)) + (int)(listSprites[i].bm.Width * Math.Sin(tmpRot)));
                            newHeight = (int)Math.Abs((listSprites[i].bm.Height * Math.Sin(tmpRot)) + (int)(listSprites[i].bm.Width * Math.Cos(tmpRot)));
                        }
                        bmRotated = new Bitmap(newWidth, newHeight);
                    }
                    else
                    {
                        bmRotated = new Bitmap(listSprites[i].bm.Width, listSprites[i].bm.Height);
                    }*/
                    bmRotated = new Bitmap(listSprites[i].bm.Width, listSprites[i].bm.Height);

                    using (var g = Graphics.FromImage(bmRotated))
                    {

                        float moveX = listSprites[i].bm.Width / 2f + listSprites[i].anchor.X;
                        float moveY = listSprites[i].bm.Height / 2f + listSprites[i].anchor.Y;
                        g.TranslateTransform(moveX, moveY);
                        g.RotateTransform(listSprites[i].rot * 360f);

                        g.TranslateTransform(-moveX, -moveY);


                        /*if (tmpRot != 0f)
                        {
                            g.DrawImage(listSprites[i].bm, 0, 0, newWidth, newHeight);
                        }
                        else
                        {
                            g.DrawImage(listSprites[i].bm, 0, 0);
                        }*/
                        g.DrawImage(listSprites[i].bm, 0, 0);

                        //g.ResetTransform();
                    }

                    listSprites[i].bm = bmRotated;
                }
                #endregion

                #region Flip
                for (int i = 0; i < listSprites.Count; i++)
                {
                    using (var g = Graphics.FromImage(listSprites[i].bm))
                    {
                        if (listSprites[i].hFlip) listSprites[i].bm.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        g.DrawImage(listSprites[i].bm, 0, 0);
                    }
                }
                // Get max width and height of the sprites
                /*
                int topPoint = int.MinValue;
                int bottomPoint = int.MaxValue;
                int leftPoint = int.MaxValue;
                int rightPoint = int.MinValue;
                for (int a = 0; a < listSprites.Count; a++)
                {
                    //Debug.WriteLine("sprite number " + a + " / Width: "+ listSprites[a].bm.Width + " / Height: " + listSprites[a].bm.Height);
                    int posTopImage = listSprites[a].pos.Y + listSprites[a].bm.Height/2 - listSprites[a].anchor.Y;
                    int posBottomImage = listSprites[a].pos.Y - listSprites[a].bm.Height/2 - listSprites[a].anchor.Y;
                    int posLeftImage = listSprites[a].pos.X - listSprites[a].bm.Width / 2 - listSprites[a].anchor.X;
                    int posRightImage = listSprites[a].pos.X + listSprites[a].bm.Width / 2 - listSprites[a].anchor.X;
                    //Debug.WriteLine("posTopImage : " + posTopImage);
                    //Debug.WriteLine("posBottomImage : " + posBottomImage);
                    //Debug.WriteLine("posLeftImage : " + posLeftImage);
                    //Debug.WriteLine("posRightImage : " + posRightImage);
                    topPoint = posTopImage > topPoint ? posTopImage : topPoint;
                    bottomPoint = posBottomImage < bottomPoint ? posBottomImage : bottomPoint;
                    leftPoint = posLeftImage < leftPoint ? posLeftImage : leftPoint;
                    rightPoint = posRightImage > rightPoint ? posRightImage : rightPoint;
                }
                int height = Math.Abs(topPoint - bottomPoint);
                int width = Math.Abs(rightPoint - leftPoint);*/

                #endregion

                #region Size object
                int width = 0; int height = 0;

                for (int a = 0; a < listSprites.Count; a++)
                {
                    int tmpWidth = (Math.Abs(listSprites[a].pos.X) + listSprites[a].bm.Width / 2 + Math.Abs(listSprites[a].anchor.X)) * 2;
                    int tmpHeight = (Math.Abs(listSprites[a].pos.Y) + listSprites[a].bm.Height / 2 + Math.Abs(listSprites[a].anchor.Y)) * 2;
                    width = tmpWidth > width ? tmpWidth : width;
                    height = tmpHeight > height ? tmpHeight : height;
                }

                if (height > width) width = height; else height = width;

                #endregion

                #region Merge sprites

                //Debug.WriteLine("width : "+width);
                //Debug.WriteLine("height : "+height);
                objBmp = new Bitmap(width, height);
                //objBmp = new Bitmap(300, 300);

                using (var g = Graphics.FromImage(objBmp))
                {
                    for (int i = 0; i < listSprites.Count; i++) //for (int i= listSprites.Count-1; i>=0;i--)
                    {
                        int x = listSprites[i].pos.X; int y = listSprites[i].pos.Y;
                        y = objBmp.Height / 2 - y; // Cause its draw from upper left and y is the move from center
                        int moveX = objBmp.Width / 2 - listSprites[i].bm.Width / 2 - listSprites[i].anchor.X;
                        int moveY = -listSprites[i].bm.Height / 2 - listSprites[i].anchor.Y;

                        RectangleF srcRect = new RectangleF(0F, 0F, listSprites[i].bm.Width, listSprites[i].bm.Height);
                        GraphicsUnit units = GraphicsUnit.Pixel;
                        g.DrawImage(listSprites[i].bm, x + moveX, y + moveY, srcRect, units);
                    }
                }
                #endregion

                objBmp64 = ResizeImage(objBmp, new Size(64, 64));
                objBmp32 = ResizeImage(objBmp, new Size(32, 32));
            }

        }
        public class SpriteSettings
        {
            public int ID;
            public Point pos;
            public float rot;
            public bool hFlip;
            public Point anchor;
            public Bitmap bm;
            public Color color;
        }
        public class Transition
        {
            public string filePath; // contain the name
            public bool pathAlreadyExist;
            public List<string> fileContent; // For import

            public bool willAnyCauseProb
            {
                get { return willObjHand1CauseProb || willObjHand2CauseProb || willObjTarget1CauseProb || willObjTarget2CauseProb; }
            }
            public bool willObjHand1CauseProb;
            public bool willObjTarget1CauseProb;
            public bool willObjHand2CauseProb;
            public bool willObjTarget2CauseProb;

            public void SetWillCauseProb(int oldNextObjNum, CustomObjectListView listObj)
            {
                if (objHand1 == null || objTarget1 == null || objHand2 == null || objTarget1 == null) { Debug.WriteLine("ONE OBJ NULL ?"); return; }// should never happen
                willObjHand1CauseProb = WillCauseProb(objHand1.id, oldNextObjNum, listObj);
                willObjTarget1CauseProb = WillCauseProb(objTarget1.id, oldNextObjNum, listObj);
                willObjHand2CauseProb = WillCauseProb(objHand2.id, oldNextObjNum, listObj);
                willObjTarget2CauseProb = WillCauseProb(objTarget2.id, oldNextObjNum, listObj);
            }

            private bool WillCauseProb(int id, int oldNextObjNum, CustomObjectListView listObj)
            {
                if (id > oldNextObjNum) // if its not ID that already exist in main
                {
                    bool find = false;
                    foreach (ObjectSettings obj in listObj.SelectedObjects)
                    {
                        if (id == obj.id) // and if dont have id in the selected one
                        {
                            find = true;
                            break;
                        }
                    }
                    if (!find) return true;
                }
                return false;
            }

            public bool haveAnyIDMiss
            {
                get { return objHand1.idNotFind || objTarget1.idNotFind || objHand2.idNotFind || objTarget2.idNotFind; }
            }


            public ObjectSettings objHand1; //Befor transition
            public ObjectSettings objTarget1; //Befor transition
            public ObjectSettings objHand2; //After transition
            public ObjectSettings objTarget2; //After transition

            public bool hand1NeedUpdate = true;
            public bool target1NeedUpdate = true;
            public bool hand2NeedUpdate = true;
            public bool target2NeedUpdate = true;

            public Bitmap GetObjHand1Bitmap()
            {
                hand1NeedUpdate = false;
                if (objHand1.idNotFind)
                    return DrawPanelOnBm(DrawPanelOnBm(DrawBmOnBm(DrawTextOnBm(empty, "ID MISS", TextPosition.CENTER), hand, 18, TextPosition.DOWN_LEFT), willObjHand1CauseProb), true);
                if (isObjHand1Timer)
                    return DrawPanelOnBm(DrawTextOnBm(stopWatch, timer, TextPosition.CENTER, new PointF(0, 5)), willObjHand1CauseProb);
                if (objHand1.isNULL)
                    return DrawPanelOnBm(DrawBmOnBm(empty, hand, 18, TextPosition.DOWN_LEFT), willObjHand1CauseProb);
                if (isObjHand1Last)
                    return DrawPanelOnBm(DrawBmOnBm(DrawTextOnBm(objHand1.objBmp64, "last"), hand, 18, TextPosition.DOWN_LEFT), willObjHand1CauseProb);

                return DrawPanelOnBm(DrawBmOnBm(objHand1.objBmp64, hand, 18, TextPosition.DOWN_LEFT), willObjHand1CauseProb);
            }
            public Bitmap GetObjTarget1Bitmap()
            {
                target1NeedUpdate = false;
                if (objTarget1.idNotFind)
                    return DrawPanelOnBm(DrawPanelOnBm(DrawTextOnBm(empty, "ID MISS", TextPosition.CENTER), willObjHand1CauseProb), true);
                if (objTarget1.isNULL)
                    return DrawPanelOnBm(empty, willObjTarget1CauseProb);
                if (isObjTarget1Last)
                    return DrawPanelOnBm(DrawTextOnBm(objTarget1.objBmp64, "last"), willObjTarget1CauseProb);
                return DrawPanelOnBm(objTarget1.objBmp64, willObjTarget1CauseProb);
            }
            public Bitmap GetObjHand2Bitmap()
            {
                hand2NeedUpdate = false;
                if (objHand2.idNotFind)
                    return DrawPanelOnBm(DrawPanelOnBm(DrawBmOnBm(DrawTextOnBm(empty, "ID MISS", TextPosition.CENTER), hand, 18, TextPosition.DOWN_LEFT), willObjHand1CauseProb), true);
                if (isObjHand1Timer)
                    return DrawPanelOnBm(empty, willObjHand2CauseProb);
                if (objHand2.isNULL)
                    return DrawPanelOnBm(DrawBmOnBm(empty, hand, 18, TextPosition.DOWN_LEFT), willObjHand2CauseProb);
                if (isObjHand2Last)
                    return DrawPanelOnBm(DrawBmOnBm(DrawTextOnBm(objHand2.objBmp64, "last"), hand, 18, TextPosition.DOWN_LEFT), willObjHand2CauseProb);
                if (isObjHand2Less)
                    return DrawPanelOnBm(DrawBmOnBm(DrawTextOnBm(objHand2.objBmp64, "-1"), hand, 18, TextPosition.DOWN_LEFT), willObjHand2CauseProb);
                if (isObjHand2Move)
                    return DrawPanelOnBm(DrawTextOnBm(objHand2.objBmp64, "Move", TextPosition.UP_LEFT), willObjHand2CauseProb);
                return DrawPanelOnBm(DrawBmOnBm(objHand2.objBmp64, hand, 18, TextPosition.DOWN_LEFT), willObjHand2CauseProb);
                //return objHand2.objBmp64;
            }
            public Bitmap GetObjTarget2Bitmap()
            {
                target2NeedUpdate = false;
                if (objTarget2.idNotFind)
                    return DrawPanelOnBm(DrawPanelOnBm(DrawTextOnBm(empty, "ID MISS", TextPosition.CENTER), willObjHand1CauseProb), true);
                if (objTarget2.isNULL)
                    return DrawPanelOnBm(empty, willObjTarget2CauseProb);
                if (isObjTarget2Last)
                    return DrawPanelOnBm(DrawTextOnBm(objTarget2.objBmp64, "last"), willObjTarget2CauseProb);
                if (isObjTarget2More)
                    return DrawPanelOnBm(DrawTextOnBm(objTarget2.objBmp64, "+1"), willObjTarget2CauseProb);
                if (isObjTarget2Less)
                    return DrawPanelOnBm(DrawTextOnBm(objTarget2.objBmp64, "-1"), willObjTarget2CauseProb);
                //if (isObjTarget2Move)
                //return DrawTextOnBitmap(objTarget2.objBmp64, "Move", TextPosition.UP_LEFT);
                return DrawPanelOnBm(objTarget2.objBmp64, willObjTarget2CauseProb);
            }

            public bool isGenericPersonTransition = false;

            public bool isObjHand1Last = false;
            public bool isObjTarget1Last = false;
            public bool isObjHand2Last = false;
            public bool isObjTarget2Last = false;

            public bool isObjHand1More = false;
            public bool isObjTarget1More = false;
            public bool isObjHand2More = false;
            public bool isObjTarget2More = false;

            public bool isObjHand1Less = false;
            public bool isObjTarget1Less = false;
            public bool isObjHand2Less = false;
            public bool isObjTarget2Less = false;

            public bool isObjHand1Move = false;
            public bool isObjTarget1Move = false;
            public bool isObjHand2Move = false;
            public bool isObjTarget2Move = false;

            public int time = 0; //in second
            public string timer
            {
                get { return TimeConverter(time); }
            }
            public int chase = 0;// 0:None/ 1:Chase/ 2:Flee/ 3:Random/ 4:North/ 5:South/ 6:East/ 7:West //TODO
            public bool isObjHand1Timer
            {
                get { if (time != 0) return true; return false; }
                //get { if (objHand1.isNULL && objHand2.isNULL) return true; return false; }
            }

            public enum TextPosition
            {
                UP_LEFT, UP_CENTER, UP_RIGHT,
                LEFT, CENTER, RIGHT,
                DOWN_LEFT, DOWN_CENTER, DOWN_RIGHT
            }
            public Bitmap DrawPanelOnBm(Bitmap bm, bool draw)
            {
                if (!draw) return bm;
                Bitmap result = new Bitmap(bm);
                using (var g = Graphics.FromImage(result))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    RectangleF rectf = new RectangleF(0, 0, bm.Width, bm.Height);
                    Pen borderBrush = new Pen(Color.FromArgb(255, Color.Red), 2);
                    SolidBrush fillBrush = new SolidBrush(Color.FromArgb(20, Color.Red));
                    GraphicsPath graphPath = RoundedRect(rectf, 6);
                    g.DrawPath(borderBrush, graphPath);
                    g.FillPath(fillBrush, graphPath);
                    //g.DrawImage(bm, font, Brushes.White, rectf);

                }
                return result;
            }
            public Bitmap DrawTextOnBm(Bitmap bm, string text, TextPosition pos = TextPosition.DOWN_RIGHT, PointF offset = default)
            {

                Bitmap result;
                if (pos == TextPosition.CENTER) //Center suppose to hapen only when timer (stopwatch)
                    result = ResizeImage(bm, new Size(64, 64));
                else
                    result = new Bitmap(bm);
                using (var g = Graphics.FromImage(result))
                {
                    Font font = new Font("Tahoma", 8);
                    SizeF textSize = g.MeasureString(text, font);
                    RectangleF rectf = new RectangleF(GetPointFOnPosition(result.Size, textSize, pos), textSize);
                    rectf.Offset(offset);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    SolidBrush brush = new SolidBrush(Color.FromArgb(150, 20, 20, 20));
                    g.FillPath(brush, RoundedRect(rectf, 2));
                    g.DrawString(text, font, Brushes.White, rectf);

                }
                return result;
            }
            public PointF GetPointFOnPosition(SizeF sizeImage, SizeF sizeText, TextPosition textPos)
            {
                switch (textPos)
                {
                    case TextPosition.UP_LEFT:
                        return new PointF(0, 0);
                    case TextPosition.UP_CENTER:
                        return new PointF(sizeImage.Width / 2 - sizeText.Width / 2, 0);
                    case TextPosition.UP_RIGHT:
                        return new PointF(sizeImage.Width - sizeText.Width / 2, 0);
                    case TextPosition.LEFT:
                        return new PointF(0, sizeImage.Height / 2 - sizeText.Height / 2);
                    case TextPosition.CENTER:
                        return new PointF(sizeImage.Width / 2 - sizeText.Width / 2, sizeImage.Height / 2 - sizeText.Height / 2);
                    case TextPosition.RIGHT:
                        return new PointF(sizeImage.Width - sizeText.Width, sizeImage.Height / 2 - sizeText.Height / 2);
                    case TextPosition.DOWN_LEFT:
                        return new PointF(0, sizeImage.Height - sizeText.Height);
                    case TextPosition.DOWN_CENTER:
                        return new PointF(sizeImage.Width / 2 - sizeText.Width / 2, sizeImage.Height - sizeText.Height);
                    case TextPosition.DOWN_RIGHT:
                        return new PointF(sizeImage.Width - sizeText.Width, sizeImage.Height - sizeText.Height);
                    default: // Return CENTER by default
                        return new PointF(sizeImage.Width / 2 - sizeText.Width / 2, sizeImage.Height / 2 - sizeText.Height / 2);
                }
            }
            public Bitmap DrawBmOnBm(Bitmap bm, Bitmap bmToDraw, int percentRatio, TextPosition pos = TextPosition.DOWN_RIGHT, PointF offset = default)
            {
                SizeF bmToDrawResize = new SizeF(bmToDraw.Width * (percentRatio / 100f), bmToDraw.Height * (percentRatio / 100f));
                //Bitmap bmToDrawTmp = ResizeImage(bmToDraw,bmToDrawResize);
                Bitmap result = new Bitmap(bm);
                using (var g = Graphics.FromImage(result))
                {
                    RectangleF rectf = new RectangleF(GetPointFOnPosition(result.Size, bmToDrawResize, pos), bmToDrawResize);
                    rectf.Offset(offset);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    //SolidBrush brush = new SolidBrush(Color.FromArgb(150, 20, 20, 20));
                    //g.FillPath(brush, RoundedRect(rectf, 2));
                    g.DrawImage(bmToDraw, rectf);
                }
                return result;
            }

            //Import Method
            public void SetTimer(int t)
            {
                time = t;
            }
            public void SetFileNameIDs(string h1, string t1, List<ObjectSettings> listObjClear)
            {
                int hand1, target1;
                int.TryParse(h1, out hand1);
                int.TryParse(t1, out target1);
                if (hand1 <= 0)
                {
                    objHand1 = new ObjectSettings();
                    objHand1.isNULL = true;
                }
                else
                {
                    bool find = false;
                    foreach (var obj in listObjClear)
                    {
                        if (obj.id == hand1) //Find
                        {
                            objHand1 = obj;
                            find = true;
                            break;
                        }
                    }
                    if (!find) //Not find not in purpose
                    {
                        objHand1 = new ObjectSettings();
                        objHand1.idNotFind = true;
                        objHand1.id = hand1;
                    }
                    //objHand1 = listObjClear.Where(s => s.id == hand1).FirstOrDefault();
                }

                if (target1 <= 0)
                {
                    objTarget1 = new ObjectSettings();
                    objTarget1.isNULL = true;
                }
                else
                {
                    bool find = false;
                    foreach (var obj in listObjClear)
                    {
                        if (obj.id == target1) //Find
                        {
                            objTarget1 = obj;
                            find = true;
                            break;
                        }
                    }
                    if (!find) //Not find not in purpose
                    {
                        objTarget1 = new ObjectSettings();
                        objTarget1.idNotFind = true;
                        objTarget1.id = target1;
                    }
                    //objTarget1 = listObjClear.Where(s => s.id == target1).FirstOrDefault();
                }
            }
            public void SetFileContentIDs(string h2, string t2, List<ObjectSettings> listObjClear)
            {
                int hand2, target2;
                int.TryParse(h2, out hand2);
                int.TryParse(t2, out target2);
                if (hand2 <= 0) //Not find in purpose
                {
                    objHand2 = new ObjectSettings();
                    objHand2.isNULL = true;
                }
                else
                {
                    bool find = false;
                    foreach (var obj in listObjClear)
                    {
                        if (obj.id == hand2) //Find
                        {
                            objHand2 = obj;
                            find = true;
                            break;
                        }
                    }
                    if (!find) //Not find not in purpose
                    {
                        objHand2 = new ObjectSettings();
                        objHand2.idNotFind = true;
                        objHand2.id = hand2;
                    }
                    //objHand2 = listObjClear.Where(s => s.id == hand2).FirstOrDefault();
                }
                if (target2 <= 0)
                {
                    objTarget2 = new ObjectSettings();
                    objTarget2.isNULL = true;
                }
                else
                {
                    bool find = false;
                    foreach (var obj in listObjClear)
                    {
                        if (obj.id == target2) //Find
                        {
                            objTarget2 = obj;
                            find = true;
                            break;
                        }
                    }
                    if (!find) //Not find not in purpose
                    {
                        objTarget2 = new ObjectSettings();
                        objTarget2.idNotFind = true;
                        objTarget2.id = target2;
                    }
                    //objTarget2 = listObjClear.Where(s => s.id == target2).FirstOrDefault();
                }
            }
            public ObjectSettings? GetObjectSettings(int index)
            {
                if (index == 0)
                    return objHand1;
                if (index == 2)
                    return objTarget1;
                if (index == 4)
                    return objHand2;
                if (index == 6)
                    return objTarget2;
                return null;
            }

        }

        public class CustomObjectListView : ObjectListView
        {

            List<ListViewItem> list_Selected = new List<ListViewItem>();
            public PictureBox pictureBox;
            public Label labelName;
            public Label labelSubName;
            public Label labelID;
            public List<ObjectSettings> listObj;
            public int x;
            public int y;

            public Form1 form1;
            public bool export;

            protected override void OnCellClick(CellClickEventArgs args) // Called after WndProc
            {
                base.OnCellClick(args);
                UpdateSelectedOne();
                UpdateOtherListView();
            }
            protected override bool HandleMouseMove(ref Message m)
            {
                if (MouseButtons == MouseButtons.Left)
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var item = HitTest(x, y).Item;
                    if (item == null) return base.HandleMouseMove(ref m);
                    if (!item.Selected)
                    {
                        SelectItem(item);
                    }
                }
                else if (MouseButtons == MouseButtons.Right)
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var item = HitTest(x, y).Item;
                    if (item == null) return base.HandleMouseMove(ref m);
                    if (item.Selected)
                    {
                        DeselectItem(item);
                    }
                }
                return base.HandleMouseMove(ref m);
            }
            protected override bool HandleLButtonDown(ref Message m)
            {
                int x = m.LParam.ToInt32() & 0xFFFF;
                int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                var item = HitTest(x, y).Item;

                SelectItem(item);

                return false;//base.HandleLButtonDown(ref m);
            }
            protected override bool HandleRButtonDown(ref Message m)
            {
                int x = m.LParam.ToInt32() & 0xFFFF;
                int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                var item = HitTest(x, y).Item;
                DeselectItem(item);

                return false;//base.HandleRButtonDown(ref m);
            }

            protected override void WndProc(ref Message m)
            {
                //Use to force update selection in last
                switch (m.Msg)
                {
                    case 516: //Down Right Click
                        //base.WndProc(ref m);
                        DeselectItem(HitTest(m.LParam.ToInt32() & 0xFFFF, (m.LParam.ToInt32() >> 16) & 0xFFFF).Item);
                        break;
                    case 517: //Up right click
                        UpdateSelectedOne();
                        UpdateOtherListView();
                        break;
                    /*case 514: //Up left click
                        UpdateSelectedOne();
                        UpdateTransitions();
                        break;*/
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            public void RefreshSelectedList()
            {
                list_Selected.Clear();
                foreach (ListViewItem item in SelectedItems)
                {
                    list_Selected.Add(item);
                }
            }
            public void UpdateSelectedOne()
            {
                foreach (var ite in list_Selected)
                {
                    //ite.Checked = true;
                    ite.Selected = true;
                }
            }
            private void SelectItem(ListViewItem item)
            {
                if (item != null)
                {
                    if (!item.Selected)
                    {
                        item.Selected = true;
                        //item.Checked = true;
                        if (!list_Selected.Contains(item)) list_Selected.Add(item);
                        UpdateSelectedOne();
                    }
                }

            }
            private void DeselectItem(ListViewItem item)
            {
                if (item != null)
                {
                    if (item.Selected)
                    {
                        item.Selected = false;
                        //item.Checked = false;
                        list_Selected.Remove(item);
                        UpdateSelectedOne();
                    }
                }

            }

            private int listCount; // used to see if the list count change
            private void UpdateOtherListView()
            {
                if (list_Selected.Count == listCount) return;
                listCount = list_Selected.Count;

                if (export)
                {
                    form1.e_UpdateSelectedListView();
                    form1.e_UpdateTransitions();

                }
                else
                {

                }
            }
            protected override void OnItemMouseHover(ListViewItemMouseHoverEventArgs e)
            {
                if (listObj[e.Item.Index] == null) return;
                string[] sName = listObj[e.Item.Index].fullName.Split('-');
                if (sName.Length > 0) labelName.Text = sName[0];
                if (sName.Length > 1) labelSubName.Text = sName[1]; else labelSubName.Text = "";
                labelID.Text = listObj[e.Item.Index].id.ToString();
                Bitmap image = listObj[e.Item.Index].objBmp;
                if (image.Size.Width > pictureBox.Width || image.Size.Height > pictureBox.Height)
                {
                    ImageList tmp = new ImageList();
                    tmp.Images.Add(image);
                    tmp.ImageSize = new Size(pictureBox.Width, pictureBox.Height);
                    if (image != null) pictureBox.Image = image;//tmp.Images[0];
                    pictureBox.Image = tmp.Images[0];
                }
                else
                {
                    pictureBox.Image = image;//tmp.Images[0];
                }
                base.OnItemMouseHover(e);
            }

        }
        public class CustomTransitionsListView : ObjectListView
        {
            public List<ListViewItem> list_Selected = new List<ListViewItem>();
            public Label? labelName;
            public Label? labelSubName;
            public Label? labelID;
            public Panel? panelOverview;
            public CheckBox? checkBox_TransOverview;
            public List<ObjectSettings>? listObj;
            public int x;
            public int y;

            public Form1? form1;
            public bool export;

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                UpdateTransitionsLabel();
            }
            protected override void OnCellClick(CellClickEventArgs args)
            {
                base.OnCellClick(args);
                UpdateSelectedOne();
                UpdateTransitionsLabel();
            }
            private int? actualRowHover;
            private int? actualColumnHover;
            protected override bool HandleMouseMove(ref Message m)
            {
                if (MouseButtons == MouseButtons.Left)
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var item = OlvHitTest(x, y).Item;
                    if (item != null && !item.Selected)
                    {
                        Transition t = (Transition)OlvHitTest(x, y).RowObject;
                        if (t.willAnyCauseProb || t.haveAnyIDMiss) //TODO: Check if we cant do better
                            return false;
                        SelectItem(item);
                    }
                }
                else if (MouseButtons == MouseButtons.Right)
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var item = OlvHitTest(x, y).Item;
                    if (item != null && item.Selected)
                    {
                        DeselectItem(item);
                    }
                }
                else // When its just Hover
                {
                    if (!checkBox_TransOverview.Checked) return base.HandleMouseMove(ref m);
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                    var hitInfo = OlvHitTest(x, y);

                    if (hitInfo.SubItem == null || (hitInfo.ColumnIndex == actualColumnHover && actualRowHover == hitInfo.RowIndex)) // if we are on same cell
                        return base.HandleMouseMove(ref m);

                    if (hitInfo.ColumnIndex == 1 || hitInfo.ColumnIndex == 3 || hitInfo.ColumnIndex == 5) // column for + and >
                    {
                        panelOverview.Visible = false;
                        return base.HandleMouseMove(ref m);
                    }
                    actualRowHover = hitInfo.RowIndex;
                    actualColumnHover = hitInfo.ColumnIndex;
                    ObjectSettings? obj = ((Transition)hitInfo.RowObject).GetObjectSettings(hitInfo.ColumnIndex);

                    if (obj == null || obj.isNULL)
                    {
                        panelOverview.Visible = false;
                        return base.HandleMouseMove(ref m);
                    }

                    //int xitem = hitInfo.SubItem.Bounds.Location.X;
                    //int yitem = hitInfo.SubItem.Bounds.Location.Y;
                    int xmove = hitInfo.SubItem.Bounds.Location.X - panelOverview.Width / 2 + hitInfo.SubItem.Bounds.Width / 2 + Parent.Left;
                    int ymove = hitInfo.SubItem.Bounds.Location.Y - panelOverview.Height + Parent.Top;
                    panelOverview.Location = new Point(xmove, ymove); //*NM
                    panelOverview.Visible = true;
                    if (obj.idNotFind)
                    {
                        labelName.ForeColor = Color.Crimson; labelSubName.ForeColor = Color.Crimson; labelID.ForeColor = Color.Crimson;
                        labelName.Text = "ID Missing";
                        labelSubName.Text = "Couldnt find Object with same ID";
                        labelID.Text = obj.id.ToString();
                    }
                    else
                    {
                        labelName.ForeColor = Color.Black; labelSubName.ForeColor = Color.Black; labelID.ForeColor = Color.Black;
                        string[] sName = obj.fullName.Split('-'); //*NM
                        if (sName.Length > 0) labelName.Text = sName[0];
                        if (sName.Length > 1) labelSubName.Text = sName[1]; else labelSubName.Text = "";
                        labelID.Text = obj.id.ToString();
                    }

                }

                return base.HandleMouseMove(ref m);
            }
            protected override bool HandleLButtonDown(ref Message m)
            {
                int x = m.LParam.ToInt32() & 0xFFFF;
                int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                //var item = HitTest(x, y).Item;
                var item = OlvHitTest(x, y).Item;
                panelOverview.Visible = false;
                if (item != null && !item.Selected)
                {
                    Transition t = (Transition)OlvHitTest(x, y).RowObject;
                    if (t.willAnyCauseProb || t.haveAnyIDMiss)
                        return true; // true mean that break
                    SelectItem(item);
                }
                return false;
            }
            protected override bool HandleRButtonDown(ref Message m)
            {
                int x = m.LParam.ToInt32() & 0xFFFF;
                int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
                //var item = HitTest(x, y).Item;
                var item = OlvHitTest(x, y).Item;
                panelOverview.Visible = false;
                Debug.WriteLine("HANDLE R BUTTON");
                if (item != null && item.Selected)
                {
                    DeselectItem(item);
                }
                return false;//base.HandleRButtonDown(ref m);
            }

            protected override void WndProc(ref Message m)
            {
                //Use to force update selection in last
                switch (m.Msg)
                {
                    case 516: //Down Right Click
                        var item = OlvHitTest(m.LParam.ToInt32() & 0xFFFF, (m.LParam.ToInt32() >> 16) & 0xFFFF).Item;
                        panelOverview.Visible = false;
                        if (item != null && item.Selected)
                        {
                            DeselectItem(item);
                        }
                        break;
                    case 517: //Up right click
                        UpdateSelectedOne();
                        UpdateTransitionsLabel();
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            public void UpdateSelectedOne()
            {
                //if (list_Selected == null) return;
                foreach (var ite in list_Selected)
                {
                    ite.Selected = true;
                }
            }
            private void SelectItem(OLVListItem item)
            {

                item.Selected = true;

                if (list_Selected != null && !list_Selected.Contains(item))
                {
                    Debug.WriteLine("SELECT");
                    Debug.WriteLine("Has deco : " + item.HasDecoration);
                    list_Selected.Add(item);
                    if (item.HasDecoration) form1.i_transList_AlreadyExist_Selected.Add((Transition)item.RowObject);
                }
                UpdateSelectedOne();
            }
            private void DeselectItem(OLVListItem item)
            {
                item.Selected = false;
                if (list_Selected != null)
                {
                    Debug.WriteLine("DESELECT");
                    Debug.WriteLine("Has deco : " + item.HasDecoration);
                    list_Selected.Remove(item);
                    if (item.HasDecoration) form1.i_transList_AlreadyExist_Selected.Remove((Transition)item.RowObject);
                }
                UpdateSelectedOne();
            }

            private int listCount;
            public void UpdateTransitionsLabel()
            {
                if (list_Selected == null) return;
                if (list_Selected.Count == listCount) return;
                listCount = list_Selected.Count;
                if (export)
                {
                    form1?.e_SetLabelsTransitions(list_Selected.Count);

                }
                else
                {
                    form1?.i_UpdateLabelsTransitions();
                }
            }
            public void i_SelectAllGoodTransitions()
            {
                list_Selected?.Clear();
                for (int i = 0; i < Items.Count; i++)
                {
                    Transition t = (Transition)GetModelObject(i);
                    if (!t.pathAlreadyExist && !t.haveAnyIDMiss)
                    {
                        Items[i].Selected = true;
                        list_Selected?.Add(Items[i]);
                    }
                }
            }
            public void e_SelectAllGoodTransitions()
            {
                list_Selected?.Clear();
                for (int i = 0; i < Items.Count; i++)
                {
                    Transition t = (Transition)GetModelObject(i);
                    if (!t.willAnyCauseProb)
                    {
                        Items[i].Selected = true;
                        list_Selected?.Add(Items[i]);
                    }
                }
            }


        }
        public class ViewListObject : ObjectListView
        {
            protected override void WndProc(ref Message m)
            {
                //Use to force no selection
                switch (m.Msg)
                {
                    case 513:
                        //base.WndProc(ref m);
                        break;
                    case 516:
                        //base.WndProc(ref m);
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                string text = "";
                if (Settings.Default.list_RepoTagName.Count != Settings.Default.list_Old_NextObjNum.Count)
                {
                    text = "list_RepoTagName & list_Old_NextObjNum have dif. count";
                }
                else
                {
                    for (int i = 0; i < Settings.Default.list_RepoTagName.Count; i++)
                    {
                        text += Settings.Default.list_RepoTagName[i] + " => " + Settings.Default.list_Old_NextObjNum[i] + "\r\n";
                    }
                }
                textBox1.Text = text;
                panel17.Visible = true;
            }
            else panel17.Visible = false;
        }

        private void i_checkBox_DeleteOldFiles_CheckedChanged_1(object sender, EventArgs e)
        {
            Settings.Default.i_DeleteAllFile = i_checkBox_DeleteOldFiles.Checked;
            Settings.Default.Save();
        }
    }
}
