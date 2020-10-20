﻿using System.Collections.Generic;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Data.Test.Model
{
    public class Blog
    {
        [Key("PK_Blog")]
        public int Id { get; set; }
        [Key("PK_Blog_2", IsPrimary = false)]
        public int Id2 { get; set; }
        public string Title { get; set; }

        public IList<BlogTag> Tags { get; set; }
        public IList<BlogPost> Posts { get; set; }
    }
}