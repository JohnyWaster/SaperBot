using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ForMiner
{

    public class SaperStatistics
    {
        public int gameCounter;
        public int victoryCounter;
        public int zeroMinesGames;
        public int twentyMinesGames;
        public int fortyMinesGames;
        public int sixtyMinesGames;
        public int eightyMinesGames;
        public int oneHundredMinesGames;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        private const int SC_CLOSE = 0xF060;
        private const int MF_BYCOMMAND = 0;

        public SaperStatistics()
        {
            gameCounter = 0;
            victoryCounter = 0;
            zeroMinesGames = 0;
            twentyMinesGames = 0;
            fortyMinesGames = 0;
            sixtyMinesGames = 0;
            eightyMinesGames = 0;
            oneHundredMinesGames = 0;
        }
        public void StatisticForVictory()
        {
            gameCounter++;
            victoryCounter++;
            Print();
        }
        public void StatisticForGameOver(int numberOfMines)
        {
            gameCounter++;
            if (numberOfMines == 99)
            {
                zeroMinesGames++;
            }
            if (numberOfMines < 99 && numberOfMines > 79)
            {
                twentyMinesGames++;
            }
            if (numberOfMines < 80 && numberOfMines > 59)
            {
                fortyMinesGames++;
            }
            if (numberOfMines < 60 && numberOfMines > 39)
            {
                sixtyMinesGames++;
            }
            if (numberOfMines < 40 && numberOfMines > 19)
            {
                eightyMinesGames++;
            }
            if (numberOfMines < 20 && numberOfMines > 0)
            {
                oneHundredMinesGames++;
            }
            Print();
        }
        private void Print()
        {
            FreeConsole();
            if (AllocConsole())
            {
                
                
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\n\n********************Статистика игры************************************");
            Console.Write("\nКоличество игр: " + gameCounter + ".\tКоличество побед: " + victoryCounter + ".\tПроцент побед: " + (int)((float)victoryCounter * 100 / (float)(gameCounter)) + "%");
            if (gameCounter - zeroMinesGames > 0)
            {
                Console.Write("\nНачатых игр:    " + (gameCounter - zeroMinesGames) + ".\t\tПроцент побед в начатых играх: " + (int)((float)victoryCounter * 100 / (float)(gameCounter - zeroMinesGames)) + "%");
            }
            Console.Write("\n\nНиже приведены проценты всех игр и начатых с данным количеством найденных мин:");
            Console.Write("\n0 мин *** 1-19мин *** 20-39мин *** 40-59мин *** 60-79мин *** 80-99мин");
            Console.Write("\n{0,4}% *** {1,6}% *** {2,7}% *** {3,7}% *** {4,7}% *** {5,7}%",
                (int)((float)zeroMinesGames * 100 / (float)(gameCounter)), (int)((float)twentyMinesGames * 100 / (float)(gameCounter)),
                (int)((float)fortyMinesGames * 100 / (float)(gameCounter)), (int)((float)sixtyMinesGames * 100 / (float)(gameCounter)),
                (int)((float)eightyMinesGames * 100 / (float)(gameCounter)), (int)((float)oneHundredMinesGames * 100 / (float)(gameCounter)));
            if (gameCounter - zeroMinesGames > 0)
            {
                Console.Write("\n********* {0,6}% *** {1,7}% *** {2,7}% *** {3,7}% *** {4,7}%",
                (int)((float)twentyMinesGames * 100 / (float)(gameCounter - zeroMinesGames)),
                (int)((float)fortyMinesGames * 100 / (float)(gameCounter - zeroMinesGames)), (int)((float)sixtyMinesGames * 100 / (float)(gameCounter - zeroMinesGames)),
                (int)((float)eightyMinesGames * 100 / (float)(gameCounter - zeroMinesGames)), (int)((float)oneHundredMinesGames * 100 / (float)(gameCounter - zeroMinesGames)));
            }
            Console.Write("\n***********************************************************************\n");
            Console.Write("Для паузы нажмите любую клавишу!\n\n\n\n\n\n\n\n\n\n\n\n\n");
               
            }

            IntPtr handle = GetConsoleWindow();
            IntPtr hMenu = GetSystemMenu(handle, false);
            DeleteMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
        }


       
        

    }
}
