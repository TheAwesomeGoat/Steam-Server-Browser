using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetSteamGameServers
{
    public class Invokers
    {
        public void LabelTextInvoker(Label label, string text)
        {
            label.Invoke(new MethodInvoker(delegate
            {
                label.Text = text;
            }));
        }
        public void AddDataItem(string[] items,DataGridView dataGrid)
        {
            dataGrid.Invoke(new MethodInvoker(delegate
            {
                dataGrid.Rows.Add(items);
            }));
        }
        public void AddListItem(string[] items, ListView view)
        {
            view.Invoke(new MethodInvoker(delegate
            {
                view.Items.Add(new ListViewItem(items));
            }));
        }
        public void EnableObject(dynamic Obj, bool Enable)
        {
            Obj.Invoke(new MethodInvoker(delegate
            {
                Obj.Enabled = Enable;
            }));
        }
        public void TextboxAddText(TextBox txtbx, string text)
        {
            txtbx.Invoke(new MethodInvoker(delegate
            {
                txtbx.Text += text + "\r\n";
            }));
        }
    }
}
