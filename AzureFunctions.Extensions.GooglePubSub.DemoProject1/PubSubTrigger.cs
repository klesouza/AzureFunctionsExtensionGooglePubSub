using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Threading;

namespace AzureFunctions.Extensions.GooglePubSub.DemoProject1 {
    public static class PubSubTrigger {

        [FunctionName("PubSubTrigger")]
        public static void Run(
            [GooglePubSubTrigger("", "projectId", "topicId", "subscriptionId", CreateSubscriptionIfDoesntExist = true, MaxBatchSize = 1000)]
                IEnumerable<string> messages,
            CancellationToken cancellationToken) {

            foreach (var message in messages) {
                System.Console.WriteLine(message);
            }
            
        }

    }
}
