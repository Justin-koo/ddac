using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace webapp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the webappUser class
public class webappUser : IdentityUser
{
    [PersonalData]
    public string? FullName { get; set; }


    [PersonalData]
    public string? Address { get; set; }

    [PersonalData]
    public string? State { get; set; }

    [PersonalData]
    public string? City { get; set; }

    [PersonalData]
    public int? Zip { get; set; }

    [PersonalData]
    public string? About { get; set; }

    [PersonalData]
    public string? FacebookLink { get; set; }

    [PersonalData]
    public string? XLink { get; set; }

    [PersonalData]
    public string? GoogleLink { get; set; }

    [PersonalData]
    public string? LinkedInLink { get; set; }

    [PersonalData]
    public string? Country { get; set; }

	[PersonalData]
	public string? ProfilePicture { get; set; }

    [PersonalData]
    public string? Specialties { get; set; }

    [PersonalData]
    public string? Languages { get; set; }

}