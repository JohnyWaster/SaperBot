using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;


namespace ForMiner
{
    class SaperImageDetectionWindows7
    {
        private static int WINDOW_WIDTH7 = 12;
        private static int WINDOW_HEIGHT7 = 12;
        private static int WINDOW_DIST7 = 6;
        private static int FIRST_WINDOW_X7 = 41;
        private static int FIRST_WINDOW_Y7 = 83;

        private static int CORRELATION_THRESHOLD7 = 119;

        private static Image<Bgr, Byte>[][] cellsWin7 = new Image<Bgr, Byte>[10][];

        private static int[][] graysWin7 = new int[10][];

        public static string path = Application.StartupPath;

        private static void InitiateCellsWin7()
        {
            cellsWin7[0] = new Image<Bgr, byte>[2] { new Image<Bgr, Byte>(path + @"\images\9win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\9win7_2.jpg"),/* new Image<Bgr, Byte>(path + @"\9win7_3.jpg"), new Image<Bgr, Byte>(path + @"\9win7_4.jpg")*/ };
            cellsWin7[1] = new Image<Bgr, byte>[5] { new Image<Bgr, Byte>(path + @"\images\1win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\1win7_2.jpg"), new Image<Bgr, Byte>(path + @"\images\1win7_3.jpg"), new Image<Bgr, Byte>(path + @"\images\1win7_4.jpg"), new Image<Bgr, Byte>(path + @"\images\1win7_5.jpg") };
            cellsWin7[2] = new Image<Bgr, byte>[5] { new Image<Bgr, Byte>(path + @"\images\2win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\2win7_2.jpg"), new Image<Bgr, Byte>(path + @"\images\2win7_3.jpg"), new Image<Bgr, Byte>(path + @"\images\2win7_4.jpg"), new Image<Bgr, Byte>(path + @"\images\2win7_5.jpg") };
            cellsWin7[3] = new Image<Bgr, byte>[4] { new Image<Bgr, Byte>(path + @"\images\3win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\3win7_2.jpg"), new Image<Bgr, Byte>(path + @"\images\3win7_3.jpg"), new Image<Bgr, Byte>(path + @"\images\3win7_4.jpg") };
            cellsWin7[4] = new Image<Bgr, byte>[4] { new Image<Bgr, Byte>(path + @"\images\4win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\4win7_2.jpg"), new Image<Bgr, Byte>(path + @"\images\4win7_3.jpg"), new Image<Bgr, Byte>(path + @"\images\4win7_4.jpg") };
            cellsWin7[5] = new Image<Bgr, byte>[2] { new Image<Bgr, Byte>(path + @"\images\5win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\5win7_2.jpg") };
            cellsWin7[6] = new Image<Bgr, byte>[2] { new Image<Bgr, Byte>(path + @"\images\6win7_1.jpg"), new Image<Bgr, Byte>(path + @"\images\6win7_2.jpg") };
            cellsWin7[7] = new Image<Bgr, byte>[1] { new Image<Bgr, Byte>(path + @"\images\6win7_1.jpg") };
            cellsWin7[8] = new Image<Bgr, byte>[1] { new Image<Bgr, Byte>(path + @"\images\6win7_1.jpg") };
            cellsWin7[9] = new Image<Bgr, byte>[2] { new Image<Bgr, Byte>(path + @"\images\0win7_1.jpg")/*, new Image<Bgr, Byte>("0win7_2.jpg")*/, new Image<Bgr, Byte>(path + @"\images\0win7_3.jpg") };
            
            for (int i = 0; i < 10; ++i)
            {
                graysWin7[i] = new int[cellsWin7[i].Length];
                for (int j = 0; j < cellsWin7[i].Length; ++j)
                {                    
                    graysWin7[i][j] = GrayAverage( cellsWin7[i][j].Convert<Gray, Byte>() );
                }
            }
        }

        //картинка окна сапера
        private Image<Bgr, Byte> saperWindow;

        //матрица, показывающая поле сапера
        private int [,] saperField = new int[16, 30];


        //конструктор, принимающий изображение окна сапера и заполняющий соответствующую матрицу
        public SaperImageDetectionWindows7(Image<Bgr, Byte> image, int[,] tableOfNumbers)
        {
            InitiateCellsWin7();

            saperWindow = image;
            ImageToMatrix(image, tableOfNumbers);

        }

