using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class AspNetUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty; 

    public string LastName { get; set; } = string.Empty;  

    public string? ProfileImage {  get; set; } = string.Empty;
}
