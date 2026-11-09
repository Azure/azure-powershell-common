// ----------------------------------------------------------------------------------
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Declares headers that belong to one logical request, rather than its LRO follow-ups.
    /// Retry clones retain these headers.
    /// </summary>
    public static class RequestHeaderScope
    {
        /// <summary>
        /// Cross-assembly metadata key consumed by generated AutoRest PowerShell runtimes.
        /// The value is an immutable snapshot implementing <see cref="IEnumerable{String}"/>.
        /// </summary>
        public const string RequestScopedHeadersKey = "Microsoft.Azure.PowerShell.RequestScopedHeaders";

        /// <summary>
        /// Marks a header as request-scoped without modifying its value.
        /// </summary>
        public static void MarkRequestScoped(HttpRequestMessage request, string headerName)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(headerName)) throw new ArgumentException("A header name is required.", nameof(headerName));

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (request.Properties.TryGetValue(RequestScopedHeadersKey, out var scopedHeaders))
            {
                if (!(scopedHeaders is IEnumerable<string> headerNames))
                {
                    throw new InvalidOperationException($"{RequestScopedHeadersKey} must contain an IEnumerable<string>.");
                }
                names.UnionWith(headerNames);
            }
            names.Add(headerName);

            // Retry clones share property values, so never mutate an existing snapshot.
            request.Properties[RequestScopedHeadersKey] = Array.AsReadOnly(names.ToArray());
        }
    }
}
