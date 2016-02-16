using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flowcort
{
    public partial class DataEntry : Form
    {
        public DataEntry()
        {
            InitializeComponent();
        }

        private void DataEntry_Load(object sender, EventArgs e)
        {
            var dataPath = AppDomain.CurrentDomain.GetData("DataDirectory");

            itemTableAdapter.Connection.ConnectionString = @"data source=|DataDirectory|FlowcortData.fct";
            sectionTableAdapter.Connection.ConnectionString = @"data source=|DataDirectory|FlowcortData.fct";

            this.itemTableAdapter.Fill(this.flowcortDataDataSet.Item);
            this.sectionTableAdapter.Fill(this.flowcortDataDataSet.Section);

            this.pictureBox1.AllowDrop = true;
            this.pictureBox2.AllowDrop = true;

            // this.itemDataGridView.Sort(itemDataGridView.Columns["Position"], ListSortDirection.Ascending);
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
            sectionBindingSource.AddNew();
            txtbxSection.Focus();
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
            itemBindingSource.AddNew();
            itemDataGridView.Focus();
        }

        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            itemBindingSource.RemoveCurrent();
            itemDataGridView.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            showImage("Image1", ".png");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            showImage("Image2", ".png");
        }

        private void showImage(string columnName, string imgType)
        {
            string fname = "Images\\" + ((DataRowView)itemBindingSource.Current).Row[columnName].ToString() + imgType;

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
            string[] fname = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            string fn = System.IO.Path.GetFileNameWithoutExtension(fname[0]);

            ((DataRowView)itemBindingSource.Current).Row["Image1"] = fn;
            pictureBox1.ImageLocation = qualifyFileName(fn + ".jpg");
        }

        private string qualifyFileName(string fname)
        {
            if (fname != ".jpg")
            {
                return "ImagesTN\\" + fname;
            }
            else
            {
                return "";
            }
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void pictureBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] fname = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            string fn = System.IO.Path.GetFileNameWithoutExtension(fname[0]);

            ((DataRowView)itemBindingSource.Current).Row["Image2"] = fn;
            pictureBox2.ImageLocation = qualifyFileName(fn + ".jpg");
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
                string img1Locn = ((DataRowView)itemBindingSource.Current).Row["Image1"].ToString() + ".jpg";
                string img2Locn = ((DataRowView)itemBindingSource.Current).Row["Image2"].ToString() + ".jpg";

                pictureBox1.ImageLocation = qualifyFileName(img1Locn);
                pictureBox2.ImageLocation = qualifyFileName(img2Locn);
            }
        }

        private void DataEntry_Shown(object sender, EventArgs e)
        {
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
            foreach (DataGridViewRow row in itemDataGridView.Rows)
            {
                if (Convert.ToBoolean(row.Cells["Event"].Value))
                {
                    row.DefaultCellStyle.BackColor = Color.PapayaWhip;
                }

                if (Convert.ToBoolean(row.Cells["Subsection"].Value))
                {
                    row.DefaultCellStyle.BackColor = Color.WhiteSmoke;
                }
            }
        }

    }
}
