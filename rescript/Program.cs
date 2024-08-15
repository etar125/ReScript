// Created by TheProject
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

namespace rescript
{
    public class Program
    {
        public static string path;
        public static List<string> vars;
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            int uFlags);

        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;
        [DllImport("Wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InternetGetConnectedState(out INET_CONNECTION_STATE lpdwFlags, uint dwReserved);

        [Flags]
        enum INET_CONNECTION_STATE : uint
        {
            INTERNET_CONNECTION_CONFIGURED = 0x40,
            INTERNET_CONNECTION_LAN = 0x02,
            INTERNET_CONNECTION_MODEM = 0x01,
            INTERNET_CONNECTION_MODEM_BUSY = 0x08,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_PROXY = 0x04,
            INTERNET_RAS_INSTALLED = 0x10
        }
        public static void Main(string[] args)
        {
            // Лист для переменных
            vars = new List<string>
            {
                "datetime=" + DateTime.Now,
                "curdir=" + Environment.CurrentDirectory,
                "sysdir=" + Environment.SystemDirectory,
                "time1=" + DateTime.Now.ToString("HH:mm:ss"),
                "time2=" + DateTime.Now.ToString("HH:mm"),
                "date1=" + DateTime.Now.ToString("dd MMMM yyyy"),
                "date2=" + DateTime.Now.ToString("dd.MM.yyyy"),
                "auver=3.0r",
                "machinename=" + Environment.MachineName,
                "username=" + Environment.UserName,
                "osver=" + Environment.OSVersion
            };

            // Проверка на наличие аргументов и установка значения переменной path
            bool have = false;
            foreach (string l in args)
            {
                string s = l;
                if (!have)
                {
                    if (s.StartsWith("file:"))
                    {
                        s = s.Remove(0, 5);
                        if (File.Exists(s))
                        {
                            path = s;
                            have = true;
                        }
                        else
                        {
                            Console.WriteLine("Not found file " + s);
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        break;
                    }
                    else if (s == "version")
                    {
                        Console.WriteLine("AuScript 3.0r by ix4Software");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else if (s.StartsWith("teh:"))
                    {
                        s = s.Remove(0, 4);
                        string[] splitstrings = { ":", "::" };
                        string[] re = s.Split(splitstrings, StringSplitOptions.None);
                        Process p = new Process();
                        p.StartInfo.FileName = re[0];
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.Arguments = "file:" + re[1];
                        if (re[2] == "true")
                            p.StartInfo.UseShellExecute = true;
                        p.Start();
                        Thread.Sleep(500);
                        Environment.Exit(0);
                    }
                    else if (s == "topmost")
                    {
                        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                        SetWindowPos(hWnd,
                            new IntPtr(HWND_TOPMOST),
                            0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE);
                    }
                    //EnsureAssociationsSet()
                    else
                    {
                        Console.WriteLine("Wrong argument " + s);
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
            }
            if (!have)
            {
                if (File.Exists("index.aus"))
                    path = "index.aus";
                else
                {
                    Console.WriteLine("Not found index.aus");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            //other
            logs = false;
            logf = "logs.txt";

            // Вызываем основной метод
            oth(0);
        }

        //Version: 0.3.2 (10.07.2022 16:21)
        //Version: 0.3.5 (14.07.2022 21:02)
        //Version: 0.3.7 (16.07.2022 20:49)
        //Version: 0.4.1 (23.07.2022 19:33)

        [STAThread]
        public static void oth(int stat)
        {
            string[] l = File.ReadAllLines(path);
            for (int i = stat; i < l.Length; i++)
            {
                try
                {
                    if (l[i].Substring(0, 1) != "_")
                    {
                        l[i] = l[i].Substring(0, 1).ToUpper() + l[i].Substring(1, l[i].Length - 1);
                    }
                    if (l[i].Substring(0, 1) == "_")
                    {
                        l[i] = l[i].Substring(1, 2).ToUpper() + l[i].Substring(2, l[i].Length - 2);
                    }
                    if (l[i].StartsWith("Printline "))
                    {
                        string ofas3 = l[i].Remove(0, 10);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        Console.WriteLine(result);
                    }
                    else if (l[i].StartsWith("Print "))
                    {
                        string ofas3 = l[i].Remove(0, 10);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        Console.Write(result);
                    }
                    else if (l[i].StartsWith("Caption "))
                    {
                        string ofas3 = l[i].Remove(0, 8);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        Console.Title = result;
                    }
                    else if (l[i].StartsWith("Variable "))
                    {
                        string ofas3 = l[i].Remove(0, 9);
                        string varvf = ofas3.Substring(0, ofas3.IndexOf("="));
                        string valuef = ofas3.Substring(ofas3.IndexOf("=") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = valuef.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        string va = result;
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                vars[ch] = varvf + "=" + va;
                                d = true;
                                break;
                            }
                        }
                        if (!d)
                        {
                            vars.Add(varvf + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Mathp "))
                    {
                        string ofas = l[i].Remove(0, 6);
                        string one = ofas.Substring(0, ofas.IndexOf("+"));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        if (one.StartsWith("$"))
                        {
                            one = one.Remove(0, 1);
                            bool ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(one + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    one = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                one = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + one + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        if (two.StartsWith("$"))
                        {
                            two = two.Remove(0, 1);
                            bool ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(two + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    two = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                two = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + two + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        try
                        {
                            double a = double.Parse(one); double b = double.Parse(two);
                            double result = a + b;
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + result;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + result);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: " + ofas + " *Check variables, arguments|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Mathm "))
                    {
                        string ofas = l[i].Remove(0, 6);
                        string one = ofas.Substring(0, ofas.IndexOf("-"));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        if (one.StartsWith("$"))
                        {
                            one = one.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(one + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    one = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                one = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + one + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        if (two.StartsWith("$"))
                        {
                            two = two.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(two + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    two = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                two = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + two + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        try
                        {
                            double a = double.Parse(one); double b = double.Parse(two);
                            double result = a - b;
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + result;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + result);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: " + ofas + " *Check variables, arguments|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Mathx "))
                    {
                        string ofas = l[i].Remove(0, 6);
                        string one = ofas.Substring(0, ofas.IndexOf("*"));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        if (one.StartsWith("$"))
                        {
                            one = one.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(one + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    one = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                one = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + one + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        if (two.StartsWith("$"))
                        {
                            two = two.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(two + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    two = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                two = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + two + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        try
                        {
                            double a = double.Parse(one); double b = double.Parse(two);
                            double result = a * b;
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + result;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + result);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: " + ofas + " *Check variables, arguments|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Mathn "))
                    {
                        string ofas = l[i].Remove(0, 6);
                        string one = ofas.Substring(0, ofas.IndexOf("/"));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        if (one.StartsWith("$"))
                        {
                            one = one.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(one + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    one = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                one = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + one + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        if (two.StartsWith("$"))
                        {
                            two = two.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(two + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    two = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                two = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + two + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        try
                        {
                            double a = double.Parse(one); double b = double.Parse(two);
                            double result = a / b;
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + result;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + result);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: " + ofas + " *Check variables, arguments|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i] == "Pause")
                        Console.ReadKey(true);
                    else if (l[i] == "Clear")
                        Console.Clear();
                    else if (l[i] == "Exit")
                        Environment.Exit(0);
                    else if (l[i].StartsWith("Set "))
                    {
                        string ofas5 = l[i];
                        ofas5 = ofas5.Remove(0, 4);
                        string varv = ofas5.Substring(0, ofas5.IndexOf("="));
                        string txt1 = ofas5.Substring(ofas5.IndexOf("=") + 1);
                        string ofas3 = txt1;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        txt1 = result;
                        Console.Write(txt1);
                        string va = Console.ReadLine();
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("If "))
                    {
                        string ofas = l[i];
                        ofas = ofas.Remove(0, 3);
                        bool ab;
                        ab = false;
                        string def = l[i].Remove(0, 3);
                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = j.Substring(0, j.IndexOf(" "));
                        string ofas3 = text;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas5 = ofas2[s];
                            if (ofas5.StartsWith("$"))
                            {
                                ofas5 = ofas5.Remove(0, 1);
                                bool abs = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas5 + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        abs = true;
                                        break;
                                    }
                                }
                                if (abs == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string texts = File.ReadAllText(logf);
                                        texts += "Not found variable " + ofas5 + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas5.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas5;
                            }
                        }
                        text = result;
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma;
                        ma = "";
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(var + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                ma = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                            ma = "null";
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        bool nc = false;
                        if (ma == text)
                        {
                            for (int abg = 0; abg < l.Length; abg++)
                            {
                                if (l[abg] == func || l[abg] == func1)
                                {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            }
                            if (nc == false)
                            {
                                if (logs == true)
                                {
                                    string textx = File.ReadAllText(logf);
                                    textx += "Not found function " + func + "|";
                                    File.WriteAllText(logf, textx);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("Ifn "))
                    {
                        string ofas = l[i];
                        ofas = ofas.Remove(0, 4);
                        bool ab;
                        ab = false;
                        string def = l[i].Remove(0, 4);
                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = j.Substring(0, j.IndexOf(" "));
                        string ofas3 = text;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas5 = ofas2[s];
                            if (ofas5.StartsWith("$"))
                            {
                                ofas5 = ofas5.Remove(0, 1);
                                bool abs = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas5 + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        abs = true;
                                        break;
                                    }
                                }
                                if (abs == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string texts = File.ReadAllText(logf);
                                        texts += "Not found variable " + ofas5 + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas5.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas5;
                            }
                        }
                        text = result;
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma;
                        ma = "";
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(var + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                ma = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                            ma = "null";
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        bool nc = false;
                        if (ma != text)
                        {
                            for (int abg = 0; abg < l.Length; abg++)
                            {
                                if (l[abg] == func || l[abg] == func1)
                                {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            }
                            if (nc == false)
                            {
                                if (logs == true)
                                {
                                    string textx = File.ReadAllText(logf);
                                    textx += "Not found function " + func + "|";
                                    File.WriteAllText(logf, textx);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("Ifc "))
                    {
                        string ofas = l[i];
                        ofas = ofas.Remove(0, 4);
                        bool ab;
                        ab = false;
                        string def = l[i].Remove(0, 3);
                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = j.Substring(0, j.IndexOf(" "));
                        string ofas3 = text;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas5 = ofas2[s];
                            if (ofas5.StartsWith("$"))
                            {
                                ofas5 = ofas5.Remove(0, 1);
                                bool abs = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas5 + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        abs = true;
                                        break;
                                    }
                                }
                                if (abs == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string texts = File.ReadAllText(logf);
                                        texts += "Not found variable " + ofas5 + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas5.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas5;
                            }
                        }
                        text = result;
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma;
                        ma = "";
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(var + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                ma = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                            ma = "null";
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        bool nc = false;
                        if (ma.Contains(text))
                        {
                            for (int abg = 0; abg < l.Length; abg++)
                            {
                                if (l[abg] == func || l[abg] == func1)
                                {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            }
                            if (nc == false)
                            {
                                if (logs == true)
                                {
                                    string textx = File.ReadAllText(logf);
                                    textx += "Not found function " + func + "|";
                                    File.WriteAllText(logf, textx);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("To "))
                    {
                        string def = l[i].Remove(0, 3);
                        bool nc = false;
                        for (int abg = 0; abg < l.Length; abg++)
                        {
                            if (l[abg] == "Function " + def || l[abg] == "function " + def)
                            {
                                //CheckStackTrace();
                                nc = true;
                                i = abg;
                            }
                        }
                        if (nc == false)
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found function " + def + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Start "))
                    {
                        string pathd = l[i].Remove(0, 6);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = pathd.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        try
                        {
                            Process.Start(result);
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + pathd + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Starta "))
                    {
                        string pathd = l[i].Remove(0, 7);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = pathd.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        try
                        {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.FileName = pathd;
                            p.Start();
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + pathd + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("_Logs "))
                    {
                        string ofas = l[i].Remove(0, 6);
                        logs = true;
                        logf = ofas;
                        File.WriteAllText(logf, "");
                    }
                    else if (l[i].StartsWith("Forecolor "))
                    {
                        string ofas = l[i].Remove(0, "forecolor ".Length);
                        if (ofas.StartsWith("$"))
                        {
                            ofas = ofas.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(ofas + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    try
                                    {
                                        ofas = txt2;
                                    }
                                    catch
                                    {
                                        if (logs == true)
                                        {
                                            string text = File.ReadAllText(logf);
                                            text += "Unkown error: STARTOFCODE " + l[i] + " ENDOFCODE*Check first argument or variable value*okkjjjjjjjjjjjjjjjjjjjjj|";
                                            File.WriteAllText(logf, text);
                                        }
                                    }
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                ofas = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                            if (ofas == "0")
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else if (ofas == "1")
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else if (ofas == "2")
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else if (ofas == "3")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            else if (ofas == "4")
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                            else if (ofas == "5")
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }
                            else if (ofas == "6")
                            {
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found color " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        else
                        {
                            if (ofas == "0")
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else if (ofas == "1")
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else if (ofas == "2")
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else if (ofas == "3")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            else if (ofas == "4")
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                            else if (ofas == "5")
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }
                            else if (ofas == "6")
                            {
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found color " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("Backcolor "))
                    {
                        string ofas = l[i].Remove(0, "forecolor ".Length);
                        if (ofas.StartsWith("$"))
                        {
                            ofas = ofas.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(ofas + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    try
                                    {
                                        ofas = txt2;
                                    }
                                    catch
                                    {
                                        if (logs == true)
                                        {
                                            string text = File.ReadAllText(logf);
                                            text += "Unkown error: STARTOFCODE " + l[i] + " ENDOFCODE*Check first argument or variable value*|";
                                            File.WriteAllText(logf, text);
                                        }
                                    }
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                ofas = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                            if (ofas == "0")
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                            }
                            else if (ofas == "1")
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                            }
                            else if (ofas == "2")
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                            }
                            else if (ofas == "3")
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                            }
                            else if (ofas == "4")
                            {
                                Console.BackgroundColor = ConsoleColor.Yellow;
                            }
                            else if (ofas == "5")
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                            }
                            else if (ofas == "6")
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found color " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        else
                        {
                            if (ofas == "0")
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                            }
                            else if (ofas == "1")
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                            }
                            else if (ofas == "2")
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                            }
                            else if (ofas == "3")
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                            }
                            else if (ofas == "4")
                            {
                                Console.BackgroundColor = ConsoleColor.Yellow;
                            }
                            else if (ofas == "5")
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                            }
                            else if (ofas == "6")
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found color " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("Waitms "))
                    {
                        string ofas = l[i].Remove(0, 7);
                        if (ofas.StartsWith("$"))
                        {
                            ofas = ofas.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(ofas + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    try
                                    {
                                        Thread.Sleep(Convert.ToInt32(txt2));
                                    }
                                    catch
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Unkown error: STARTOFCODE " + l[i] + " ENDOFCODE*Check first argument or variable value*|";
                                        File.WriteAllText(logf, text);
                                    }
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Thread.Sleep(Convert.ToInt32(ofas));
                            }
                            catch
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: STARTOFCODE " + l[i] + " ENDOFCODE*Check first argument*|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Waits "))
                    {
                        string ofas = l[i].Remove(0, 6);
                        if (ofas.StartsWith("$"))
                        {
                            ofas = ofas.Remove(0, 1);
                            bool ab;
                            ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(ofas + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    try
                                    {
                                        Thread.Sleep(Convert.ToInt32(txt2) * 1000);
                                    }
                                    catch
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Unkown error: STARTOFCODE " + l[i] + " ENDOFCODE*Check first argument or variable value*|";
                                        File.WriteAllText(logf, text);
                                    }
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + ofas + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Thread.Sleep(Convert.ToInt32(ofas) * 1000);
                            }
                            catch
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: STARTOFCODE " + l[i] + " ENDOFCODE*Check first argument*|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i] == "Desktopoff")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer /v NoDesktop /t REG_DWORD /d 1 /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i] == "Allappsoff")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\RestrictRun /v 1 /t REG_DWORD /d %SystemRoot%\explorer.exe /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i] == "Controlpanoff")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCU\Software\Microsoft\Windows\Current Version\Policies\Explorer /v NoControlPanel /t REG_DWORD /d 1 /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i] == "CADoff")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"reg add HKCUSoftwareMicrosoftWindowsCurrentVersionPoliciesSystem /v DisableTaskMgr /t REG_DWORD /d 1 /f >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i] == "Cursoroff")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + "del \"%SystemRoot%Cursors*.*\" >nul";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i] == "Milionfolders")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + "FOR /L %%i IN (1,1,1000000) DO md %%i";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i] == "Systemdelete")
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/C " + @"del %systemroot%\system32\HAL.dll";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }
                    else if (l[i].StartsWith("Startargs "))
                    {
                        string drag = l[i].Remove(0, 10);
                        string app = drag.Substring(0, drag.IndexOf("_"));
                        string args = drag.Substring(drag.IndexOf("_") + 1);
                        string ofas3 = args;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        args = result;
                        string ofas4 = app;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas5 = ofas4.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int h = 0; h < ofas5.Length; h++)
                        {
                            string ofas = ofas5[h];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        app = results;
                        Process p = new Process();
                        p.StartInfo.FileName = app;
                        p.StartInfo.Arguments = args;
                        p.Start();
                    }
                    else if (l[i].StartsWith("Startargsa "))
                    {
                        string drag = l[i].Remove(0, 11);
                        string app = drag.Substring(0, drag.IndexOf("_"));
                        string args = drag.Substring(drag.IndexOf("_") + 1);
                        string ofas3 = args;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        args = result;
                        string ofas4 = app;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas5 = ofas4.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int h = 0; h < ofas5.Length; h++)
                        {
                            string ofas = ofas5[h];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        app = results;
                        Process p = new Process();
                        p.StartInfo.FileName = app;
                        p.StartInfo.Arguments = args;
                        p.StartInfo.UseShellExecute = true;
                        p.Start();
                    }
                    else if (l[i].StartsWith("Readline "))
                    {
                        string bypass = l[i].Remove(0, 9);
                        string[] splitstrs = { "|||", "|^|" };
                        string[] splt = bypass.Split(splitstrs, StringSplitOptions.None);
                        string ofas3 = splt[0];
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        splt[0] = result;
                        string ofas7 = splt[2];
                        string[] splitstringss = { "&&&", "&^&" };
                        string[] ofas6 = ofas7.Split(splitstringss, StringSplitOptions.None);
                        string resultss = "";
                        for (int s = 0; s < ofas6.Length; s++)
                        {
                            string ofas = ofas6[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        resultss += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    resultss += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                resultss += tit;
                            }
                            else
                            {
                                resultss += ofas;
                            }
                        }
                        splt[2] = resultss;
                        string paths = splt[0];
                        string varv = splt[1];
                        string[] textxa;
                        int ln = Convert.ToInt32(splt[2]);
                        if (File.Exists(paths))
                        {
                            textxa = File.ReadAllLines(paths);
                            string va = "null";
                            try
                            {
                                va = textxa[ln];
                            }
                            catch
                            {

                            }
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + va;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + va);
                            }
                        }
                        else
                        {
                            textxa = File.ReadAllLines(path);
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + paths + "|";
                                File.WriteAllText(logf, text);
                            }
                            string va = "null";
                            try
                            {
                                va = textxa[ln];
                            }
                            catch
                            {

                            }
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + va;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + va);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Writeline "))
                    {
                        string bypass = l[i].Remove(0, 10);
                        string[] splitstrs = { "|||", "|^|" };
                        string[] splt = bypass.Split(splitstrs, StringSplitOptions.None);
                        string ofas3 = splt[0];
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        splt[0] = result;
                        string ofas5 = splt[1];
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas4 = ofas5.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas4.Length; s++)
                        {
                            string ofas = ofas4[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        splt[1] = results;
                        string ofas7 = splt[2];
                        string[] splitstringss = { "&&&", "&^&" };
                        string[] ofas6 = ofas7.Split(splitstringss, StringSplitOptions.None);
                        string resultss = "";
                        for (int s = 0; s < ofas6.Length; s++)
                        {
                            string ofas = ofas6[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        resultss += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    resultss += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                resultss += tit;
                            }
                            else
                            {
                                resultss += ofas;
                            }
                        }
                        splt[2] = resultss;
                        string paths = splt[0];
                        string txta = splt[1];
                        string[] textxa;
                        if (splt[2] == "-1")
                        {
                            File.WriteAllText(paths, txta);
                        }
                        else
                        {
                            int ln = Convert.ToInt32(splt[2]);
                            if (File.Exists(paths))
                            {
                                try
                                {
                                    textxa = File.ReadAllLines(paths);
                                    textxa[ln] = txta;
                                    File.WriteAllLines(paths, textxa);
                                }
                                catch
                                {
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found line " + ln + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    else if (l[i].StartsWith("Crdir "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        Directory.CreateDirectory(result);
                    }
                    else if (l[i].StartsWith("Rmdir "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        Directory.Delete(result, true);
                    }
                    else if (l[i].StartsWith("Moved "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string tos = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = froms.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        froms = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = tos.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        tos = results;
                        Directory.Move(froms, tos);
                    }
                    else if (l[i].StartsWith("Movef "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string tos = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = froms.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        froms = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = tos.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        tos = results;
                        File.Move(froms, tos);
                    }
                    else if (l[i].StartsWith("Copyf "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string tos = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = froms.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        froms = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = tos.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        tos = results;
                        File.Copy(froms, tos);
                    }
                    else if (l[i].StartsWith("Named "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string froms = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string tos = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = froms.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        froms = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = tos.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        tos = results;
                        Directory.Move(froms, tos);
                    }
                    else if (l[i].StartsWith("Crfil "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        File.WriteAllText(result, "");
                    }
                    else if (l[i].StartsWith("Rmfil "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        if (File.Exists(result))
                            File.Delete(result);
                        else
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + result + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Addline "))
                    {
                        string bypass = l[i].Remove(0, 8);
                        string[] splitstrs = { "|||", "|^|" };
                        string[] splt = bypass.Split(splitstrs, StringSplitOptions.None);
                        string ofas3 = splt[0];
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        splt[0] = result;
                        string ofas5 = splt[1];
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas4 = ofas5.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas4.Length; s++)
                        {
                            string ofas = ofas4[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        splt[1] = results;
                        string pathas = splt[0];
                        string txtad = splt[1];
                        try
                        {
                            File.AppendAllText(pathas, Environment.NewLine + txtad);
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + pathas + "    *or other error|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Printlines "))
                    {
                        string bypass = l[i].Remove(0, 11);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = bypass.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        bypass = result;
                        string[] textxa;
                        if (File.Exists(bypass))
                        {
                            textxa = File.ReadAllLines(bypass);
                            for (int lna = 0; lna < textxa.Length; lna++)
                            {
                                Console.WriteLine(textxa[lna]);
                            }
                        }

                        else
                        {
                            textxa = File.ReadAllLines(path);
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + bypass + "|";
                                File.WriteAllText(logf, text);
                            }
                            for (int lna = 0; lna < textxa.Length; lna++)
                            {
                                Console.WriteLine(textxa[lna]);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Removechars "))
                    {
                        string ofas3 = l[i].Remove(0, 12);
                        string strt = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string ana = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string valuef = ana.Substring(0, ana.IndexOf("="));
                        string varvf = ana.Substring(ana.IndexOf("=") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = valuef.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        string da = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = strt.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        string fa = results;
                        string va = "";
                        bool abs = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                va = txt2;
                                abs = true;
                                break;
                            }
                        }
                        if (abs == false)
                        {
                            va = "null";
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found variable " + varvf + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        /*
	                     * Зачем эти строки? ДЛя проверки.
	                    Console.WriteLine(fa);
	                    Console.WriteLine(da);
	                    Console.WriteLine(varvf);
	                    */
                        try
                        {
                            int chfr = Convert.ToInt32(da);
                            int chfr2 = Convert.ToInt32(fa);
                            va = va.Remove(chfr2, chfr);
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varvf + "="))
                                {
                                    vars[ch] = varvf + "=" + va;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varvf + "=" + va);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Wrong value: " + valuef + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Substring "))
                    {
                        string ofas3 = l[i].Remove(0, 10);
                        string strt = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string ana = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string valuef = ana.Substring(0, ana.IndexOf("="));
                        string varvf = ana.Substring(ana.IndexOf("=") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = valuef.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        string da = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = strt.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        string fa = results;
                        string va = "";
                        bool abs = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                va = txt2;
                                abs = true;
                                break;
                            }
                        }
                        if (abs == false)
                        {
                            va = "null";
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found variable " + varvf + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        /*
	                     * Зачем эти строки? ДЛя проверки.
	                    Console.WriteLine(fa);
	                    Console.WriteLine(da);
	                    Console.WriteLine(varvf);
	                    */
                        try
                        {
                            int chfr = Convert.ToInt32(da);
                            int chfr2 = Convert.ToInt32(fa);
                            va = va.Substring(chfr2, chfr);
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varvf + "="))
                                {
                                    vars[ch] = varvf + "=" + va;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varvf + "=" + va);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Wrong value: " + valuef + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Ifstw "))
                    {
                        string ofas = l[i];
                        ofas = ofas.Remove(0, 6);
                        bool ab;
                        ab = false;
                        string def = l[i].Remove(0, 6);
                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = j.Substring(0, j.IndexOf(" "));
                        string ofas3 = text;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas5 = ofas2[s];
                            if (ofas5.StartsWith("$"))
                            {
                                ofas5 = ofas5.Remove(0, 1);
                                bool abs = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas5 + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        abs = true;
                                        break;
                                    }
                                }
                                if (abs == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string texts = File.ReadAllText(logf);
                                        texts += "Not found variable " + ofas5 + "|";
                                        File.WriteAllText(logf, texts);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas5.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas5;
                            }
                        }
                        text = result;
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma;
                        ma = "";
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(var + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                ma = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                            ma = "null";
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        bool nc = false;
                        if (ma.StartsWith(text))
                        {
                            for (int abg = 0; abg < l.Length; abg++)
                            {
                                if (l[abg] == func || l[abg] == func1)
                                {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            }
                            if (nc == false)
                            {
                                if (logs == true)
                                {
                                    string textx = File.ReadAllText(logf);
                                    textx += "Not found function " + func + "|";
                                    File.WriteAllText(logf, textx);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("#"))
                    {
                        //comment1
                        //comment2
                    }
                    else if (l[i].StartsWith("Kill "))
                    {
                        string ofas3 = l[i].Remove(0, 5);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        Process[] localByName = Process.GetProcessesByName(result);
                        foreach (Process p in localByName)
                        {
                            p.Kill();
                        }
                    }
                    else if (l[i].StartsWith("Length "))
                    {
                        string ofas5 = l[i];
                        ofas5 = ofas5.Remove(0, 7);
                        string varv = ofas5.Substring(0, ofas5.IndexOf(" "));
                        string txt1 = ofas5.Substring(ofas5.IndexOf(" ") + 1);
                        int len = 0;
                        string ofas = varv;
                        bool ab = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(ofas + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                len = txt2.Length;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                        {
                            len = 0;
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found variable " + ofas + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(txt1 + "="))
                            {
                                vars[ch] = txt1 + "=" + len;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(txt1 + "=" + len);
                        }

                    }
                    else if (l[i].StartsWith("Longlength "))
                    {
                        string ofas5 = l[i];
                        ofas5 = ofas5.Remove(0, 11);
                        string varv = ofas5.Substring(0, ofas5.IndexOf(" "));
                        string txt1 = ofas5.Substring(ofas5.IndexOf(" ") + 1);
                        int len = 0;
                        string ofas = varv;
                        string file = "";
                        bool ab = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(ofas + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                file = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                        {
                            file = path;
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found variable " + ofas + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        try
                        {
                            string[] lns = File.ReadAllLines(file);
                            len = lns.Length;
                        }
                        catch
                        {
                            len = 0;
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file?: " + file + "   *OR UNKOWN ERROR|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(txt1 + "="))
                            {
                                vars[ch] = txt1 + "=" + len;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(txt1 + "=" + len);
                        }
                    }
                    else if (l[i].StartsWith("_Code "))
                    {
                        string ofas3 = l[i].Remove(0, 6);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        if (result == "ver")
                        {
                            Console.WriteLine("AuScript v3.0 Release by ix4Software");
                        }
                    }
                    else if (l[i].StartsWith("Process "))
                    {
                        string ofas3 = l[i].Remove(0, 8);
                        string varv = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string froms = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = froms.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        froms = result;
                        Process[] pname = Process.GetProcessesByName(froms);
                        if (pname.Length == 0)
                        {
                            string va = "false";
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + va;
                                    break;
                                }
                                vars.Add(varv + "=" + va);
                            }
                        }
                        else
                        {
                            string va = "true";
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + va;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + va);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Screenshot "))
                    {
                        string pathe = l[i].Remove(0, 11);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = pathe.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        pathe = result;
                        Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        Graphics graphics = Graphics.FromImage(printscreen as Image);
                        graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
                        printscreen.Save(pathe, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else if (l[i].StartsWith("Admin "))
                    {

                        bool _runas;
                        string ofas3 = l[i].Remove(0, 6);
                        string varv = ofas3;
                        string va = "false";
                        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                        {
                            WindowsPrincipal principal = new WindowsPrincipal(identity);
                            _runas = principal.IsInRole(WindowsBuiltInRole.Administrator);
                        }
                        if (_runas)
                            va = "true";
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("File "))
                    {
                        string drag = l[i].Remove(0, 5);
                        string app = drag.Substring(0, drag.IndexOf(" "));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "false";
                        string ofas4 = app;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas5 = ofas4.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int h = 0; h < ofas5.Length; h++)
                        {
                            string ofas = ofas5[h];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        app = results;
                        if (File.Exists(app))
                            va = "true";
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("If+ "))
                    {
                        string ofas = l[i];
                        ofas = ofas.Remove(0, 4);
                        bool ab;
                        ab = false;
                        string def = l[i].Remove(0, 4);
                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = j.Substring(0, j.IndexOf(" "));
                        string ofas3 = text;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas5 = ofas2[s];
                            if (ofas5.StartsWith("$"))
                            {
                                ofas5 = ofas5.Remove(0, 1);
                                bool abs = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas5 + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        abs = true;
                                        break;
                                    }
                                }
                                if (abs == false)
                                {
                                    result += "0";
                                    if (logs == true)
                                    {
                                        string texts = File.ReadAllText(logf);
                                        texts += "Not found variable " + ofas5 + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas5.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas5;
                            }
                        }
                        text = result;
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma;
                        ma = "";
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(var + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                ma = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                            ma = "0";
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        double a = 0; double b = 0;
                        try
                        {
                            a = double.Parse(ma);
                            b = double.Parse(text);
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string texts = File.ReadAllText(logf);
                                texts += "Wrong argument|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        bool nc = false;
                        if (a > b)
                        {
                            for (int abg = 0; abg < l.Length; abg++)
                            {
                                if (l[abg] == func || l[abg] == func1)
                                {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            }
                            if (nc == false)
                            {
                                if (logs == true)
                                {
                                    string textx = File.ReadAllText(logf);
                                    textx += "Not found function " + func + "|";
                                    File.WriteAllText(logf, textx);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("If- "))
                    {
                        string ofas = l[i];
                        ofas = ofas.Remove(0, 4);
                        bool ab;
                        ab = false;
                        string def = l[i].Remove(0, 4);
                        string var = def.Substring(0, def.IndexOf("="));
                        string j = def.Remove(0, var.Length + 1);
                        string text = j.Substring(0, j.IndexOf(" "));
                        string ofas3 = text;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas5 = ofas2[s];
                            if (ofas5.StartsWith("$"))
                            {
                                ofas5 = ofas5.Remove(0, 1);
                                bool abs = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas5 + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        abs = true;
                                        break;
                                    }
                                }
                                if (abs == false)
                                {
                                    result += "0";
                                    if (logs == true)
                                    {
                                        string texts = File.ReadAllText(logf);
                                        texts += "Not found variable " + ofas5 + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas5.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas5;
                            }
                        }
                        text = result;
                        string func = "Function " + def.Substring(def.IndexOf(" ") + 1);
                        string func1 = "function " + def.Substring(def.IndexOf(" ") + 1);
                        string ma;
                        ma = "";
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(var + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                ma = txt2;
                                ab = true;
                                break;
                            }
                        }
                        if (ab == false)
                            ma = "0";
                        //Console.WriteLine(var + "|" + j + "|" + text + "|" + func + "|" + ma);
                        double a = 0; double b = 0;
                        try
                        {
                            a = double.Parse(ma);
                            b = double.Parse(text);
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string texts = File.ReadAllText(logf);
                                texts += "Wrong argument|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        bool nc = false;
                        if (a < b)
                        {
                            for (int abg = 0; abg < l.Length; abg++)
                            {
                                if (l[abg] == func || l[abg] == func1)
                                {
                                    nc = true;
                                    i = abg;
                                    break;
                                }
                            }
                            if (nc == false)
                            {
                                if (logs == true)
                                {
                                    string textx = File.ReadAllText(logf);
                                    textx += "Not found function " + func + "|";
                                    File.WriteAllText(logf, textx);
                                }
                            }
                        }
                    }
                    else if (l[i].StartsWith("Sizef "))
                    {
                        string drag = l[i].Remove(0, 6);
                        string app = drag.Substring(0, drag.IndexOf(" "));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "0";
                        string ofas4 = app;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas5 = ofas4.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int h = 0; h < ofas5.Length; h++)
                        {
                            string ofas = ofas5[h];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        app = results;
                        if (File.Exists(app))
                        {
                            System.IO.FileInfo file = new System.IO.FileInfo(app);
                            double size = file.Length;
                            va = size.ToString();
                        }
                        else
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found file " + app + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Sized "))
                    {
                        string drag = l[i].Remove(0, 6);
                        string app = drag.Substring(0, drag.IndexOf(" "));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "0";
                        string ofas4 = app;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas5 = ofas4.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int h = 0; h < ofas5.Length; h++)
                        {
                            string ofas = ofas5[h];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        app = results;
                        if (Directory.Exists(app))
                        {
                            string[] files = Directory.GetFiles(app);
                            double size = 0;
                            foreach (string fi in files)
                            {
                                System.IO.FileInfo file = new System.IO.FileInfo(fi);
                                size += file.Length;
                            }
                            va = size.ToString();
                        }
                        else
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Not found directory " + app + "|";
                                File.WriteAllText(logf, text);
                            }
                        }
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Directory "))
                    {
                        string drag = l[i].Remove(0, 11);
                        string app = drag.Substring(0, drag.IndexOf(" "));
                        string varv = drag.Substring(drag.IndexOf(" ") + 1);
                        string va = "false";
                        string ofas4 = app;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas5 = ofas4.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int h = 0; h < ofas5.Length; h++)
                        {
                            string ofas = ofas5[h];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        app = results;
                        if (Directory.Exists(app))
                            va = "true";
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i] == "Shutdown")
                        Process.Start("shutdown", "/s /t 0");
                    else if (l[i] == "Restart")
                        Process.Start("shutdown", "/r /t 0");
                    else if (l[i] == "Sleep")
                        Process.Start("shutdown", "/h /t 0");
                    else if (l[i].StartsWith("Keyseta "))
                    {
                        string ofas5 = l[i];
                        ofas5 = ofas5.Remove(0, 8);
                        string varv = ofas5.Substring(0, ofas5.IndexOf("="));
                        string txt1 = ofas5.Substring(ofas5.IndexOf("=") + 1);
                        string ofas3 = txt1;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        txt1 = result;
                        Console.Write(txt1);
                        ConsoleKeyInfo va = Console.ReadKey();
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va.Key;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va.Key);
                        }
                    }
                    else if (l[i].StartsWith("Keysete "))
                    {
                        string ofas5 = l[i];
                        ofas5 = ofas5.Remove(0, 8);
                        string varv = ofas5.Substring(0, ofas5.IndexOf("="));
                        string txt1 = ofas5.Substring(ofas5.IndexOf("=") + 1);
                        string ofas3 = txt1;
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = ofas3.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        txt1 = result;
                        Console.Write(txt1);
                        ConsoleKeyInfo va = Console.ReadKey(true);
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va.Key;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va.Key);
                        }
                    }
                    else if (l[i].StartsWith("Download "))
                    {
                        string ofas3 = l[i].Remove(0, 9);
                        string froms = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string tos = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = froms.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        froms = result;
                        string[] splitstrings = { "&&&", "&^&" };
                        string[] ofas2s = tos.Split(splitstrings, StringSplitOptions.None);
                        string results = "";
                        for (int s = 0; s < ofas2s.Length; s++)
                        {
                            string ofas = ofas2s[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        results += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    results += "null";
                                    if (logs == true)
                                    {
                                        string text = File.ReadAllText(logf);
                                        text += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                results += tit;
                            }
                            else
                            {
                                results += ofas;
                            }
                        }
                        tos = results;
                        WebClient webClient = new WebClient();
                        string patha = tos;
                        webClient.DownloadFile(froms, patha);
                    }
                    else if (l[i].StartsWith("Internet "))
                    {
                        string ofas3 = l[i].Remove(0, 9);
                        string varv = ofas3;
                        string va = "false";
                        INET_CONNECTION_STATE flags;
                        if (InternetGetConnectedState(out flags, 0U) && (flags & INET_CONNECTION_STATE.INTERNET_CONNECTION_CONFIGURED) == INET_CONNECTION_STATE.INTERNET_CONNECTION_CONFIGURED)
                        {
                            va = "true";
                        }
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varv + "="))
                            {
                                vars[ch] = varv + "=" + va;
                                d = true;
                                break;
                            }

                        }
                        if (!d)
                        {
                            vars.Add(varv + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Randomd "))
                    {
                        string ofas = l[i].Remove(0, 8);
                        string one = ofas.Substring(0, ofas.IndexOf(" "));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        if (one.StartsWith("$"))
                        {
                            one = one.Remove(0, 1);
                            bool ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(one + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    one = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                one = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + one + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        if (two.StartsWith("$"))
                        {
                            two = two.Remove(0, 1);
                            bool ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(two + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    two = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                two = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + two + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        try
                        {
                            double a = double.Parse(one); double b = double.Parse(two);
                            Random r = new Random();
                            double result = NextDouble(r, a, b);
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + result;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + result);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: " + ofas + " *Check variables, arguments|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Randomi "))
                    {
                        string ofas = l[i].Remove(0, 8);
                        string one = ofas.Substring(0, ofas.IndexOf(" "));
                        string tam = ofas.Remove(0, one.Length + 1);
                        string two = tam.Substring(0, tam.IndexOf("="));
                        string varv = ofas.Substring(ofas.IndexOf("=") + 1);
                        if (one.StartsWith("$"))
                        {
                            one = one.Remove(0, 1);
                            bool ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(one + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    one = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                one = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + one + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        if (two.StartsWith("$"))
                        {
                            two = two.Remove(0, 1);
                            bool ab = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(two + "="))
                                {
                                    string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                    two = txt2;
                                    ab = true;
                                    break;
                                }
                            }
                            if (ab == false)
                            {
                                two = "0";
                                if (logs == true)
                                {
                                    string text = File.ReadAllText(logf);
                                    text += "Not found variable " + two + "|";
                                    File.WriteAllText(logf, text);
                                }
                            }
                        }
                        try
                        {
                            int a = int.Parse(one); int b = int.Parse(two);
                            Random r = new Random();
                            int result = r.Next(a, b);
                            bool d = false;
                            for (int ch = 0; ch < vars.Count; ch++)
                            {
                                if (vars[ch].StartsWith(varv + "="))
                                {
                                    vars[ch] = varv + "=" + result;
                                    d = true;
                                    break;
                                }

                            }
                            if (!d)
                            {
                                vars.Add(varv + "=" + result);
                            }
                        }
                        catch
                        {
                            if (logs == true)
                            {
                                string text = File.ReadAllText(logf);
                                text += "Unkown error: " + ofas + " *Check variables, arguments|";
                                File.WriteAllText(logf, text);
                            }
                        }
                    }
                    else if (l[i].StartsWith("Messagebox "))
                    {
                        string ofas3a = l[i].Remove(0, 11);
                        string[] splitstringa = { "|||", "|^|" };
                        string[] ofams = ofas3a.Split(splitstringa, StringSplitOptions.None);
                        string text = ofams[0];
                        string name = ofams[1];
                        string theme = ofams[2];
                        string size = ofams[3];
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = text.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        text = result;
                        ofas2 = name.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        name = result;
                        ofas2 = theme.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        theme = result;
                        ofas2 = size.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        size = result;
                        msgbox.Controls.Clear();
                        msgbox.Text = name;
                        if (theme == "dark")
                            msgbox.BackColor = Color.DarkGray;
                        if (theme == "light")
                            msgbox.BackColor = Color.White;
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
                        if (theme == "dark")
                            la.ForeColor = Color.White;
                        if (theme == "light")
                            la.ForeColor = Color.Black;
                        la.Dock = DockStyle.Fill;
                        Button ba = new Button();
                        ba.Text = "OK";
                        ba.FlatStyle = FlatStyle.Popup;
                        if (theme == "dark")
                        {
                            ba.BackColor = Color.DimGray;
                            ba.ForeColor = Color.White;
                        }
                        if (theme == "light")
                        {
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
                    }
                    else if (l[i].StartsWith("Inputbox "))
                    {
                        string ofas3a = l[i].Remove(0, 9);
                        string[] splitstringa = { "|||", "|^|" };
                        string[] ofams = ofas3a.Split(splitstringa, StringSplitOptions.None);
                        string text = ofams[0];
                        string name = ofams[1];
                        string theme = ofams[2];
                        string size = ofams[3];
                        invar = ofams[4];
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2 = text.Split(splitstring, StringSplitOptions.None);
                        string result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        text = result;
                        ofas2 = name.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        name = result;
                        ofas2 = theme.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        theme = result;
                        ofas2 = size.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        size = result;
                        inbox.Controls.Clear();
                        inbox.Text = name;
                        if (theme == "dark")
                            inbox.BackColor = Color.DarkGray;
                        if (theme == "light")
                            inbox.BackColor = Color.White;
                        string[] splitstrings = { "x", "X" };
                        string[] sizes = size.Split(splitstrings, StringSplitOptions.None);
                        Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        Console.WriteLine(sizes[0] + "x" + sizes[1]);
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
                        if (theme == "dark")
                            la.ForeColor = Color.White;
                        if (theme == "light")
                            la.ForeColor = Color.Black;
                        la.Dock = DockStyle.Fill;
                        Button ba = new Button();
                        ba.Text = "OK";
                        ba.FlatStyle = FlatStyle.Popup;
                        if (theme == "dark")
                        {
                            ba.BackColor = Color.DimGray;
                            ba.ForeColor = Color.White;
                        }
                        if (theme == "light")
                        {
                            ba.BackColor = Color.Silver;
                            ba.ForeColor = Color.Black;
                        }
                        ba.Font = new Font("Consolas", 8, FontStyle.Bold);
                        ba.Dock = DockStyle.Bottom;
                        ba.Click += InBoxClose;
                        if (theme == "dark")
                        {
                            tb.BackColor = Color.DimGray;
                            tb.ForeColor = Color.White;
                        }
                        if (theme == "light")
                        {
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
                    }
                    else if (l[i].StartsWith("Choosebox "))
                    {
                        string ofas3a = l[i].Remove(0, 10);
                        string[] splitstringa = { "|||", "|^|" };
                        string[] ofams = ofas3a.Split(splitstringa, StringSplitOptions.None);
                        string name = ofams[0];
                        string theme = ofams[1];
                        string size = ofams[2];
                        string patha = ofams[4];
                        chvar = ofams[3];
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2;
                        string result;
                        ofas2 = name.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        name = result;
                        ofas2 = theme.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        theme = result;
                        ofas2 = size.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        size = result;
                        chbox.Controls.Clear();
                        chbox.Text = name;
                        if (theme == "dark")
                            chbox.BackColor = Color.DarkGray;
                        if (theme == "light")
                            chbox.BackColor = Color.White;
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
                        if (theme == "dark")
                        {
                            ba.BackColor = Color.DimGray;
                            ba.ForeColor = Color.White;
                        }
                        if (theme == "light")
                        {
                            ba.BackColor = Color.Silver;
                            ba.ForeColor = Color.Black;
                        }
                        ba.Font = new Font("Consolas", 8, FontStyle.Bold);
                        ba.Dock = DockStyle.Bottom;
                        ba.Click += ChBoxClose;
                        if (theme == "dark")
                        {
                            lb.BackColor = Color.DimGray;
                            lb.ForeColor = Color.White;
                        }
                        if (theme == "light")
                        {
                            lb.BackColor = Color.Silver;
                            lb.ForeColor = Color.Black;
                        }
                        lb.Font = new Font("Consolas", 8, FontStyle.Bold);
                        lb.Dock = DockStyle.Fill;
                        string[] items;
                        if (File.Exists(patha))
                        {
                            items = File.ReadAllLines(patha);
                        }
                        else
                        {
                            items = new string[]
                            {
                                "NULL",
                                "ERROR"
                            };
                        }
                        for (int h = 0; h < items.Length; h++)
                        {
                            lb.Items.Add(items[h]);
                        }
                        chbox.Controls.Add(ba);
                        chbox.Controls.Add(lb);
                        chbox.ShowDialog();
                        //Console.WriteLine(text + "|" + name + "|" + theme + "|" + size);
                    }
                    else if (l[i].StartsWith("Datef "))
                    {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("dd MMMM yyyy");
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                vars[ch] = varvf + "=" + va;
                                d = true;
                                break;
                            }
                        }
                        if (!d)
                        {
                            vars.Add(varvf + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Dates "))
                    {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("dd.MM.yyyy");
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                vars[ch] = varvf + "=" + va;
                                d = true;
                                break;
                            }
                        }
                        if (!d)
                        {
                            vars.Add(varvf + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Timef "))
                    {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("HH:mm:ss");
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                vars[ch] = varvf + "=" + va;
                                d = true;
                                break;
                            }
                        }
                        if (!d)
                        {
                            vars.Add(varvf + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Times "))
                    {
                        string varvf = l[i].Remove(0, 6);
                        string va = DateTime.Now.ToString("HH:mm");
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(varvf + "="))
                            {
                                vars[ch] = varvf + "=" + va;
                                d = true;
                                break;
                            }
                        }
                        if (!d)
                        {
                            vars.Add(varvf + "=" + va);
                        }
                    }
                    else if (l[i].StartsWith("Indexof "))
                    {
                        string ofas3 = l[i].Remove(0, 8);
                        string sym = ofas3.Substring(0, ofas3.IndexOf(" "));
                        string j = ofas3.Substring(ofas3.IndexOf(" ") + 1);
                        string froma = j.Substring(0, j.IndexOf("="));
                        string to = j.Substring(j.IndexOf("=") + 1);
                        string[] splitstring = { "&&&", "&^&" };
                        string[] ofas2;
                        string result;
                        bool a1b = false;
                        ofas2 = sym.Split(splitstring, StringSplitOptions.None);
                        result = "";
                        for (int s = 0; s < ofas2.Length; s++)
                        {
                            string ofas = ofas2[s];
                            if (ofas.StartsWith("$"))
                            {
                                ofas = ofas.Remove(0, 1);
                                bool ab = false;
                                for (int ch = 0; ch < vars.Count; ch++)
                                {
                                    if (vars[ch].StartsWith(ofas + "="))
                                    {
                                        string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                        result += txt2;
                                        ab = true;
                                        break;
                                    }
                                }
                                if (ab == false)
                                {
                                    result += "null";
                                    if (logs == true)
                                    {
                                        string text1 = File.ReadAllText(logf);
                                        text1 += "Not found variable " + ofas + "|";
                                        File.WriteAllText(logf, text1);
                                    }
                                }
                            }
                            else if (ofas.StartsWith("/$"))
                            {
                                string tit = ofas.Remove(0, 1);
                                result += tit;
                            }
                            else
                            {
                                result += ofas;
                            }
                        }
                        sym = result;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(froma + "="))
                            {
                                string txt2 = vars[ch].Substring(vars[ch].IndexOf("=") + 1);
                                froma = txt2;
                                a1b = true;
                                break;
                            }
                        }
                        if (!a1b)
                        {
                            froma = "null";
                            if (logs == true)
                            {
                                string text1 = File.ReadAllText(logf);
                                text1 += "Not found variable " + froma + "|";
                                File.WriteAllText(logf, text1);
                            }
                        }
                        int va = froma.IndexOf(sym);
                        bool d = false;
                        for (int ch = 0; ch < vars.Count; ch++)
                        {
                            if (vars[ch].StartsWith(to + "="))
                            {
                                vars[ch] = to + "=" + va;
                                d = true;
                                break;
                            }
                        }
                        if (!d)
                        {
                            vars.Add(to + "=" + va);
                        }
                    }
                    else
                    {
                        if (logs == true)
                        {
                            string texts = File.ReadAllText(logf);
                            texts += "Wrong command on line " + i.ToString() + ", text: " + l[i] + " *or end of custom???|";
                            File.WriteAllText(logf, texts);
                        }
                    }
                }
                catch
                {
                    if (logs == true)
                    {
                        string texts = File.ReadAllText(logf);
                        texts += "Wrong command on line " + i.ToString() + ", text: " + l[i] + " *or end of custom??? or any error|";
                        File.WriteAllText(logf, texts);
                    }
                }
            }
        }

        public static double NextDouble(Random RandGenerator, double MinValue, double MaxValue)
        {
            return RandGenerator.NextDouble() * (MaxValue - MinValue) + MinValue;
        }

        public static void MsgBoxClose(object sender, EventArgs e)
        {
            msgbox.Close();
        }
        public static void InBoxClose(object sender, EventArgs e)
        {
            string va = tb.Text;
            bool d = false;
            for (int ch = 0; ch < vars.Count; ch++)
            {
                if (vars[ch].StartsWith(invar + "="))
                {
                    vars[ch] = invar + "=" + va;
                    d = true;
                    break;
                }
            }
            if (!d)
            {
                vars.Add(invar + "=" + va);
            }
            invar = "";
            inbox.Close();
        }
        public static void ChBoxClose(object sender, EventArgs e)
        {
            try
            {
                string va = lb.SelectedItem.ToString();
                bool d = false;
                for (int ch = 0; ch < vars.Count; ch++)
                {
                    if (vars[ch].StartsWith(chvar + "="))
                    {
                        vars[ch] = chvar + "=" + va;
                        d = true;
                        break;
                    }
                }
                if (!d)
                {
                    vars.Add(chvar + "=" + va);
                }
                chvar = "";
                chbox.Close();
            }
            catch
            {

            }
        }
    }
}