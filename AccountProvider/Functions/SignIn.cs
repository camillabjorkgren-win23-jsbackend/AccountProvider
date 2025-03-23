using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace AccountProvider.Functions
{
    public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager)
    {
        private readonly ILogger<SignIn> _logger = logger;
        private readonly SignInManager<UserAccount> _signInManager = signInManager;
        private readonly UserManager<UserAccount> _userManager = userManager;
       
        [Function("SignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
           
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : StreamReader :: {ex.Message}");
            }

            if (body != null)
            {
                LoginRequest loginRequest = null!;

                try
                {
                    loginRequest = JsonConvert.DeserializeObject<LoginRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR : JsonConvert.DeserializeObject<LoginRequest> :: {ex.Message}");
                }
                if (loginRequest != null && !string.IsNullOrEmpty(loginRequest.Email) && !string.IsNullOrEmpty(loginRequest.Password))
                {
                    try
                    {
                        var userAccount = await _userManager.FindByEmailAsync(loginRequest.Email);
                        if (userAccount != null)
                        {
                            var result = await _signInManager.CheckPasswordSignInAsync(userAccount, loginRequest.Password, false);
                            if (result.Succeeded)
                            {

                                return new OkObjectResult("accesstoken");
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ERROR : _signInManager.PasswordSignInAsync :: {ex.Message}");
                    }
                    return new UnauthorizedResult();

                }
            }
            return new BadRequestResult();
        }
    }
}
