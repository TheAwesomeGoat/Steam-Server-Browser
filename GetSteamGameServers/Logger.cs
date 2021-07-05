using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetSteamGameServers
{
    public class Logger
    {
        public TextBox LogTextBox { get; set; } 
        private void TextBoxTextInvoker(TextBox textBox, string text)
        {
            textBox.Invoke(new MethodInvoker(delegate
            {
                textBox.Text += text+"\r\n";
            }));
        }

        public void Log(string text)
        {
            TextBoxTextInvoker(LogTextBox, text);
        } 
    }
}
