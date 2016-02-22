using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flowcort
{
    public partial class DataEntry : Form
    {
        private string _DBFile = "FlowcortData";
        private string _ConnectionString = @"data source=|DataDirectory|FlowcortData.fct";
        public string ConnectionString{
            get{ return _ConnectionString; }
            set{ 
                _ConnectionString = value;
                var m = Regex.Match( _ConnectionString, @"(.*\|DataDirectory\|\\*)(.*)(.fct)");
                _DBFile = m.Groups[2].Value.ToString();

                _ImageFolder = AppDomain.CurrentDomain.BaseDirectory + _DBFile + "\\FullSize";
                _ThumbnailFolder = AppDomain.CurrentDomain.BaseDirectory + _DBFile +"\\Thumbnail";
            }
        }
            // = @"data source=|DataDirectory|FlowcortData.fct";

        private long _NextItemPosition = 0;
        private long _NextSectionPosition = 0;

        private string _ImageFolder;
        private string _ThumbnailFolder;

        public long NextItemPosition { get { _NextItemPosition += 10; return _NextItemPosition; } set { _NextItemPosition = value; } }
        public long NextSectionPosition { get { _NextSectionPosition += 10; return _NextSectionPosition; } set { _NextSectionPosition = value; } }

        public DataEntry()
        {
            InitializeComponent();         
        }

        private void DataEntry_Load(object sender, EventArgs e)
        {
            this.pictureBox1.AllowDrop = true;
            this.pictureBox2.AllowDrop = true;
        }

        private void DataEntry_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.flowcortDataDataSet.HasChanges())
            {
                e.Cancel = !saveData();
            }
        }

        private void sectionBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.sectionBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.flowcortDataDataSet);

        }

        private void btnPrior_Click(object sender, EventArgs e)
        {
            sectionBindingSource.MovePrevious();
            itemDataGridView.Focus();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            sectionBindingSource.MoveNext();
            itemDataGridView.Focus();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (flowcortDataDataSet.HasChanges())
                saveData();

            DataRowView drv = (DataRowView)sectionBindingSource.AddNew();
            drv.Row["Position"] = NextSectionPosition;
            txtbxSection.Focus();
        }

        private long GetMaxPosition(String tableName)
        {
            long result = 0;

            using (SQLiteConnection con = new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT MAX(Position) FROM " + tableName;

                con.Open();
                var obj = cmd.ExecuteScalar();
                if (obj != null)
                    result = Convert.ToInt64(obj);
            }

            return result;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            sectionBindingSource.RemoveCurrent();
            itemDataGridView.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveData())
            {
                btnPrior.Enabled = true;
                btnNext.Enabled = true;
                btnAdd.Enabled = true;
                btnDelete.Enabled = true;

            }
        }

        private Boolean saveData()
        {
            try
            {
                this.Validate();
                this.itemBindingSource.EndEdit();
                this.sectionBindingSource.EndEdit();
                this.tableAdapterManager.UpdateAll(this.flowcortDataDataSet);

                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Update failed with : " + ex.Message);
                return false;
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (flowcortDataDataSet.HasChanges())
                saveData();

            DataRowView drv = (DataRowView)itemBindingSource.AddNew();
            drv.Row["Position"] = NextItemPosition;
            itemDataGridView.Focus();
        }

        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            itemBindingSource.RemoveCurrent();
            itemDataGridView.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            showImage("Image1");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            showImage("Image2");
        }

        private void showImage(string columnName)
        {
            string fname = _ImageFolder + "\\"  + ((DataRowView)itemBindingSource.Current).Row[columnName].ToString();

            if (File.Exists(fname))
            {
                showImageFullSize(fname);
            }
        }

        private void showImageFullSize(string imgLocn)
        {
            using (Form form = new Form())
            {
                Bitmap img = new Bitmap(imgLocn);

                form.StartPosition = FormStartPosition.CenterScreen;
                form.Text = "Click or close to continue ...";
                form.ClientSize = img.Size;

                PictureBox pb = new PictureBox();
                pb.Dock = DockStyle.Fill;
                pb.Click += pb_Click;
                pb.Image = img;

                form.Controls.Add(pb);
                form.ShowDialog();
            }
        }

        void pb_Click(object sender, EventArgs e)
        {
            ((PictureBox)sender).FindForm().Close();
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            SaveAndDrawImage(pictureBox1, "Image1", e);
        }

        private string qualifyFileName(string fname)
        {
            fname = _ThumbnailFolder + "\\" + fname;

            if (File.Exists(fname))
                return fname;
            else
                return "";
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        // todo review all drag and drop. We should only allow it from the expected folders OR save the file to the expected folders
        private void pictureBox2_DragDrop(object sender, DragEventArgs e)
        {
            SaveAndDrawImage(pictureBox2, "Image2", e);
        }

        private void SaveAndDrawImage(PictureBox pb, String dataColumn, DragEventArgs e)
        {
            string[] fname = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            string fn = Path.GetFileName( fname[0] );

            if (!FileExistsInImagesFolder(fname[0]))
            {
                using (Bitmap bmp = new Bitmap(fname[0]))
                    fn = ConvertAndSaveImage(bmp);
            }

            ((DataRowView)itemBindingSource.Current).Row[dataColumn] = fn;
            pb.ImageLocation = qualifyFileName(fn);
        }

        private bool FileExistsInImagesFolder(String fname)
        {
            string fn = Path.GetFileName(fname);
            return File.Exists( _ThumbnailFolder + "\\" + fn );
        }

        private void pictureBox2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void btnDeleteImage1_Click(object sender, EventArgs e)
        {
            ((DataRowView)itemBindingSource.Current).Row["Image1"] = "";
            pictureBox1.ImageLocation = "";
            itemDataGridView.Focus();
        }

        private void btnDeleteImage2_Click(object sender, EventArgs e)
        {
            ((DataRowView)itemBindingSource.Current).Row["Image2"] = "";
            pictureBox2.ImageLocation = "";
            itemDataGridView.Focus();
        }

        private void itemBindingSource_PositionChanged(object sender, EventArgs e)
        {
            updateImagePicture();
        }

        private void updateImagePicture()
        {
            pictureBox1.ImageLocation = "";
            pictureBox2.ImageLocation = "";

            if (itemBindingSource.Current != null)
            {
                string img1Locn = ((DataRowView)itemBindingSource.Current).Row["Image1"].ToString();
                string img2Locn = ((DataRowView)itemBindingSource.Current).Row["Image2"].ToString();

                pictureBox1.ImageLocation = qualifyFileName(img1Locn);
                pictureBox2.ImageLocation = qualifyFileName(img2Locn);
            }
        }

        private void DataEntry_Shown(object sender, EventArgs e)
        {
            itemTableAdapter.Connection.ConnectionString = ConnectionString;
            sectionTableAdapter.Connection.ConnectionString = ConnectionString;

            _NextItemPosition = GetMaxPosition("Item");
            _NextSectionPosition = GetMaxPosition("Section");

            this.sectionTableAdapter.Fill(this.flowcortDataDataSet.Section);
            this.itemTableAdapter.Fill(this.flowcortDataDataSet.Item);

            this.itemDataGridView.Focus();
        }

        private void sectionBindingSource_AddingNew(object sender, AddingNewEventArgs e)
        {
            btnPrior.Enabled = false;
            btnNext.Enabled = false;
            btnAdd.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void itemDataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach(DataGridViewRow row in itemDataGridView.Rows)
                ColorizeRow(row);
        }

        private void ColorizeRow(DataGridViewRow row)
        {
            if (row != null)
            {
                bool evnt = Convert.ToBoolean(row.Cells["Event"].Value);
                bool subSection = Convert.ToBoolean(row.Cells["Subsection"].Value);

                if (evnt || subSection)
                {
                    row.DefaultCellStyle.ForeColor = SystemColors.WindowText;

                    if (evnt)
                    {
                        row.DefaultCellStyle.BackColor = Color.PapayaWhip;
                    }

                    if (subSection)
                    {
                        row.DefaultCellStyle.BackColor = Color.WhiteSmoke;
                    }
                }
            }


        }

        private void itemDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            itemDataGridView.Rows[e.RowIndex].Cells[3].Selected = true;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu m = new ContextMenu();
                MenuItem paste = new MenuItem();

                if (Clipboard.ContainsImage())
                {
                    string img = ((sender as PictureBox) == pictureBox1) ? "Image1" : "Image2";
                    paste.Text = "Paste to " + img;
                    paste.Click += paste_Click;
                }
                else
                {
                    paste.Text = "Nothing on clipboard";
                    paste.Enabled = false;
                }

                m.MenuItems.Add(paste);
                m.Show(sender as PictureBox, new Point(e.X, e.Y));
            }
        }

        private void paste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                string fname = ConvertAndSaveImage(Clipboard.GetImage());

                MenuItem mi = sender as MenuItem;
                PictureBox pb;
                pb = (mi.Text == "Paste to Image1") ? pictureBox1 : pictureBox2;

                //pb.Image = FlowcortImage.FixedSize(Clipboard.GetImage(), 208, 117);
                //pb.Image.Save(dirThumbnail + "\\" + rnd, System.Drawing.Imaging.ImageFormat.Jpeg);

                if (pb == pictureBox1)
                {
                    ((DataRowView)itemBindingSource.Current).Row["Image1"] = fname;
                    pictureBox1.ImageLocation = qualifyFileName(fname);
                }
                else
                {
                    ((DataRowView)itemBindingSource.Current).Row["Image2"] = fname;
                    pictureBox2.ImageLocation = qualifyFileName(fname);
                }
            }
        }

        private string ConvertAndSaveImage(Image p)
        {
            string result = "";

            if (p != null)
            {
                string dirImages = AppDomain.CurrentDomain.BaseDirectory + _DBFile;
                string dirFullSize = dirImages + "\\FullSize";
                string dirThumbnail = dirImages + "\\Thumbnail";

                System.IO.Directory.CreateDirectory(dirImages);
                System.IO.Directory.CreateDirectory(dirFullSize);
                System.IO.Directory.CreateDirectory(dirThumbnail);

                string rnd = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".jpg";
                p.Save(dirFullSize + "\\" + rnd, System.Drawing.Imaging.ImageFormat.Jpeg);

                Image tn = FlowcortImage.FixedSize( p, 208, 117 );
                tn.Save(dirThumbnail + "\\" + rnd, System.Drawing.Imaging.ImageFormat.Jpeg);

                result = rnd;
            }

            return result;
        }

    }
}
