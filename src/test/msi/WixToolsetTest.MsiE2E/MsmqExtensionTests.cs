// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.MsiE2E
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;
    using WixTestTools;
    using Xunit.Abstractions;

    public class MsmqExtensionTests : MsiE2ETests
    {
        public MsmqExtensionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [RuntimePrereqFeatureFact("MSMQ-Container", "MSMQ-Server")]
        public void CanInstallAndUninstallMsmq()
        {
            var product = this.CreatePackageInstaller("MsmqInstall");
            product.InstallProduct(MSIExec.MSIExecReturnCode.SUCCESS);

            string queuePath = @".\private$\example-queue";

            Assert.True(MessageQueue.Exists(queuePath), "The message queue was not created.");

            using (MessageQueue queue = new MessageQueue(queuePath))
            {
                // Get the access control list for the queue
                MessageQueueAccessControlList acl = queue.GetPermissions();

                // Check if the Everyone group has GetProperties permissions
                bool foundEveryoneGetPropertiesAccess = false;
                foreach (MessageQueueAccessControlEntry entry in acl)
                {
                    IdentityReference identity = entry.Identity;
                    MessageQueueAccessRights rights = entry.GenericAccessRights;

                    // Check if the entry is for the Everyone group
                    if (identity.Value == new SecurityIdentifier(WellKnownSidType.WorldSid, null).Value)
                    {
                        foundEveryoneGetPropertiesAccess = (rights & MessageQueueAccessRights.GetProperties) == MessageQueueAccessRights.GetProperties;
                    }
                }

                Assert.True(foundEveryoneGetPropertiesAccess, "The Everyone group does not have GetProperties permissions.");
            }
        
            product.UninstallProduct(MSIExec.MSIExecReturnCode.SUCCESS);
        }
    }
}
