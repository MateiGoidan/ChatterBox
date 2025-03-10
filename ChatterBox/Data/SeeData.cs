﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChatterBox.Models;

namespace ChatterBox.Data
{
    public class SeeData
    {
        public static void Initialize(IServiceProvider _ServiceProvider)
        {
            using (var _Context = new ApplicationDbContext(_ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (_Context.Roles.Any())
                {
                    return;
                }

                string _GUID_Admin_Role = Guid.NewGuid().ToString();
                string _GUID_User_Role = Guid.NewGuid().ToString();

                string _GUID_Admin = Guid.NewGuid().ToString();

                _Context.Roles.AddRange
                (
                    new IdentityRole { Id = _GUID_Admin_Role, Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Id = _GUID_User_Role, Name = "User", NormalizedName = "USER" }
                );

                var _Hasher = new PasswordHasher<ApplicationUser>();

                _Context.AppUsers.AddRange
                (
                    new ApplicationUser
                    {
                        Id = _GUID_Admin,
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = _Hasher.HashPassword(null, "Parola_2")
                    }
                );

                _Context.UserRoles.AddRange
                (
                    new IdentityUserRole<string>
                    {
                        RoleId = _GUID_Admin_Role,
                        UserId = _GUID_Admin
                    }
                );

                _Context.SaveChanges();
            };

        }
    }
}
