﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Formatting
{
    /// <summary>
    /// it holds onto trivia information between two tokens
    /// </summary>
    internal abstract class TriviaData
    {
        protected const int TokenPairIndexNotNeeded = int.MinValue;

        private readonly AnalyzerConfigOptions _optionSet;
        private readonly string _language;

        protected TriviaData(AnalyzerConfigOptions optionSet, string language)
        {
            Contract.ThrowIfNull(optionSet);
            _optionSet = optionSet;
            _language = language;
        }

        protected AnalyzerConfigOptions OptionSet => _optionSet;
        protected string Language => _language;

        public int LineBreaks { get; protected set; }
        public int Spaces { get; protected set; }

        public bool SecondTokenIsFirstTokenOnLine { get { return this.LineBreaks > 0; } }

        public abstract bool TreatAsElastic { get; }
        public abstract bool IsWhitespaceOnlyTrivia { get; }
        public abstract bool ContainsChanges { get; }

        public abstract IEnumerable<TextChange> GetTextChanges(TextSpan span);

        public abstract TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules);

        public abstract TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken);

        public abstract TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken);

        public abstract void Format(
            FormattingContext context,
            ChainedFormattingRules formattingRules,
            Action<int, TriviaData> formattingResultApplier,
            CancellationToken cancellationToken,
            int tokenPairIndex = TokenPairIndexNotNeeded);
    }
}
