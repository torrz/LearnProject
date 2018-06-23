using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Controllers;
using User.API.Data;
using System.Collections.Generic;
using System.Linq;

namespace User.API.UnitTests
{
    public class UserControllerUnitTests
    {
        private UserContext GetUserContext()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var userContext = new UserContext(options);

            userContext.Users.Add(new Models.AppUser
            {
                Id = 1,
                Name = "torrz"
            });

            userContext.SaveChanges();
            return userContext;
        }

        private (UserController controller, UserContext userContext) GetUserController()
        {
            var context = GetUserContext();

            var loggerMoq = new Mock<ILogger<UserController>>();
            var logger = loggerMoq.Object;

            return (controller: new UserController(context, logger), userContext: context);
        }


        [Fact]
        public async Task Get_ReturnRightUser_WithExpectedParameters()
        {
            (UserController controller, UserContext userContext) = GetUserController();
            var response = await controller.Get();

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Id.Should().Be(1);
            appUser.Name.Should().Be("torrz");
            //Assert.IsType<JsonResult>(response);
        }

        [Fact]
        public async Task Patch_ReturnNewName_WithExceptedNewNameParameter()
        {
            (UserController controller, UserContext userContext) = GetUserController();

            var document = new JsonPatchDocument<Models.AppUser>();
            document.Replace(u => u.Name, "wen");
            var response = await controller.Patch(document);


            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser= result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Name.Should().Be("wen");

            //assert name value in ef context
            var userModel =await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Should().NotBeNull();
            userModel.Name.Should().Be("wen");

        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WithAddNewProperty()
        {
            (UserController controller, UserContext userContext) = GetUserController();

            var document = new JsonPatchDocument<Models.AppUser>();
            document.Replace(u => u.Properties, new List<Models.UserProperty> {
                new Models.UserProperty{ Key="fin_industry",Value="»¥ÁªÍø",Text="»¥ÁªÍø" }
            });
            var response = await controller.Patch(document);


            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Properties.Count.Should().Be(1);
            appUser.Properties.First().Value.Should().Be("»¥ÁªÍø");
            appUser.Properties.First().Key.Should().Be("fin_industry");

            //assert properties value in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Count.Should().Be(1);
            userModel.Properties.First().Value.Should().Be("»¥ÁªÍø");
            userModel.Properties.First().Key.Should().Be("fin_industry");

        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WithRemoveProperty()
        {
            (UserController controller, UserContext userContext) = GetUserController();

            var document = new JsonPatchDocument<Models.AppUser>();
            document.Replace(u => u.Properties, new List<Models.UserProperty> {
            });
            var response = await controller.Patch(document);


            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Properties.Should().BeEmpty();

            //assert properties value in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Should().BeEmpty();

        }
    }
}
