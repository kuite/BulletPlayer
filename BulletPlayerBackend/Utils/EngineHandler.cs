using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace BulletPlayerBackend.Utils
{
    public class EngineHandler
    {
        public bool IsRunning { get; set; }

        public EngineHandler()
        {
            
        }

        public Process TurnEngineOn()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.ErrorDialog = false;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = "C:\\.net projects\\BulletPlayer\\Data\\engine.exe";

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
            var moves = "";
            foreach (var variable in resolvedMoveList)
                moves = moves + variable;  

            process.StandardInput.WriteLine("position startpos moves " + moves);
            process.StandardInput.WriteLine("go");
            System.Threading.Thread.Sleep(2000);
            process.StandardInput.WriteLine("stop");

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
