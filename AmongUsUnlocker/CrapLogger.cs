using System;
using System.Collections.Generic;
using System.Text;

namespace AmongUsUnlocker
{
    public class CrapLogger
    {
        public string Name { get; private set; }

        public CrapLogger(string name)
        {
            Name = name;
        }
        private void SetTitle(string format, object[] args)
        {
            Console.Title = $"{Name} - {string.Format(format, args)}";
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine("[{0}] >>> {1} <<<", Name, string.Format(format, args));
            SetTitle(format, args);
        }
        public void Log(string format, params object[] args)
        {
            Console.WriteLine("[{0}] [#] {1}", Name, string.Format(format, args));
            SetTitle(format, args);
        }
        public void Info(string format, params object[] args)
        {
            Console.WriteLine("[{0}] [!] {1}", Name, string.Format(format, args));
            SetTitle(format, args);
        }
        public void Warn(string format, params object[] args)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[{0}] [!> {1} <!]", Name, string.Format(format, args));
            SetTitle(format, args);
            Console.ForegroundColor = col;
        }
    }
}
