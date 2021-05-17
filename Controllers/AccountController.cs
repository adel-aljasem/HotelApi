using Common;
using DataAcesss.Data;
using Hotel_Api.Helper;
using Mailjet.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly APISettings apiSettings;
        private readonly EmailSender emailSender;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager , IOptions<APISettings> options , EmailSender emailSender)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
            apiSettings = options.Value;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] UserRequsetDTO userRequsetDTO)
        {
            if (userRequsetDTO == null || !ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = new ApplicationUser
            {
                UserName = userRequsetDTO.Email,
                Email = userRequsetDTO.Email,
                Name = userRequsetDTO.Name,
                PhoneNumber = userRequsetDTO.PhoneNo,
                EmailConfirmed = true
            };


            var MailRespwon = await emailSender.SendEmailAsync(userRequsetDTO.Email, "تم التسجيل بنجاح", "تم تسجيل بنجاح بحساب " + userRequsetDTO.Name);

            if (!MailRespwon.IsSuccessStatusCode)
            {
                return BadRequest(new RegisterationResponseDTO {IsRegisterationSuccessful = false });
            }

            var result = await userManager.CreateAsync(user, userRequsetDTO.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new RegisterationResponseDTO { Errors = errors,IsRegisterationSuccessful = false});
            }

            var roleResult = await userManager.AddToRoleAsync(user,SD.Role_Customer);

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description);
                return BadRequest(new RegisterationResponseDTO { Errors = errors, IsRegisterationSuccessful = false });
            }


            return StatusCode(201);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] AuthenticationDTO authenticationDTO)
        {
            var result = await signInManager.PasswordSignInAsync(authenticationDTO.UserName, authenticationDTO.Password, false, false);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(authenticationDTO.UserName);
                if(user == null)
                {
                    return Unauthorized(new AuthenticationResponseDTO
                    {
                        IsAuthSuccessful = false,
                        Errors = "invalid Authentication"
                    });
                }


                //everything is valid and we need to login the user

                var signincreditals = GetSigningCredentials();
                var claims = await GetClaims(user);

                var tokenOptions = new JwtSecurityToken(issuer:apiSettings.ValidIssuer, audience:apiSettings.ValidAudience, claims:claims, expires:DateTime.Now.AddDays(30), signingCredentials:signincreditals);
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                return Ok(new AuthenticationResponseDTO
                {
                    IsAuthSuccessful = true,
                    Token = token,
                    UserDto = new UserDTO
                    {
                        Name=user.Name,
                        Id=user.Id,
                        Email = user.Email,
                        PhoneNo = user.PhoneNumber
                    }
                });
            }
            else
            {
                return Unauthorized(new AuthenticationResponseDTO
                {
                    IsAuthSuccessful = false,
                    Errors = "Invalid"
                }) ;
            }
        }

        private SigningCredentials GetSigningCredentials()
        {
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSettings.SecretKey));
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Email),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Id",user.Id)
            };
            var roles = await userManager.GetRolesAsync(await userManager.FindByNameAsync(user.Email));

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
    }
}
