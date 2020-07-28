using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Coop_Blog.Models
{
    public class BlogPost
    {
        // Id= Primary Key
        public int Id { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        [AllowHtml]
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public string Slug { get; set; }


        public string MediaPath { get; set; }
        public bool Published { get; set; }
        //Navigational Properties
        public virtual ICollection<Comment> Comments { get; set; }
        public BlogPost()
        {
            Comments = new HashSet<Comment>();
        }

    }
}