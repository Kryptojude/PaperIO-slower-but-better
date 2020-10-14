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
    public partial class Player
    {
        public int number;
        public Color[] color;
        public Rectangle rec;
        public Point vector = new Point(1, 0);
        public List<Rectangle> area = new List<Rectangle>();
        public List<Rectangle> line = new List<Rectangle>();
        public List<Rectangle> temp = new List<Rectangle>();
        public bool inside = true;
        public bool movementBlocked = false;
        public static bool showAlgo = false;
        public Keys[] keys;
        static Point[] directions = new Point[4] { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0) }; //Up, right, down, left
        public Player otherP;
        public static List<Point> pointListBackup = new List<Point>();
        static List<Point> pointList = new List<Point>();
        public bool frozen = false;
        const int ts = Form1.ts;
        static Random random = new Random();

        public void AddArea(Form form, Rectangle[,] canvas)
        {
            foreach (Rectangle lineRec in line) //Go through each rec in line, starting at the first
            {
                foreach (Point dir in directions) //Go through the two oppo point Arrays
                {
                    //OOB check
                    int yCoord = lineRec.Y / ts + dir.Y;
                    int xCoord = lineRec.X / ts + dir.X;
                    if (xCoord >= canvas.GetLength(1) || yCoord >= canvas.GetLength(0) ||
                        xCoord < 0 || yCoord < 0)
                        continue; //next direction

                    Rectangle adjRec = canvas[yCoord, xCoord];
                    //Check if adcRec is empty
                    if (area.Contains(adjRec) || line.Contains(adjRec) || temp.Contains(adjRec))
                        continue; //next direction

                    //Start fillAlgorithm based on empty tile
                    temp.Add(adjRec);
                    if (FillAlgorithm(form, canvas)) //If it returns true, then no out of bounds was found
                        FinishAddArea(canvas);
                }
            }

            //All lineRecs have been gone through
            //and all encapsulated areas have been added to area
            //Check if line went over enemy tiles
            foreach (Rectangle lineRec in line)
            {
                if (otherP.area.Contains(lineRec))
                {
                    otherP.area.Remove(lineRec);
                }
            }
            area.AddRange(line);
            line.Clear();

            otherP.DisconnectedCheck(form, canvas);
        }

        private void DisconnectedCheck(Form form, Rectangle[,] canvas)
        {
            //Get random rec of area
            Rectangle rec = area[random.Next(area.Count)];
            //Start fillAlgo from this rec
            temp.Add(rec);
            for (int r = 0; r < temp.Count; r++)
            {
                Rectangle tempRec = temp[r];
                foreach (Point dir in directions)
                {
                    int newY = tempRec.Y / ts + dir.Y;
                    int newX = tempRec.X / ts + dir.X;
                    if (newY < 0 || newX < 0 || newY >= canvas.GetLength(0) || newX >= canvas.GetLength(1)) //Reached out of bounds?
                        continue; //Check next direction
                    else
                    {
                        Rectangle adjRec = canvas[newY, newX];
                        if (area.Contains(adjRec) && !temp.Contains(adjRec)) //AdjRec is part of area and not also of temp
                        {
                            temp.Add(adjRec);
                            if (showAlgo)
                                form.Refresh();
                        }
                    }
                }
            }
            //Check which area recs have not been filled
            List<Rectangle> areaNotFilled = new List<Rectangle>();
            foreach (Rectangle areaRec in area)
            {
                if (!temp.Contains(areaRec))
                {
                    areaNotFilled.Add(areaRec);
                }
            }
            if (areaNotFilled.Count != 0)
            {
                //check which area part the player is in and delete other part
                if (temp.Contains(this.rec))
                {
                    foreach (Rectangle areaNotFilledRec in areaNotFilled)
                    {
                        area.Remove(areaNotFilledRec);
                    }
                }
                else
                {
                    foreach (Rectangle tempRec in temp)
                    {
                        area.Remove(tempRec);
                    }
                }
            }

            temp.Clear();
        }

        private bool FillAlgorithm(Form form, Rectangle[,] canvas)
        {
            for (int r = 0; r < temp.Count; r++)
            {
                Rectangle tempRec = temp[r];
                foreach (Point dir in directions)
                {
                    int newY = tempRec.Y / ts + dir.Y;
                    int newX = tempRec.X / ts + dir.X;
                    if (newY < 0 || newX < 0 || newY >= canvas.GetLength(0) || newX >= canvas.GetLength(1)) //Reached out of bounds? 
                    {
                        temp.Clear();
                        return false; //Then abort the filling, the next empty adjRec of the lineRec will be used to fill from
                    }
                    else
                    {
                        Rectangle adjRec = canvas[tempRec.Y / ts + dir.Y, tempRec.X / ts + dir.X];
                        if (!area.Contains(adjRec) && !line.Contains(adjRec) && !temp.Contains(adjRec)) //AdjRec is empty
                        {
                            temp.Add(adjRec);
                            if (showAlgo)
                                form.Refresh();
                        }
                    }
                }
            }
            return true;
        }

        //Is called when an area was filled successfully (no OOB)
        //Then it handles enemy tile within filled area
        //and adds the temp area to player.area and clears temp
        private void FinishAddArea(Rectangle[,] canvas)
        {
            //Check if enemy player rec was encapsulated
            if (temp.Contains(otherP.rec))
                otherP.Reset(canvas);
            //Check if enemy area tiles were added to temp
            foreach (Rectangle tempRec in temp)
            {
                if (otherP.area.Contains(tempRec))
                {
                    otherP.area.Remove(tempRec);
                }
            }

            area.AddRange(temp);
            temp.Clear();
        }

        public void Reset(Rectangle[,] canvas)
        {
            line.Clear(); //Remove this player's line
            //Restore the pointList array from pointListBackup, so it contains all possible starting positions again
            pointList.Clear(); 
            foreach (Point p in pointListBackup)
            {
                pointList.Add(p);
            }

            //Determine random starting position
            newStartPos:;
            Point r = pointList[random.Next(pointList.Count)];
            int xStart = r.X;
            int yStart = r.Y;
            pointList.Remove(r); //remove that pos from possible future starts

            if (pointList.Count == 0)
                GameOver();

            area.Clear();
            for (int y = yStart; y < yStart + 10; y++)
            {
                for (int x = xStart; x < xStart + 10; x++)
                {
                    if (otherP.area.Contains(canvas[y, x]) || otherP.line.Contains(canvas[y, x])) //Enemy found within new starting area?
                    {
                        //Then restart
                        goto newStartPos;
                    }
                    else
                        area.Add(canvas[y, x]);
                }
            }

            rec = area[50];
        }

        private void GameOver()
        {
            Application.Exit();
        }
    }
}
