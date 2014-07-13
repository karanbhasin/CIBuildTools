using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace DeploySQLDAC {
    public class DACDeployer {
        private string _targetServer = null;
        public String TargetServer {
            get {
                if (String.IsNullOrEmpty(_targetServer))
                {
                    _targetServer = System.Configuration.ConfigurationManager.ConnectionStrings["TargetServer"].ConnectionString;
                }
                return _targetServer;
            }
            set
            {
                _targetServer = value;
            }
        }

        private string _sourceServer = null;
        public String SourceServer
        {
            get
            {
                if (String.IsNullOrEmpty(_sourceServer))
                {
                    _sourceServer = System.Configuration.ConfigurationManager.ConnectionStrings["SourceServer"].ConnectionString;
                }
                return _sourceServer;
            }
            set
            {
                _sourceServer = value;
            }
        }

        private string _filePath = null;
        public String FilePath
        {
            get
            {
                if (String.IsNullOrEmpty(_filePath))
                {
                    _filePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"];
                }
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }


        private string _targetDB = null;
        public String TargetDB
        {
            get
            {
                if (String.IsNullOrEmpty(_targetDB))
                {
                    _targetDB = System.Configuration.ConfigurationManager.AppSettings["TargetDB"];
                }
                return _targetDB;
            }
            set
            {
                _targetDB = value;
            }
        }

        private string _sourceDB = null;
        public String SourceDB
        {
            get
            {
                if (String.IsNullOrEmpty(_sourceDB))
                {
                    _sourceDB = System.Configuration.ConfigurationManager.AppSettings["SourceDB"];
                }
                return _sourceDB;
            }
            set
            {
                _sourceDB = value;
            }
        }

        private bool _isOperationSet = false;
        public OperationType _operationType = OperationType.GenerateDACPAC;
        public OperationType OperationType
        {
            get
            {
                if(!_isOperationSet){
                    // try to parse from the appsettings
                    if (!Enum.TryParse<OperationType>(System.Configuration.ConfigurationManager.AppSettings["OperationType"], out _operationType))
                    {
                        _operationType = OperationType.GenerateDACPAC;
                    }
                    _isOperationSet = true;
                }
                return _operationType;
            }
            set
            {
                 _isOperationSet = true;
                _operationType = value;
            }
        }

        // source server
        // destination server
        // source db
        // destination db
        // filename

        public void Start()
        {
            DacServices svc = new DacServices(SourceServer);
            String filePath = getDACFilePath();
            switch (OperationType)
            {
                case DeploySQLDAC.OperationType.GenerateDACPAC:
                    Extract(GetService(SourceServer), SourceDB, filePath);
                    break;
                case DeploySQLDAC.OperationType.DeployDACPAC:
                    Extract(GetService(SourceServer), SourceDB, filePath);
                    Deploy(GetService(TargetServer), TargetDB, filePath);
                    break;
                case DeploySQLDAC.OperationType.GenerateBACPAC:
                    Export(GetService(SourceServer), SourceDB, filePath);
                    break;
                case DeploySQLDAC.OperationType.DeployBACPAC:
                    Export(GetService(SourceServer), SourceDB, filePath);
                    Import(GetService(TargetServer), TargetDB, filePath);
                    break;
                case DeploySQLDAC.OperationType.DeployDACFromFile:
                    Deploy(GetService(TargetServer), TargetDB, this.FilePath);
                    break;
                case DeploySQLDAC.OperationType.DeployBACFromFile:
                    Import(GetService(TargetServer), TargetDB, this.FilePath);
                    break;
            }
        }
        
        private string getDACFilePath()
        {
            string filePath = FilePath;
            string extension = "bacpac";
            if (OperationType == DeploySQLDAC.OperationType.DeployDACPAC || OperationType == DeploySQLDAC.OperationType.GenerateDACPAC)
            {
                extension = "dacpac";
            }
            string formatString = "{0}\\{1}_To_{2}_at_{3}.{4}.{5}.{6}.{7}";
            if (OperationType == DeploySQLDAC.OperationType.GenerateBACPAC || OperationType == DeploySQLDAC.OperationType.GenerateDACPAC)
            {
                formatString = "{0}\\{1}_at_{3}.{4}.{5}.{6}.{4}";
            }
            if(filePath.EndsWith("\\")){
                filePath = filePath.Substring(0, filePath.Length - 1);
            }
            DateTime now = DateTime.Now;
            return string.Format(formatString, filePath, SourceDB, TargetDB, now.Month, now.Day, now.Year, now.Millisecond, extension);
            //return string.Format("{0}\\{1}.{4}", filePath, SourceDB, TargetDB, DateTime.Now.ToString("o"), extension);
        }

        private DacServices GetService(string server)
        {
            DacServices svc = new DacServices(server);
            svc.Message += new EventHandler<DacMessageEventArgs>(receiveDacServiceMessageEvent);
            svc.ProgressChanged += new EventHandler<DacProgressEventArgs>(receiveDacServiceProgessEvent);
            return svc;
        }

        private void Extract(DacServices svc, string SourceDatabaseName, string Path)
        {
            Console.WriteLine("\n\rPerforming Extract of {0} to {1} at {2}", SourceDatabaseName, Path, System.DateTime.Now.ToLongTimeString());

            DacExtractOptions dacExtractOptions = new DacExtractOptions
            {
                ExtractApplicationScopedObjectsOnly = true,
                ExtractReferencedServerScopedElements = false,
                VerifyExtraction = true,
                Storage = DacSchemaModelStorageType.Memory
            };

            svc.Extract(Path, SourceDatabaseName, "Sample DACPAC", new Version(1, 0, 0), "Sample Extract", null, dacExtractOptions);
        }

        private void Deploy(DacServices svc, string TargetDatabaseName, string Path)
        {
            Console.WriteLine("\n\rPerforming Deploy of {0} to {1} at {2}", Path, TargetDatabaseName, System.DateTime.Now.ToLongTimeString());

            using (DacPackage dacpac = DacPackage.Load(Path))
            {
                //svc.Deploy(dacpac, TargetDatabaseName);
                svc.Deploy(dacpac, TargetDatabaseName, true);
            }
        }

        private void Export(DacServices svc, string SourceDatabaseName, string Path)
        {
            Console.WriteLine("\n\rPerforming Export of {0} to {1} at {2}", SourceDatabaseName, Path, System.DateTime.Now.ToLongTimeString());

            svc.ExportBacpac(Path, SourceDatabaseName);
        }

        private void Import(DacServices svc, string TargetDatabaseName, string Path)
        {
            Console.WriteLine("\n\rPerforming Import of {0} to {1} at {2}", Path, TargetDatabaseName, System.DateTime.Now.ToLongTimeString());

            using (BacPackage bacpac = BacPackage.Load(Path))
            {
                svc.ImportBacpac(bacpac, TargetDatabaseName);
            }
        }
 
        private void receiveDacServiceMessageEvent(object sender, DacMessageEventArgs e)
        {
            Console.WriteLine(string.Format("Message Type:{0} Prefix:{1} Number:{2} Message:{3}", e.Message.MessageType, e.Message.Prefix, e.Message.Number, e.Message.Message));
        }

        private void receiveDacServiceProgessEvent(object sender, DacProgressEventArgs e)
        {
            Console.WriteLine(string.Format("Progress Event:{0} Progrss Status:{1}", e.Message, e.Status));
        }
    }
}
