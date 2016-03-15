using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



//新增的命名空間
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Runtime.InteropServices;
using System.Collections;

namespace Twitter_Website_mining
{   
    public partial class Form1 : Form
    {
        public Form1() { InitializeComponent();}

//讀取csv檔案變數---------------------------------------------------------------------

        //存放csv檔案資料
        List<string> data = new List<string>();

        //計算csv行數
        int count = 0;

        //存放主使用者收藏篇數
        int main_userhave;

        //推薦的所有網站
        List<string> suggestWebsite = new List<string>();
        List<double> suggestPower = new List<double>();
        List<string> suggestRelatedUser = new List<string>();
        List<string> suggestRelatedTag = new List<string>();
        List<string> UserRelatedTag = new List<string>();
        //存放Twitter網頁
        List<string> twitterpage = new List<string>();
        List<string>[] twitterpageArray;

        //存放短網址
        List<string> shorturl = new List<string>();

        //存放網址
        List<string> website = new List<string>();
        List<string>[] websiteArray;

        //Tag標籤
        List<string>[] tagArray;

        //存放網站類別
        List<string> category = new List<string>();

        //存放網站標題
        List<string> title = new List<string>();

        //存放網站關鍵字
        List<string> web_keyword = new List<string>();

        //存放使用者關鍵字
        List<string> user_keyword = new List<string>();

        //存放推薦比分
        List<string> compare = new List<string>();

        //存放 Page-Rank 網站比分
        List<string> pagerank = new List<string>();

        //存放 Sub-Page-Rank 網站比分
        List<string> subpagerank = new List<string>();

        //存放 youtube觀看數 網站比分
        List<string> youtubewatched = new List<string>();

        //存放 youtube喜歡人數 網站比分
        List<string> youtubelike = new List<string>();

        //存放 youtube不喜歡人數 網站比分
        List<string> youtubeunlike = new List<string>();

        //存放 facebook分享 網站比分
        List<string> facebookshare = new List<string>();

        //存放 facebook讚 網站比分
        List<string> facebooklike = new List<string>();

        //存放 facebook觀看數 網站比分
        List<string> facebookwatched = new List<string>();

        //存放 關鍵字字典
        List<string>[] wordDictionary = new List<string>[5];

        //存放字典關鍵字頻率
        List<int>[] wordFeqDictionary = new List<int>[5];

//-----以下為第一種推薦所使用的變數--------------------------
          
        //存放使用者名稱
        List<string> username = new List<string>();

        //存放其他使用者與主使用者共同次數
        List<int> usercount = new List<int>();

        //存放其他使用者的自身收藏篇數
        List<double> userhave = new List<double>();

        //存放主使用者與其他使用者相似度
        List<double> similarity = new List<double>();

        //存放主使用者自身關注
        List<string> Author = new List<string>();

        //存放其他使用者所收集的URL網址
        List<string> others_website = new List<string>();

        //存放權重來源使用者名稱
        List<string> others_comefrom = new List<string>();

        //存放推薦權重
        List<double> Url_compare = new List<double>();

//-----以下為第二種推薦所使用的變數--------------------------

        //存放網站的收藏者
        List<string> web_user = new List<string>();

        //存放網站的收藏者比分
        List<double> web_user_compare = new List<double>();
        
//---------------------------------------------------------------------讀取csv檔案變數

//通用函式----------------------------------------------------------------------------
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public const int WM_CLOSE = 0x10;
        private void StartKiller()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000; //3秒啓動
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            KillMessageBox();
            //停止Timer
            ((System.Windows.Forms.Timer)sender).Stop();
        }
        private void KillMessageBox()
        {
            //依MessageBox的標題,找出MessageBox的視窗
            IntPtr ptr = FindWindow(null, "指令碼錯誤");
            IntPtr ptr2 = FindWindow(null, "MessageBox");
            if (ptr != IntPtr.Zero)
            {
                //找到則關閉MessageBox視窗
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            if (ptr2 != IntPtr.Zero)
            {
                //找到則關閉MessageBox視窗
                PostMessage(ptr2, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }
        //取得網頁原始碼
        private string GetHTMLSourceCode(string url)
        {
            try
            {
                HttpWebRequest request = (WebRequest.Create(url)) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception err)
            {
                return null;
            }
        }
        //取得網頁標題
        private string UrlTitle(string url)
        {
            try
            {
                string source = GetHTMLSourceCode(url);
                string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                return title;
            }
            catch (Exception err)
            {
                return null;
            }
        }
        //取得網頁描述
        private string GetMetaTagValuesUtf8(string url)
        {
            try
            {
                string src = string.Empty;
                try
                {
                    src = GetHTMLSourceCode(url);
                }
                catch
                {
                    return "cannot connect";
                }
                string tempSrc = src.Replace("<div", "|<div");
                tempSrc.Replace(tempSrc, " ");
                tempSrc = tempSrc.Trim();

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(tempSrc.Replace("<", " <"));
                try
                {
                    var metaTags = doc.DocumentNode.SelectNodes("//meta");

                    if (metaTags != null)
                    {
                        foreach (var tag in metaTags)
                        {
                            if (((tag.Attributes["name"] != null) ||
                                 (tag.Attributes["Name"] != null) ||
                                 (tag.Attributes["NAME"] != null)) &&
                                ((tag.Attributes["content"] != null) ||
                                 (tag.Attributes["Content"] != null) ||
                                 (tag.Attributes["CONTENT"] != null)) &&
                                ((tag.Attributes["name"].Value == "description") ||
                                 (tag.Attributes["name"].Value == "Description") ||
                                 (tag.Attributes["name"].Value == "DESCRIPTION") ||
                                 (tag.Attributes["Name"].Value == "description") ||
                                 (tag.Attributes["Name"].Value == "Description") ||
                                 (tag.Attributes["Name"].Value == "DESCRIPTION") ||
                                 (tag.Attributes["NAME"].Value == "description") ||
                                 (tag.Attributes["NAME"].Value == "Description") ||
                                 (tag.Attributes["NAME"].Value == "DESCRIPTION"))
                                )
                            {
                                string temp = tag.Attributes["content"].Value;
                                if (temp == null) temp = tag.Attributes["Content"].Value;
                                if (temp == null) temp = tag.Attributes["CONTENT"].Value;
                                return temp;
                            }
                            else if (((tag.Attributes["property"] != null) ||
                                     (tag.Attributes["Property"] != null) ||
                                     (tag.Attributes["PPOPERTY"] != null)) &&
                                    ((tag.Attributes["content"] != null) ||
                                     (tag.Attributes["Content"] != null) ||
                                     (tag.Attributes["CONTENT"] != null)) &&
                                    ((tag.Attributes["property"].Value == "og:description") ||
                                     (tag.Attributes["Property"].Value == "og:description") ||
                                     (tag.Attributes["PROPERTY"].Value == "og:description"))
                                    )
                            {
                                string temp = tag.Attributes["content"].Value;
                                if (temp == null) temp = tag.Attributes["Content"].Value;
                                if (temp == null) temp = tag.Attributes["CONTENT"].Value;
                                return temp;
                            }
                        }
                    }
                }
                catch
                {
                    return "null";
                }
            }
            catch
            {
                return "null";
            }
            return "null";
        }
        //取得網頁圖片
        private string GetMetaTagImageUtf8(string url)
        {
            try
            {
                string src = string.Empty;
                try
                {
                    src = GetHTMLSourceCode(url);
                }
                catch
                {
                    return "cannot connect";
                }
                string tempSrc = src.Replace("<div", "|<div");
                tempSrc.Replace(tempSrc, " ");
                tempSrc = tempSrc.Trim();

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(tempSrc.Replace("<", " <"));
                try
                {
                    var metaTags = doc.DocumentNode.SelectNodes("//meta");

                    if (metaTags != null)
                    {
                        foreach (var tag in metaTags)
                        {
                            if (((tag.Attributes["name"] != null) ||
                                 (tag.Attributes["Name"] != null) ||
                                 (tag.Attributes["NAME"] != null)) &&
                                ((tag.Attributes["content"] != null) ||
                                 (tag.Attributes["Content"] != null) ||
                                 (tag.Attributes["CONTENT"] != null)) &&
                                ((tag.Attributes["name"].Value == "description") ||
                                 (tag.Attributes["name"].Value == "Description") ||
                                 (tag.Attributes["name"].Value == "DESCRIPTION") ||
                                 (tag.Attributes["Name"].Value == "description") ||
                                 (tag.Attributes["Name"].Value == "Description") ||
                                 (tag.Attributes["Name"].Value == "DESCRIPTION") ||
                                 (tag.Attributes["NAME"].Value == "description") ||
                                 (tag.Attributes["NAME"].Value == "Description") ||
                                 (tag.Attributes["NAME"].Value == "DESCRIPTION")))
                            {
                                string temp = tag.Attributes["content"].Value;
                                if (temp == null) temp = tag.Attributes["Content"].Value;
                                if (temp == null) temp = tag.Attributes["CONTENT"].Value;
                                return temp;
                            }
                        }
                    }
                }
                catch
                {
                    return "null";
                }
            }
            catch
            {
                return "null";
            }
            return "null";
        }
        
//-----------------------------------------------------------------------------通用函式        

        private void 使用者ToolStripMenuItem_Click(object sender, EventArgs e) { }

        private void 網站ToolStripMenuItem_Click(object sender, EventArgs e) { }

        //程式載入時排版
        private void Form1_Load(object sender, EventArgs e)
        {
            if(easyDisplayToolStripMenuItem.Checked == true)
            {
                splitterAnalysis.Visible = false;
                panelAnalysis.Visible = false;
                panelEasyDisplay.Visible = true;
                panelAnalysis.Size = new System.Drawing.Size(0, 0);
                panelDisplayStart.Size = new System.Drawing.Size(0, 0);
                splitterCenterLeft.Size = new System.Drawing.Size(0, 0);
                splitterCenterRight.Size = new System.Drawing.Size(0, 0);
                //隱藏資料顯示
                
                //調整左邊EasyDisplay
                int Wid = panelEasyDisplay.Width;
                //改成點擊StartTitle顯示
                
                //panelMainLeft.Size = new System.Drawing.Size(0, 0);
                //panelDisplayStart.Size = new System.Drawing.Size(this.Width, 0);
                panelMainLeft.Size = new System.Drawing.Size(Wid / 2, 0);
                panelLeft.Size = new System.Drawing.Size(panelMainLeft.Width - 21, 0);
                panelLeft1.Size = new System.Drawing.Size(Wid - 42, 0);
                panelLeft2.Size = new System.Drawing.Size(Wid - 42, 0);
                //panelLeft2.Size = new System.Drawing.Size(Wid, 0);
                //調整右邊EasyDisplay
                panelMainRight.Size = new System.Drawing.Size(Wid / 2, 0);
                panelRight.Size = new System.Drawing.Size(panelMainRight.Width - 21, 0);
                panelRight1.Size = new System.Drawing.Size(Wid - 42, 0);
                panelRight2.Size = new System.Drawing.Size(Wid - 42, 0);
                panelRight3.Size = new System.Drawing.Size(Wid - 42, 0);

            }
            else
            {
                splitterAnalysis.Visible = false;
                panelAnalysis.Visible = true;
                panelEasyDisplay.Visible = false;
                panelAnalysis.Size = new System.Drawing.Size(this.Width, this.Height);
            }
        }
//載入網址Button1-----------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IDataObject data = Clipboard.GetDataObject();
                if (tabControl1.SelectedIndex == 0)
                {
                    string cellUrl = Convert.ToString(dataGridView1.SelectedCells[0].Value);
                    if (cellUrl.Contains("http"))
                    {
                        webBrowser1.Url = new Uri(cellUrl);
                    }
                }
                else if (data.GetDataPresent(DataFormats.Text))
                {
                    string cellUrl = Convert.ToString(data.GetData(DataFormats.Text));
                    if (cellUrl.Contains("http"))
                    {
                        webBrowser1.Url = new Uri(cellUrl);
                    }
                }
            }
            catch (Exception err){ }
        }
//-----------------------------------------------------------------------------載入網址

//顯示剩餘資料-----------------------------------------------------------------------------
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (web_keyword.Count > 0)
            {
                label11.Text = web_keyword[e.RowIndex];
                label12.Text = user_keyword[e.RowIndex];
                label13.Text = compare[e.RowIndex];
                label17.Text = title[e.RowIndex];
            }
        }
//-----------------------------------------------------------------------------顯示剩餘資料

//讀取網站按鈕-----------------------------------------------------------------------------------
        private void 網站ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string line;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                //清空資訊
                ToClearAll(sender, e);

