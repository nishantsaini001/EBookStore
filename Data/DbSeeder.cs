using BooksShoppingProjectMVC.Constant;
using Microsoft.AspNetCore.Identity;

namespace BooksShoppingProjectMVC.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userMgr = service.GetService<UserManager<IdentityUser>>();
            var roleMgr = service.GetService<RoleManager<IdentityRole>>();

            await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));

            // create admin user

            var admin = new IdentityUser
            {
                UserName="admin@gmail.com",
                Email="admin@gmail.com",
                EmailConfirmed=true

            };


            var UserInDb = await userMgr.FindByEmailAsync(admin.Email);

            if (UserInDb is null)
            {
                await userMgr.CreateAsync(admin,"Admin@123");
                await userMgr.AddToRoleAsync(admin,Roles.Admin.ToString());
            }

        }
    }
}
