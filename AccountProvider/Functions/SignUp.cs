using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AccountProvider.Functions;

public class SignUp(ILogger<SignUp> logger, UserManager<UserAccount> usermanager)
{
    private readonly ILogger<SignUp> _logger = logger;
    private readonly UserManager<UserAccount> _userManager = usermanager;

  

    [Function("SignUp")]
    //[ServiceBusOutput("verification_request", Connection = "ServiceBusConnection")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var standardRole = "User";
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
            UserRegistrationRequest urr = null!;

            try
            {
                urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : JsonConvert.DeserializeObject<UserRegistrationRequest> :: {ex.Message}");
            }
            if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password) && !string.IsNullOrEmpty(urr.FirstName) && !string.IsNullOrEmpty(urr.LastName))
            {
                if (!await _userManager.Users.AnyAsync())
                {
                    standardRole = "SuperAdmin";
                }

                if (!await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                {
                    var userAccount = new UserAccount
                    {
                        FirstName = urr.FirstName,
                        LastName = urr.LastName,
                        Email = urr.Email,
                        UserName = urr.Email,
                        Created = DateTime.Now,
                    };
                    try
                    {
                        var result = await _userManager.CreateAsync(userAccount, urr.Password);
                        if (result.Succeeded)
                        {
                            //try
                            //{
                            //    using var http = new HttpClient();
                            //    StringContent content = new StringContent(JsonConvert.SerializeObject(new
                            //    {
                            //        Email = userAccount.Email,

                            //    }), Encoding.UTF8, "application/json");
                            //    var response = await http.PostAsync("", content);
                            //}
                            //catch (Exception ex)
                            //{
                            //    _logger.LogError($"ERROR :  http.PostAsync :: {ex.Message}");
                            //}

                            var roleResult = await _userManager.AddToRoleAsync(userAccount, standardRole);
                            if (standardRole == "SuperAdmin" && roleResult.Succeeded)
                            {
                                roleResult = await _userManager.AddToRoleAsync(userAccount, "User");
                                roleResult = await _userManager.AddToRoleAsync(userAccount, "Admin");
                                roleResult = await _userManager.AddToRoleAsync(userAccount, "CIO");
                            }
                            return new OkResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ERROR :  _userManager.CreateAsync :: {ex.Message}");
                    }
                }                
                else
                {
                    return new ConflictResult();
                }
            }
        }
        return new BadRequestResult();
    }

}



