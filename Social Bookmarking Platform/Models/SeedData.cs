﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using System.Collections.Generic;

namespace Social_Bookmarking_Platform.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
               
            if (context.Roles.Any())
                {
                    return;
                }
            
            context.Roles.AddRange(

            new IdentityRole {
                Id = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                Name = "Admin", 
                NormalizedName = "Admin".ToUpper() },
            
            new IdentityRole{
                Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", 
                Name = "User", 
                NormalizedName = "User".ToUpper() }
            );

            var hasher = new PasswordHasher<ApplicationUser>();


            context.Users.AddRange(
                new ApplicationUser {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,"Admin1!")
                },

                new ApplicationUser {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,"User1!")
                }
            );

            context.UserRoles.AddRange(
                new IdentityUserRole<string> {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },

                new IdentityUserRole<string> {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                }
            );
                context.SaveChanges();
            }
        }
    }
}