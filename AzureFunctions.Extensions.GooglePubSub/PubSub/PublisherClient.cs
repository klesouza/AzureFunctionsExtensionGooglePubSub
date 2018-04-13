﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.GooglePubSub.PubSub {
    internal sealed class PublisherClient : PubSubBaseClient {

        private readonly string topicId;

        public PublisherClient(byte[] serviceAccountCredentials, string projectId, string topicId)
                : base(serviceAccountCredentials, projectId) {

            if (string.IsNullOrWhiteSpace(topicId)) { throw new ArgumentNullException(nameof(topicId)); }

            this.topicId = topicId;
        }

        public Task<IEnumerable<string>> PublishAsync(IEnumerable<string> messages, CancellationToken cancellationToken) {

            var pubSubMessages = new { messages = messages.Select(c => new { data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(c)) }) };

            return SendAsync(HttpMethod.Post, $"topics/{topicId}:publish", pubSubMessages, cancellationToken)
                .ContinueWith((postTask) => {

                    HttpResponseMessage post = postTask.Result;

                    if (post.IsSuccessStatusCode) {

                        if (post.Content.Headers.ContentEncoding.Contains("gzip")) {

                            return post.Content.ReadAsByteArrayAsync()
                                .ContinueWith((readAsByteArrayTask) => {
                                    byte[] resultByteArray = readAsByteArrayTask.Result;

                                    var memoryStream = new MemoryStream();
                                    using (var zipStream = new System.IO.Compression.GZipStream(new MemoryStream(resultByteArray), System.IO.Compression.CompressionMode.Decompress)) {
                                        zipStream.CopyTo(memoryStream);
                                    }

                                    var resultString = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                                    var publishResponse = JsonConvert.DeserializeObject<PublishResponse>(resultString);

                                    return publishResponse?.messageIds;
                                });

                        } else {

                            return post.Content.ReadAsStringAsync()
                                .ContinueWith((readAsStringTask) => {
                                    string resultString = readAsStringTask.Result;
                                    var publishResponse = JsonConvert.DeserializeObject<PublishResponse>(resultString);

                                    return publishResponse.messageIds;
                                });
                        }

                    } else {
                        throw new ApplicationException("pubSub publish failed");
                    }

                }).Unwrap();
        }

        private class PublishResponse {
            public IEnumerable<string> messageIds { get; set; }
        }

    }
}