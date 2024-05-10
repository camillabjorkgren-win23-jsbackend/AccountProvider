using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace AccountProvider.Functions
{
    public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager)
    {
        private readonly ILogger<SignIn> _logger = logger;
        private readonly SignInManager<UserAccount> _signInManager = signInManager;

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
                        var result = await _signInManager.PasswordSignInAsync(loginRequest.Email, loginRequest.Password, loginRequest.RememberMe, false);
                        if (result.Succeeded)
                        {
                            // Get token from tokenprovider

                            return new OkObjectResult("accesstoken");
                        }
                        else
                        {
                            return new UnauthorizedResult();
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ERROR : _signInManager.PasswordSignInAsync :: {ex.Message}");
                    }

                }
            }
            return new BadRequestResult();
        }
    }
}
