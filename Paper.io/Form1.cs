using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paper.io
{
    public partial class Form1 : Form
    {
        public const int ts = 30;
        Rectangle[,] canvas = new Rectangle[30, 50];

        public Player[] players = new Player[]
        {
            new Player
            {
                number = 0,
                keys = new Keys[5] { Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.NumPad5 },
                                    //Player, area, line
                color = new Color[] { Color.Blue, Color.DarkBlue, Color.FromArgb(255, 135, 223, 255) },
            },
            new Player
            {
                number = 1,
                keys = new Keys[5] { Keys.A, Keys.D, Keys.W, Keys.S, Keys.D5 },
                color = new Color[] { Color.Red, Color.DarkRed, Color.OrangeRed },
            },
        };

        public Form1()
        {
            InitializeComponent();

            //Tell Player instances what the other player is
            players[0].otherP = players[1];
            players[1].otherP = players[0];

            //Create canvas and pointArrayBackup
            for (int y = 0; y < canvas.GetLength(0); y++)
            {
                for (int x = 0; x < canvas.GetLength(1); x++)
                {
                    canvas[y, x] = new Rectangle(x* ts, y* ts, ts, ts);
                    if (x < canvas.GetLength(1) - 10 && y < canvas.GetLength(0) - 10)
                        Player.pointListBackup.Add(new Point(canvas[y, x].X / ts, canvas[y, x].Y / ts));
                }
            }

            Width = canvas.GetLength(1) * ts + 16;
            Height = canvas.GetLength(0) * ts + 39;

            //Get initial area for players
            players[0].Reset(canvas);
            players[1].Reset(canvas);

            Timer timer = new Timer { Interval = 100, Enabled = true };
            timer.Tick += Timer_Tick;
            Paint += Form1_Paint;
            KeyDown += Form1_KeyPress;
        }

        private void Form1_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                Player.showAlgo = Player.showAlgo == true ? false : true;
            else
            {
                foreach (Player player in players)
                {
                    if (!player.movementBlocked)
                    {
                        if (e.KeyCode == player.keys[0])
                        {
                            if (player.vector.X != 1)
                            {
                                player.vector.X = -1;
                                player.vector.Y = 0;
                                player.movementBlocked = true;
                            }
                        }
                        else if (e.KeyCode == player.keys[1])
                        {
                            if (player.vector.X != -1)
                            {
                                player.vector.X = 1;
                                player.vector.Y = 0;
                                player.movementBlocked = true;
                            }
                        }
                        else if (e.KeyCode == player.keys[2])
                        {
                            if (player.vector.Y != 1)
                            {
                                player.vector.X = 0;
                                player.vector.Y = -1;
                                player.movementBlocked = true;
                            }
                        }
                        else if (e.KeyCode == player.keys[3])
                        {
                            if (player.vector.Y != -1)
                            {
                                player.vector.X = 0;
                                player.vector.Y = 1;
                                player.movementBlocked = true;
                            }
                        }
                        else if (e.KeyCode == player.keys[4])
                        {
                            player.frozen = player.frozen == true ? false : true;
                        }
                    }

                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            ////Draw canvas
            //for (int y = 0; y < canvas.GetLength(0); y++)
            //{
            //    for (int x = 0; x < canvas.GetLength(1); x++)
            //    {
            //        e.Graphics.DrawRectangle(Pens.Black, canvas[y, x]);
            //    }
            //}
            //Draw player area
            foreach (Player player in players)
            {
                foreach (Rectangle rec in player.area)
                {
                    e.Graphics.FillRectangle(new SolidBrush(player.color[1]), rec);
                }
            }
            foreach (Player player in players)
            {
                //Draw player line
                for (int r = 0; r < player.line.Count; r++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(player.color[2]), player.line[r]);
                }
                //Draw Player
                e.Graphics.FillRectangle(new SolidBrush(player.color[0]), player.rec);
                //Draw temp recs
                foreach (Rectangle rec in player.temp)
                {
                    e.Graphics.FillRectangle(Brushes.Red, rec);
                }
            }
            //Draw scores
            foreach (Player player in players)
            {
                e.Graphics.DrawString(player.area.Count.ToString(), Font, Brushes.Red, player.number * (Width - 90) , 0);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach(Player player in players)
            {
                player.movementBlocked = false;
                if (!player.frozen)
                {
                    player.rec.X += player.vector.X * ts;
                    player.rec.Y += player.vector.Y * ts;

                    //Out of range check for new player position
                    if (player.rec.X / ts >= canvas.GetLength(1) || player.rec.Y / ts >= canvas.GetLength(0) ||
                        player.rec.X / ts < 0 || player.rec.Y / ts < 0)
                    {
                        player.Reset(canvas);
                        continue;
                    }

                    Rectangle canvasRec = canvas[player.rec.Y / ts, player.rec.X / ts];
                    //Check line collisions
                    foreach(Player _player in players)
                        if (_player.line.Contains(canvasRec))
                        {
                            _player.Reset(canvas);
                            canvasRec = canvas[player.rec.Y / ts, player.rec.X / ts]; //playerPosition (canvasRec) was changed by the reset, so update it for the following checks
                        }
                
                    if (player.area.Contains(canvasRec) && player.inside == false) //I entered my area from outside
                    {
                        player.inside = true;
                        player.AddArea(this, canvas);
                    }
                    if (!player.area.Contains(canvasRec)) //I'm outside my area
                    {
                        player.inside = false;
                        player.line.Add(canvasRec);
                    }
                }
            }

            Refresh();
        }
    }
}
