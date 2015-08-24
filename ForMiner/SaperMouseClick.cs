using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ForMiner
{
    class SaperMouseClick
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        //параметры ячеек сапера, ширина, высота, расстояние между ячейками, координаты первой ячейки
        private int WINDOW_WIDTH;
        private int WINDOW_HEIGHT;
        private int WINDOW_DIST;
        private int FIRST_WINDOW_X;
        private int FIRST_WINDOW_Y;
        private int SCREEN_WIDTH;
        private int SCREEN_HEIGHT;
        private int SPEED_ADDER;

        //координаты верхнего правого угла окна игры
        public int leftSideOfSaperWindow;
        public int topSideOfSaperWindow;
        public Rectangle rect;

        public SaperMouseClick(Rectangle rectangle)
        {
            leftSideOfSaperWindow = rectangle.Left;
            topSideOfSaperWindow = rectangle.Top;
            rect = rectangle;
            if (Form1.version == Form1.SAPER_TYPE.XP)
            {
                WINDOW_WIDTH = 13;
                WINDOW_HEIGHT = 13;
                WINDOW_DIST = 3;
                FIRST_WINDOW_X = 16;
                FIRST_WINDOW_Y = 105;
                SPEED_ADDER = 0;
            }
            if (Form1.version == Form1.SAPER_TYPE.Seven)
            {
                WINDOW_WIDTH = 12;
                WINDOW_HEIGHT = 12;
                WINDOW_DIST = 6;
                FIRST_WINDOW_X = 41;
                FIRST_WINDOW_Y = 83;
                SPEED_ADDER = 10;
            }
            SCREEN_WIDTH = SystemInformation.PrimaryMonitorSize.Width;
            SCREEN_HEIGHT = SystemInformation.PrimaryMonitorSize.Height;
        }

        public void LeftClick(int y, int x)
        {
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

            mouse_event((int)(MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

        }

        public void RightClick(int y, int x, SaperCell[,] field)
        {

            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);


            mouse_event((int)(MouseEventFlags.RIGHTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

            mouse_event((int)(MouseEventFlags.RIGHTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
            if (Form1.version == Form1.SAPER_TYPE.XP)
            {
                if (SaperImageDetection.IfAnyChanges(field, rect, y, x) == false)
                {
                    FirstTimeRightClick(y, x, field);
                }
            }
            if (Form1.version == Form1.SAPER_TYPE.Seven)
            {
                if (SaperImageDetectionWindows7.IfAnyChanges(field, rect, y, x) == false)
                {
                    FirstTimeRightClick(y, x, field);
                }
            }

        }

        //по неизвестным причинам при первом нажатии правой кнопки мыши, метод RightClick
        //неправильно срабатывает, для этого вставлен данный костыль
        public void FirstTimeRightClick(int y, int x, SaperCell[,] field)
        {
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            mouse_event((int)(MouseEventFlags.RIGHTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
            (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
            mouse_event((int)(MouseEventFlags.RIGHTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

            mouse_event((int)(MouseEventFlags.RIGHTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

        }

        public void VictoryWin7()
        {
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                  (topSideOfSaperWindow + FIRST_WINDOW_Y + 12 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 12 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 12 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

            mouse_event((int)(MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + 12 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);
        }

        //одновременное нажатие левой и правой кнопками мыши
        public void LeftAndRightClick(int y, int x)
        {
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            mouse_event((int)(MouseEventFlags.RIGHTDOWN | MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
            (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / 768, 0, 0);
            mouse_event((int)(MouseEventFlags.RIGHTDOWN | MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

            mouse_event((int)(MouseEventFlags.RIGHTUP | MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST) + WINDOW_WIDTH / 2) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + y * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
        }

        //нажатие на желтую рожицу для новой игры
        public void NewGame()
        {
            if (Form1.version == Form1.SAPER_TYPE.XP)
            {
                mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + 15 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                    (topSideOfSaperWindow + FIRST_WINDOW_Y - 2 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

                mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 15 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                    (topSideOfSaperWindow + FIRST_WINDOW_Y - 2 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
                mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 15 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                    (topSideOfSaperWindow + FIRST_WINDOW_Y - 2 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

                System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

                mouse_event((int)(MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + 15 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                   (topSideOfSaperWindow + FIRST_WINDOW_Y - 2 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);
            }
            if (Form1.version == Form1.SAPER_TYPE.Seven)
            {
                mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                   (topSideOfSaperWindow + FIRST_WINDOW_Y + 11 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

                mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                    (topSideOfSaperWindow + FIRST_WINDOW_Y + 11 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);
                mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                    (topSideOfSaperWindow + FIRST_WINDOW_Y + 11 * (WINDOW_HEIGHT + WINDOW_DIST) + WINDOW_HEIGHT / 2) * 65536 / SCREEN_HEIGHT, 0, 0);

                System.Threading.Thread.Sleep(10 * (Form1.mouseSpeed + SPEED_ADDER));

                mouse_event((int)(MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + 22 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                   (topSideOfSaperWindow + FIRST_WINDOW_Y + 11 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);
            }
        }

        //нажатие на кнопку OK после установления рекорда
        public void RecordConfirm()
        {
            System.Threading.Thread.Sleep(3000);
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + 4 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);

            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 4 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 4 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(100);

            mouse_event((int)(MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + 4 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(3000);


            //нажатие на еще 1 всплывший ОК
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), (leftSideOfSaperWindow + FIRST_WINDOW_X + 11 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);

            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 11 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), (leftSideOfSaperWindow + FIRST_WINDOW_X + 11 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
                (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);

            System.Threading.Thread.Sleep(100);

            mouse_event((int)(MouseEventFlags.LEFTUP), (leftSideOfSaperWindow + FIRST_WINDOW_X + 11 * (WINDOW_WIDTH + WINDOW_DIST)) * 65536 / SCREEN_WIDTH,
               (topSideOfSaperWindow + FIRST_WINDOW_Y + 8 * (WINDOW_HEIGHT + WINDOW_DIST)) * 65536 / SCREEN_HEIGHT, 0, 0);
        }
    }
}
