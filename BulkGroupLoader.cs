//Description:
//Loads users into a group from a specified file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.Win32;
using System.Reflection;
using SystemsAdminPro.Utility;


namespace BulkGroupLoader
{
    class BGLMain
    {
        struct CMDArguments
        {
            public string strInputFileName;
            public string strGroupName;
            public bool bParseCmdArguments;
        }

        struct NameElements
        {
            public string strnameelement1;
            public string strnameelement2;
            public string strsinglenameelement;
        }

        // find group and retrieve groupprincipal; if found, continue
        // find user; if found, add to the specified group

        static void funcPrintParameterSyntax()
        {
            Console.WriteLine("BulkGroupLoader v1.0 (c) 2011 SystemsAdminPro.com");
            Console.WriteLine();
            Console.WriteLine("Description: Load users into a group using the user account's name");
            Console.WriteLine();
            Console.WriteLine("Parameter syntax:");
            Console.WriteLine();
            Console.WriteLine("Use the following required parameters in the following order:");
            Console.WriteLine("-run                 required parameter");
            Console.WriteLine("-group:              to specify the group to load");
            Console.WriteLine("-file:               to specify the file containing user list");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("BulkGroupLoader -run -group:Group1 -file:Group1Members.txt");
        }

        static GroupPrincipal funcGetGroup(PrincipalContext ctx, string strGroupName)
        {
            try
            {
                GroupPrincipal newGroupPrincipal = GroupPrincipal.FindByIdentity(ctx, IdentityType.Name, strGroupName);
                // or use PrincipalSearcher

                // [DebugLine] Console.WriteLine(newGroupPrincipal.Name);

                return newGroupPrincipal;
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return null;
            }
        }

