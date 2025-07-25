﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using Microsoft.PowerToys.Telemetry;
using Microsoft.PowerToys.Telemetry.Events;

namespace Microsoft.PowerToys.PreviewHandler.Bgcode.Telemetry.Events
{
    /// <summary>
    /// A telemetry event to be raised when an error has occurred in the preview pane.
    /// </summary>
    [EventData]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public class BgcodeFilePreviewError : EventBase, IEvent
    {
        /// <summary>
        /// Gets or sets the error message to log as part of the telemetry event.
        /// </summary>
        public string Message { get; set; }

        /// <inheritdoc/>
        public PartA_PrivTags PartA_PrivTags => PartA_PrivTags.ProductAndServicePerformance;
    }
}
