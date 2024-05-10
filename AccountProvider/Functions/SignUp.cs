using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AccountProvider.Functions;

public class SignUp
{
    private readonly ILogger<SignUp> _logger;
    private readonly UserManager<UserAccount> _userManager;
    private readonly QueueClient _queueClient;

    public SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        string serviceBusConnection = Environment.GetEnvironmentVariable("ServiceBusConnection")!;
        string queueName = Environment.GetEnvironmentVariable("ServiceBusQueueName")!;
        _queueClient = new QueueClient(serviceBusConnection, queueName);
    }

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
                urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body)!;
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
                            try
                            {
                                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                                {
                                    Email = userAccount.Email,
                                })));
                                await _queueClient.SendAsync(message);
                                _logger.LogInformation("Message sent to Service Bus queue");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"ERROR : JsonConvert.DeserializeObject<UserRegistrationRequest> :: {ex.Message}");
                            }

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



