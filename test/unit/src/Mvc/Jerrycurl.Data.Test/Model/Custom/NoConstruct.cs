﻿namespace Jerrycurl.Data.Test.Model.Custom
{
    public class NoConstruct
    {
        public int Int { get; set; }
        public string String { get; set; }

        public NoConstruct(string s)
        {
            this.String = s;
        }
    }
}