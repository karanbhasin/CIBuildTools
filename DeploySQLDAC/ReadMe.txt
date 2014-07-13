case DeploySQLDAC.OperationType.GenerateDACPAC(/o:1), GenerateBACPAC(/o:3) (/f should be the name of the correct .bacpac/ .dacpac file)
deploysqldac.exe /s: /sd: /f: /o:

case DeploySQLDAC.OperationType.DeployDACPAC(/o:2), DeployBACPAC(/o:4) (/f should be the name of the correct .bacpac/ .dacpac file)
deploysqldac.exe /s: /sd: /t: /d: /f: /o:

DeploySQLDAC.OperationType.DeployDACFromFile(/o:5): DeployBACFromFile(/o:6):
deploysqldac.exe /t: /d: /f: /o:

Possible warnings/exceptions
----------------------------
You may get an version incompatibility exception. Make sure that .dacpac/ .back file you are deploying was created using the same version of SQL server 
as the target machine