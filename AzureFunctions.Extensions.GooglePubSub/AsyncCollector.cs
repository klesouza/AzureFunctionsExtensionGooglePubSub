﻿using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.GooglePubSub
{

    internal class AsyncCollector : ICollector<string>, IAsyncCollector<string>
    {

        private readonly GooglePubSubAttribute googlePubSubAttribute;
        private readonly Microsoft.Extensions.Logging.ILogger logger;
        private List<string> items = new List<string>();

        public AsyncCollector(GooglePubSubAttribute googlePubSubAttribute, Microsoft.Extensions.Logging.ILogger logger)
        {
            this.googlePubSubAttribute = GooglePubSubAttribute.GetAttributeByConfiguration(googlePubSubAttribute);
            this.logger = logger;
        }

        void ICollector<string>.Add(string item)
        {
            items.Add(item);
        }

        Task IAsyncCollector<string>.AddAsync(string item, CancellationToken cancellationToken)
        {
            items.Add(item);
            return Task.WhenAll();
        }

        Task IAsyncCollector<string>.FlushAsync(CancellationToken cancellationToken)
        {

            if (items.Any())
            {

                var publisher = PublisherClientCache.GetTopicsClient(googlePubSubAttribute);

                return
                    publisher.PublishAsync($"projects/{googlePubSubAttribute.ProjectId}/topics/{googlePubSubAttribute.TopicId}",
                    new TransparentApiClient.Google.PubSub.V1.Schema.PublishRequest()
                    {
                        messages = items.Select(c => new TransparentApiClient.Google.PubSub.V1.Schema.PubsubMessage()
                        {
                            data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(c))
                        })
                    }, null, cancellationToken)
                    .ContinueWith((publishTask) =>
                    {
                        if (publishTask.IsFaulted)
                        {
                            throw publishTask.Exception;
                        }
                        else
                        {
                            logger.LogInformation($"Sent {items.Count()} items to PubSub.");
                        }
                    });

            }
            else
            {
                logger.LogInformation("No items to write to PubSub.");
            }

            return Task.CompletedTask;
        }

    }
}