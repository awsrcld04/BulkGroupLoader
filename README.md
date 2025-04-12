
# BulkGroupLoader

DESCRIPTION: 
- Loads users into a group from a specified file.

> NOTES: "v1.0" was completed in 2011. 

## Requirements:

Operating System Requirements:
- Windows Server 2003 or higher (32-bit)
- Windows Server 2008 or higher (32-bit)

Additional software requirements:
Microsoft .NET Framework v3.5


## Operation and Configuration:

Command-line parameters:
- run (Required parameter)
- group (specify the group to load the accounts into)
- file (specify the file containing the user account list)

Example:
BulkGroupLoader -run -group:Group1 -file:Group1Members.txt
