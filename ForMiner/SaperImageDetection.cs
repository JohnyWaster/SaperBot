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
    class SaperImageDetection
    {
        //константы, задающие размеры ячеек в сапере, расстояние между ними, координаты первой ячейки и пороги для корреляции
        private static int WINDOW_WIDTH = 13;
        private static int WINDOW_HEIGHT = 13;
        private static int WINDOW_DIST = 3;
        private static int FIRST_WINDOW_X = 16;
        private static int FIRST_WINDOW_Y = 105;

        private static int CORRELATION_THRESHOLD = 155;


        public static string path = Application.StartupPath;
        //массив содержит картинки со всеми 10 типами ячеек
        private static Image<Bgr, Byte>[] cells = new Image<Bgr, Byte>[12] {           
            new Image<Bgr, Byte>(path + @"\images\0.png"),
            new Image<Bgr, Byte>(path + @"\images\1.jpg"),
            new Image<Bgr, Byte>(path + @"\images\2.jpg"),
            new Image<Bgr, Byte>(path + @"\images\3.jpg"),
            new Image<Bgr, Byte>(path + @"\images\4.jpg"),
            new Image<Bgr, Byte>(path + @"\images\5.jpg"),
            new Image<Bgr, Byte>(path + @"\images\6.jpg"),
            new Image<Bgr, Byte>(path + @"\images\7.jpg"),
            new Image<Bgr, Byte>(path + @"\images\6.jpg"),
            new Image<Bgr, Byte>(path + @"\images\9.jpg"),
            new Image<Bgr, Byte>(path + @"\images\-2.jpg"),
            new Image<Bgr, Byte>(path + @"\images\-3.jpg")
            };


        //картинка окна сапера
        private Image<Bgr, Byte> saperWindow;

        //матрица, показывающая поле сапера
        private int[,] saperField = new int[16, 30];


        //конструктор, принимающий изображение окна сапера и заполняющий соответствующую матрицу
        public SaperImageDetection(Image<Bgr, Byte> image, int[,] tableOfNumbers)
        {
            saperWindow = image;
            ImageToMatrix(image, tableOfNumbers);
        }

        //функция принимает на вход два окошка одинакового размера и сравнивает их
        private static bool Correlation(Image<Bgr, Byte> image1, Image<Bgr, Byte> image2)
        {
            int sum = 0;

            var normal1 = image1.Convert<Gray, Byte>().ThresholdBinary(new Gray(90), new Gray(255));
            var normal2 = image2.Convert<Gray, Byte>().ThresholdBinary(new Gray(90), new Gray(255));

            for (int i = 0; i < WINDOW_WIDTH; ++i)
                for (int j = 0; j < WINDOW_HEIGHT; ++j)
                {
                    if (normal1[i, j].Intensity == normal2[i, j].Intensity)
                    {
                        sum++;
                    }
                }
            if (sum > CORRELATION_THRESHOLD)
                return true;

            return false;
        }

        //функция принимает на вход ячейку и сравнивает ее со всеми 10 шаблонными окошками, возвращает тип ячейки
        private static int CellRecognition(Image<Bgr, Byte> image)
        {
            for (int i = 0; i < 12; ++i)
            {
                if (Correlation(image, cells[i]) == true)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int Differ9TypeFrom0Type(Image<Bgr, Byte> image)
        {
            int sum = 0;
            var normal9 = cells[9].Convert<Gray, Byte>().ThresholdBinary(new Gray(193), new Gray(255));
            var normalImage = image.Convert<Gray, Byte>().ThresholdBinary(new Gray(193), new Gray(255));
            for (int i = 0; i < WINDOW_WIDTH; ++i)
                for (int j = 0; j < WINDOW_HEIGHT; ++j)
                {
                    if (normalImage[i, j].Intensity == normal9[i, j].Intensity)
                    {
                        sum++;
                    }
                }
            if (sum > CORRELATION_THRESHOLD)
                return 9;

            return 0;
        }

        private static int Differ5TypeFrom6Type(Image<Bgr, Byte> image)
        {
            int sum = 0;
            var normal6 = cells[6].Convert<Gray, Byte>().ThresholdBinary(new Gray(60), new Gray(255));
            var normalImage = image.Convert<Gray, Byte>().ThresholdBinary(new Gray(60), new Gray(255));
            for (int i = 0; i < WINDOW_WIDTH; ++i)
                for (int j = 0; j < WINDOW_HEIGHT; ++j)
                {
                    if (normalImage[i, j].Intensity == normal6[i, j].Intensity)
                    {
                        sum++;
                    }
                }
            if (sum > CORRELATION_THRESHOLD)
                return 6;

            return 5;
        }

        //функция распознает картинку и заполняет поле сапера числами
        private void ImageToMatrix(Image<Bgr, Byte> image, int[,] tableOfNumbers)
        {
            for (int i = 0; i < 16; ++i)
                for (int j = 0; j < 30; ++j)
                {
                    if (tableOfNumbers[i, j] == 9)
                    {
                        var cell = image.GetSubRect(new Rectangle(FIRST_WINDOW_X + j * (WINDOW_WIDTH + WINDOW_DIST),
                            FIRST_WINDOW_Y + i * (WINDOW_WIDTH + WINDOW_DIST), WINDOW_WIDTH, WINDOW_HEIGHT));
                        saperField[i, j] = CellRecognition(cell);
                    }
                    else
                    {
                        saperField[i, j] = tableOfNumbers[i, j];
                    }

                }
            //после того, как все числа были распознаны необходимо отделить неоткрыте клетки(тип 9) от клеток, не касающихся мин(тип 0)
            for (int i = 0; i < 16; ++i)
                for (int j = 0; j < 30; ++j)
                {
                    if (saperField[i, j] == 0)
                    {
                        var cell = image.GetSubRect(new Rectangle(FIRST_WINDOW_X + j * (WINDOW_WIDTH + WINDOW_DIST),
                        FIRST_WINDOW_Y + i * (WINDOW_WIDTH + WINDOW_DIST), WINDOW_WIDTH, WINDOW_HEIGHT));
                        saperField[i, j] = Differ9TypeFrom0Type(cell);
                    }
                    if (saperField[i, j] == 5)
                    {
                        var cell = image.GetSubRect(new Rectangle(FIRST_WINDOW_X + j * (WINDOW_WIDTH + WINDOW_DIST),
                        FIRST_WINDOW_Y + i * (WINDOW_WIDTH + WINDOW_DIST), WINDOW_WIDTH, WINDOW_HEIGHT));
                        saperField[i, j] = Differ5TypeFrom6Type(cell);
                    }
                }

        }

        public int[,] GetSaperField()
        {
            return saperField;
        }

        //принимает на вход таблицу ячеек сапера и прямоугольник окна с игрой, проверяет, 
        //изменилась ли значение в клетке с координатами x,y
        public static bool IfAnyChanges(SaperCell[,] table, Rectangle rect, int y, int x)
        {
            //получает текущую диспозицию, исходя из окна
            var gameBmp = Form1.GetScreenImage(rect);
            Image<Bgr, Byte> img = new Image<Bgr, byte>(gameBmp);

            var cell = img.GetSubRect(new Rectangle(FIRST_WINDOW_X + x * (WINDOW_WIDTH + WINDOW_DIST),
                            FIRST_WINDOW_Y + y * (WINDOW_WIDTH + WINDOW_DIST), WINDOW_WIDTH, WINDOW_HEIGHT));
            int value = CellRecognition(cell);
            if (value == 0)
            {
                value = Differ9TypeFrom0Type(cell);
            }
            if (value == 5)
            {
                value = Differ5TypeFrom6Type(cell);
            }

            if (table[y, x].value != value)
                return true;

            return false;
        }
    }
}
