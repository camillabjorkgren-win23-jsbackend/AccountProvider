using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class Verify(ILogger<Verify> logger, UserManager<UserAccount> userManager)
{
    private readonly ILogger<Verify> _logger = logger;
    private readonly UserManager<UserAccount> _userManager = userManager;

    [Function("Verify")]
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
            VerificationRequest vr = null!;

            try
            {
                vr = JsonConvert.DeserializeObject<VerificationRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : JsonConvert.DeserializeObject<VerificationRequest> :: {ex.Message}");
            }

            if (vr != null && !string.IsNullOrEmpty(vr.Email) && !string.IsNullOrEmpty(vr.VerificationCode))
            {
                try
                {

                    //SKICKA IN B�DE KOD OCH EMAIL?

                    //using var http = new HttpClient();
                    //StringContent content = new StringContent(JsonConvert.SerializeObject(vr), Encoding.UTF8, "application/json");
                    //var response = await http.PostAsync("https://verificationprovider-silicon-camilla.azurewebsites.net/api/validate?code=y88D2-qlrXm5-KuFGf9kYo3-RgqiUN23aMZfkGM_ovOmAzFuI9m1Bg==", content);
                   
                    if (true)
                    {

                        var userAccount = await _userManager.FindByEmailAsync(vr.Email);
                        if (userAccount != null)
                        {
                            userAccount.EmailConfirmed = true;
                            await _userManager.UpdateAsync(userAccount);

                            if (await _userManager.IsEmailConfirmedAsync(userAccount))
                            {
                                return new OkResult();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR :  http.PostAsync :: {ex.Message}");
                }

            }
        }
        return new UnauthorizedResult();
    }

}
