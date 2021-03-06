﻿namespace Cake.Host.Scripting.Mono.CodeGen.Parsing
{
    public sealed class ScriptBlock
    {
        private readonly int _index;
        private readonly string _content;
        private readonly bool _hasScope;

        public int Index
        {
            get { return _index; }
        }

        public string Content
        {
            get { return _content; }
        }

        public bool HasScope
        {
            get { return _hasScope; }
        }

        public ScriptBlock(int index, string content, bool hasScope)
        {
            _index = index;
            _content = content;
            _hasScope = hasScope;
        }
    }
}