        static UserPrincipal funcGetUser(PrincipalContext ctx, string strUserName)
        {
            try
            {
                NameElements objNameElements;

                // [DebugLine] Console.WriteLine("inside GetUser: {0}", strUserName);

                objNameElements = funcParseUserName(strUserName);

                // [DebugLine] Console.WriteLine("inside GetUser: {0}", objNameElements.strsinglenameelement);

                // [DebugLine] if (objNameElements.strnameelement1 != "")
                // [DebugLine] {
                // [DebugLine]     Console.WriteLine("nameelement1: {0}", objNameElements.strnameelement1);
                // [DebugLine] }

                UserPrincipal newUserPrincipal = new UserPrincipal(ctx);

                PrincipalSearcher ps = new PrincipalSearcher(newUserPrincipal);

                // Create an in-memory user object to use as the query example.
                UserPrincipal u = new UserPrincipal(ctx);

                // Set properties on the user principal object.
                //u.GivenName = "Jim";
                //u.Surname = "Daly";
                u.Name = objNameElements.strsinglenameelement;

                // Tell the PrincipalSearcher what to search for.
                ps.QueryFilter = u;

                // Run the query. The query locates users 
                // that match the supplied user principal object. 
                Principal newPrincipal = ps.FindOne();

                if (newPrincipal != null)
                {
                    // [DebugLine] Console.WriteLine("newPrincipal name: {0}", newPrincipal.Name);
                    newUserPrincipal = UserPrincipal.FindByIdentity(ctx, IdentityType.Name, newPrincipal.Name);
                    return newUserPrincipal;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return null;
            }
        }

        static NameElements funcParseUserName(string strNameToParse)
        {
            NameElements newNameElements;

            newNameElements.strnameelement1 = "";
            newNameElements.strnameelement2 = "";
            newNameElements.strsinglenameelement = "";

            try
            {
                if (strNameToParse.Contains(" "))
                {
                    string[] arrElements = strNameToParse.Split(' ');
                    if (arrElements != null)
                    {
                        newNameElements.strnameelement1 = arrElements[0];
                        newNameElements.strnameelement2 = arrElements[1];
                    }
                }

                if (strNameToParse.Contains(","))
                {
                    string[] arrElements = strNameToParse.Split(',');
                    if (arrElements != null)
                    {
                        newNameElements.strnameelement1 = arrElements[0];
                        newNameElements.strnameelement2 = arrElements[1];
                    }
                }

                if (newNameElements.strnameelement1 == "" & newNameElements.strnameelement2 == "")
                {
                    newNameElements.strsinglenameelement = strNameToParse;
                    // [DebugLine] Console.WriteLine("inside funcParseUSerName: {0}",newNameElements.strsinglenameelement);
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

            return newNameElements;
        }

        static bool funcAddUserToGroup(GroupPrincipal groupprincipal1, UserPrincipal userprincipal1)
        {
            try
            {
                if (!userprincipal1.IsMemberOf(groupprincipal1))
                {
                    groupprincipal1.Members.Add(userprincipal1);
                    groupprincipal1.Save();
                    return true;
                }
                else
                {
                    Construct.WriteToOutputLogFile(userprincipal1.Name + " is already a member of group " + groupprincipal1.Name);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return false;
            }
        }

        static CMDArguments funcParseCmdArguments(string[] cmdargs)
        {
            CMDArguments objCMDArguments = new CMDArguments();

            try
            {
                bool bCmdArg1Complete = false;

                if (cmdargs[0] == "-run" & cmdargs.Length > 1)
                {
                    if (cmdargs[1].Contains("-group:"))
                    {
                        // [DebugLine] Console.WriteLine(cmdargs[1].Substring());
                        objCMDArguments.strGroupName = cmdargs[1].Substring(7);
                        bCmdArg1Complete = true;

                        if (bCmdArg1Complete & cmdargs.Length > 2)
                        {
                            if (cmdargs[2].Contains("-file:"))
                            {
                                // [DebugLine] Console.WriteLine(cmdargs[2].Substring());
                                objCMDArguments.strInputFileName = cmdargs[2].Substring(6);
                                objCMDArguments.bParseCmdArguments = true;
                            }
                        }
                    }
                }
                else
                {
                    objCMDArguments.bParseCmdArguments = false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

            return objCMDArguments;
        }

        static void funcProgramExecution(CMDArguments objCMDArguments)
        {
            // if bEventLogStartStop, log start event
            try
            {
                Construct.ProgramRegistryTag(Construct.strProgramName);

                if (Construct.CheckForFile(objCMDArguments.strInputFileName))
                {
                    PrincipalContext ctxDomain = Construct.CreateDomainPrincipalContext();

                    GroupPrincipal objGroupPrincipal = funcGetGroup(ctxDomain, objCMDArguments.strGroupName);

                    if (objGroupPrincipal != null)
                    {
                        TextReader trInputFile = new StreamReader(objCMDArguments.strInputFileName);
                        Construct.OpenOutputLog(Construct.strProgramName);
                        Construct.WriteToOutputLogFile("--------BulkGroupLoader started");

                        string strOutputMsg = "";

                        using (trInputFile)
                        {
                            string strNewLine = "";

                            while ((strNewLine = trInputFile.ReadLine()) != null)
                            {

                                UserPrincipal objUserPrincipal = funcGetUser(ctxDomain, strNewLine);

                                if (objUserPrincipal != null)
                                {
                                    if (funcAddUserToGroup(objGroupPrincipal, objUserPrincipal))
                                    {
                                        strOutputMsg = "User " + strNewLine + " was added to " + objGroupPrincipal.Name;
                                        Construct.WriteToOutputLogFile(strOutputMsg);
                                    }
                                    else
                                    {
                                        strOutputMsg = "User " + strNewLine + " was NOT added to " + objGroupPrincipal.Name;
                                        Construct.WriteToOutputLogFile(strOutputMsg);
                                    }
                                }
                                else
                                {
                                    strOutputMsg = "User " + strNewLine + " was not found";
                                    Construct.WriteToOutputLogFile(strOutputMsg);
                                }
                            }
                        }

                        trInputFile.Close();

                        Construct.WriteToOutputLogFile("--------BulkGroupLoader stopped");

                        Construct.CloseOutputLogFile();
                    }
                    else
                    {
                        Console.WriteLine("Group could not be found. Check parameters.");
                        Construct.WriteToErrorLogFile("Group could not be found. Check parameters.");
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

            // if bEventLogStartStop, log stop event
        }

        static void funcGetFuncCatchCode(string strFunctionName, Exception currentex)
        {
            string strCatchCode = "";

            Dictionary<string, string> dCatchTable = new Dictionary<string, string>();
            dCatchTable.Add("funcGetFuncCatchCode", "p:f0");
            dCatchTable.Add("funcPrintParameterSyntax", "f1");
            dCatchTable.Add("funcParseCmdArguments", "p:f2");
            dCatchTable.Add("funcProgramExecution", "p:f3");
            dCatchTable.Add("funcGetGroup", "p:f4");
            dCatchTable.Add("funcGetUser", "p:f5");
            dCatchTable.Add("funcParseUserName", "p:f6");
            dCatchTable.Add("funcAddUserToGroup", "p:f7");

            if (dCatchTable.ContainsKey(strFunctionName))
            {
                strCatchCode = "err" + dCatchTable[strFunctionName] + ": ";
            }

            //[DebugLine] Console.WriteLine(strCatchCode + currentex.GetType().ToString());
            //[DebugLine] Console.WriteLine(strCatchCode + currentex.Message);

            Construct.WriteToErrorLogFile(strCatchCode + currentex.GetType().ToString());
            Construct.WriteToErrorLogFile(strCatchCode + currentex.Message);
        }

        static void Main(string[] args)
        {
            try
            {
                Construct.strProgramName = "BulkGroupLoader";
              
                if (Construct.LicenseActivation())
                {
                    if (args.Length == 0)
                    {
                        Construct.PrintParameterWarning(Construct.strProgramName);
                    }
                    else
                    {
                        if (args[0] == "-?")
                        {
                            funcPrintParameterSyntax();
                        }
                        else
                        {
                            string[] arrArgs = args;
                            CMDArguments objArgumentsProcessed = funcParseCmdArguments(arrArgs);

                            if (objArgumentsProcessed.bParseCmdArguments)
                            {
                                funcProgramExecution(objArgumentsProcessed);
                            }
                            else
                            {
                                Construct.PrintParameterWarning(Construct.strProgramName);
                            } // check objArgumentsProcessed.bParseCmdArguments
                        } // check args[0] = "-?"
                    } // check args.Length == 0
                } // funcLicenseCheck()
            }
            catch (Exception ex)
            {
                Console.WriteLine("errm0: {0}", ex.Message);
            }
        }
    }
}
