﻿using System;
using System.Collections.Generic;
using Cake.Core.Diagnostics;

namespace Cake.Host.Arguments
{
    /// <summary>
    /// Responsible for parsing <see cref="Verbosity"/>.
    /// </summary>
    public sealed class VerbosityParser
    {
        private readonly Dictionary<string, Verbosity> _lookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerbosityParser"/> class.
        /// </summary>
        public VerbosityParser()
        {
            _lookup = new Dictionary<string, Verbosity>(StringComparer.OrdinalIgnoreCase)
            {
                { "q", Verbosity.Quiet },
                { "quiet", Verbosity.Quiet },
                { "m", Verbosity.Minimal },
                { "minimal", Verbosity.Minimal },
                { "n", Verbosity.Normal },
                { "normal", Verbosity.Normal },
                { "v", Verbosity.Verbose },
                { "verbose", Verbosity.Verbose },
                { "d", Verbosity.Diagnostic },
                { "diagnostic", Verbosity.Diagnostic }
            };
        }

        /// <summary>
        /// Parses the provided string to a <see cref="Verbosity"/>.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The verbosity.</returns>
        public Verbosity Parse(string value)
        {
            Verbosity verbosity;
            if (_lookup.TryGetValue(value, out verbosity))
            {
                return verbosity;
            }
            const string format = "The value '{0}' is not a valid verbosity.";
            var message = string.Format(format, value ?? string.Empty);
            throw new InvalidOperationException(message);
        }
    }
}