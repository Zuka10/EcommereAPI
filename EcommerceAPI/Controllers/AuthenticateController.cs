﻿using Catalog.Domain;
using Catalog.Facade.Services;
using Ecommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

namespace Ecommerce.API.Controllers
{
    [Route("api/authenticate")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AuthenticateController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, "Username already exists");

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status400BadRequest, "User creation failed! Please check user details and try again.");

            await _emailService.SendConfirmationEmail(model.Email!, user);
            return Ok("User created Successfully!");
        }

        [HttpGet]
        [Route("confirmEmail")]
        public async Task<string> ConfirmEmail(string userId, string token)
        {
            try
            {
                return await _emailService.ConfirmEmail(userId, token);
            }
            catch (Exception)
            {
                return "Email confirmation failed";
                throw;
            }
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logged out successfully");
        }
    }
}