﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MusicApp2017.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Define custom properties here

        //Foreign Key
        public int GenreID { get; set; }
    }

}