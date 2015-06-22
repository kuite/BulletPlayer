﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            var moves = String.Empty;
            if (resolvedMoveList != null)
                foreach (var variable in resolvedMoveList)
                    moves = moves + variable;  

            if (moves != "")
                process.StandardInput.WriteLine("position startpos moves " + moves);
            else
                process.StandardInput.WriteLine("position startpos");
            process.StandardInput.WriteLine("go movetime 50");
            System.Threading.Thread.Sleep(100);

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
