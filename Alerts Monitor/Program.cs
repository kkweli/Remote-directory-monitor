using System;
using System.IO;
using System.Net;
using System.Speech.Synthesis;
using System.Net.Mail;
using System.Configuration;
using System.Timers;
using System.Runtime;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace DirListing
{

    class Program
    {
        private string DomainName = "domain-name";
        private string UserName = "user";
        private string PassWord = "password";
        private string SharedFolder = @"network path i.e \\network\folder";
        private int FileCount = @"network path i.e \\network\folder".Length;
        private string ClientAddress = Environment.MachineName;
        private string HostName = "DR_SBK_22D";
        private PerformanceCounter theCPUCounter = new PerformanceCounter(
                "Process", "% Processor Time",   Process.GetCurrentProcess().ProcessName);
        private PerformanceCounter theMemCounter = new PerformanceCounter(
            "Process", "Working Set",Process.GetCurrentProcess().ProcessName);


        static void Main(string[] args)
        {
            //initialize attributes class
            Program Att = new Program();
            Process MyProcess = Process.GetCurrentProcess();

            if (Att.HostName != Att.ClientAddress)
            {
                Console.Title = "Alerts Monitoring Tool - Client " + Att.ClientAddress;
            }
            else
            {
                Console.Title = "Alerts Monitoring Tool - Host " + Att.ClientAddress;
            }
            Console.SetWindowSize(100, 50);
            Console.BufferHeight = 20000;
            Console.BufferWidth = 100;
            Console.CursorVisible = false;
            

            Timer myTimer = new Timer(3 * 60 * 1000);
                myTimer.Elapsed += new ElapsedEventHandler(myTimer_Elapsed);
                myTimer.AutoReset = true;
                myTimer.Start();

              //monitor process perfomance




            var crednzial = new NetworkCredential(Att.UserName, Att.PassWord, Att.DomainName);
            try
            {
                using (new NetworkConnection(Att.SharedFolder, crednzial))
                {
                  
                    while (true)
                    {
                        var StatCpu = Convert.ToInt64(Att.theCPUCounter.NextValue()) + "%";
                        var AppMem = Convert.ToInt64(MyProcess.PrivateMemorySize64 / 1024 / 1024) +"MB";
                        var StatMem = Convert.ToInt64(Att.theMemCounter.NextValue() / 1024 / 1024) + "MB";
                        var AppName = MyProcess;
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("\n{0}\nCPU utilization {1}" +
                            "\nMemory utilization {2}\nWorking set Memory utilization {3}" +
                            "\nBuffer Height (rows) {4}\nBuffer Width (columns) {5}\n\n",
                            AppName, StatCpu, StatMem, AppMem, Console.BufferHeight, Console.BufferWidth);
                        //header
                        Console.SetCursorPosition(0, 7);
                        Console.WriteLine("\n" + "\tMonitoring : {0}", Att.SharedFolder);
                        ListFilesInDirectory(Att.SharedFolder);
                        System.Threading.Thread.Sleep(2000);

                        //manage memory by invoking garbage collection
                        GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                        GC.Collect();
                       
                    }                    
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Connection failure to remote resource.\n\n{0}",e.Source);
                //Environment.Exit(0);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("\n\n\tExperiencing Error Network Connection\n\n\tSource : {0}", e.Message);

            }
            
        }

        static void myTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //initialize attributes class
            Program Att = new Program();
            int FileCount = 0;
            //Console.WriteLine("value conn {0}", a);

            try
            {
                FileCount = Directory.GetFiles(@"network path i.e \\network\folder").Length;
            }
            catch (IOException c)
            {
                Console.WriteLine("\n\n\tExperiencing Error Network Connection\n\n\tSource : {0}", c.Message);
                System.Threading.Thread.Sleep(10000);
            }
            catch (UnauthorizedAccessException c)
            {
                Console.WriteLine("\n\n\tExperiencing Error Network Connection\n\n\tSource : {0}", c.Message);

            }

            string to = "send receipeints";
            string subject = "Files Alerts Queue ("+ FileCount + ")";
            string body = "Alerts Queue Threshold violations.\n" +
                "Items in queue in excess of " + FileCount + "\n"
                + "\nNo-Reply.\nAMTool.";
            if (FileCount > 50)
                if (Att.ClientAddress == Att.HostName)
            {

                    using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["FromEmail"], to))
                    {
                        mm.Subject = subject;
                        mm.Body = body;
                        mm.IsBodyHtml = false;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = ConfigurationManager.AppSettings["Host"];
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
                        smtp.UseDefaultCredentials = false;
                        smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        Console.WriteLine("\tSending Email......");
                        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate,
                     X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
                        try
                        {
                            smtp.Send(mm);
                            Console.WriteLine("\tEmail Sent OK.");
                            smtp.Dispose();
                            Console.WriteLine("\tEmail connection closed.");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\tEmail not sent cause : {0}.", ex);
                            var erroLog = @"..\..\smsAlert\\Logs\\smtp_error.txt";
                             using (StreamWriter writer = new StreamWriter(erroLog))
                             {
                                 writer.WriteLine(DateTime.Now.ToString() + ex +"\n");
                             }
                            smtp.Dispose();
                            Console.WriteLine("\tEmail connection closed.");
                        }
                    }
                }
        }

        static void ListFilesInDirectory(string workingDirectory)
        {
            //initialize attributes
            Program Att = new Program();
            int FileCount = 0;
            //Console.WriteLine("value conn {0}", a);

            try
            {
                FileCount = Directory.GetFiles(@"network path i.e \\network\folder").Length;
            }
            catch (IOException e)
            {
                Console.WriteLine("\n\n\tExperiencing Error Network Connection\n\n\tSource : {0}", e.Message);
                System.Threading.Thread.Sleep(10000);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("\n\n\tExperiencing Error Network Connection\n\n\tSource : {0}", e.Message);

            }

            //create array of files found in directory
            string[] filePaths = Directory.GetFiles(workingDirectory);

            //counter
            int coInc = 9;
            int fileCnt = 0;
            //loop through array
           
                foreach (string filePath in filePaths)
                {
                try
                {
                    coInc += 1;
                    fileCnt += 1;
                    Console.SetCursorPosition(0, coInc);
                    Console.WriteLine("\t" + fileCnt + " : " + Path.GetFileName(filePath));
                    //Console.WriteLine("\t"+filePath+" line - "+coInc);
                }
                catch(ArgumentOutOfRangeException)
                {

                }

                }

            try
            {
                int conCom = coInc + 2;
                Console.SetCursorPosition(0, conCom);

                //if (FileCount > coInc)
                //Console.WriteLine("\tFile listing truncated {0} items, console maximum height buffer.", fileCnt - 1);
                Console.WriteLine("\tCurrent file(s) counter - {0}", FileCount);
                //Console.WriteLine("\n" + "\tCurrent file(s) counter - {0} line - {1}\n", FileCount,conCom);
            }
            catch(ArgumentOutOfRangeException)
            {

            }

            // Initialize a new instance of the SpeechSynthesizer.
            SpeechSynthesizer synth = new SpeechSynthesizer();

            // Configure the audio output. 
            synth.SetOutputToDefaultAudioDevice();
            synth.Rate = -2;
            synth.Volume = 100;

            if (FileCount > 50)
                if (FileCount < 70)
            {
                    //raise alarm
                    Console.WriteLine("\tWarning, files queue escalating @ current {0} files.", FileCount);
                    //say something.
                    synth.Speak("Warning! Alerts in excess of " + FileCount + " files.");

                } else
                {
                    int beepFrequency = 623;
                    int beepDuration = 2000;
                   
                    //raise alarm
                    Console.WriteLine("\tAlert!! files queue increased to {0}", FileCount);                    
                    Console.Beep(beepFrequency, beepDuration);

                }
        }
    }
}
