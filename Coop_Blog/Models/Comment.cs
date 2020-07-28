using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Coop_Blog.Models
{
    public class Comment
    {
        //Create property that acts as a unique identifier for any single comment
        public int Id { get; set; }
        //Foreign Key AKA refer to the primary key (Id)
        public int BlogPostId { get; set; }
        //Foreign Key AKA refer to user who wrote the comment
        public string AuthorId { get; set; }
        //record and store content of comment
        public string Body { get; set; }
        //record and store time in which post was created
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        //only to be used by Mod or Admin
        public string UpdateReason { get; set; }
        //inform this comment that it is tightly coupled with its parent of Author
        public virtual ApplicationUser Author { get; set; }

        //How to tell the comment who it's parent blog post is
        public virtual BlogPost BlogPost { get; set; }


    }
}