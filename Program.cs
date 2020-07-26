using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UE4_Shader_Booster
{
    class Program
    {
        static void Main(string[] args)
        {
            var time = 7000; // Time in ms to monitor and update UE4 shader processes
            var index = 2; // starting index
            var priorities = new List<ProcessPriorityClass> // Placing in sensible order
            {
                ProcessPriorityClass.Idle,
                ProcessPriorityClass.BelowNormal,
                ProcessPriorityClass.Normal,
                ProcessPriorityClass.AboveNormal,
                ProcessPriorityClass.High,
                ProcessPriorityClass.RealTime
            };

            var compiliers = new List<Process>();

            Console.WindowWidth = 50;
            Console.WriteLine($"─────────────────────────────────────────────────");
            Console.WriteLine($" Monitoring UE4 ShaderCompileWorkers Processes");
            Console.WriteLine();
            Console.WriteLine($" WARNING: High priorities can cause instability");
            Console.WriteLine($" Press + and - to change priority, ESC to exit");
            Console.WriteLine($"─────────────────────────────────────────────────");

            updateTitle();

            void WriteLine(string Message, ConsoleColor Color, int CursorLeft = 1)
            {
                Console.ForegroundColor = Color;
                Console.CursorLeft = CursorLeft;
                Console.WriteLine($"{DateTime.Now:hh:mm:ss} - {Message}");
            }

            #region Updates

            void updateTitle() => Console.Title = $"UE4 Shader Booster - {priorities[index]}";

            void update(Process p)
            {
                p.PriorityBoostEnabled = true;
                p.PriorityClass = priorities[index];
                WriteLine($"ID: {p.Id} - {p.PriorityClass}", ConsoleColor.White);
                Console.ForegroundColor = ConsoleColor.White;
            }

            #endregion

            #region Monitor Processes

            var t = new System.Timers.Timer(time);

            t.Elapsed += (s0, e0) =>
            {
                var processes = Process.GetProcessesByName("ShaderCompileWorker");
                foreach (var p in processes)
                {
                    if (compiliers.Where(x => x.Id == p.Id).FirstOrDefault() == null)
                    {
                        update(p);
                        compiliers.Add(p);

                        p.EnableRaisingEvents = true;

                        p.Exited += (s, e) =>
                        {
                            WriteLine($"ID: {p.Id} - Closed", ConsoleColor.DarkGray);
                            compiliers.Remove(p);
                        };
                    }
                }
            };

            t.AutoReset = true;
            t.Start();

            #endregion

            #region User Input

            void WaitInput()
            {
                var oldindex = index;
                var key = Console.ReadKey(true).Key;

                // Exit Program
                if (key == ConsoleKey.Escape)
                    return;

                // Increase priority
                if ((key == ConsoleKey.OemPlus || key == ConsoleKey.Add)
                    && index < priorities.Count - 1)
                    index++;

                // Decrease priority
                if ((key == ConsoleKey.OemMinus || key == ConsoleKey.Subtract)
                    && index > 0)
                    index--;

                // User feedback
                if (oldindex != index)
                {
                    WriteLine($"Priority changed to {priorities[index]}", ConsoleColor.Blue);
                    updateTitle();
                    foreach (var c in compiliers)
                        update(c);
                }

                // Re-wait for user input
                WaitInput();
            }

            #endregion

            WaitInput();
        }
    }
}