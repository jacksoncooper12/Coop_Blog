using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Coop_Blog.Models
{
    public class DetailsModel
    {
        public IEnumerable<BlogPost> BlogPosts { get; set; }
        public BlogPost Post { get; set; }
    }
}