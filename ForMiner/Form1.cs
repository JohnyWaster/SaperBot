using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Security;


namespace ForMiner
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder str, int max);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]


        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //прямоугольник будет содержать координаты окна с игрой
        public static RECT rect;

        //
        public static System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        //поток, в котором будет идти игра
        public static Thread saper;

        //поток, следящий за клавиатурой
        public static Thread hooker;

        //поток следящий за мышью
        public static Thread mouseHooker;

        //для скорости игры
        public static int mouseSpeed;

        //для того, чтобы hooker знал, когда игра началась и работал иначе
        public static bool gameStarted = false;

        //включение статистики
        public static bool statisticsOn = false;

        //версия сапера: старый, новый, либо не распознанный, тогда зависаем
        public static SAPER_TYPE version;
        public enum SAPER_TYPE 
        {
            XP,
            Seven,
            NotRecognized
        }

        //в новом сапере после проигрыша/победы появляется новое окно, закрывающее основное, не дает сосчитать мины, поэтому будем вручную
        public static int numberOfMinesWin7 = 0;

        //для синхронизации потоков 
        public static EventWaitHandle wh = new AutoResetEvent(false);

        //если нажата кнопка стоп, тогда не будем реагировать на клавиши для продолжения
        public static bool globalPause = false;

        //получение снимка экрана
        public static Bitmap GetScreenImage(Rectangle rect)
        {
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }

        //основной цикл игры, получает старую диспозицию и счетчик статистики
        public static int[,] GameIteration(int[,] oldTableOfNumbers, SaperStatistics counter)
        {
            //провнряем, на месте ли окно, если нет, то открываем новое
            if (CheckSaperWindow() == false)
            {
                StartNewSaperProcess();
                FillTableWithEmptyCells(oldTableOfNumbers);
            }

            var handle = FindWindow(null, "Сапер");
            if (handle == IntPtr.Zero)
            {
                handle = FindWindow(null, "MineSweeper");
                if (handle == IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
                }
            }

            //получает координаты окна сапера(прямоугольник)
            GetWindowRect(handle, out rect);

            //определяем версию сапера
            if (rect.Bottom - rect.Top == 371 && rect.Right - rect.Left == 506)
            {
                version = SAPER_TYPE.XP;
            }
            else if (rect.Bottom - rect.Top == 409 && rect.Right - rect.Left == 616)
            {
                version = SAPER_TYPE.Seven;
                System.Threading.Thread.Sleep(500);
            }
            else
            {
                version = SAPER_TYPE.NotRecognized;
                MessageBox.Show("Извините, данная версия не поддерживается либо размеры окна изменены!");
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }

            //вот и наш прямоугольник с игрой
            Rectangle gameScreenRect = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            //получает изображение игры
            Bitmap gameBmp = GetScreenImage(gameScreenRect);
            Image<Bgr, Byte> img = new Emgu.CV.Image<Bgr, byte>(gameBmp);

            int[,] newTableOfNumbers = null;

            if (version == SAPER_TYPE.XP)
            {
                //распознает изображение игры, заполняет матрицу числами, соответствующими текущей диспозиции на экране
                SaperImageDetection saperTableOfNumbers = new SaperImageDetection(img, oldTableOfNumbers);

                //запоминает текущую диспозицию на экране
                newTableOfNumbers = saperTableOfNumbers.GetSaperField();
            }
            if (version == SAPER_TYPE.Seven)
            {
                //распознает изображение игры, заполняет матрицу числами, соответствующими текущей диспозиции на экране
                SaperImageDetectionWindows7 saperTableOfNumbers = new SaperImageDetectionWindows7(img, oldTableOfNumbers);

                //запоминает текущую диспозицию на экране
                newTableOfNumbers = saperTableOfNumbers.GetSaperField();
            }
            
            //по матрице с числами заполняет матрицу из элементов класса "ячейка сапера"
            SaperFieldOfCells fieldOfCells = new SaperFieldOfCells(newTableOfNumbers);

            //позволяет проще управлять мышью в ходе алгоритма игры
            SaperMouseClick mouse = new SaperMouseClick(gameScreenRect);

            //класс, содержащий различные алгоритмы по прохождению игры, получает текущую диспозицию и мышь
            SaperAlgorithm alg = new SaperAlgorithm(fieldOfCells, mouse);

            //проверка на проигрыш/победу
            if (version == SAPER_TYPE.XP)
            {
                if (alg.IfGameOver(counter) == true)
                {
                    FillTableWithEmptyCells(newTableOfNumbers);
                    return newTableOfNumbers;
                }                
                if (alg.IfVictory(counter) == true)
                {
                    FillTableWithEmptyCells(newTableOfNumbers);
                    return newTableOfNumbers;
                }
            }
            if (version == SAPER_TYPE.Seven) 
            {
                if (SaperImageDetectionWindows7.IfGameOverWin7(img) == true)
                {
                    System.Threading.Thread.Sleep(4000);
                    mouse.NewGame();
                    System.Threading.Thread.Sleep(1000);
                    //получает изображение игры
                    var gameBmp2 = GetScreenImage(gameScreenRect);
                    Image<Bgr, Byte> img2 = new Image<Bgr, byte>(gameBmp2);
                    if (SaperImageDetectionWindows7.IfNewGameStarted(img2) == false)
                    {
                        if (statisticsOn == true)
                        {
                            counter.StatisticForVictory();
                        }
                        mouse.VictoryWin7();
                    }
                    else
                    {
                        if (statisticsOn == true)
                        {
                            counter.StatisticForGameOver(numberOfMinesWin7);
                        }
                    }
                    FillTableWithEmptyCells(newTableOfNumbers);
                    return newTableOfNumbers;
                }
            }
            if (PauseResume() == true)
            {
                return newTableOfNumbers;
            }

            //собственно игра, эти алгоритмы описаны в классе SaperAlgorithm
            alg.SimpleAlgorithm();
            if (PauseResume() == true)
            {
                return newTableOfNumbers;
            }
            alg.HardAlgorithm();
            if (PauseResume() == true)
            {
                return newTableOfNumbers;
            }
            alg.DoubleClickingAlgorithm();
            if (PauseResume() == true)
            {
                return newTableOfNumbers;
            }

            if (version == SAPER_TYPE.Seven)
            {
                numberOfMinesWin7 = fieldOfCells.numberOfMines;
            }

            //если после предыдущих алгоритмов диспозиция не изменилась, то щелкаем в любую неоткрытую клетку
            if (SaperAlgorithm.EndOfCycle(oldTableOfNumbers, newTableOfNumbers) == true)
            {
                alg.CleverRandomClick();
            }
            if (PauseResume() == true)
            {
                return newTableOfNumbers;
            }
            
            return newTableOfNumbers;
        }

        //заполнение таблицы девятками - типо свежее, неоткрытое поле, это связано с оптимизацией распознавания
        public static void FillTableWithEmptyCells(int[,] table)
        {
            for (int i = 0; i < 16; ++i)
                for (int j = 0; j < 30; ++j)
                {
                    table[i, j] = 9;
                }
        }

        //проверка окна с игрой, за одно и разворачивает, если свернуто
        public static bool CheckSaperWindow()
        {
            Process[] processes;

            if (System.Environment.OSVersion.VersionString.Contains("5."))
            {
                processes = Process.GetProcessesByName("winmine");
            }
            else
            {
                processes = Process.GetProcessesByName("MineSweeper");
            }

            if (processes.Any()) //a copy is already running
            {
                var handle1 = processes.First().MainWindowHandle;
                ShowWindow(handle1, SW_RESTORE);

                var handle2 = GetForegroundWindow();
                int len = GetWindowTextLength(handle2);
                StringBuilder str = new StringBuilder(len);
                bool forWaiting = false;
                GetWindowText(handle2, str, 50);
                if (str.ToString() != "Сапер" && str.ToString() != "MineSweeper")
                {
                    forWaiting = true;                   
                }
                SetForegroundWindow(handle1);
                if (forWaiting == true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }

            //смотрит, открыта ли игра
            var handle = FindWindow(null, "Сапер");
            if (handle == IntPtr.Zero)
            {
                handle = FindWindow(null, "MineSweeper");
                if (handle == IntPtr.Zero)
                {
                    return false;
                }
            }
            
            return true;
        }

        //открытие новой игры, ищет по стандартным путям
        public static void StartNewSaperProcess()
        {
            Process proc = new Process();
            if (System.Environment.OSVersion.VersionString.Contains("5."))
            {
                proc.StartInfo.FileName = @"C:\WINDOWS\system32\winmine.exe";
                try
                {
                    if (File.Exists(@"C:\WINDOWS\system32\winmine.exe") == false)
                    {
                        FileNotFoundException e = new FileNotFoundException(" не найден, так что откройте сапера сами и перезапустите приложение", @"C:\WINDOWS\system32\winmine.exe");
                        throw e;
                    }
                }
                catch (FileNotFoundException e)
                {
                    MessageBox.Show(e.FileName + e.Message);
                    return;
                }
            }
            else
            {
                proc.StartInfo.FileName = @"C:\Program Files\Microsoft Games\Minesweeper\MineSweeper.exe";
                try
                {
                    if (File.Exists(@"C:\Program Files\Microsoft Games\Minesweeper\MineSweeper.exe") == false)
                    {
                        FileNotFoundException e = new FileNotFoundException(" не найден, так что откройте сапера сами и перезапустите приложение", @"C:\Program Files\Microsoft Games\Minesweeper\MineSweeper.exe");
                        throw e;
                    }
                }
                catch (FileNotFoundException e)
                {
                    MessageBox.Show(e.FileName + e.Message);
                    return;
                }
            }
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            proc.Start();
            System.Threading.Thread.Sleep(5000);    
            
        }

        //запуск бесконечного цикла с игрой
        public static void SaperGame()
        {          
            int[,] oldTableOfNumbers = new int[16, 30];
            FillTableWithEmptyCells(oldTableOfNumbers);
            SaperStatistics counter = new SaperStatistics();
 
            for (; ; )
            {
                oldTableOfNumbers = GameIteration(oldTableOfNumbers, counter);
            }
        }

        //метод для синхронизации потоков
        public static bool PauseResume()
        {
            if (globalPause == false)
            {                
                if (KeyboardHooker.isPaused == false)
                {
                    wh.Set();
                    return false;
                }
                else
                {
                    wh.WaitOne();
                    return true;
                }
            }
            else
            {
                wh.WaitOne();
                return true;
            }
        }

        public static void KeyboardHook()
        {
            KeyboardHooker hooker = new KeyboardHooker();
        }

        public void MouseHook()
        {
            MouseHooker hooker = new MouseHooker();        
        }

        //метод для включения игры, если компьютер бездействует
        public void IfReadyToWakeUp(object notUsed,
                                            EventArgs myEventArgs)
        {          
            if (checkBox2.Checked == true && numericUpDown1.Value > 0)
            {               
                if (MouseHooker.time.ElapsedMilliseconds >= numericUpDown1.Value * 60000
                    && KeyboardHooker.time.ElapsedMilliseconds >= numericUpDown1.Value * 60000)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = false;
                    notifyIcon1.Visible = true;
                    this.Activate();           
                    button1.PerformClick();
                }                 
            }            
        }


        public Form1()
        {
            InitializeComponent();

            //проверяем, должна ли стоять галочка об автозагрузке
            try
            {
                Microsoft.Win32.RegistryKey Key =
                Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);

                if (Key != null)
                {
                    if (Key.GetValueNames().Contains("ForMiner") == true)
                    {
                        checkBox1.Checked = true;
                    }
                    Key.Close();
                }
            }
            catch (SecurityException e)
            {

            }

            try
            {
                Microsoft.Win32.RegistryKey Key1 =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\ForMiner", true);

                if (Key1 != null)
                {
                    //устанавливаем сохраненное время для заставки
                    for (int i = 0; i < 101; ++i)
                    {
                        if (Key1.GetValueNames().Contains(i.ToString()) == true)
                        {
                            checkBox2.Checked = true;
                            numericUpDown1.Value = i;
                            break;
                        }
                        else
                        {
                            label3.Hide();
                            numericUpDown1.Hide();
                            label4.Hide();
                        }
                    }

                    //устанавливаем сохраненную скорость
                    for (int i = 120; i < 131; ++i)
                    {
                        if (Key1.GetValueNames().Contains(i.ToString()) == true)
                        {
                            trackBar1.Value = i - 120;
                        }
                    }

                    //проверяем, нужна ли статистика
                    if (Key1.GetValueNames().Contains("150") == true)
                    {
                        checkBox3.Checked = true;
                        statisticsOn = true;
                    }
                    else
                    {
                        statisticsOn = false;
                    }
                    Key1.Close();
                }
            }
            catch (SecurityException e)
            {

            }
            mouseSpeed = trackBar1.Value;

            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

            //запускаем перехватчика клавиатуры и мыши(для заставки)
            mouseHooker = new Thread(MouseHook);
            mouseHooker.IsBackground = true;
            mouseHooker.Start();
            hooker = new Thread(KeyboardHook);
            hooker.IsBackground = true;
            hooker.Start();

            //таймер каждые 20 секунд проверяет, не пора ли врубать игру
            timer.Tick += new EventHandler(IfReadyToWakeUp);
            timer.Interval = 20000;
            timer.Start();

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIcon1.Visible = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        //добавление в автозагрузку
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {           
            var path = Application.StartupPath;
            try
            {
                Microsoft.Win32.RegistryKey Key =
                Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);

                if (Key != null)
                {
                    if (checkBox1.Checked == true)
                    {
                        //добавляем первый параметр - название ключа  
                        // Второй параметр - это путь к   
                        // исполняемому файлу нашей программы.  
                        Key.SetValue("ForMiner", path + "\\ForMiner.exe");
                    }
                    else
                    {
                        Key.DeleteValue("ForMiner");
                    }
                    Key.Close();
                }
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(ex.Message);
                checkBox1.Checked = false;
            }
        }

        //для скорости
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value < 4)
            {
                mouseSpeed = trackBar1.Value;
            }
            else
            {
                mouseSpeed = 3 * trackBar1.Value;
            }

            Microsoft.Win32.RegistryKey Key =
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
            "SOFTWARE\\ForMiner\\");
            try
            {
                if (Key != null)
                {
                    for (int i = 120; i < 131; ++i)
                    {
                        if (Key.GetValueNames().Contains(i.ToString()) == true)
                        {
                            Key.DeleteValue(i.ToString());
                        }
                    }

                    Key.SetValue(Convert.ToString(trackBar1.Value + 120), 0);
                    Key.Close();
                }
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        //кнопка старт
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.gameStarted = true;
            if (saper == null)
            {
                saper = new Thread(SaperGame);
                saper.IsBackground = true;
                saper.Start();

                globalPause = false;
                wh.Set();
            }
            else
            {
                globalPause = false;
                KeyboardHooker.isPaused = false;
                wh.Set();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                this.Activate();           
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (saper != null)
            {
                saper.Abort();                             
                FreeConsole();               
            }
            timer.Stop();
            mouseHooker.Abort();
            hooker.Abort(); 
            notifyIcon1.Visible = false;
            FreeConsole();
        }

        //кнопка стоп
        private void button2_Click(object sender, EventArgs e)
        {
            globalPause = true;
            FreeConsole();
        }

        
        //заставка
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Win32.RegistryKey Key =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                "SOFTWARE\\ForMiner\\");

                if (checkBox2.Checked == true)
                {
                    Key.SetValue(Convert.ToString(numericUpDown1.Value), 0);
                    label3.Show();
                    numericUpDown1.Show();
                    label4.Show();
                }
                else
                {
                    for (int i = 0; i < 121; ++i)
                    {
                        if (Key.GetValueNames().Contains(i.ToString()) == true)
                        {
                            Key.DeleteValue(i.ToString());
                        }
                    }
                    label3.Hide();
                    numericUpDown1.Hide();
                    label4.Hide();
                }
                Key.Close();
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(ex.Message);
                checkBox2.Checked = false;
            }
        }

        //заставка
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 120)
            {
                numericUpDown1.Value = 120;
            }
            try
            {
                Microsoft.Win32.RegistryKey Key =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                "SOFTWARE\\ForMiner\\");

                for (int i = 0; i < 121; ++i)
                {
                    if (Key.GetValueNames().Contains(i.ToString()) == true)
                    {
                        Key.DeleteValue(i.ToString());
                    }
                }
                Key.SetValue(Convert.ToString(numericUpDown1.Value), 0);
                label3.Show();
                numericUpDown1.Show();

                Key.Close();
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //статистика
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Win32.RegistryKey Key =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                "SOFTWARE\\ForMiner\\");
                if (checkBox3.Checked == true)
                {
                    Key.SetValue("150", 0);
                    statisticsOn = true;
                }
                else
                {
                    Key.DeleteValue("150");
                    statisticsOn = false;
                }
                Key.Close();
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(ex.Message);
                checkBox3.Checked = false;
            }
        }
    }
}
