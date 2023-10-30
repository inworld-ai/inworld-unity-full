/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;

namespace Inworld.Entities
{
    [Serializable]
    public class BillingAccountRespone
    {
        public List<BillingAccount> billingAccounts;
    }
    [Serializable]
    public class BillingAccount
    {
        public string name;
        public string displayName;
    }
}
