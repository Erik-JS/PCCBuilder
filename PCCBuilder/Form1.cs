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

namespace PCCBuilder
{
    public partial class Form1 : Form
    {

        public static Form1 mainform;

        public static Color errorColor = Color.FromArgb(255, 88, 88);

        public struct ColoredItem
        {
            public Color foregroundColor;
            public string text;
        }

        public Form1()
        {
            InitializeComponent();
            mainform = this;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstBox_DrawItem);
            // DrawMode->OwnerDrawFixed = deal with the item height yourself
            this.listBox1.ItemHeight = this.listBox1.Font.Height;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openPCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "PCC|*.pcc" };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            PCC MyPCC = PCC.LoadFromFile(ofd.FileName);
            if (MyPCC == null)
                return;
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "XML|*.xml" };
            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            if (MyPCC.ExportXML(sfd.FileName))
                MessageBox.Show("Done.");
            else
                MessageBox.Show("Error.");
        }

        public static void Log(string text, object textColor = null)
        {
            ColoredItem ci;
            ci.foregroundColor = textColor != null ? (Color)textColor: mainform.listBox1.ForeColor;
            ci.text = String.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, text);
            mainform.listBox1.Items.Add(ci);
            mainform.listBox1.SelectedIndex = mainform.listBox1.Items.Count - 1;
            mainform.listBox1.SelectedIndex = -1;
            mainform.listBox1.Refresh();
        }

        //public static int LogDirect(ColoredItem coloredItem)
        //{
        //    mainform.listBox1.Items.Add(coloredItem);
        //    mainform.listBox1.Refresh();
        //    return mainform.listBox1.Items.Count - 1;
        //}

        //public static void UpdateLog(int index, ColoredItem coloredItem)
        //{
        //    mainform.listBox1.Items[index] = coloredItem;
        //    mainform.listBox1.Refresh();
        //}

        // Based on: http://www.thescarms.com/dotNet/CustomListBox.aspx
        private void lstBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            // sender: control
            ListBox lb = (ListBox)sender;
            // current item: (ColoredItem)object
            ColoredItem ci = (ColoredItem)lb.Items[e.Index];
            // draw item's background
            e.DrawBackground();
            // check if item is selected
            Color drawColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? SystemColors.HighlightText : ci.foregroundColor;
            Brush fgBrush = new SolidBrush(drawColor);
           // draw string with color
            e.Graphics.DrawString(ci.text, e.Font, fgBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void openXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "XML|*.xml" };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "PCC|*.pcc" };
            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            PCC.ConvertXMLToPCC(ofd.FileName, sfd.FileName);
        }

    }
}
