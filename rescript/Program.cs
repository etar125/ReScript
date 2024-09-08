// by etar125
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Globalization;
using System.Web;
using System.Net;
using System.Windows.Forms;
using System.Drawing;

namespace rescript {
    public class Program {
        #region Variables
        public static string path = "index.aus";
        public static Dictionary<string, string> vars;
        public static bool logs;
        public static string logf;
        public static Form msgbox = new Form();
        public static Form inbox = new Form();
        public static Form chbox = new Form();
        public static TextBox tb = new TextBox();
        public static bool clickedinbox;
        public static string invar;
        public static string chvar;
        public static ListBox lb = new ListBox();


        [DllImport("Wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InternetGetConnectedState(out INET_CONNECTION_STATE lpdwFlags, uint dwReserved);

        [Flags]
        enum INET_CONNECTION_STATE : uint {
            INTERNET_CONNECTION_CONFIGURED = 0x40,
            INTERNET_CONNECTION_LAN = 0x02,
            INTERNET_CONNECTION_MODEM = 0x01,
            INTERNET_CONNECTION_MODEM_BUSY = 0x08,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_PROXY = 0x04,
            INTERNET_RAS_INSTALLED = 0x10
        }
        #endregion
        public static List<string> l = new List<string> { };
        public static List<string> FileToList(string file) {
            if (!File.Exists(file)) throw new FileNotFoundException("Not found file " + file);
            List<string> r = new List<string> { };
            foreach (string a in File.ReadAllLines(file)) r.Add(a);
            return r;
        } public static char[] TrimChars = { ' ', '\t' };

        public static string version = "ReScript 1.06 by etar125";

        public static void Main(string[] args) {
            // Основные переменные
            vars = new Dictionary<string, string> {
                { "datetime", DateTime.Now.ToString() },
                { "curdir", Environment.CurrentDirectory },
                { "sysdir", Environment.SystemDirectory },
                { "time1", DateTime.Now.ToString("HH:mm:ss") },
                { "time2", DateTime.Now.ToString("HH:mm") },
                { "date1", DateTime.Now.ToString("dd MMMM yyyy") },
                { "date2", DateTime.Now.ToString("dd.MM.yyyy") },
                { "auver", "1.06" },
                { "machinename", Environment.MachineName },
                { "username", Environment.UserName },
                { "osver", Environment.OSVersion.VersionString }
            };

            // Проверка на наличие аргументов и установка значения переменной path
            bool have = false;
            foreach (string l in args) {
                string s = l;
                if (!have) {
                    if (s.StartsWith("file:")) {
                        s = s.Remove(0, 5);
                        if (File.Exists(s)) {
                            path = s;
                            have = true;
                        } else {
                            Console.WriteLine("Not found file " + s);
                        }
                        break;
                    } else if (s == "version" || s == "ver") {
                        Console.WriteLine(version);
                        Console.ReadKey();
                        Environment.Exit(0);
                    } else {
                        if (File.Exists(s)) {
                            path = s;
                            have = true;
                        } else {
                            Console.WriteLine("Not found file " + s);
                        }
                        /*Console.WriteLine("Wrong argument " + s);
                        Console.ReadKey();
                        Environment.Exit(0);*/
                    }
                    /*else if (s.StartsWith("teh:")) {
                        s = s.Remove(0, 4);
                        string[] splitstrings = { ":", "::" };
                        stwring[] re = s.Split(splitstrings, StringSplitOptions.None);
                        Process p = new Process();
                        p.StartInfo.FileName = re[0];
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.Arguments = "file:" + re[1];
                        if (re[2] == "true")
                            p.StartInfo.UseShellExecute = true;
                        p.Start();
                        Thread.Sleep(500);
                        Environment.Exit(0);
                    } else if (s == "topmost") {
                        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                        SetWindowPos(hWnd,
                            new IntPtr(HWND_TOPMOST),
                            0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE);
                    }
                    EnsureAssociationsSet()*/
                }
            }
            if (!File.Exists(path)) {
                Console.WriteLine("Not found " + path);
                Console.ReadKey();
                Environment.Exit(0);
            }

            //other
            logs = false;
            logf = "logs.txt";

            l = FileToList(path);
            //List<string> temp = new List<string> { };

            /*bool no_task = true;
            do {
                no_task = true;
                for(int i = 0; i < l.Count; ++i) {
                    string s = l[i].TrimStart(TrimChars);
                    if (s.ToLower().StartsWith("include ")) {
                        s = l[i].Remove(0, 8);
                        l.RemoveAt(i);
                        if (!File.Exists(s + ".aus")) {
                            Console.WriteLine("Not found file " + s + ".aus");
                            Environment.Exit(0);
                        } 
                        temp = FileToList(s + ".aus");
                        temp.AddRange(l);
                        l = temp;
                        no_task = false;

                    }
                }
            } while (!no_task);*/


            // Вызываем основной метод
            oth(0);
        }

        /*Version: 0.3.2 (10.07.2022 16:21)
          Version: 0.3.5 (14.07.2022 21:02)
          Version: 0.3.7 (16.07.2022 20:49)
          Version: 0.4.1 (23.07.2022 19:33)*/

        #region VF
        public static string ConvertS(string src) {
            string[] ofas2 = src.Split(new string[] { "&&&", "&^&" }, StringSplitOptions.None);
            string result = "";
            for (int s = 0; s < ofas2.Length; s++) {
                string ofas = ofas2[s];
                if (ofas.StartsWith("$")) {
                    ofas = ofas.Remove(0, 1);
                    if (vars.ContainsKey(ofas)) result += vars[ofas];
                    else {
                        result += "null";
                        if (logs == true) {
                            string text = "Not found variable " + ofas + "\n";
                            File.AppendAllText(logf, text);
                        }
                    }
                } else if (ofas.StartsWith("/$")) {
                    string tit = ofas.Remove(0, 1);
                    result += tit;
                } else result += ofas;
            }
            return result;
        } public static string ConvertN(string src) {
            if (src.StartsWith("$")) {
                string ofas = src.Remove(0, 1);
                if (vars.ContainsKey(ofas)) return vars[ofas];
                else {
                    if (logs == true) {
                        string text = "Not found variable " + ofas + "\n";
                        File.AppendAllText(logf, text);
                    }
                    return "-0";
                }
            } return src;
        }
        public static string ConvertV(string src) {
            if (vars.ContainsKey(src)) return vars[src];
            else {
                if (logs == true) {
                    string text = "Not found variable " + src + "\n";
                    File.AppendAllText(logf, text);
                }
                return "null";
            }
        }
        #endregion
        
        public static void oth(int stat)
        {
            for (int i = stat; i < l.Count; i++) {
                l[i] = l[i].TrimStart(TrimChars);
                try {
                    if (l[i][0] != '_') l[i] = l[i].Substring(0, 1).ToUpper() + l[i].Substring(1);
                    else l[i] = "_" + l[i].Substring(1, 1).ToUpper() + l[i].Substring(2);
                    if (l[i].StartsWith("Printline ")) Console.WriteLine(ConvertS(l[i].Remove(0, 10)));
                    else if (l[i].StartsWith("Print ")) Console.Write(ConvertS(l[i].Remove(0, 6)));
                    else if (l[i].StartsWith("Caption ")) Console.Title = ConvertS(l[i].Remove(0, 8));
                    else if (l[i].StartsWith("Variable ")) {
                        string ofas3 = l[i].Remove(0, 9);
                        string varvf = ofas3.Substring(0, ofas3.IndexOf("="));
                        string valuef = ofas3.Substring(ofas3.IndexOf("=") + 1);
                        string va = ConvertS(valuef);
                        if (vars.ContainsKey(varvf)) vars[varvf] = valuef;
                        else vars.Add(varvf, valuef);
                    } else if (l[i].StartsWith("Math")) {
                        string ofas = l[i].Remove(0, 4);
                        string op = ofas.Substring(0, 1);
                        ofas = ofas.Remove(0, 2);
                        string one = ofas.Substring(0, ofas.IndexOf(" "));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        one = ConvertN(one);
                        two = ConvertN(two);
                        try {
                            double a = double.Parse(one); double b = double.Parse(two);
                            double result = 0;
                            if (op == "p") result = a + b;
                            else if (op == "m") result = a - b;
                            else if (op == "x") result = a * b;
                            else if (op == "n") result = a / b;
                            else if (op == "o") result = a % b;
                            if (vars.ContainsKey(varv)) vars[varv] = result.ToString();
                            else vars.Add(varv, result.ToString());
                        } catch {
                            if (logs == true) {
                                string text = "Unkown error: " + l[i] + " *Check variables, arguments\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i] == "Pause") Console.ReadKey(true);
                    else if (l[i] == "Clear") Console.Clear();
                    else if (l[i] == "Exit") Environment.Exit(0);
                    else if (l[i].StartsWith("Set ")) {
                        string ofas5 = l[i];
                        ofas5 = ofas5.Remove(0, 4);
                        string varv = ofas5.Substring(0, ofas5.IndexOf("="));
                        Console.Write(ConvertS(ofas5.Substring(ofas5.IndexOf("=") + 1)));
                        string va = Console.ReadLine();
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("If")) {
                        /* e ==
                         * n !=
                         * c Contains
                         * stw startswith
                         * + >
                         * - <
                         */

                        string def = l[i].Remove(0, 2);
                        int si = def.IndexOf(" ");
                        string op = def.Substring(0, si);
                        def = def.Substring(si + 1);

                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = ConvertS(j.Substring(0, j.IndexOf(" ")));
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma = "null";
                        if (vars.ContainsKey(var)) ma = vars[var];
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        bool nc = false;
                        bool yes = false;
                        if (op == "e" || op == "") yes = ma == text;
                        else if (op == "n") yes = ma != text;
                        else if (op == "c") yes = ma.Contains(text);
                        else if (op == "stw") yes = ma.StartsWith(text);
                        else if (op == "+") {
                            try {
                                yes = double.Parse(ma) > double.Parse(text);
                            }
                            catch {
                                if (logs == true) {
                                    string textx = "Unkown error: " + l[i] + " *Check variables, arguments\n";
                                    File.AppendAllText(logf, text);
                                }
                            }
                        } else if (op == "-") {
                            try {
                                yes = double.Parse(ma) < double.Parse(text);
                            } catch {
                                if (logs == true) {
                                    string textx = "Unkown error: " + l[i] + " *Check variables, arguments\n";
                                    File.AppendAllText(logf, text);
                                }
                            }
                        }
                        if (yes) {
                            for (int abg = 0; abg < l.Count; abg++) {
                                if (l[abg] == func || l[abg] == func1) {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            } if (nc == false) {
                                if (logs == true) {
                                    string textx = "Not found function " + func + "\n";
                                    File.AppendAllText(logf, textx);
                                }
                            }
                        }
                    } else if (l[i].StartsWith("To ")) {
                        string def = l[i].Remove(0, 3);
                        bool nc = false;
                        for (int abg = 0; abg < l.Count; abg++) {
                            if (l[abg] == "Function " + def || l[abg] == "function " + def) {
                                //CheckStackTrace();
                                nc = true;
                                i = abg;
                            }
                        }
                        if (nc == false) {
                            if (logs == true) {
                                string textx = "Not found function " + def + "\n";
                                File.AppendAllText(logf, textx);
                            }
                        }
                    } else if (l[i].StartsWith("Start ")) {
                        string pathd = l[i].Remove(0, 6);
                        pathd = ConvertS(pathd);
                        try {
                            Process.Start(pathd);
                        } catch {
                            if (logs == true) {
                                string text = "Not found file " + pathd + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("Starta ")) {
                        string pathd = l[i].Remove(0, 7);
                        pathd = ConvertS(pathd);
                        
                        try {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.FileName = pathd;
                            p.Start();
                        } catch {
                            if (logs == true) {
                                string text = "Not found file " + pathd + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("_Logs ")) {
                        string ofas = l[i].Remove(0, 6);
                        logs = true;
                        logf = ofas;
                        File.WriteAllText(logf, "");
                    } else if (l[i].StartsWith("Forecolor ")) Console.BackgroundColor = (ConsoleColor)int.Parse(ConvertN(l[i].Remove(0, "forecolor ".Length)));
                    else if (l[i].StartsWith("Backcolor ")) Console.BackgroundColor = (ConsoleColor)int.Parse(ConvertN(l[i].Remove(0, "forecolor ".Length)));
                    else if (l[i].StartsWith("Waitms ")) {
                        try {
                            Thread.Sleep(int.Parse(ConvertN(l[i].Remove(0, 7))));
                        } catch {
                            string text = "Unkown error: " + l[i] + " *Check first argument*\n";
                            File.AppendAllText(logf, text);
                        }
                    } else if (l[i].StartsWith("Waits ")) {
                        try {
                            Thread.Sleep(int.Parse(ConvertN(l[i].Remove(0, 6))) * 1000);
                        } catch {
                            string text = "Unkown error: " + l[i] + " *Check first argument*\n";
                            File.AppendAllText(logf, text);
                        }
                    } else if (l[i] == "Desktopoff") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer /v NoDesktop /t REG_DWORD /d 1 /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i] == "Allappsoff") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\RestrictRun /v 1 /t REG_DWORD /d %SystemRoot%\explorer.exe /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i] == "Controlpanoff") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCU\Software\Microsoft\Windows\Current Version\Policies\Explorer /v NoControlPanel /t REG_DWORD /d 1 /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i] == "CADoff") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCUSoftwareMicrosoftWindowsCurrentVersionPoliciesSystem /v DisableTaskMgr /t REG_DWORD /d 1 /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i] == "Cursoroff") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + "del \"%SystemRoot%Cursors*.*\" >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i] == "Milionfolders") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + "FOR /L %%i IN (1,1,1000000) DO md %%i";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i] == "Systemdelete") {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"del %systemroot%\system32\HAL.dll";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    } else if (l[i].StartsWith("Startargs ")) {
                        string drag = l[i].Remove(0, 10);
                        string app = ConvertS(drag.Substring(0, drag.IndexOf("_")));
                        string args = ConvertS(drag.Substring(drag.IndexOf("_") + 1));
                        Process p = new Process();
                        p.StartInfo.FileName = app;
                        p.StartInfo.Arguments = args;
                        p.Start();
                    } else if (l[i].StartsWith("Startargsa ")) {
                        string drag = l[i].Remove(0, 11);
                        string app = ConvertS(drag.Substring(0, drag.IndexOf("_")));
                        string args = ConvertS(drag.Substring(drag.IndexOf("_") + 1));
                        Process p = new Process();
                        p.StartInfo.FileName = app;
                        p.StartInfo.Arguments = args;
                        p.StartInfo.UseShellExecute = true;
                        p.Start();
                    } else if (l[i].StartsWith("Readline ")) {
                        string bypass = l[i].Remove(0, 9);
                        string[] splitstrs = { "|||", "|^|" };
                        string[] splt = bypass.Split(splitstrs, StringSplitOptions.None);
                        splt[0] = ConvertS(splt[0]);
                        splt[2] = ConvertS(splt[2]);
                        string paths = splt[0];
                        string varv = splt[1];
                        string[] textxa;
                        int ln = Convert.ToInt32(splt[2]);
                        if (File.Exists(paths)) {
                            textxa = File.ReadAllLines(paths);
                            string va = "null";
                            try {
                                va = textxa[ln];
                            } catch {

                            }
                            if (vars.ContainsKey(varv)) vars[varv] = va;
                            else vars.Add(varv, va);

                        } else {
                            textxa = File.ReadAllLines(path);
                            if (logs == true) {
                                string text = "Not found file " + paths + "\n";
                                File.AppendAllText(logf, text);
                            }
                            string va = "null";
                            try {
                                va = textxa[ln];
                            } catch {

                            }
                            if (vars.ContainsKey(varv)) vars[varv] = va;
                            else vars.Add(varv, va);
                        }
                    } else if (l[i].StartsWith("Writeline ")) {
                        string bypass = l[i].Remove(0, 10);
                        string[] splitstrs = { "|||", "|^|" };
                        string[] splt = bypass.Split(splitstrs, StringSplitOptions.None);
                        splt[0] = ConvertS(splt[0]);
                        splt[1] = ConvertS(splt[1]);
                        splt[2] = ConvertN(splt[2]);
                        string paths = splt[0];
                        string txta = splt[1];
                        string[] textxa;
                        if (splt[2] == "-1") {
                            File.WriteAllText(paths, txta);
                        } else {
                            if (File.Exists(paths)) {
                                int ln = int.Parse(splt[2]);
                                try {
                                    textxa = File.ReadAllLines(paths);
                                    textxa[ln] = txta;
                                    File.WriteAllLines(paths, textxa);
                                } catch {
                                    if (logs == true) {
                                        string text = "Not found line " + ln + "\n";
                                        File.AppendAllText(logf, text);
                                    }
                                }
                            } else File.WriteAllText(paths, txta);
                        }
                    } else if (l[i].StartsWith("Crdir ")) Directory.CreateDirectory(ConvertS(l[i].Remove(0, 6)));
                    else if (l[i].StartsWith("Rmdir ")) Directory.Delete(ConvertS(l[i].Remove(0, 6)), true);
                    else if (l[i].StartsWith("Moved ")) {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ConvertS(ofas3.Substring(0, ofas3.IndexOf(" ")));
                        string tos = ConvertS(ofas3.Substring(ofas3.IndexOf(" ") + 1));
                        Directory.Move(froms, tos);
                    } else if (l[i].StartsWith("Movef ")) {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ConvertS(ofas3.Substring(0, ofas3.IndexOf(" ")));
                        string tos = ConvertS(ofas3.Substring(ofas3.IndexOf(" ") + 1));
                        File.Move(froms, tos);
                    } else if (l[i].StartsWith("Copyf ")) {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ConvertS(ofas3.Substring(0, ofas3.IndexOf(" ")));
                        string tos = ConvertS(ofas3.Substring(ofas3.IndexOf(" ") + 1));
                        File.Copy(froms, tos);
                    } else if (l[i].StartsWith("Named ")) {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ConvertS(ofas3.Substring(0, ofas3.IndexOf(" ")));
                        string tos = ConvertS(ofas3.Substring(ofas3.IndexOf(" ") + 1));
                        Directory.Move(froms, tos);
                    } else if (l[i].StartsWith("Crfil "))  File.WriteAllText(ConvertS(l[i].Remove(0, 6)), "");
                    else if (l[i].StartsWith("Rmfil ")) {
                        string result = ConvertS(l[i].Remove(0, 6));
                        if (File.Exists(result))
                            File.Delete(result);
                        else {
                            if (logs == true) {
                                string text = "Not found file " + result + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("Addline ")) {
                        string bypass = l[i].Remove(0, 8);
                        string[] splitstrs = { "|||", "|^|" };
                        string[] splt = bypass.Split(splitstrs, StringSplitOptions.None);
                        splt[0] = ConvertS(splt[0]);
                        splt[1] = ConvertS(splt[1]);
                        string pathas = splt[0];
                        string txtad = splt[1];
                        try {
                            File.AppendAllText(pathas, txtad);
                        } catch {
                            if (logs == true) {
                                string text = "Not found file " + pathas + "    *or other error\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("Printlines ")) {
                        string bypass = ConvertS(l[i].Remove(0, 11));
                        string[] textxa;
                        if (File.Exists(bypass)) {
                            textxa = File.ReadAllLines(bypass);
                            for (int lna = 0; lna < textxa.Length; lna++) {
                                Console.WriteLine(textxa[lna]);
                            }
                        } else {
                            textxa = File.ReadAllLines(path);
                            if (logs == true) {
                                string text = "Not found file " + bypass + "\n";
                                File.AppendAllText(logf, text);
                            }
                            for (int lna = 0; lna < textxa.Length; lna++) {
                                Console.WriteLine(textxa[lna]);
                            }
                        }
                    } else if (l[i].StartsWith("Removechars ")) {
                        string ofas3 = l[i].Remove(0, 12);
                        string strt = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string ana = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string valuef = ana.Substring(0, ana.IndexOf("="));
                        string varvf = ana.Substring(ana.IndexOf("=") + 1);
                        string va = "null";
                        if (vars.ContainsKey(varvf)) va = vars[varvf];
                        else {
                            if (logs == true) {
                                string text = "Not found variable " + varvf + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }

                        try {
                            int chfr = Convert.ToInt32(ConvertN(valuef));
                            int chfr2 = Convert.ToInt32(ConvertN(strt));
                            va = va.Remove(chfr2, chfr);
                            if (vars.ContainsKey(varvf)) vars[varvf] = va;
                            else vars.Add(varvf, va);
                        } catch {
                            if (logs == true) {
                                string text = "Wrong value: " + l[i] + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("Substring ")) {
                        string ofas3 = l[i].Remove(0, 10);
                        string strt = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string ana = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string valuef = ana.Substring(0, ana.IndexOf("="));
                        string varvf = ana.Substring(ana.IndexOf("=") + 1);
                        string va = "null";
                        if (vars.ContainsKey(varvf)) va = vars[varvf];
                        else {
                            if (logs == true) {
                                string text = "Not found variable " + varvf + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }

                        try {
                            int chfr = Convert.ToInt32(ConvertN(valuef));
                            int chfr2 = Convert.ToInt32(ConvertN(strt));
                            va = va.Substring(chfr2, chfr);
                            if (vars.ContainsKey(varvf)) vars[varvf] = va;
                            else vars.Add(varvf, va);
                        } catch {
                            if (logs == true) {
                                string text = "Wrong value: " + l[i] + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("#")) continue;
                    else if (l[i].StartsWith("Kill ")) {
                        Process[] localByName = Process.GetProcessesByName(ConvertS(l[i].Remove(0, 5)));
                        foreach (Process p in localByName) {
                            p.Kill();
                        }
                    } else if (l[i].StartsWith("Length ")) {
                        string ofas = l[i].Remove(0, 7);
                        string varv = ofas.Substring(0, ofas.IndexOf(" "));
                        string txt1 = ofas.Substring(ofas.IndexOf(" ") + 1);
                        int len = -1;
                        if (vars.ContainsKey(varv)) len = vars[varv].Length;
                        else {
                            if (logs == true) {
                                string text = "Not found variable " + varv + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                        if (vars.ContainsKey(varv)) vars[varv] = len.ToString();
                        else vars.Add(varv, len.ToString());
                    } else if (l[i].StartsWith("Longlength ")) {
                        string ofas = l[i].Remove(0, 11);
                        string varv = ofas.Substring(0, ofas.IndexOf(" "));
                        string txt1 = ofas.Substring(ofas.IndexOf(" ") + 1);
                        int len = -1;
                        string file = "index.aus";
                        if (vars.ContainsKey(varv)) file = vars[varv];
                        else {
                            if (logs == true) {
                                string text = "Not found variable " + varv + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                        if (File.Exists(file)) len = File.ReadAllLines(file).Length;
                        else {
                            if (logs == true) {
                                string text = "Not found file " + file + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                        if (vars.ContainsKey(txt1)) vars[txt1] = len.ToString();
                        else vars.Add(txt1, len.ToString());
                    } else if (l[i].StartsWith("_Code ") && ConvertS(l[i].Remove(0, 6)) == "ver") Console.WriteLine(version);
                    else if (l[i].StartsWith("Process ")) {
                        string ofas3 = l[i].Remove(0, 8);
                        string varv = ofas3.Substring(0, ofas3.IndexOf(" "));
                        Process[] pname = Process.GetProcessesByName(ConvertS(ofas3.Substring(ofas3.IndexOf(" ") + 1)));
                        string va = "true";
                        if (pname.Length == 0) va = "false";
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("Screenshot ")) {
                        Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        Graphics graphics = Graphics.FromImage(printscreen as Image);
                        graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
                        printscreen.Save(ConvertS(l[i].Remove(0, 11)), System.Drawing.Imaging.ImageFormat.Png);
                    } else if (l[i].StartsWith("Admin ")) {
                        bool _runas = false;
                        string ofas3 = l[i].Remove(0, 6);
                        string varv = ofas3;
                        string va = "false";
                        using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) {
                            WindowsPrincipal principal = new WindowsPrincipal(identity);
                            _runas = principal.IsInRole(WindowsBuiltInRole.Administrator);
                        }
                        if (_runas)
                            va = "true";
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("File ")) {
                        string drag = l[i].Remove(0, 5);
                        string app = ConvertS(drag.Substring(0, drag.IndexOf(" ")));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "false";
                        if (File.Exists(app))
                            va = "true";
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("Sizef ")) {
                        string drag = l[i].Remove(0, 6);
                        string app = ConvertS(drag.Substring(0, drag.IndexOf(" ")));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "0";
                        if (File.Exists(app)) {
                            FileInfo file = new FileInfo(app);
                            double size = file.Length;
                            va = size.ToString();
                        } else {
                            if (logs == true) {
                                string text = "Not found file " + app + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("Sized ")) {
                        string drag = l[i].Remove(0, 6);
                        string app = ConvertS(drag.Substring(0, drag.IndexOf(" ")));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "0";
                        if (Directory.Exists(app)) {
                            string[] files = Directory.GetFiles(app);
                            double size = 0;
                            foreach (string fi in files) {
                                System.IO.FileInfo file = new System.IO.FileInfo(fi);
                                size += file.Length;
                            }
                            va = size.ToString();
                        } else {
                            if (logs == true) {
                                string text = "Not found directory " + app + "\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("Directory ")) {
                        string drag = l[i].Remove(0, 11);
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "false";
                        if (Directory.Exists(ConvertS(drag.Substring(0, drag.IndexOf(" ")))))
                            va = "true";
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i] == "Shutdown") Process.Start("shutdown", "/s /t 0");
                    else if (l[i] == "Restart") Process.Start("shutdown", "/r /t 0");
                    else if (l[i] == "Sleep") Process.Start("shutdown", "/h /t 0");
                    else if (l[i].StartsWith("Keyseta ")) {
                        string ofas5 = l[i].Remove(0, 8);
                        string varv = ofas5.Substring(0, ofas5.IndexOf("="));
                        Console.Write(ConvertS(ofas5.Substring(ofas5.IndexOf("=") + 1)));
                        ConsoleKeyInfo va = Console.ReadKey();
                        if (vars.ContainsKey(varv)) vars[varv] = va.Key.ToString();
                        else vars.Add(varv, va.Key.ToString());
                    } else if (l[i].StartsWith("Keysete ")) {
                        string ofas5 = l[i].Remove(0, 8);
                        string varv = ofas5.Substring(0, ofas5.IndexOf("="));
                        Console.Write(ConvertS(ofas5.Substring(ofas5.IndexOf("=") + 1)));
                        ConsoleKeyInfo va = Console.ReadKey(true);
                        if (vars.ContainsKey(varv)) vars[varv] = va.Key.ToString();
                        else vars.Add(varv, va.Key.ToString());
                    } else if (l[i].StartsWith("Download ")) {
                        string ofas3 = l[i].Remove(0, 9);
                        string froms = ConvertS(ofas3.Substring(0, ofas3.IndexOf(" ")));
                        string tos = ConvertS(ofas3.Substring(ofas3.IndexOf(" ") + 1));
                        WebClient webClient = new WebClient();
                        string patha = tos;
                        webClient.DownloadFile(froms, patha);
                    } else if (l[i].StartsWith("Internet ")) {
                        string ofas3 = l[i].Remove(0, 9);
                        string varv = ofas3;
                        string va = "false";
                        INET_CONNECTION_STATE flags;
                        if (InternetGetConnectedState(out flags, 0U) && (flags & INET_CONNECTION_STATE.INTERNET_CONNECTION_CONFIGURED) == INET_CONNECTION_STATE.INTERNET_CONNECTION_CONFIGURED) va = "true";
                        if (vars.ContainsKey(varv)) vars[varv] = va;
                        else vars.Add(varv, va);
                    } else if (l[i].StartsWith("Randomd ")) {
                        string ofas = l[i].Remove(0, 8);
                        string one = ConvertN(ofas.Substring(0, ofas.IndexOf(" ")));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = ConvertN(tam.Substring(0, tam.IndexOf("=")));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        try {
                            double a = double.Parse(one); double b = double.Parse(two);
                            Random r = new Random();
                            double result = NextDouble(r, a, b);
                            if (vars.ContainsKey(varv)) vars[varv] = result.ToString();
                            else vars.Add(varv, result.ToString());
                        } catch {
                            if (logs == true) {
                                string text = "Unkown error: " + ofas + " *Check variables, arguments\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("Randomi ")) {
                        string ofas = l[i].Remove(0, 8);
                        string one = ConvertN(ofas.Substring(0, ofas.IndexOf(" ")));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = ConvertN(tam.Substring(0, tam.IndexOf("=")));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        try {
                            int a = int.Parse(one); int b = int.Parse(two);
                            Random r = new Random();
                            int result = r.Next(a, b);
                            if (vars.ContainsKey(varv)) vars[varv] = result.ToString();
                            else vars.Add(varv, result.ToString());
                        } catch {
                            if (logs == true) {
                                string text = "Unkown error: " + ofas + " *Check variables, arguments\n";
                                File.AppendAllText(logf, text);
                            }
                        }
                    } else if (l[i].StartsWith("Messagebox ")) {
                        string ofas3a = l[i].Remove(0, 11);
                        string[] splitstringa = { "|||", "|^|" };
                        string[] ofams = ofas3a.Split(splitstringa, StringSplitOptions.None);
                        string text = ConvertS(ofams[0]);
                        string name = ConvertS(ofams[1]);
                        string theme = ConvertS(ofams[2]);
                        string size = ConvertS(ofams[3]);
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        msgbox.Controls.Clear();
                        msgbox.Text = name;
                        if (theme == "dark") msgbox.BackColor = Color.DarkGray;
                        else if (theme == "light") msgbox.BackColor = Color.White;
                        string[] splitstrings = { "x", "X" };
                        string[] sizes = size.Split(splitstrings, StringSplitOptions.None);
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        //Console.WriteLine(sizes[0] + "x" + sizes[1]);
                        int wi = int.Parse(sizes[0]);
                        int hi = int.Parse(sizes[1]);
                        msgbox.Width = wi;
                        msgbox.Height = hi;
                        msgbox.FormBorderStyle = FormBorderStyle.FixedSingle;
                        msgbox.ShowIcon = false;
                        msgbox.MaximizeBox = false;
                        msgbox.SizeGripStyle = SizeGripStyle.Hide;
                        msgbox.ShowInTaskbar = false;
                        Label la = new Label();
                        la.Text = text;
                        la.Font = new Font("Consolas", 8, FontStyle.Regular);
                        if (theme == "dark") la.ForeColor = Color.White;
                        else if (theme == "light") la.ForeColor = Color.Black;
                        la.Dock = DockStyle.Fill;
                        Button ba = new Button();
                        ba.Text = "OK";
                        ba.FlatStyle = FlatStyle.Popup;
                        if (theme == "dark") {
                            ba.BackColor = Color.DimGray;
                            ba.ForeColor = Color.White;
                        } else if (theme == "light") {
                            ba.BackColor = Color.Silver;
                            ba.ForeColor = Color.Black;
                        }
                        ba.Font = new Font("Consolas", 8, FontStyle.Bold);
                        ba.Dock = DockStyle.Bottom;
                        ba.Click += MsgBoxClose;
                        msgbox.Controls.Add(ba);
                        msgbox.Controls.Add(la);
                        msgbox.ShowDialog();
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                    } else if (l[i].StartsWith("Inputbox ")) {
                        string ofas3a = l[i].Remove(0, 9);
                        string[] splitstringa = { "|||", "|^|" };
                        string[] ofams = ofas3a.Split(splitstringa, StringSplitOptions.None);
                        string text = ConvertS(ofams[0]);
                        string name = ConvertS(ofams[1]);
                        string theme = ConvertS(ofams[2]);
                        string size = ConvertS(ofams[3]);
                        invar = ofams[4];
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        inbox.Controls.Clear();
                        inbox.Text = name;
                        if (theme == "dark") inbox.BackColor = Color.DarkGray;
                        else if (theme == "light") inbox.BackColor = Color.White;
                        string[] splitstrings = { "x", "X" };
                        string[] sizes = size.Split(splitstrings, StringSplitOptions.None);
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        //Console.WriteLine(sizes[0] + "x" + sizes[1]);
                        int wi = int.Parse(sizes[0]);
                        int hi = int.Parse(sizes[1]);
                        inbox.Width = wi;
                        inbox.Height = hi;
                        inbox.FormBorderStyle = FormBorderStyle.FixedSingle;
                        inbox.ShowIcon = false;
                        inbox.MaximizeBox = false;
                        inbox.SizeGripStyle = SizeGripStyle.Hide;
                        inbox.ShowInTaskbar = false;
                        Label la = new Label();
                        la.Text = text;
                        la.Font = new Font("Consolas", 8, FontStyle.Regular);
                        if (theme == "dark") la.ForeColor = Color.White;
                        else if (theme == "light") la.ForeColor = Color.Black;
                        la.Dock = DockStyle.Fill;
                        Button ba = new Button();
                        ba.Text = "OK";
                        ba.FlatStyle = FlatStyle.Popup;
                        if (theme == "dark") {
                            ba.BackColor = Color.DimGray;
                            ba.ForeColor = Color.White;
                        } else if (theme == "light") {
                            ba.BackColor = Color.Silver;
                            ba.ForeColor = Color.Black;
                        }
                        ba.Font = new Font("Consolas", 8, FontStyle.Bold);
                        ba.Dock = DockStyle.Bottom;
                        ba.Click += InBoxClose;
                        if (theme == "dark") {
                            tb.BackColor = Color.DimGray;
                            tb.ForeColor = Color.White;
                        } else if (theme == "light") {
                            tb.BackColor = Color.Silver;
                            tb.ForeColor = Color.Black;
                        }
                        tb.Font = new Font("Consolas", 8, FontStyle.Bold);
                        tb.Dock = DockStyle.Bottom;
                        inbox.Controls.Add(tb);
                        inbox.Controls.Add(ba);
                        inbox.Controls.Add(la);
                        inbox.ShowDialog();
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                    } else if (l[i].StartsWith("Choosebox ")) {
                        string ofas3a = l[i].Remove(0, 10);
                        string[] splitstringa = { "|||", "|^|" };
                        string[] ofams = ofas3a.Split(splitstringa, StringSplitOptions.None);
                        string name = ConvertS(ofams[0]);
                        string theme = ConvertS(ofams[1]);
                        string size = ConvertS(ofams[2]);
                        string patha = ConvertS(ofams[3]);
                        chvar = ofams[3];
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        chbox.Controls.Clear();
                        chbox.Text = name;
                        if (theme == "dark") chbox.BackColor = Color.DarkGray;
                        else if (theme == "light") chbox.BackColor = Color.White;
                        string[] splitstrings = { "x", "X" };
                        string[] sizes = size.Split(splitstrings, StringSplitOptions.None);
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        //Console.WriteLine(sizes[0] + "x" + sizes[1]);
                        int wi = int.Parse(sizes[0]);
                        int hi = int.Parse(sizes[1]);
                        chbox.Width = wi;
                        chbox.Height = hi;
                        chbox.FormBorderStyle = FormBorderStyle.FixedSingle;
                        chbox.ShowIcon = false;
                        chbox.MaximizeBox = false;
                        chbox.SizeGripStyle = SizeGripStyle.Hide;
                        chbox.ShowInTaskbar = false;
                        Button ba = new Button();
                        ba.Text = "CHOOSE";
                        ba.FlatStyle = FlatStyle.Popup;
                        if (theme == "dark") {
                            ba.BackColor = Color.DimGray;
                            ba.ForeColor = Color.White;
                        } else if (theme == "light") {
                            ba.BackColor = Color.Silver;
                            ba.ForeColor = Color.Black;
                        }
                        ba.Font = new Font("Consolas", 8, FontStyle.Bold);
                        ba.Dock = DockStyle.Bottom;
                        ba.Click += ChBoxClose;
                        if (theme == "dark") {
                            lb.BackColor = Color.DimGray;
                            lb.ForeColor = Color.White;
                        } else if (theme == "light") {
                            lb.BackColor = Color.Silver;
                            lb.ForeColor = Color.Black;
                        }
                        lb.Font = new Font("Consolas", 8, FontStyle.Bold);
                        lb.Dock = DockStyle.Fill;
                        string[] items;
                        if (File.Exists(patha)) {
                            items = File.ReadAllLines(patha);
                        } else {
                            items = new string[]
                            {
                                "NULL",
                                "ERROR"
                            };
                        }
                        for (int h = 0; h < items.Length; h++) {
                            lb.Items.Add(items[h]);
                        }
                        chbox.Controls.Add(ba);
                        chbox.Controls.Add(lb);
                        chbox.ShowDialog();
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                    } else if (l[i].StartsWith("Datef ")) {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("dd MMMM yyyy");
                        if (vars.ContainsKey(varvf)) vars[varvf] = va;
                        else vars.Add(varvf, va);
                    } else if (l[i].StartsWith("Dates ")) {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("dd.MM.yyyy");
                        if (vars.ContainsKey(varvf)) vars[varvf] = va;
                        else vars.Add(varvf, va);
                    } else if (l[i].StartsWith("Timef ")) {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("HH:mm:ss");
                        if (vars.ContainsKey(varvf)) vars[varvf] = va;
                        else vars.Add(varvf, va);
                    } else if (l[i].StartsWith("Times ")) {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("HH:mm");
                        if (vars.ContainsKey(varvf)) vars[varvf] = va;
                        else vars.Add(varvf, va);
                    } else if (l[i].StartsWith("Indexof ")) {
                        string ofas3 = l[i].Remove(0, 8);
                        string sym = ConvertS(ofas3.Substring(0, ofas3.IndexOf(" ")));
                        string j = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string froma = ConvertV(j.Substring(0, j.IndexOf("=")));
                        string to = j.Substring(j.IndexOf("=") + 1);
                        int va = froma.IndexOf(sym);
                        if (vars.ContainsKey(to)) vars[to] = va.ToString();
                        else vars.Add(to, va.ToString());
                    } else {
                        if (logs == true) {
                            string texts = "Wrong command on line " + i.ToString() + ", text: " + l[i] + " *or end of custom???\n";
                            File.AppendAllText(logf, texts);
                        }
                    }
                }
                catch
                {
                    if (logs == true)
                    {
                        string texts = "Wrong command on line " + i.ToString() + ", text: " + l[i] + " *or end of custom??? or any error\n";
                        File.AppendAllText(logf, texts);
                    }
                }
            }
        }

        public static double NextDouble(Random RandGenerator, double MinValue, double MaxValue) {
            return RandGenerator.NextDouble() * (MaxValue - MinValue) + MinValue;
        }

        public static void MsgBoxClose(object sender, EventArgs e) {
            msgbox.Close();
        }
        public static void InBoxClose(object sender, EventArgs e) {
            string va = tb.Text;
            if (vars.ContainsKey(invar)) vars[invar] = va;
            else vars.Add(invar, va);
            invar = "";
            inbox.Close();
        }
        public static void ChBoxClose(object sender, EventArgs e) {
            try {
                string va = lb.SelectedItem.ToString();
                if (vars.ContainsKey(chvar)) vars[chvar] = va;
                else vars.Add(chvar, va);
                chvar = "";
                chbox.Close();
            } catch {
                string texts = "ChooseBox error: no item selected maybe\n";
                File.AppendAllText(logf, texts);
            }
        }
    }
}