                //讀取網站並放入data
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                label17.Text = file.ReadLine().Replace(",","");
                label10.Text = file.ReadLine().Replace(",", "");
                string cellUrl = Convert.ToString(label10.Text);
                webBrowser1.Url = new Uri(cellUrl);

                while ((line = file.ReadLine()) != null)
                {
                    data.Add(line);
                    count++;
                }
                file.Close();

                dataGridView2.RowCount = count;
                dataGridView2.ColumnCount = 2;
                for (int i = 0; i < count; i++)
                {
                    string[] cut = data[i].Split(',');

                    username.Add(cut[0]);
                    compare.Add(cut[1]);
                    web_keyword.Add(cut[2]);
                    user_keyword.Add(cut[3]);

                    dataGridView2.Rows[i].Cells[0].Value = i + 1;
                    dataGridView2.Rows[i].Cells[1].Value = username[i];
                }
            }
        }
//-----------------------------------------------------------------------------------讀取網站按鈕

//顯示剩餘資料-----------------------------------------------------------------------------------

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(web_keyword.Count>10){
            label14.Text = web_keyword[e.RowIndex];
            label15.Text = user_keyword[e.RowIndex];
            label16.Text = compare[e.RowIndex];}
        }
//-----------------------------------------------------------------------------------顯示剩餘資料


//讀取使用者--------------------------------------------------------------------------

        private void 使用者ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string line;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //清空資訊
                ToClearAll(sender, e);

                //讀取使用者並放入data
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                label10.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                file.ReadLine();
                while ((line = file.ReadLine()) != null)
                {
                    data.Add(line);
                    count++;
                }
                file.Close();
                
                //設定datagridview並存放資料到各list陣列
                dataGridView1.RowCount = count;
                dataGridView1.ColumnCount = 20;
                for (int i = 0; i < count; i++)
                {
                    string[] cut = data[i].Split(',');

                    twitterpage.Add(cut[0]);
                    shorturl.Add(cut[1]);
                    website.Add(cut[2]);
                    category.Add(cut[3]);
                    title.Add(cut[4]);
                    web_keyword.Add(cut[5]);
                    user_keyword.Add(cut[6]);
                    compare.Add(cut[7]);
                    pagerank.Add(cut[8]);
                    subpagerank.Add(cut[9]);
                    youtubewatched.Add(cut[10]);
                    youtubelike.Add(cut[11]);
                    youtubeunlike.Add(cut[12]);
                    facebookshare.Add(cut[13]);
                    facebooklike.Add(cut[14]);
                    facebookwatched.Add(cut[15]);

                    dataGridView1.Rows[i].Cells[0].Value = i + 1;
                    dataGridView1.Rows[i].Cells[1].Value = twitterpage[i];
                    dataGridView1.Rows[i].Cells[2].Value = shorturl[i];
                    dataGridView1.Rows[i].Cells[3].Value = website[i];
                    dataGridView1.Rows[i].Cells[4].Value = category[i];
                    dataGridView1.Rows[i].Cells[5].Value = title[i];
                    dataGridView1.Rows[i].Cells[6].Value = web_keyword[i];
                    dataGridView1.Rows[i].Cells[7].Value = user_keyword[i];
                    dataGridView1.Rows[i].Cells[8].Value = compare[i];
                    dataGridView1.Rows[i].Cells[9].Value = pagerank[i];
                    dataGridView1.Rows[i].Cells[10].Value = subpagerank[i];
                    dataGridView1.Rows[i].Cells[11].Value = youtubewatched[i];
                    dataGridView1.Rows[i].Cells[12].Value = youtubelike[i];
                    dataGridView1.Rows[i].Cells[13].Value = youtubeunlike[i];
                    dataGridView1.Rows[i].Cells[14].Value = facebookshare[i];
                    dataGridView1.Rows[i].Cells[15].Value = facebooklike[i];
                    dataGridView1.Rows[i].Cells[16].Value = facebookwatched[i];

                    if (checkBox1.Checked)
                    {
                        tabControl1.TabPages[2].Controls.Add(new LinkLabel() { Text = website[i], Location = new Point(10, 32 * i + 10 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.Gray });
                        tabControl1.TabPages[2].Controls.Add(new Label() { Text = UrlTitle(website[i]), Location = new Point(10, 32 * i + 25 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.SlateGray });
                        tabControl1.TabPages[2].Controls.Add(new Label() { Text = GetMetaTagValuesUtf8(website[i]), Location = new Point(10, 32 * i + 40 + 64 * i), Size = new Size(tabControl1.Width - 32, 40), BackColor = Color.LightGray });

                    }
                }  
            }
        }
//--------------------------------------------------------------------------讀取使用者
        

//清空資訊-----------------------------------------------------------------------------
        private void ToClearAll(object sender, EventArgs e){
                count = 0;
                data.Clear();
                twitterpage.Clear();
                shorturl.Clear();
                website.Clear();
                category.Clear();
                title.Clear();
                web_keyword.Clear();
                user_keyword.Clear();
                compare.Clear();
                pagerank.Clear();
                subpagerank.Clear();
                youtubewatched.Clear();
                youtubelike.Clear();
                youtubeunlike.Clear();
                facebookshare.Clear();
                facebooklike.Clear();
                facebookwatched.Clear();
                username.Clear();
                dataGridView1.Rows.Clear();
                }
//讀取使用者收藏清單--------------------------------------------------------------------------
//-----------------------------------------------------------------------------清空資訊
        private void 使用者收藏清單ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string line;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //清空資訊
                ToClearAll(sender,e);

                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);

                //讀取第一行、主使用者收藏篇數
                line = file.ReadLine().Replace(",", "");
                main_userhave = System.Convert.ToInt32(line);

                //讀取收藏清單並放入data
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("切割資料用")) { break; }
                    else                            { data.Add(line);   count++;}
                    //此count用以紀錄切割資料用的位子
                }

                //讀取共同收藏次數資料
                while ((line = file.ReadLine()) != null) data.Add(line);

                file.Close();
                //收藏清單存放
                for (int i = 0; i < count; i++)
                {
                    string[] cut = data[i].Split(',');
                    twitterpage.Add(cut[0]);
                    website.Add(cut[1]);

                    int check = 0;
                    for (int j = 0; j < Author.Count; j++)
                    {
                        if (cut[2] == Author[j])
                        { check = 1; break; }
                    }

                    if (check == 0)
                    { 
                        Author.Add(cut[2]);
                        listBox1.Items.Add(cut[2]);
                    }
                }
                

                //共同收藏次數存放
                for (int i = count; i < data.Count; i++)
                {
                    string[] cut = data[i].Split(',');
                    username.Add(cut[0]);
                    usercount.Add(System.Convert.ToInt32(cut[1]));
                    userhave.Add(System.Convert.ToInt32(cut[2]));
                    similarity.Add(0);
                }
                
                //收藏清單放入datagridview3
                dataGridView3.RowCount = twitterpage.Count ;
                dataGridView3.ColumnCount = 3;
                dataGridView3.Columns[0].HeaderText = "編號";
                dataGridView3.Columns[1].HeaderText = "Twitter網址";
                dataGridView3.Columns[2].HeaderText = "URL";
                for (int i = 0; i < twitterpage.Count; i++)
                {
                    dataGridView3.Rows[i].Cells[0].Value = i+1;
                    dataGridView3.Rows[i].Cells[1].Value = twitterpage[i];
                    dataGridView3.Rows[i].Cells[2].Value = website[i];
                }


                //計算相似度
                for (int i = 0 ; i < username.Count; i++)
                {
                    similarity[i] = usercount[i] / ( (Math.Sqrt(userhave[i])) * (Math.Sqrt(main_userhave)) );
                }

                //排序
                for (int i = 0; i < username.Count; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (similarity[i] > similarity[j])
                        {

                            double just_now = similarity[i];
                            similarity[i] = similarity[j];
                            similarity[j] = just_now;

                            just_now = userhave[i];
                            userhave[i] = userhave[j];
                            userhave[j] = just_now;

                            int just_now_2 = usercount[i];
                            usercount[i] = usercount[j];
                            usercount[j] = just_now_2;

                            string just_now_3 = username[i];
                            username[i] = username[j];
                            username[j] = just_now_3;
                        }
                    }
                }

                //相似度放入datagridview4
                dataGridView4.RowCount = username.Count;
                dataGridView4.ColumnCount = 4;
                dataGridView4.Columns[0].HeaderText = "使用者名稱";
                dataGridView4.Columns[1].HeaderText = "相似度";
                dataGridView4.Columns[2].HeaderText = "共同篇數";
                dataGridView4.Columns[3].HeaderText = "自身篇數";
                for (int i = 0; i < username.Count; i++)
                {
                    dataGridView4.Rows[i].Cells[0].Value = username[i];
                    dataGridView4.Rows[i].Cells[1].Value = similarity[i];
                    dataGridView4.Rows[i].Cells[2].Value = usercount[i];
                    dataGridView4.Rows[i].Cells[3].Value = userhave[i];
                    chart1.Series["相似度"].Points.AddXY(username[i], similarity[i]);
                }
            }
        }
