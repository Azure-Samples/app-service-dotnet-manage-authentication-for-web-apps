// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ManageWebAppWithAuthentication
{
    /**
     * Azure App Service sample for managing authentication for web apps.
     *  - Create 4 web apps under the same new app service plan with:
     *    - Active Directory login for 1
     *    - Facebook login for 2
     *    - Google login for 3
     *    - Microsoft login for 4
     */

    public class Program
    {
        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string suffix = ".azurewebsites.net";
            string app1Name = Utilities.CreateRandomName("webapp1-");
            string app2Name = Utilities.CreateRandomName("webapp2-");
            string app3Name = Utilities.CreateRandomName("webapp3-");
            string app4Name = Utilities.CreateRandomName("webapp4-");
            string app1Url = app1Name + suffix;
            string app2Url = app2Name + suffix;
            string app3Url = app3Name + suffix;
            string app4Url = app4Name + suffix;
            string rgName = Utilities.CreateRandomName("rg1NEMV_");
            var lro = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
            var resourceGroup = lro.Value;

            try
            {


                //============================================================
                // Create a web app with a new app service plan

                Utilities.Log("Creating web app " + app1Name + " in resource group " + rgName + "...");

                var webSiteCollection = resourceGroup.GetWebSites();
                var webSiteData = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    }
                };
                var webSite_lro = await webSiteCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app1Name, webSiteData);
                var webSite = webSite_lro.Value;

                Utilities.Log("Created web app " + webSite.Data.Name);
                Utilities.Print(webSite);

                //============================================================
                // Set up active directory authentication

                Utilities.Log("Please create an AD application with redirect URL " + app1Url);
                Utilities.Log("Application ID is:");
                string applicationId = Utilities.ReadLine();
                Utilities.Log("Tenant ID is:");
                string tenantId = Utilities.ReadLine();

                Utilities.Log("Updating web app " + app1Name + " to use active directory login...");

                await webSite.UpdateAuthSettingsV2Async(new Azure.ResourceManager.AppService.Models.SiteAuthSettingsV2()
                {
                    IdentityProviders = new AppServiceIdentityProviders()
                    {
                        AzureActiveDirectory = new AppServiceAadProvider()
                        {
                            Registration = new AppServiceAadRegistration(),
                            Validation = new AppServiceAadValidation()
                            {
                                DefaultAuthorizationPolicy = new DefaultAuthorizationPolicy()
                            }
                        }
                    }
                });

                Utilities.Log("Added active directory login to " + webSite.Data.Name);
                Utilities.Print(webSite);

                //============================================================
                // Create a second web app

                Utilities.Log("Creating another web app " + app2Name + " in resource group " + rgName + "...");
                var planId = webSite.Data.AppServicePlanId;
                var webSite2Collection = resourceGroup.GetWebSites();
                var webSite2Data = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    },
                    AppServicePlanId = planId,
                };
                var webSite2_lro = await webSite2Collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app2Name, webSite2Data);
                var webSite2 = webSite2_lro.Value;

                Utilities.Log("Created web app " + webSite2.Data.Name);
                Utilities.Print(webSite2);

                //============================================================
                // Set up Facebook authentication

                Utilities.Log("Please create a Facebook developer application with whitelisted URL " + app2Url);
                Utilities.Log("App ID is:");
                string fbAppId = Utilities.ReadLine();
                Utilities.Log("App secret is:");
                string fbAppSecret = Utilities.ReadLine();

                Utilities.Log("Updating web app " + app2Name + " to use Facebook login...");

                await webSite2.UpdateAuthSettingsAsync(new SiteAuthSettings()
                {
                    FacebookAppId = fbAppId,
                    FacebookAppSecret = fbAppSecret
                });

                Utilities.Log("Added Facebook login to " + webSite2.Data.Name);
                Utilities.Print(webSite2);

                //============================================================
                // Create a 3rd web app with a public GitHub repo in Azure-Samples

                Utilities.Log("Creating another web app " + app3Name + "...");

                var webSite3Collection = resourceGroup.GetWebSites();
                var webSite3Data = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    },
                    AppServicePlanId = planId,
                };
                var webSite3_lro = await webSite3Collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app3Name, webSite3Data);
                var webSite3 = webSite3_lro.Value;

                Utilities.Log("Created web app " + webSite3.Data.Name);
                Utilities.Print(webSite3);

                //============================================================
                // Set up Google authentication

                Utilities.Log("Please create a Google developer application with redirect URL " + app3Url);
                Utilities.Log("Client ID is:");
                string gClientId = Utilities.ReadLine();
                Utilities.Log("Client secret is:");
                string gClientSecret = Utilities.ReadLine();

                Utilities.Log("Updating web app " + app3Name + " to use Google login...");

                await webSite2.UpdateAuthSettingsAsync(new SiteAuthSettings()
                {
                    GoogleClientId = gClientId,
                    GoogleClientSecret = gClientSecret
                });

                Utilities.Log("Added Google login to " + webSite3.Data.Name);
                Utilities.Print(webSite3);

                //============================================================
                // Create a 4th web app

                Utilities.Log("Creating another web app " + app4Name + "...");
                var webSite4Collection = resourceGroup.GetWebSites();
                var webSite4Data = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    },
                    AppServicePlanId = planId,
                };
                var webSite4_lro = await webSite4Collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app4Name, webSite4Data);
                var webSite4 = webSite4_lro.Value;

                Utilities.Log("Created web app " + webSite4.Data.Name);
                Utilities.Print(webSite4);

                //============================================================
                // Set up Google authentication

                Utilities.Log("Please create a Microsoft developer application with redirect URL " + app4Url);
                Utilities.Log("Client ID is:");
                string clientId = Utilities.ReadLine();
                Utilities.Log("Client secret is:");
                string clientSecret = Utilities.ReadLine();

                Utilities.Log("Updating web app " + app3Name + " to use Microsoft login...");

                await webSite2.UpdateAuthSettingsAsync(new SiteAuthSettings()
                {
                    MicrosoftAccountClientId = clientId,
                    MicrosoftAccountClientSecret = clientSecret,
                });

                Utilities.Log("Added Microsoft login to " + webSite4.Data.Name);
                Utilities.Print(webSite4);
            }
            finally
            {
                try
                {
                    Utilities.Log("Deleting Resource Group: " + rgName);
                    await resourceGroup.DeleteAsync(WaitUntil.Completed);
                    Utilities.Log("Deleted Resource Group: " + rgName);
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                // Print selected subscription
                Utilities.Log("Selected subscription: " + client.GetSubscriptions().Id);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}