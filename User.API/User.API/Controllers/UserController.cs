using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.API.Data;
using Microsoft.AspNetCore.JsonPatch;

namespace User.API.Controllers
{
    [Route("api/users")]
    public class UserController : BaseController
    {
        private UserContext _userContext;
        private ILogger<UserController> _logger;

        public UserController(UserContext userContext,ILogger<UserController> logger)
        {
            _userContext = userContext;
            _logger = logger;
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(u => u.Properties)
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            if (user == null)
            {
                throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
                
                //return NotFound();
            }

            return Json(user);
        }

        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<Models.AppUser> patch)
        {
            var user = await _userContext.Users
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            patch.ApplyTo(user);

            foreach (var property in user.Properties)
            {
                _userContext.Entry(property).State = EntityState.Detached;
            }

            var origionProperties = await _userContext.UserProperties
                .AsNoTracking()
                .Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync();

            var allProperties = origionProperties.Union(user.Properties).Distinct();//①合并去重

            var removedProperties = origionProperties.Except(user.Properties);//②原有的除去请求的=需删除的
            var newProperties = allProperties.Except(origionProperties);//③全部除去原有的=需新增的

            foreach (var property in removedProperties)
            {
                _userContext.Entry(property).State = EntityState.Deleted;
                //_userContext.Remove(property);
            }

            foreach (var property in newProperties)
            {
                _userContext.Entry(property).State = EntityState.Added;
                //_userContext.Add(property);
            }

            _userContext.Users.Update(user);
            _userContext.SaveChanges();
            return Json(user); 
        }
    }
}
