using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BulletPlayerBackend.Utils
{
    public class EngineHandler
    {
        public bool IsRunning { get; set; }

        public Process TurnEngineOn()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.ErrorDialog = false;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\engine.exe";

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            IsRunning = true;
            return process;
        }

        public void KillEngineProcess(Process process)
        {  
            process.Close();
            IsRunning = false;
        }

        public string GetCalculateMove(Process process, List<string> resolvedMoveList)
        {
            var moveTime = 200;
            var moves = String.Empty;
            if (resolvedMoveList != null)
                foreach (var variable in resolvedMoveList)
                    moves = moves + variable;

            if (resolvedMoveList.Count > 24)
                moveTime = 500;
            if (resolvedMoveList.Count > 60)
                moveTime = 150;

            if (moves != "")
                process.StandardInput.WriteLine("position startpos moves " + moves);
            else
                process.StandardInput.WriteLine("position startpos");
            process.StandardInput.WriteLine("go movetime " + moveTime);
            System.Threading.Thread.Sleep(moveTime + 10);

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