        //функция принимает на вход два окошка одинакового размера и сравнивает их
        private static bool Correlation(Image<Bgr, Byte> image1, Image<Bgr, Byte> image2, int gray)
        {
            int sum = 0;

            var gray1 = image1.Convert<Gray, Byte>();
            var gray2 = image2.Convert<Gray, Byte>();
            var normal1 = gray1.ThresholdBinary(new Gray( GrayAverage(gray1) ), new Gray(255));
            var normal2 = gray2.ThresholdBinary(new Gray(gray), new Gray(255));


                for (int i = 0; i < WINDOW_WIDTH7; ++i)
                    for (int j = 0; j < WINDOW_HEIGHT7; ++j)
                    {
                        if (normal1[i, j].Intensity == normal2[i, j].Intensity)
                        {
                            sum++;
                        }
                    }
                if (sum > CORRELATION_THRESHOLD7)
                    return true;

            return false;
        }

        //функция принимает на вход ячейку и сравнивает ее со всеми 10 шаблонными окошками, возвращает тип ячейки
        private static int CellRecognitionWin7(Image<Bgr, Byte> image)
        {
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < cellsWin7[i].Length; ++j)
                {
                    if (Correlation(image, cellsWin7[i][j], graysWin7[i][j]) == true)
                    {
                        if (i == 0)
                        {
                            return 9;
                        }
                        if (i == 9)
                        {
                            return 0;
                        }
                        return i;
                    }
                }
            }
            return -1;
        }

        public int[,] GetSaperField()
        {
            return saperField;
        }

        //функция распознает картинку и заполняет поле сапера числами
        private void ImageToMatrix(Image<Bgr, Byte> image, int[,] tableOfNumbers)
        {
            for (int i = 0; i < 16; ++i)
                for (int j = 0; j < 30; ++j)
                {
                    if(tableOfNumbers[i,j] == 9)
                    {
                        var cell = image.GetSubRect(new Rectangle(FIRST_WINDOW_X7 + j * (WINDOW_WIDTH7 + WINDOW_DIST7),
                            FIRST_WINDOW_Y7 + i * (WINDOW_WIDTH7 + WINDOW_DIST7), WINDOW_WIDTH7, WINDOW_HEIGHT7));
                        saperField[i, j] = CellRecognitionWin7(cell);          
                    }
                    else
                    {
                        saperField[i, j] = tableOfNumbers[i, j];
                    }
                }
        }

        public static bool IfGameOverWin7(Image<Bgr, Byte> image)
        {
            int sum = 0;
            for (int i = 2; i < 12; ++i)
                for (int j = 4; j < 25; ++j)
                {
                    var cell = image.GetSubRect(new Rectangle(FIRST_WINDOW_X7 + j * (WINDOW_WIDTH7 + WINDOW_DIST7),
                           FIRST_WINDOW_Y7 + i * (WINDOW_WIDTH7 + WINDOW_DIST7), WINDOW_WIDTH7, WINDOW_HEIGHT7));
                    if (CellRecognitionWin7(cell) != -1)
                    {
                        sum++;
                    }
                }
            if (sum < 100)
            {               
                return true;
            }
            return false;
        }

        public static bool IfNewGameStarted(Image<Bgr, Byte> image)
        {
            for (int i = 0; i < 16; ++i)
                for (int j = 0; j < 30; ++j)
                {
                    var cell = image.GetSubRect(new Rectangle(FIRST_WINDOW_X7 + j * (WINDOW_WIDTH7 + WINDOW_DIST7),
                           FIRST_WINDOW_Y7 + i * (WINDOW_WIDTH7 + WINDOW_DIST7), WINDOW_WIDTH7, WINDOW_HEIGHT7));
                    if (CellRecognitionWin7(cell) != 9)
                    {
                        return false;
                    }
                }
            return true;
        }

        private static int GrayAverage(Image<Gray, Byte> img)
        {
            float average = 0;
            for (int i = 0; i < WINDOW_WIDTH7; ++i)
                for (int j = 0; j < WINDOW_HEIGHT7; ++j)
                {
                    average += (float)((float)img[i, j].Intensity / (float)(WINDOW_WIDTH7 * WINDOW_HEIGHT7));
                }
            return (int)average;
        }

        public static bool IfAnyChanges(SaperCell[,] table, Rectangle rect, int y, int x)
        {
            //получает текущую диспозицию, исходя из окна
            var gameBmp = Form1.GetScreenImage(rect);
            Image<Bgr, Byte> img = new Image<Bgr, byte>(gameBmp);

            InitiateCellsWin7();

            var cell = img.GetSubRect(new Rectangle(FIRST_WINDOW_X7 + x * (WINDOW_WIDTH7 + WINDOW_DIST7),
                            FIRST_WINDOW_Y7 + y * (WINDOW_WIDTH7 + WINDOW_DIST7), WINDOW_WIDTH7, WINDOW_HEIGHT7));
            int value = CellRecognitionWin7(cell);

            if (table[y, x].value != value)
                return true;

            return false;
        }
    }
}