//--------------------------------------------------------------------------讀取使用者收藏清單

//讀取其他使用者收藏清單----------------------------------------------------------------------
        private void 其他使用者收藏清單ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            data.Clear();
            string line;
            int check;
            int already_look = 0;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                //篩掉標頭
                file.ReadLine();
                while ((line = file.ReadLine()) != null)  data.Add(line);
                file.Close();
                for (int i = 0; i < data.Count; i++)
                {
                    check = 0;
                    already_look = 0;
                    string[] cut = data[i].Split(',');
                    //先檢查是否有URL
                    if (cut[2] == "") {}
                    else
                    {
                        //檢查此網址是否為本身關注
                        for (int l = 0; l < Author.Count; l++)
                            if (cut[1].Contains(Author[l])) { already_look = 1; break; }

                        if (already_look == 0)
                        {
                            for (int j = 0; j < username.Count; j++)
                            {
                                //檢查此使用者是否有在username的群集內
                                if (cut[0] == username[j])
                                {
                                    for (int k = 0; k < others_website.Count; k++)
                                    {
                                        //檢查網址是否已經被儲存、若有，則權重疊加、來源疊加
                                        if (others_website[k] == cut[2])
                                        {
                                            others_comefrom[k] += "、" + cut[0];
                                            Url_compare[k] += similarity[j];
                                            check = 1;
                                            break;
                                        }
                                    }
                                    //若沒有被儲存過、則放入list
                                    if (check == 0)
                                    {
                                        others_website.Add(cut[2]);
                                        others_comefrom.Add(cut[0]);
                                        Url_compare.Add(similarity[j]);
                                    }
                                }
                            }
                        }




                    }
                }
                //設定datagridview並存放資料到各list陣列
                dataGridView1.RowCount = others_website.Count;
                dataGridView1.ColumnCount = 20;

                for (int i = 0; i < others_website.Count; i++)
                    for (int j = 0; j < others_website.Count;j++ )
                    {
                        if (Url_compare[i] > Url_compare[j])
                        {
                            string just = others_website[i];
                            others_website[i] = others_website[j];
                            others_website[j] = just;

                            just = others_comefrom[i];
                            others_comefrom[i] = others_comefrom[j];
                            others_comefrom[j] = just;

                            double just_2 = Url_compare[i];
                            Url_compare[i] = Url_compare[j];
                            Url_compare[j] = just_2;
                        }
                    }
                    dataGridView1.Columns[0].HeaderText = "推薦網站";
                    dataGridView1.Columns[1].HeaderText = "來源使用者";
                    dataGridView1.Columns[2].HeaderText = "推薦權重";
                    dataGridView1.Columns[3].HeaderText = "Page-Rank";
                    dataGridView1.Columns[4].HeaderText = "推薦程度";
                    dataGridView1.Columns[5].HeaderText = "以推薦權重的排序";
                    for (int i = 0; i < others_website.Count; i++)
                    {
                        dataGridView1.Rows[i].Cells[0].Value = others_website[i];
                        dataGridView1.Rows[i].Cells[1].Value = others_comefrom[i];
                        dataGridView1.Rows[i].Cells[2].Value = Url_compare[i];
                        dataGridView1.Rows[i].Cells[5].Value = i + 1;

                        //GUI顯示CheckBox(舊版)
                        if(i<10)
                        if (checkBox1.Checked)
                        {
                            tabControl1.TabPages[2].Controls.Add(new LinkLabel() { Text = others_website[i], Location = new Point(10, 32 * i + 10 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.Gray });
                            tabControl1.TabPages[2].Controls.Add(new Label() { Text = Url_compare[i].ToString() + "( " + others_comefrom[i] +" )", Location = new Point(10, 32 * i + 25 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.LightBlue });
                            tabControl1.TabPages[2].Controls.Add(new Label() { Text = UrlTitle(others_website[i]), Location = new Point(10, 32 * i + 40 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.SlateGray });
                            tabControl1.TabPages[2].Controls.Add(new Label() { Text = GetMetaTagValuesUtf8(others_website[i]), Location = new Point(10, 32 * i + 55 + 64 * i), Size = new Size(tabControl1.Width - 32, 40), BackColor = Color.LightGray });

                        }
                    }
            }
        }
//讀取其他使用者收藏清單----------------------------------------------------------------------

//讀取網站收藏者資料----------------------------------------------------------------------
        private void 網站收藏者資料ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string line;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //清空資訊
                ToClearAll(sender, e);
                int ToNext = 0;
                int webcount = -1;
                int Repeat = 0;
                int countrows = 0;
                
                

                //讀取並做切割
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                while ((line = file.ReadLine()) != null) 
                {
                    if (line.Contains("http://"))
                    {
                        string[] cut = line.Split(',');
                        twitterpage.Add(cut[0]);
                        website.Add(cut[1]);
                        userhave.Add(0);
                        ToNext = 1;
                        webcount++;
                    }
                    else
                    {
                        line = line.Replace(",", "");

                        //檢查名稱集合是否有重複
                        for (int i = 0; i < web_user.Count; i++ )
                        {
                            if (line == web_user[i]) { Repeat = 1; break; }
                        }

                        if (Repeat == 0) { web_user.Add(line); web_user_compare.Add(0); }
                        Repeat = 0;

                        if (ToNext == 1) { username.Add(line); ToNext = 0; }
                        else username[webcount] += "," + line;
                    }
                }
                file.Close();

                //計算網頁之間相似度
                dataGridView5.RowCount = website.Count;
                dataGridView5.ColumnCount = website.Count;



                    for (int i = 0; i < website.Count; i++)
                    {
                        dataGridView5.Columns[i].HeaderText = "web"+ ( i + 1 );
                        dataGridView5.Rows[i].HeaderCell.Value = "web" + (i + 1);
                        string[] cut = username[i].Split(',');
                        for (int j = 0; j < website.Count; j++)
                        {
                            if (i == j) break;

                            userhave[j] = 0;

                            for (int k = 0; k < cut.Length ; k++)
                                if (username[j].Contains(cut[k])) userhave[j]++;

                            dataGridView5.Rows[i].Cells[j].Value = userhave[j] / 25;
                            dataGridView5.Rows[j].Cells[i].Value = userhave[j] / 25;
                        }
                    }

                    dataGridView2.RowCount = website.Count * web_user.Count;
                    dataGridView2.ColumnCount = 3;
                    dataGridView2.Columns[0].HeaderText = "網站連結";
                    dataGridView2.Columns[1].HeaderText = "使用者";
                    dataGridView2.Columns[2].HeaderText = "推薦比較";
                    //針對每一個website推薦使用者
                    for(int i = 0;i<website.Count;i++)
                    {
                        
                        for (int j = 0; j < web_user.Count; j++)
                        {
                            //如果user本身已收藏則捨去推薦
                            if (username[i].Contains(web_user[j])) {}

                            //如果沒有則計算推薦
                            else
                            {
                                for (int k = 0; k < website.Count; k++)
                                {
                                    if (i == k) { }
                                    else if (username[k].Contains(web_user[j]))
                                    {
                                        web_user_compare[j] += Convert.ToDouble(dataGridView5.Rows[i].Cells[k].Value.ToString());
                                    }
                                }
                            }
                        }
                        //排列順序
                        for (int l = 0; l < web_user.Count; l++)
                            for(int m = 0; m <web_user.Count ;m++)
                            {
                                if (web_user_compare[l] > web_user_compare[m]) 
                                {
                                    string just = web_user[l];
                                    web_user[l] = web_user[m];
                                    web_user[m] = just;

                                    double just_2 = web_user_compare[l];
                                    web_user_compare[l] = web_user_compare[m];
                                    web_user_compare[m] = just_2;
                                }
                            }

                    

                        //展示
                        for (int l = 0; l < web_user.Count; l++) 
                        {
                            if (web_user_compare[l] > 0)
                            {
                                dataGridView2.Rows[countrows].HeaderCell.Value = "web" + (i + 1);
                                dataGridView2.Rows[countrows].Cells[0].Value = website[i];
                                dataGridView2.Rows[countrows].Cells[1].Value = web_user[l];
                                dataGridView2.Rows[countrows].Cells[2].Value = web_user_compare[l];
                                countrows++;
                            }
                        }


                        if(i<10)
                        if (checkBox1.Checked)
                        {
                            tabControl1.TabPages[2].Controls.Add(new LinkLabel() { Text = website[i], Location = new Point(10, 32 * i + 10 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.Gray });
                            tabControl1.TabPages[2].Controls.Add(new Label() { Text = web_user[0] + ":  " + web_user_compare[0] + "、" + web_user[1] + ":  " + web_user_compare[1] + "、" + web_user[2] + ":  " + web_user_compare[2], Location = new Point(10, 32 * i + 25 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.LightBlue });
                            tabControl1.TabPages[2].Controls.Add(new Label() { Text = UrlTitle(website[i]), Location = new Point(10, 32 * i + 40 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.SlateGray });
                            tabControl1.TabPages[2].Controls.Add(new Label() { Text = GetMetaTagValuesUtf8(website[i]), Location = new Point(10, 32 * i + 55 + 64 * i), Size = new Size(tabControl1.Width - 32, 40), BackColor = Color.LightGray });

                        }
                        //清空
                        for (int l = 0; l < web_user_compare.Count; l++) web_user_compare[l] = 0;
                }
                //針對每一個website推薦使用者結束
            }
        }
//----------------------------------------------------------------------讀取網站收藏者資料
//讀取網站下的連結----------------------------------------------------------------------
        bool loadingHtml;
        

        private void PrintDocument(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            loadingHtml = false;
        }
        private void 網站下的連結ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string line;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //清空資訊
                ToClearAll(sender, e);

                //讀取使用者並放入data
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                label10.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                file.ReadLine();
                while ((line = file.ReadLine()) != null)
                {
                    data.Add(line);
                    count++;
                }
                file.Close();
                dataGridView1.Rows.Clear();
                //設定datagridview並存放資料到各list陣列
                dataGridView1.RowCount = count;
                dataGridView1.ColumnCount = 20;
                for (int i = 0; i < count; i++)
                {
                    string[] cut = data[i].Split(',');

                    twitterpage.Add(cut[0]);
                    shorturl.Add(cut[1]);
                    website.Add(cut[2]);
                    category.Add(cut[3]);
                    title.Add(cut[4]);
                    web_keyword.Add(cut[5]);
                    user_keyword.Add(cut[6]);
                    compare.Add(cut[7]);
                    pagerank.Add(cut[8]);
                    subpagerank.Add(cut[9]);
                    youtubewatched.Add(cut[10]);
                    youtubelike.Add(cut[11]);
                    youtubeunlike.Add(cut[12]);
                    facebookshare.Add(cut[13]);
                    facebooklike.Add(cut[14]);
                    facebookwatched.Add(cut[15]);

                    dataGridView1.Rows[i].Cells[0].Value = i + 1;
                    dataGridView1.Rows[i].Cells[1].Value = twitterpage[i];
                    dataGridView1.Rows[i].Cells[2].Value = shorturl[i];
                    dataGridView1.Rows[i].Cells[3].Value = website[i];
                    dataGridView1.Rows[i].Cells[4].Value = category[i];
                    dataGridView1.Rows[i].Cells[5].Value = title[i];
                    dataGridView1.Rows[i].Cells[6].Value = web_keyword[i];
                    dataGridView1.Rows[i].Cells[7].Value = user_keyword[i];
                    dataGridView1.Rows[i].Cells[8].Value = compare[i];
                    dataGridView1.Rows[i].Cells[9].Value = pagerank[i];
                    dataGridView1.Rows[i].Cells[10].Value = subpagerank[i];
                    dataGridView1.Rows[i].Cells[11].Value = youtubewatched[i];
                    dataGridView1.Rows[i].Cells[12].Value = youtubelike[i];
                    dataGridView1.Rows[i].Cells[13].Value = youtubeunlike[i];
                    dataGridView1.Rows[i].Cells[14].Value = facebookshare[i];
                    dataGridView1.Rows[i].Cells[15].Value = facebooklike[i];
                    dataGridView1.Rows[i].Cells[16].Value = facebookwatched[i];

                    //GUI介面
                    /*if (checkBox1.Checked)
                    {
                        tabControl1.TabPages[2].Controls.Add(new LinkLabel() { Text = website[i], Location = new Point(10, 32 * i + 10 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.Gray });
                        tabControl1.TabPages[2].Controls.Add(new Label() { Text = UrlTitle(website[i]), Location = new Point(10, 32 * i + 25 + 64 * i), Size = new Size(tabControl1.Width - 32, 15), BackColor = Color.SlateGray });
                        tabControl1.TabPages[2].Controls.Add(new Label() { Text = GetMetaTagValuesUtf8(website[i]), Location = new Point(10, 32 * i + 40 + 64 * i), Size = new Size(tabControl1.Width - 32, 40), BackColor = Color.LightGray });
                    }*/

                }
                dataGridView6.Rows.Clear();
                //抓取網頁下連結
                dataGridView6.RowCount = 10000;
                dataGridView6.ColumnCount = 2;
                WebBrowser browser;
                for (int i = 0, a = 0; i < count; i++)
                {

                    browser = new WebBrowser { Name = "browser" + i.ToString(), ScriptErrorsSuppressed = true };
                    browser.Navigate(website[i]);
                    loadingHtml = true;
                    browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(PrintDocument);
                    //StartKiller();
                    while (loadingHtml)
                    {
                        Application.DoEvents();
                    }
                    System.Windows.Forms.HtmlDocument doc = browser.Document;


                    try
                    {
                        for (int d = 0; d < doc.All.Count; d++)
                        {
                            for (int j = 0; j < doc.All[d].GetElementsByTagName("a").Count; j++)
                            {
                                string url = doc.All[d].GetElementsByTagName("a")[j].GetAttribute("href").ToString();
                                if (url.Contains("http"))
                                {
                                    dataGridView6.Rows[a].Cells[0].Value = url;
                                    a++;
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {

                    }
                }

                //排序
                dataGridView6.Sort(dataGridView6.Columns[0], System.ComponentModel.ListSortDirection.Descending);
                //dataGridView6.Update();

                //清理重複
                for (int i = 0, j = 0, k = 0; j < 10000 - 1; j++)
                {
                    if (dataGridView6.Rows[i].Cells[0].Value != null && dataGridView6.Rows[j + 1].Cells[0].Value != null)
                    {

                        string value_a = dataGridView6.Rows[i].Cells[0].Value.ToString();
                        string value_b = dataGridView6.Rows[j + 1].Cells[0].Value.ToString();
                        if (value_a == value_b)
                        {
                            dataGridView6.Rows[j + 1].Cells[0].Value = null;
                            k++;
                        }
                        else
                        {
                            dataGridView6.Rows[i].Cells[1].Value = k;
                            i = j + 1;
                            k = 0;
                        }
                    }

                }

                //排序
                dataGridView6.Sort(dataGridView6.Columns[1], System.ComponentModel.ListSortDirection.Descending);
                MessageBox.Show("網頁下連結完成", "MessageBox");
            }
        }
//----------------------------------------------------------------------讀取網站下的連結
//使用者推薦及網站推薦GUI
        ListBox userList, urlList;
        Panel listPanel;
        int GUI_num;
        private void button2_Click(object sender, EventArgs e)
        {
            GUI_num = Convert.ToInt32(textBox1.Text);
            tabControl1.TabPages[2].Controls.Clear();
            if (tabControl1.SelectedIndex == 0)
            {
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    //dataGridView1.Rows[i].Cells[0].Value = others_website[i];
                    //dataGridView1.Rows[i].Cells[1].Value = others_comefrom[i];
                    //dataGridView1.Rows[i].Cells[2].Value = Url_compare[i];

                    if (i < GUI_num)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value != null)
                        {
                            tabControl1.TabPages[2].Controls.Add(new TextBox()
                            {
                                Text = dataGridView1.Rows[i].Cells[0].Value.ToString(),
                                Location = new Point(10, 32 * i + 10 + 64 * i),
                                Size = new Size(tabControl1.Width - 32, 15),
                                BackColor = Color.Gray
                            });
                            if (dataGridView1.Rows[i].Cells[1].Value != null && dataGridView1.Rows[i].Cells[2].Value != null)
                            {
                                tabControl1.TabPages[2].Controls.Add(new TextBox()
                                {
                                    Text = dataGridView1.Rows[i].Cells[2].Value.ToString() + "( " + dataGridView1.Rows[i].Cells[1].Value.ToString() + " )",
                                    Location = new Point(10, 32 * i + 25 + 64 * i),
                                    Size = new Size(tabControl1.Width - 32, 15),
                                    BackColor = Color.LightBlue
                                });
                            }

                            tabControl1.TabPages[2].Controls.Add(new TextBox()
                            {
                                Text = UrlTitle(dataGridView1.Rows[i].Cells[0].Value.ToString()),
                                Location = new Point(10, 32 * i + 40 + 64 * i),
                                Size = new Size(tabControl1.Width - 32, 15),
                                BackColor = Color.SlateGray
                            });


                            tabControl1.TabPages[2].Controls.Add(new TextBox()
                            {
                                Text = GetMetaTagValuesUtf8(dataGridView1.Rows[i].Cells[0].Value.ToString()),
                                Location = new Point(10, 32 * i + 55 + 64 * i),
                                Size = new Size(tabControl1.Width - 32, 40),
                                BackColor = Color.LightGray
                            });

                        }
                    }
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {

                //網頁清單
                dataGridView2.Sort(dataGridView2.Columns[0], System.ComponentModel.ListSortDirection.Descending);
                
                urlList = new ListBox()
                {
                    BorderStyle = BorderStyle.None,
                    Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Name = "urlList",
                    Location = new Point(32, 42),
                    Size = new Size(200, 100)
                };
                tabControl1.TabPages[2].Controls.Add(urlList);
                string website = null;
                int webCount = 0;
                for (int i = 0; i < dataGridView2.RowCount; i++)
                {
                    if (dataGridView2.Rows[i].Cells[0].Value != null)
                    {
                        if (website != dataGridView2.Rows[i].Cells[0].Value.ToString())
                        {
                            webCount++;
                            urlList.Items.Add(dataGridView2.Rows[i].Cells[0].Value);
                            website = dataGridView2.Rows[i].Cells[0].Value.ToString();
                        }
                    }

                }
                tabControl1.TabPages[2].Controls.Add(new Label()
                {
                    BorderStyle = BorderStyle.None,
                    Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Text = "網頁清單:" + webCount,
                    Size = new Size(200, 20),
                    Location = new Point(32, 12),
                });
                //使用者清單
                dataGridView2.Sort(dataGridView2.Columns[1], System.ComponentModel.ListSortDirection.Descending);
                
                userList = new ListBox()
                {
                    BorderStyle = BorderStyle.None,
                    Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Name = "userList",
                    Location = new Point(286, 42),
                    Size = new Size(200, 100)
                };
                
                tabControl1.TabPages[2].Controls.Add(userList);
                string username = null;
                int userCount = 0;
                for (int i = 0; i < dataGridView2.RowCount; i++)
                {
                    if (dataGridView2.Rows[i].Cells[1].Value != null)
                    {
                        if (username != dataGridView2.Rows[i].Cells[1].Value.ToString())
                        {
                            userCount++;
                            userList.Items.Add(dataGridView2.Rows[i].Cells[1].Value);
                            username = dataGridView2.Rows[i].Cells[1].Value.ToString();
                        }
                    }

                }
                tabControl1.TabPages[2].Controls.Add(new Label()
                {
                    BorderStyle = BorderStyle.None,
                    Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Text = "使用者清單:" + userCount,
                    Size = new Size(200,20),
                    Location = new Point(286, 12),
                });
                dataGridView2.Sort(dataGridView2.Columns[2], System.ComponentModel.ListSortDirection.Descending);
                urlList.SelectedIndexChanged += urlList_SelectedIndexChanged;
                userList.SelectedIndexChanged += userList_SelectedIndexChanged;
            }
            
            
        }

        void userList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();

            string user = userList.SelectedItem.ToString();
            tabControl1.TabPages[2].Controls.Remove(listPanel);
            listPanel = new Panel()
            {
                AutoScroll = true,
                Name = "listPanel",
                Dock = DockStyle.Fill
            };
            tabControl1.TabPages[2].Controls.Add(listPanel);

            int TextboxURL_Size_Wid = tabControl1.Width - 100;
            int TextboxURL_Size_Hig = 15;
            int TextboxURL_Size_X = 10;
            int TextboxURL_Size_Y = 150;
            int TextboxURL_Dis = 25;

            int TextboxCom_Size_Wid = 32;
            int TextboxCom_Size_Hig = 15;
            int TextboxCom_Size_X = TextboxURL_Size_X + TextboxURL_Size_Wid;
            int TextboxCom_Size_Y = TextboxURL_Size_Y;
            int TextboxCom_Dis = TextboxURL_Dis;

            for (int i = 0, j = 0; i < dataGridView2.RowCount; i++)
            {
                if (dataGridView2.Rows[i].Cells[1].Value != null)
                {
                    if (user == dataGridView2.Rows[i].Cells[1].Value.ToString())
                    {
                        //顯示結果的數量
                        if (j < GUI_num)
                        {
                            //顯示網站網址
                            listPanel.Controls.Add(new TextBox()
                            {
                                Text = dataGridView2.Rows[i].Cells[0].Value.ToString(),
                                Location = new Point(TextboxURL_Size_X, TextboxURL_Size_Y + TextboxURL_Dis * j),
                                Size = new Size(TextboxURL_Size_Wid, TextboxURL_Size_Hig),
                                BackColor = Color.LightGray,
                                ImeMode = ImeMode.Off
                            });
                            //顯示推薦比分
                            listPanel.Controls.Add(new TextBox()
                            {
                                Text = dataGridView2.Rows[i].Cells[2].Value.ToString(),
                                Location = new Point(TextboxCom_Size_X, TextboxCom_Size_Y + TextboxCom_Dis * j),
                                Size = new Size(TextboxCom_Size_Wid, TextboxCom_Size_Hig),
                                BackColor = Color.LightBlue,
                                ImeMode = ImeMode.Off
                            });
                            j++;
                        }
                    }
                }
            }
        }

        void urlList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            
            string website = urlList.SelectedItem.ToString();
            tabControl1.TabPages[2].Controls.Remove(listPanel);
            listPanel = new Panel()
            {
                AutoScroll = true,
                Name = "listPanel",
                Dock = DockStyle.Fill
            };
            tabControl1.TabPages[2].Controls.Add(listPanel);

            int TextboxUser_Size_Wid = tabControl1.Width - 100;
            int TextboxUser_Size_Hig = 15;
            int TextboxUser_Size_X = 10;
            int TextboxUser_Size_Y = 150;
            int TextboxUser_Dis = 25;

            int TextboxCom_Size_Wid = 32;
            int TextboxCom_Size_Hig = 15;
            int TextboxCom_Size_X = TextboxUser_Size_X + TextboxUser_Size_Wid;
            int TextboxCom_Size_Y = TextboxUser_Size_Y;
            int TextboxCom_Dis = TextboxUser_Dis;

            for (int i = 0,j = 0; i < dataGridView2.RowCount; i++)
            {
                if (dataGridView2.Rows[i].Cells[0].Value != null)
                {
                    if (website == dataGridView2.Rows[i].Cells[0].Value.ToString())
                    {
                    
                        
                        //顯示結果的數量
                        if (j < GUI_num)
                        {
                                //顯示推薦使用者
                                listPanel.Controls.Add(new TextBox()
                                {
                                    Text = dataGridView2.Rows[i].Cells[1].Value.ToString(),
                                    Location = new Point(TextboxUser_Size_X, TextboxUser_Size_Y + TextboxUser_Dis * j),
                                    Size = new Size(TextboxUser_Size_Wid, TextboxUser_Size_Hig),
                                    BackColor = Color.LightGray,
                                    ImeMode = ImeMode.Off
                                });
                                //顯示推薦比分
                                listPanel.Controls.Add(new TextBox()
                                {
                                    Text = dataGridView2.Rows[i].Cells[2].Value.ToString(),
                                    Location = new Point(TextboxCom_Size_X, TextboxCom_Size_Y + TextboxCom_Dis * j),
                                    Size = new Size(TextboxCom_Size_Wid, TextboxCom_Size_Hig),
                                    BackColor = Color.LightBlue,
                                    ImeMode = ImeMode.Off
                                });
                                j++;
                        }
                    }
                }

            }
            

        }

        
        
//----------------------------------------------------------------------使用者推薦及網站推薦GUI
        private void 關鍵字字典資料ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> wordList = new List<string>();
            List<List<string>> wordList_file = new List<List<string>>();
            if(folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folder = folderBrowserDialog1.SelectedPath;
                wordList_file.Clear();
                // 取得資料夾內所有檔案
                foreach (string fname in System.IO.Directory.GetFiles(folder))
                {
                    string line;

                    // 一次讀取一行
                    System.IO.StreamReader file = new System.IO.StreamReader(fname);
                    while ((line = file.ReadLine()) != null)
                    {
                        wordList.Add(line.Trim());
                    }
                    //wordList_file.Add(wordList);
                    //wordList = new List<string>();


                    file.Close();
                }
                //字典初始化
                for (int i = 0; i < 5; i++)
                {
                    wordDictionary[i] = new List<string>();
                    wordFeqDictionary[i] = new List<int>();
                }
                //取的字彙是"(Na)","(Nb)","(Nc)","(Nd)","(Ncd)"屬性
                for (int k = 0; k < wordList.Count; k++)
                {
                    string[] find_word = { "(Na)", "(Nb)", "(Nc)", "(Nd)", "(Ncd)" };
                    for (int l = 0; l < 5; l++)
                    {

                        string source = wordList[k];
                        List<string> words = new List<string>();
                        for (int i = 0; i < source.Length; i++)
                        {
                            string word = "";

                            int find_index = source.IndexOf(find_word[l], i);
                            if (find_index != -1) { i = find_index + find_word.Length; }
                            else i = source.Length;
                            int j = find_index - 1;
                            try
                            {
                                while (j >= 0 && source[j] != ')' && source[j] != ' ' && source[j] != '　')
                                {
                                    word = source[j] + word;
                                    j--;
                                }
                            }
                            catch (Exception err) { }
                            if (word != "") words.Add(word);

                        }

                        if (words.Count != 0) wordDictionary[l].AddRange(words);
                    }
                }

                
                dataGridView7.RowCount = 20000;
                dataGridView7.ColumnCount = 15;
                dataGridView7.Columns[0].HeaderCell.Value = "(Na)";
                dataGridView7.Columns[2].HeaderCell.Value = "(Nb)";
                dataGridView7.Columns[4].HeaderCell.Value = "(Nc)";
                dataGridView7.Columns[6].HeaderCell.Value = "(Nd)";
                dataGridView7.Columns[8].HeaderCell.Value = "(Ncd)";
                dataGridView7.Columns[10].HeaderCell.Value = "(合併)";

                dataGridView8.RowCount = 20000;
                dataGridView8.ColumnCount = 2;
                dataGridView8.Columns[0].HeaderCell.Value = "關鍵字";
                dataGridView8.Columns[1].HeaderCell.Value = "出現頻率";
                for (int l = 0; l < 5; l++)
                {
                        wordDictionary[l].Sort();
                        //清理重複
                        for (int i = 0, j = 0, k = 1; j < wordDictionary[l].Count - 1; j++)
                        {
                            if (wordDictionary[l][i] != null &&  wordDictionary[l][j + 1] != null)
                            {
                                string value_a = wordDictionary[l][i].ToString();
                                string value_b = wordDictionary[l][j + 1].ToString();
                                if (value_a == value_b)
                                {
                                    wordDictionary[l][j + 1] = null;
                                    k++;
                                }
                                else
                                {
                                    wordFeqDictionary[l].Add(k);
                                    i = j + 1;
                                    k = 1;
                                }
                                if (j == wordDictionary[l].Count - 2)
                                {
                                    wordFeqDictionary[l].Add(k);
                                }
                            }
                        }
                        //if (wordDictionary[l][(wordDictionary[l].Count-1)] != null) wordFeqDictionary[l].Add(1);
                        //清理空值及計算次數
                       
                        for (int i = 0; i < wordDictionary[l].Count; i++)
                        {
                            if(wordDictionary[l][i] == null) wordDictionary[l].RemoveAt(i--);
                        }
                    
                }
                for (int l = 0,j = 0; l < 5; l++)
                {
                    for (int i = 0; i < wordDictionary[l].Count; i++)
                    {
                        dataGridView7.Rows[i].Cells[l * 2].Value = wordDictionary[l][i].ToString();
                        dataGridView7.Rows[i].Cells[l * 2 + 1].Value = wordFeqDictionary[l][i];
                        dataGridView7.Rows[j].Cells[10].Value = wordDictionary[l][i].ToString();
                        dataGridView7.Rows[j].Cells[11].Value = wordFeqDictionary[l][i];
                        dataGridView8.Rows[j].Cells[0].Value = wordDictionary[l][i].ToString();
                        dataGridView8.Rows[j].Cells[1].Value = wordFeqDictionary[l][i];
                        j++;
                    }
                }
                dataGridView8.Sort(dataGridView8.Columns[1], System.ComponentModel.ListSortDirection.Descending);
            }
            

        }
        string keyword_folder;
        string text_folder;
        private void 網站關鍵字資料ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //清空資訊
                //ToClearAll(sender, e);

                //讀取網站ID對照並放入data
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                file.ReadLine();
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    data.Add(line);
                    count++;
                }
                file.Close();

                //將網站顯示於ListBox2
                List<string> keyword_id = new List<string>();
                List<string> keyword_url = new List<string>();
                
                for (int i = 0; i < count; i++)
                {
                    string[] cut = data[i].Split(',');

                    keyword_id.Add(cut[0]);
                    keyword_url.Add(cut[1]);
                    listBox2.Items.Add(cut[0]+"，"+cut[1]);
                }


                //開啟斷詞資料夾
                if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    
                    
                    keyword_folder = folderBrowserDialog1.SelectedPath;


                } 
                if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    

                    text_folder = folderBrowserDialog1.SelectedPath;


                }
                listBox2.SelectedIndexChanged += listBox2_SelectedIndexChanged;
            }
        }

        void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            //throw new NotImplementedException();
            int id = listBox2.SelectedIndex+1;
            string filename = keyword_folder +"\\"+ id + ".txt";
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            string line;
            List<string> wordList = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                wordList.Add(line.Trim());
            }
            file.Close();
            //文本資料
            richTextBox2.Clear();
            filename = text_folder + "\\" + id + ".txt";
            file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                richTextBox2.AppendText(line);
            }
            file.Close();
            //字典初始化
            for (int i = 0; i < 5; i++)
            {
                wordDictionary[i] = new List<string>();
                wordFeqDictionary[i] = new List<int>();
            }
            //取的字彙是"(Na)","(Nb)","(Nc)","(Nd)","(Ncd)"屬性
            for (int k = 0; k < wordList.Count; k++)
            {
                string[] find_word = { "(Na)", "(Nb)", "(Nc)", "(Nd)", "(Ncd)" };
                
                for (int l = 0; l < 5; l++)
                {

                    string source = wordList[k];
                    List<string> words = new List<string>();
                    for (int i = 0; i < source.Length; i++)
                    {
                        string word = "";

                        int find_index = source.IndexOf(find_word[l], i);
                        if (find_index != -1) { i = find_index + find_word.Length; }
                        else i = source.Length;
                        int j = find_index - 1;
                        try
                        {
                            while (j >= 0 && source[j] != ')' && source[j] != ' ' && source[j] != '　')
                            {
                                word = source[j] + word;
                                j--;
                            }
                        }
                        catch (Exception err) { }
                        if (word != "") words.Add(word);

                    }

                    if (words.Count != 0) wordDictionary[l].AddRange(words);
                }
            }

            for (int l = 0; l < 5; l++)
            {
                wordDictionary[l].Sort();
                //清理重複
                for (int i = 0, j = 0, k = 1; j < wordDictionary[l].Count - 1; j++)
                {
                    if (wordDictionary[l][i] != null && wordDictionary[l][j + 1] != null)
                    {
                        string value_a = wordDictionary[l][i].ToString();
                        string value_b = wordDictionary[l][j + 1].ToString();
                        if (value_a == value_b)
                        {
                            wordDictionary[l][j + 1] = null;
                            k++;
                        }
                        else
                        {
                            wordFeqDictionary[l].Add(k);
                            i = j + 1;
                            k = 1;
                        }
                        if (j == wordDictionary[l].Count - 2)
                        {
                            wordFeqDictionary[l].Add(k);
                        }
                    }
                }
                try
                {
                    if (wordDictionary[l][(wordDictionary[l].Count - 1)] != null) wordFeqDictionary[l].Add(1);
                }
                catch (Exception err) { }
                //清理空值
                for (int i = 0; i < wordDictionary[l].Count; i++)
                {
                    if (wordDictionary[l][i] == null) wordDictionary[l].RemoveAt(i--);
                }

            }
            int[] FontBigger = new int [System.Convert.ToInt32(textBox5.Text)];
            List<int> wordFeqDictionary_clone = new List<int>();
            foreach(var feq in wordFeqDictionary){
                wordFeqDictionary_clone.AddRange(feq);
            }
            wordFeqDictionary_clone.Sort();
            wordFeqDictionary_clone.Reverse();

            if (wordFeqDictionary_clone.Count != 0)
            {
                    FontBigger[0] = wordFeqDictionary_clone[0];
            }
            for (int k = 1,m = 1; k < wordFeqDictionary_clone.Count && m < System.Convert.ToInt32(textBox5.Text) ; k++)
            {
                if (wordFeqDictionary_clone != null)
                {
                    if (wordFeqDictionary_clone[k] != wordFeqDictionary_clone[k - 1]) {
                        FontBigger[m] = wordFeqDictionary_clone[k];
                        m++;
                    }
                    
                }
                
            }

            for (int k = 0; k < System.Convert.ToInt32(textBox5.Text); k++)
            {
                if (wordFeqDictionary_clone != null)
                {
                    for (int l = 0, j = 0; l < 5; l++)
                    {
                        for (int i = 0; i < wordDictionary[l].Count; i++)
                        {
                            if (FontBigger[k] == wordFeqDictionary[l][i]) richTextBox1.AppendText(wordDictionary[l][i] + "，" + wordFeqDictionary[l][i] + "\n");
                            j++;
                        }
                    }
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 8 && listBox2.SelectedIndex != -1)
            {

                string whichone = listBox2.Items[listBox2.SelectedIndex].ToString();
                string[] cut = whichone.Split('，');
                string id = cut[0];
                string url = cut[1];
                string value = "";
                int keyword_count = 0;
                dataGridView9.Rows.Clear();
                dataGridView9.RowCount = 20000;
                dataGridView9.ColumnCount = 2;
                foreach (var text in richTextBox1.Text)
                {
                    if (text.ToString() == "，")
                    {
                        dataGridView9.Rows[keyword_count].Cells[0].Value = value;
                        value = "";
                    }
                    else if(text.ToString() == "\n")
                    {
                        dataGridView9.Rows[keyword_count].Cells[1].Value = System.Convert.ToInt32(value);
                        value = "";
                        keyword_count++;
                    }
                    else
                    {
                        value += text;
                    }
                }

                
                int compare_count = System.Convert.ToInt32(textBox5.Text);
                string[] userKeyword = new string [compare_count];
                string[] websiteKeyword = new string [compare_count];
                int userKeyword_count = 0;
                int websiteKeyword_count = 0;
                for (int i = 0; i < System.Convert.ToInt32(textBox5.Text); i++)
                {
                    try
                    {
                        if (dataGridView8.Rows[i].Cells[0].Value != null)
                        {
                            userKeyword[i] = dataGridView8.Rows[i].Cells[0].Value.ToString();
                            userKeyword_count++;
                        }
                            
                    }
                    catch (Exception err)
                    {

                    }
                    try
                    {
                        if (dataGridView9.Rows[i].Cells[0].Value != null)
                        {
                            websiteKeyword[i] = dataGridView9.Rows[i].Cells[0].Value.ToString();
                            websiteKeyword_count++;
                        }
                        
                    }
                    catch (Exception err)
                    {

                    }
                    
                }
                double jaccard = 0;
                for (int i = 0; i < userKeyword_count; i++)
                {
                    for (int j = 0; j < websiteKeyword_count; j++)
                    {
                        if( userKeyword[i] == websiteKeyword[j] )
                        {
                            jaccard++;
                        }
                    }
                }
                jaccard = jaccard / (userKeyword_count + websiteKeyword_count - jaccard);

                int TextboxUser_Size_Wid = panel12.Width - 80;
                int TextboxUser_Size_Hig = 15;
                int TextboxUser_Size_X = 10;
                int TextboxUser_Size_Y = 50;
                int TextboxUser_Dis = 25;

                int TextboxCom_Size_Wid = 32;
                int TextboxCom_Size_Hig = 15;
                int TextboxCom_Size_X = TextboxUser_Size_X + TextboxUser_Size_Wid;
                int TextboxCom_Size_Y = TextboxUser_Size_Y;
                int TextboxCom_Dis = TextboxUser_Dis;

                int control_count = System.Convert.ToInt32(panel12.Controls.Count);
                //顯示推薦網頁
                panel12.Controls.Add(new TextBox()
                {
                    Text = id + "，" + url,
                    Location = new Point(TextboxUser_Size_X, TextboxUser_Size_Y + TextboxUser_Dis * control_count),
                    Size = new Size(TextboxUser_Size_Wid, TextboxUser_Size_Hig),
                    BackColor = Color.LightGray,
                    ImeMode = ImeMode.Off
                });
                //顯示Jaccard比分
                panel12.Controls.Add(new TextBox()
                {
                    Text = jaccard.ToString(),
                    Location = new Point(TextboxCom_Size_X, TextboxCom_Size_Y + TextboxCom_Dis * control_count),
                    Size = new Size(TextboxCom_Size_Wid, TextboxCom_Size_Hig),
                    BackColor = Color.LightBlue,
                    ImeMode = ImeMode.Off
                });
                
            }
        }

        private void dataGridView8_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    if (dataGridView8.Rows[e.RowIndex].Selected == false)
                    {
                        dataGridView8.ClearSelection();
                        dataGridView9.ClearSelection();
                        dataGridView8.Rows[e.RowIndex].Selected = true;
                    }
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void 移除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView8.SelectedRows)
            {
                if (!row.IsNewRow)
                    dataGridView8.Rows.Remove(row);
            }
            foreach (DataGridViewRow row in dataGridView9.SelectedRows)
            {
                if (!row.IsNewRow)
                    dataGridView9.Rows.Remove(row);
            }
        }

        private void dataGridView9_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    if (dataGridView9.Rows[e.RowIndex].Selected == false)
                    {
                        dataGridView8.ClearSelection();
                        dataGridView9.ClearSelection();
                        dataGridView9.Rows[e.RowIndex].Selected = true;
                    }
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void pageRank加強ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string line;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default);
                file.ReadLine();
                data.Clear();
                while ((line = file.ReadLine()) != null) data.Add(line);
                for (int i = 0; i < data.Count; i++)
                {
                    string[] cut = data[i].Split(',');
                    double After_Page_Rank = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value) * Convert.ToDouble(cut[2]);
                    dataGridView1.Rows[i].Cells[3].Value = Convert.ToInt32(cut[2]);
                    dataGridView1.Rows[i].Cells[4].Value = After_Page_Rank;
                }
            }
        }
        //2015_10_23新增網頁反推薦簡單顯示
        private void panelEasyDisplay_SizeChanged(object sender, EventArgs e)
        {
            Form1_Load(sender, e);
        }
        private void easyDisplayToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Form1_Load(sender, e);
        }
        private void analysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analysisToolStripMenuItem.Checked = true;
            easyDisplayToolStripMenuItem.Checked = false;
            Form1_Load(sender, e);
        }

        private void 版面ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void easyDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analysisToolStripMenuItem.Checked = false;
            easyDisplayToolStripMenuItem.Checked = true;
            Form1_Load(sender, e);
        }
        private void buttonFindUser_Click(object sender, EventArgs e)
        {
            //下一頁
            panelMainLeft.Size = new System.Drawing.Size(panelEasyDisplay.Width, panelEasyDisplay.Height);
            int Wid = panelLeft.Width;
            int Hig = panelLeft.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainLeft.Width - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelLeft.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelLeft.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonLeft1ToRight_Click(object sender, EventArgs e)
        {
            //上一頁
            panelMainLeft.Size = new System.Drawing.Size(panelEasyDisplay.Width / 2, panelEasyDisplay.Height);
            int Wid = panelLeft.Width;
            int Hig = panelLeft.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainLeft.Width -21 - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelLeft.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelLeft.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonLeft1ToLeft_Click(object sender, EventArgs e)
        {
            //下一頁
            int Wid = panelLeft1.Width;
            int Hig = panelLeft1.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainLeft.Width - 0) : Wid - 0;

            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelLeft1.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);

                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;

            }
            panelLeft1.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonLeft2ToRight_Click(object sender, EventArgs e)
        {
            //上一頁
            int Wid = panelLeft1.Width;
            int Hig = panelLeft1.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainLeft.Width - 42 - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelLeft1.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelLeft1.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonFindWeb_Click(object sender, EventArgs e)
        {
            //下一頁
            panelMainLeft.Size = new System.Drawing.Size(0, panelEasyDisplay.Height);
            int Wid = panelRight.Width;
            int Hig = panelRight.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainRight.Width - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelRight.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelRight.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonRight1ToLeft_Click(object sender, EventArgs e)
        {
            //上一頁
            panelMainLeft.Size = new System.Drawing.Size(panelEasyDisplay.Width / 2, panelEasyDisplay.Height);
            int Wid = panelRight.Width;
            int Hig = panelRight.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainRight.Width - 21 - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelRight.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelRight.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonRight1ToRight_Click(object sender, EventArgs e)
        {
            //下一頁
            int Wid = panelRight1.Width;
            int Hig = panelRight1.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainRight.Width - 0) : Wid - 0;

            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelRight1.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);

                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;

            }
            panelRight1.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonRight2ToLeft_Click(object sender, EventArgs e)
        {
            //上一頁
            int Wid = panelRight1.Width;
            int Hig = panelRight1.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainRight.Width - 42 - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelRight1.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelRight1.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonRight2ToRight_Click(object sender, EventArgs e)
        {
            //下一頁
            int Wid = panelRight2.Width;
            int Hig = panelRight2.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainRight.Width - 0) : Wid - 0;

            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelRight2.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);

                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;

            }
            panelRight2.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonRight3ToLeft_Click(object sender, EventArgs e)
        {
            //上一頁
            int Wid = panelRight2.Width;
            int Hig = panelRight2.Height;
            int change_speed = 1;
            int path = Wid == 0 ? Wid - (panelMainRight.Width - 63 - 0) : Wid - 0;
            for (int pixel = path; pixel != 0; pixel = pixel > 0 ? pixel - change_speed : pixel + change_speed)
            {
                panelRight2.Size = new System.Drawing.Size((Wid - path) + pixel, Hig);
                if (pixel > 100 || pixel < -100)
                    change_speed = 4;
                else if (pixel < 50 || pixel > -50)
                    change_speed = 1;
                else
                    change_speed = 2;
            }
            panelRight2.Size = new System.Drawing.Size((Wid - path), Hig);
        }

        private void buttonRightToCenter_Click(object sender, EventArgs e)
        {
            buttonRight3ToLeft_Click(sender, e);
            buttonRight2ToLeft_Click(sender, e);
            buttonRight1ToLeft_Click(sender, e);

        }

        private void buttonLeftToCenter_Click(object sender, EventArgs e)
        {
            buttonLeft2ToRight_Click(sender, e);
            buttonLeft1ToRight_Click(sender, e);
        }

        private void labelDisplayTitle_Click(object sender, EventArgs e)
        {
            //panelMainLeft.Size = new System.Drawing.Size(panelEasyDisplay.Width / 2, 0);
            //panelLeft.Size = new System.Drawing.Size(panelMainLeft.Width - 21, 0);
            //panelDisplayStart.Size = new System.Drawing.Size(0, 0);
        }

        private void buttonLoadWebsiteList_Click(object sender, EventArgs e)
        {
            網站收藏者資料ToolStripMenuItem_Click(sender, e);
            try
            {
                listBoxWebsiteList.Items.Clear();
                foreach(string web in website)
                {
                    listBoxWebsiteList.Items.Add(web);
                }
                listBoxWebsiteList.Enabled = true;
                listBoxWebsiteList.BackColor = Color.White;
                
                
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private void listBoxWebsiteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxWebsiteList.SelectedIndex >= 0)
            {
                textBoxSuggestNum.Enabled = true;
                buttonSuggest.Enabled = true;
            }
            else
            {
                textBoxSuggestNum.Enabled = false;
                buttonSuggest.Enabled = false;
            }
        }

        private void textBoxSuggestNum_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBoxSuggestNum_Click(object sender, EventArgs e)
        {
            textBoxSuggestNum.Text = "";
        }

        //網頁反推薦_推薦按鈕
        private void buttonSuggest_Click(object sender, EventArgs e)
        {
            int suggestNum = Convert.ToInt32(textBoxSuggestNum.Text);
            string website = listBoxWebsiteList.SelectedItem.ToString();
            labelLeft2Web.Text = website;

            panelLeft2Result.Controls.Clear();
            
            int TextboxUser_Size_Wid = panelLeft2Result.Width - 100;
            int TextboxUser_Size_Hig = 15;
            int TextboxUser_Size_X = 10;
            int TextboxUser_Size_Y = 10;
            int TextboxUser_Dis = 40;

            int TextboxCom_Size_Wid = 40;
            int TextboxCom_Size_Hig = 15;
            int TextboxCom_Size_X = TextboxUser_Size_X + TextboxUser_Size_Wid;
            int TextboxCom_Size_Y = TextboxUser_Size_Y;
            int TextboxCom_Dis = TextboxUser_Dis;

            for (int i = 0, j = 0; i < dataGridView2.RowCount; i++)
            {
                if (dataGridView2.Rows[i].Cells[0].Value != null)
                {
                    if (website == dataGridView2.Rows[i].Cells[0].Value.ToString())
                    {


                        //顯示結果的數量
                        if (j < suggestNum)
                        {
                            //顯示推薦使用者
                            panelLeft2Result.Controls.Add(new TextBox()
                            {
                                Text = dataGridView2.Rows[i].Cells[1].Value.ToString(),
                                Location = new Point(TextboxUser_Size_X, TextboxUser_Size_Y + TextboxUser_Dis * j),
                                Size = new Size(TextboxUser_Size_Wid, TextboxUser_Size_Hig),
                                BackColor = Color.LightCyan,
                                ImeMode = ImeMode.Off
                            });
                            //顯示推薦比分
                            panelLeft2Result.Controls.Add(new TextBox()
                            {
                                Text = dataGridView2.Rows[i].Cells[2].Value.ToString(),
                                Location = new Point(TextboxCom_Size_X, TextboxCom_Size_Y + TextboxCom_Dis * j),
                                Size = new Size(TextboxCom_Size_Wid, TextboxCom_Size_Hig),
                                BackColor = Color.MediumTurquoise,
                                ImeMode = ImeMode.Off
                            });
                            j++;
                        }
                    }
                }

            }
            //下一步
            buttonLeft1ToLeft_Click(sender, e);
        }
        //2015_10_23新增網頁反推薦簡單顯示---------------------------------------------------------------------END
        double[][] user_similarity;
        private void button4LoadUserFavoriteList_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folder = folderBrowserDialog1.SelectedPath;
                //twitterpageAaary = new List<string>[]
                // 取得資料夾內所有檔案
                username.Clear();
                foreach (string fname in System.IO.Directory.GetFiles(folder))
                {
                    username.Add(Path.GetFileNameWithoutExtension(fname));
                }
                twitterpageArray = new List<string>[username.Count];
                websiteArray = new List<string>[username.Count];
                tagArray = new List<string>[username.Count];
                user_similarity = new double[username.Count][];
                for (int i = 0; i < username.Count; i++)
                {
                    user_similarity[i] = new double[username.Count];
                }
                
                for (int i = 0; i < username.Count; i++)
                {
                    string line;
                    string fname = folder + "\\" + username[i] + ".txt";

                    twitterpageArray[i] = new List<string>();
                    websiteArray[i] = new List<string>();
                    tagArray[i] = new List<string>();
                    // 一次讀取一行
                    try
                    {
                        System.IO.StreamReader file = new System.IO.StreamReader(fname);
                        //讀取標頭檔
                        file.ReadLine();
                        double[] count = new double[i];
                        for (int j = 0; j < i; j++)
                        {
                            count[j] = 0;
                        }
                
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] cut = line.Split('\t');
                            if (cut.Length >= 2) { 
                                twitterpageArray[i].Add(cut[1]);
                                //計算相似度
                                for (int j = 0; j < i; j++)
                                {
                                    for(int k=0; k<twitterpageArray[j].Count; k++)
                                    {
                                        if (twitterpageArray[j][k] == cut[1])
                                        {
                                            count[j]++;
                                        }
                                    }
                                }
                            }
                            if (cut.Length >= 3)
                                websiteArray[i].Add(cut[2]);
                            if (cut.Length >= 4)
                                tagArray[i].Add(cut[3]);
                            if (cut.Length <= 3)
                                tagArray[i].Add("");
                        }
                        for (int j = 0; j < i; j++)
                        {
                            
                            user_similarity[i][j] = user_similarity[j][i] = count[j];
                        }
                        file.Close();
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.ToString());
                    }

                }
                dataGridView11.Rows.Clear();
                dataGridView11.RowCount = username.Count;
                dataGridView11.ColumnCount = username.Count;
                for (int i = 0; i < username.Count; i++)
                {
                    for (int j = i + 1; j < username.Count; j++)
                    {
                        user_similarity[i][j] = user_similarity[j][i] 
                            = user_similarity[j][i] / Math.Sqrt(websiteArray[j].Count) / Math.Sqrt(websiteArray[i].Count);
                        dataGridView11.Rows[i].Cells[j].Value = dataGridView11.Rows[j].Cells[i].Value 
                            = user_similarity[i][j];
                    }
                }

                listBoxUserList.BackColor = Color.White;
                listBoxUserList.Enabled = true;
                listBoxUserList.Items.Clear();
                for (int i = 0; i < username.Count; i++)
                {
                    //使用者顯示於畫面
                    listBoxUserList.Items.Add(username[i]);
                }


               
                
            }

        }

        private void listBoxUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUserList.SelectedIndex >= 0)
            {
                
                UserRelatedTag.Clear();
                comboBoxRight1Tag.Items.Clear();
                try
                {
                    listBoxUserFavoriteList.BackColor = Color.White;
                    listBoxUserFavoriteList.Enabled = true;
                    textBoxSuggestWebsiteNum.Enabled = true;
                    buttonSuggestWebsite.Enabled = true;

                    listBoxUserFavoriteList.Items.Clear();
                    int selectedIndex = listBoxUserList.SelectedIndex;
                    for (int i = 0; i < websiteArray[selectedIndex].Count; i++)
                    {
                        string url = websiteArray[selectedIndex][i].ToString();
                        //try{
                            string tag = tagArray[selectedIndex][i].ToString();
                        //}
                        //catch(Exception err){}
                        if (url != "")
                            listBoxUserFavoriteList.Items.Add(url);
                        if (tag != "")
                        {
                            UserRelatedTag.Add(tag);
                            if(!comboBoxRight1Tag.Items.Contains(tag))
                            comboBoxRight1Tag.Items.Add(tag);
                        }
                        
                            
                    }
                }
                catch (Exception) { }
                
            }
            else
            {
                //listBoxUserFavoriteList.BackColor = Color.Gray;
                listBoxUserFavoriteList.Items.Clear();
                listBoxUserFavoriteList.Enabled = false;
                textBoxSuggestWebsiteNum.Enabled = false;
                buttonSuggestWebsite.Enabled = false;
            }

            
            
        }

        private void textBoxSuggestWebsiteNum_Click(object sender, EventArgs e)
        {
            textBoxSuggestWebsiteNum.Text = "";
        }

        private void buttonSuggestWebsite_Click(object sender, EventArgs e)
        {
            
            suggestWebsite.Clear();
            suggestPower.Clear();
            suggestRelatedUser.Clear();
            suggestRelatedTag.Clear();

            int user_suggest_index = listBoxUserList.SelectedIndex;
            labelRight2User.Text = listBoxUserList.SelectedItem.ToString();
            //計算推薦權重
            for (int i = 0; i < username.Count; i++)
            {
                //與其他使用者比較(去除自己)
                if(i != user_suggest_index)
                {
                    for(int j=0; j<websiteArray[i].Count; j++)
                    {
                        if(websiteArray[i][j] != "")
                        {
                            double suggestPowerNum = 0;
                            for(int k=0; k<websiteArray[user_suggest_index].Count; k++)
                            {
                                //原本使用者已經收藏
                                if(websiteArray[i][j] != websiteArray[user_suggest_index][k])
                                {
                                    suggestPowerNum = user_similarity[i][user_suggest_index];
                                }
                            }
                            //網址已經存在在推薦網址(使用者相似度要相加)
                            int suggestWebsite_index = -1;
                            for (int k = 0; k < suggestWebsite.Count; k++)
                            {
                                if(suggestWebsite[k] == websiteArray[i][j])
                                {
                                    suggestWebsite_index = k;
                                    break;
                                }
                            }
                            //一個人收藏的許多文章連結到同網址(去除)
                            if (suggestWebsite_index != -1)
                            {
                                if (!(suggestRelatedUser[suggestWebsite_index].Contains(username[i])))
                                {
                                    suggestPower[suggestWebsite_index]
                                        = suggestPower[suggestWebsite_index] + suggestPowerNum;
                                    suggestRelatedUser[suggestWebsite_index]
                                        = suggestRelatedUser[suggestWebsite_index] + "、" + username[i];
                                }
                                try
                                {
                                    if (!(suggestRelatedTag[suggestWebsite_index].Contains(tagArray[i][j])))
                                    {
                                        suggestRelatedTag[suggestWebsite_index]
                                            = suggestRelatedUser[suggestWebsite_index] + "、" + tagArray[i][j];
                                    }
                                }
                                catch (Exception err) { }
                                
                            }
                            else 
                            {
                            suggestRelatedUser.Add(username[i]);
                            suggestWebsite.Add(websiteArray[i][j]);
                            suggestPower.Add(suggestPowerNum);
                            try
                            {
                                suggestRelatedTag.Add(tagArray[i][j]);
                            }
                            catch(Exception err){ }
                            }
                            
                            
                        }
                    }
                }
                
            }
            //顯示於DataGridView
            try
            {
                dataGridView10.Rows.Clear();
                dataGridView10.RowCount = suggestWebsite.Count;
                dataGridView10.ColumnCount = 4;
            }
            catch (Exception err) { };
            for (int i = 0; i < suggestWebsite.Count; i++)
            {

                dataGridView10.Rows[i].Cells[0].Value = suggestWebsite[i];
                dataGridView10.Rows[i].Cells[1].Value = suggestPower[i];
                dataGridView10.Rows[i].Cells[2].Value = suggestRelatedUser[i];
                try
                {
                    dataGridView10.Rows[i].Cells[3].Value = suggestRelatedTag[i];
                }
                catch (Exception err) { }
            }

            //輸出前幾個
            try
            {
                dataGridView10.Sort(dataGridView10.Columns[1], System.ComponentModel.ListSortDirection.Descending);
            }
            catch (Exception err) { }
            int output_num = System.Convert.ToInt32(textBoxSuggestWebsiteNum.Text);
            panelRight2Result.Controls.Clear();

            int TextboxUrl_Size_Wid = panelRight2Result.Width - 100 - 500;
            int TextboxUrl_Size_Hig = 15;
            int TextboxUrl_Size_X = 10;
            int TextboxUrl_Size_Y = 10;
            int TextboxUrl_Dis = 40;

            

            int TextboxRelatedUser_Size_Wid = 200;
            int TextboxRelatedUser_Size_Hig = 15;
            int TextboxRelatedUser_Size_X = TextboxUrl_Size_X + TextboxUrl_Size_Wid;
            int TextboxRelatedUser_Size_Y = TextboxUrl_Size_Y;
            int TextboxRelatedUser_Dis = TextboxUrl_Dis;

            int TextboxCom_Size_Wid = 65;
            int TextboxCom_Size_Hig = 15;
            int TextboxCom_Size_X = TextboxUrl_Size_X + TextboxUrl_Size_Wid + TextboxRelatedUser_Size_Wid;
            int TextboxCom_Size_Y = TextboxUrl_Size_Y;
            int TextboxCom_Dis = TextboxUrl_Dis;

            int TextboxTag_Size_Wid = 100;
            int TextboxTag_Size_Hig = 15;
            int TextboxTag_Size_X = TextboxUrl_Size_X + TextboxUrl_Size_Wid + TextboxRelatedUser_Size_Wid + TextboxCom_Size_Wid;
            int TextboxTag_Size_Y = TextboxUrl_Size_Y;
            int TextboxTag_Dis = TextboxUrl_Dis;

            for (int i = 0, j = 0; i < output_num; i++)
            {
                try
                {
                    if (dataGridView10.Rows[i].Cells[0] != null)
                    {
                        if (dataGridView10.Rows[i].Cells[0].Value.ToString().Contains("http") ||
                            dataGridView10.Rows[i].Cells[0].Value.ToString().Contains("Http") ||
                            dataGridView10.Rows[i].Cells[0].Value.ToString().Contains("HTtp") ||
                            dataGridView10.Rows[i].Cells[0].Value.ToString().Contains("HTTp") ||
                            dataGridView10.Rows[i].Cells[0].Value.ToString().Contains("HTTP"))
                        {
                            //顯示推薦使用者
                            panelRight2Result.Controls.Add(new TextBox()
                            {
                                Text = dataGridView10.Rows[i].Cells[0].Value.ToString(),
                                Location = new Point(TextboxUrl_Size_X, TextboxUrl_Size_Y + TextboxUrl_Dis * j),
                                Size = new Size(TextboxUrl_Size_Wid, TextboxUrl_Size_Hig),
                                BackColor = Color.LightCyan,
                                ImeMode = ImeMode.Off
                            });

                            //顯示相關使用者
                            panelRight2Result.Controls.Add(new TextBox()
                            {
                                Text = dataGridView10.Rows[i].Cells[2].Value.ToString(),
                                Location = new Point(TextboxRelatedUser_Size_X, TextboxRelatedUser_Size_Y + TextboxRelatedUser_Dis * j),
                                Size = new Size(TextboxRelatedUser_Size_Wid, TextboxRelatedUser_Size_Hig),
                                BackColor = Color.PaleTurquoise,
                                ImeMode = ImeMode.Off
                            });
                            //顯示推薦比分
                            panelRight2Result.Controls.Add(new TextBox()
                            {
                                Text = ((double)dataGridView10.Rows[i].Cells[1].Value).ToString("0.##0"),
                                Location = new Point(TextboxCom_Size_X, TextboxCom_Size_Y + TextboxCom_Dis * j),
                                Size = new Size(TextboxCom_Size_Wid, TextboxCom_Size_Hig),
                                BackColor = Color.MediumTurquoise,
                                ImeMode = ImeMode.Off
                            });
                            //顯示推薦Tag
                            panelRight2Result.Controls.Add(new TextBox()
                            {
                                Text = dataGridView10.Rows[i].Cells[3].Value.ToString(),
                                Location = new Point(TextboxTag_Size_X, TextboxTag_Size_Y + TextboxTag_Dis * j),
                                Size = new Size(TextboxTag_Size_Wid, TextboxTag_Size_Hig),
                                BackColor = Color.MediumTurquoise,
                                ImeMode = ImeMode.Off
                            });
                            j++;

                        }
                        else
                        {
                            i--;
                        }
                    }
                }
                catch (Exception err) { }
            }

            //下一步
            this.buttonRight1ToRight_Click(sender, e);
            

        }

        private void buttonRight3Jaccard_Click(object sender, EventArgs e)
        {
            listBoxRight3UserWord.Items.Clear();
            listBoxRight3UserWord.BackColor = Color.White;
            int[] userTagCount;
            userTagCount = new int[UserRelatedTag.Count];
            if (UserRelatedTag != null)
            {
                for(int i=0, find = 0; i<UserRelatedTag.Count; i++)
                {
                    for(int j=0; j<listBoxRight3UserWord.Items.Count; j++)
                    {
                        if(UserRelatedTag[i].ToString() == listBoxRight3UserWord.Items[j].ToString())
                        {
                            userTagCount[j]++;
                            find = 1;
                            break;
                        }
                    }
                    if (find == 0)
                    {
                        listBoxRight3UserWord.Items.Add(UserRelatedTag[i]);
                        userTagCount[listBoxRight3UserWord.Items.Count - 1] = 1;
                    }
                    find = 0;
                }
            }
            
            listBoxRight3SuggestWord.Items.Clear();
            listBoxRight3SuggestWord.BackColor = Color.White;
            int[] suggestTagCount;
            suggestTagCount = new int[dataGridView10.RowCount];
            if (dataGridView10 != null)
            {
                for (int i = 0, find = 0; i < dataGridView10.RowCount; i++)
                {
                    for (int j = 0; j < listBoxRight3SuggestWord.Items.Count; j++)
                    {
                        if (dataGridView10.Rows[i].Cells[3].Value.ToString() == listBoxRight3SuggestWord.Items[j].ToString())
                        {
                            suggestTagCount[j]++;
                            find = 1;
                            break;
                        }
                    }
                    if (find == 0)
                    {
                        listBoxRight3SuggestWord.Items.Add(dataGridView10.Rows[i].Cells[3].Value.ToString());
                        suggestTagCount[listBoxRight3SuggestWord.Items.Count - 1] = 1;
                    }
                    find = 0;
                }
            }
            //開始計算評估值
            int jaccardNum = System.Convert.ToInt32(textBoxRight3JaccardNum.Text);
            double jaccard = 0;
            double k = 0;
            double l = 0;
            double sameWordNum = 0;
            for (int i = 0; i < listBoxRight3UserWord.Items.Count && i < jaccardNum; i++)
            {
                l = 0;
                for (int j = 0; j < listBoxRight3SuggestWord.Items.Count && j < jaccardNum; j++)
                {
                    if(listBoxRight3SuggestWord.Items[j].ToString() == listBoxRight3UserWord.Items[i].ToString())
                    {
                        sameWordNum++;
                    }

                    l++;
                }
                k++;
            }
            jaccard = sameWordNum / (k + l - sameWordNum);
            labelJaccard.Text = jaccard.ToString("0.##0");

            for (int i = 0; i < listBoxRight3UserWord.Items.Count; i++)
            {
                listBoxRight3UserWord.Items[i] = listBoxRight3UserWord.Items[i] + "[" + userTagCount[i] + "]";
            }
            for (int i = 0; i < listBoxRight3SuggestWord.Items.Count; i++)
            {
                listBoxRight3SuggestWord.Items[i] = listBoxRight3SuggestWord.Items[i] + "[" + suggestTagCount[i] + "]";
            }
            buttonRight2ToRight_Click(sender, e);
        }

        private void 離開ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBoxRight3JaccardNum_Click(object sender, EventArgs e)
        {
            textBoxRight3JaccardNum.Text = "";
        }

        

        

        
    }
}
