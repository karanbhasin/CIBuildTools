using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeploySQLDAC
{
    public enum OperationType
    {
        /// <summary>
        /// Generate only the .dacpac file, but dont deploy it to the destination server
        /// </summary>
        GenerateDACPAC = 1,
        /// <summary>
        /// Generate the .dacpac file, and deploy it to the destination server
        /// </summary>
        DeployDACPAC,
        /// <summary>
        /// Generate only the .bacpac file, but dont deploy it to the destination server
        /// </summary>
        GenerateBACPAC,
        /// <summary>
        /// Generate the .bacpac file, and deploy it to the destination server
        /// </summary>
        DeployBACPAC,
        /// <summary>
        /// Deploy a .dacpac file to the destination server. The .dacpac file should already exist
        /// </summary>
        DeployDACFromFile,
        /// <summary>
        /// Deploy a .bacpac file to the destination server. The .bacpac file should already exist
        /// </summary>
        DeployBACFromFile
    }
}
