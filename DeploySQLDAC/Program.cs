using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dac;

namespace DeploySQLDAC {
    class Program {
        static void Main(string[] args) {
            DateTime startTime = System.DateTime.Now;
            Console.WriteLine("\n\rStarted at {0}", startTime.ToLongTimeString());

            try
            {
                // /sd:PepperDB /t:"Server=tcp:qnu0gm0tor.database.windows.net,1433;Database=PepperDB;User ID=dbuser@qnu0gm0tor;Password=Passw0rd!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;" /f:"C:\Articles\Jenkins"
                //  /t  : target server
                //  /s  : source server
                //  /f  : file path to generate the .dacpac/ .bacpac file in
                //  /d  : target database
                //  /sd : source database
                //  /o  : OperationType default to 1
                var parsedArgs = args.Where(x => x.StartsWith("/"))
                    .ToDictionary(s => s.Substring(0, s.IndexOf(":")), s => s.Substring(s.IndexOf(":") + 1));
                DACDeployer deployer = new DACDeployer();
                if (parsedArgs.ContainsKey("/t"))
                {
                    deployer.TargetServer = parsedArgs["/t"];
                }
                if (parsedArgs.ContainsKey("/d"))
                {
                    deployer.TargetDB = parsedArgs["/d"];
                }

                if (parsedArgs.ContainsKey("/s"))
                {
                    deployer.SourceServer = parsedArgs["/s"];
                }
                if (parsedArgs.ContainsKey("/sd"))
                {
                    deployer.SourceDB = parsedArgs["/sd"];
                }

                if (parsedArgs.ContainsKey("/o"))
                {
                    OperationType defaultVal = OperationType.GenerateDACPAC;
                    if(!Enum.TryParse<OperationType>(parsedArgs["/o"], out defaultVal)){
                        Console.WriteLine("Invalid value for switch :o");
                    } else {
                        deployer.OperationType = defaultVal;
                    }
                }

                if (parsedArgs.ContainsKey("/f"))
                {
                    deployer.FilePath = parsedArgs["/f"];
                }

                deployer.Start();
            }
                catch (DacServicesException e)
            {
                Console.WriteLine("Error Encountered:{0} Inner Exception: {1}", e.Messages, e.InnerException);
            }
            
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            finally
            {
                Console.WriteLine("Completed at {0} in {1} minutes {2} seconds {3} milliseconds", System.DateTime.Now.ToLongTimeString(), System.DateTime.Now.Subtract(startTime).Minutes, System.DateTime.Now.Subtract(startTime).Seconds, System.DateTime.Now.Subtract(startTime).Milliseconds);
                Console.WriteLine("\n\rStrike key to end execution");
                Console.ReadKey();
            }
        }
    }
}
