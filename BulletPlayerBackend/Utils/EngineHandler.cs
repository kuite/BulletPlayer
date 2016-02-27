using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace BulletPlayerBackend.Utils
{
    public class EngineHandler
    {
        public bool IsRunning { get; set; }
        public Process Process { get; set; }

        public Process TurnEngineOn()
        {
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                FileName = AppDomain.CurrentDomain.BaseDirectory + "\\engine.exe"
            };

            Process = new Process();
            Process.StartInfo = startInfo;
            Process.Start();
            IsRunning = true;
            return Process;
        }

        public void TurnEngineOff()
        {  
            Process.Close();
            IsRunning = false;
        }

        public string GetCalculateMove(Process process, List<string> resolvedMoveList)
        {
            var moveTime = 100;
            var moves = String.Empty;
            if (resolvedMoveList != null) 
            {
                moves = resolvedMoveList.Aggregate(moves, (current, variable) => current + variable);
            }

            if (resolvedMoveList.Count > 16)
                moveTime = 1000;
            if (resolvedMoveList.Count > 60)
                moveTime = 100;

            if (moves != "")
                process.StandardInput.WriteLine("position startpos moves " + moves);
            else
                process.StandardInput.WriteLine("position startpos");
            process.StandardInput.WriteLine("go movetime " + moveTime);
            Thread.Sleep(moveTime + 10); //TODO: probably unnecessary code

            string lastLine = null;
            while (!process.StandardOutput.EndOfStream)
            {
                lastLine = process.StandardOutput.ReadLine();
                if (lastLine.Contains("best"))
                    break;
            }
            var splittedLine = Regex.Split(lastLine, " ");
            return splittedLine[1];
        }
    }
}
