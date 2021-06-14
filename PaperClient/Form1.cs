using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleTCP;

namespace PaperClient
{
    public partial class Form1 : Form
    {
        SimpleTcpClient client;
        byte[] serverData;
        Color[] colors = new Color[7] { Color.White, Color.Blue, Color.DarkBlue, Color.FromArgb(255, 135, 223, 255), Color.Red, Color.DarkRed, Color.OrangeRed };
        Color[] color1 = new Color[] { Color.Blue, Color.DarkBlue, Color.FromArgb(255, 135, 223, 255) };
        Color[] color2 = new Color[] { Color.Red, Color.DarkRed, Color.OrangeRed };
        const int ts = 30;
        Size gridSize = new Size(50,30);

        public Form1()
        {
            InitializeComponent();

            client = new SimpleTcpClient();
            client.DataReceived += Client_DataReceived;

            Paint += Form1_Paint;
            KeyDown += SendMessage;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(serverData != null)
            {
                // Paint the game based on the data from server
                for (int i = 0; i < serverData.Length; i++)
                {
                    Color color = colors[serverData[i]];
                    Rectangle rectangle = new Rectangle(i % gridSize.Width, i / gridSize.Width, ts, ts);
                    e.Graphics.FillRectangle(new SolidBrush(color), rectangle);
                }
            }
        }

        private void Client_DataReceived(object sender, byte[] e)
        {
            // Save sever data
            serverData = e;
            // Call Paint function
            Refresh();
        }

        private void Connect_click(object sender, EventArgs e)
        {
            client.Connect(IP_textbox.Text, Convert.ToInt32(Port_textbox.Text));
        }

        private void SendMessage(object sender, KeyEventArgs e)
        {
            string key;
            client.Write(key);
        }
    }
}
