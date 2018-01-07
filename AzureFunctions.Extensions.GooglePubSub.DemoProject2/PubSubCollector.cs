using Microsoft.Azure.WebJobs;

namespace AzureFunctions.Extensions.GooglePubSub.DemoProject2 {
    public static class PubSubCollector {
        [FunctionName("PubSubCollector")]
        [Disable]
        public static void Run(
           [TimerTrigger("0 */1 * * * *", RunOnStartup = true)]TimerInfo myTimer,
           [GooglePubSub("MyGooglePubSubConfig")] ICollector<string> messages
           ) {

            messages.Add("I have a new message");

        }
    }
}
