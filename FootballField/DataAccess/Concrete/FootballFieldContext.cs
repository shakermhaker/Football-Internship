using System;
using System.Collections.Generic;
using System.Text;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Core.Entities.Concrete;
   

namespace FootballField.DataAccess.Concrete.EntityFramework;

public class FootballFieldContext : DbContext
{
    public FootballFieldContext(DbContextOptions<FootballFieldContext> options) : base(options)
    {
        
    }
    


}

