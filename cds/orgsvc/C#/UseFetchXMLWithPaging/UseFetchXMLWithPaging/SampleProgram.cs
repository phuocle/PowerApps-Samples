﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerApps.Samples
{
     public partial  class SampleProgram
    {
        [STAThread] // Added to support UX
        static void Main(string[] args)
        {

            CrmServiceClient service = null;
            try
            {
                service = SampleHelpers.Connect("Connect");
                if (service.IsReady)
                {
                    #region Sample Code
                    //////////////////////////////////////////////
                    #region Set up
                    SetUpSample(service);
                    #endregion Set up
                    #region Demonstrate

                    // Define the fetch attributes.
                    // Set the number of records per page to retrieve.
                    int fetchCount = 3;
                    // Initialize the page number.
                    int pageNumber = 1;
                    // Initialize the number of records.
                    int recordCount = 0;
                    // Specify the current paging cookie. For retrieving the first page, 
                    // pagingCookie should be null.
                    string pagingCookie = null;

                    // Create the FetchXml string for retrieving all child accounts to a parent account.
                    // This fetch query is using 1 placeholder to specify the parent account id 
                    // for filtering out required accounts. Filter query is optional.
                    // Fetch query also includes optional order criteria that, in this case, is used 
                    // to order the results in ascending order on the name data column.
                    string fetchXml = string.Format(@"<fetch version='1.0' 
                                                    mapping='logical' 
                                                    output-format='xml-platform'>
                                                    <entity name='account'>
                                                        <attribute name='name' />
                                                        <attribute name='emailaddress1' />
                                                        <order attribute='name' descending='false'/>
                                                        <filter type='and'>
                                                            <condition attribute='parentaccountid' 
                                                                operator='eq' value='{0}' uiname='' uitype='' />
                                                        </filter>
                                                    </entity>
                                                </fetch>",
                                                    parentAccountId);

                    Console.WriteLine("Retrieving data in pages\n");
                    Console.WriteLine("#\tAccount Name\t\t\tEmail Address");

                    while (true)
                    {
                        // Build fetchXml string with the placeholders.
                        string xml = CreateXml(service, fetchXml, pagingCookie, pageNumber, fetchCount);

                        // Excute the fetch query and get the xml result.
                        var fetchRequest1 = new RetrieveMultipleRequest
                        {
                            Query = new FetchExpression(xml)
                        };

                        EntityCollection returnCollection = ((RetrieveMultipleResponse)service.Execute(fetchRequest1)).EntityCollection;

                        foreach (var c in returnCollection.Entities)
                        {
                            System.Console.WriteLine("{0}.\t{1}\t\t{2}", ++recordCount, c.Attributes["name"], c.Attributes["emailaddress1"]);
                        }

                        // Check for morerecords, if it returns 1.
                        if (returnCollection.MoreRecords)
                        {
                            Console.WriteLine("\n****************\nPage number {0}\n****************", pageNumber);
                            Console.WriteLine("#\tAccount Name\t\t\tEmail Address");

                            // Increment the page number to retrieve the next page.
                            pageNumber++;

                            // Set the paging cookie to the paging cookie returned from current results.                            
                            pagingCookie = returnCollection.PagingCookie;
                        }
                        else
                        {
                            // If no more records in the result nodes, exit the loop.
                            break;
                        }
                    }

                    #region Clean up
                    CleanUpSample(service);
                    #endregion Clean up
                }
                #endregion Demonstrate
                else
                {
                    const string UNABLE_TO_LOGIN_ERROR = "Unable to Login to Common Data Service";
                    if (service.LastCrmError.Equals(UNABLE_TO_LOGIN_ERROR))
                    {
                        Console.WriteLine("Check the connection string values in cds/App.config.");
                        throw new Exception(service.LastCrmError);
                    }
                    else
                    {
                        throw service.LastCrmException;
                    }
                }
            }
            #endregion Sample Code
            catch (Exception ex)
            {
                SampleHelpers.HandleException(ex);
            }

            finally
            {
                if (service != null)
                    service.Dispose();

                Console.WriteLine("Press <Enter> to exit.");
                Console.ReadLine();
            }
        }
            }
}
