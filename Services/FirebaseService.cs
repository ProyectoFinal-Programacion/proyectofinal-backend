using FirebaseAdmin.Auth;

namespace ManoVecinaAPI.Services;

public class FirebaseService
{
    public async Task<string> CreateCustomTokenAsync(string uid)
    {
        var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
        return token;
    }
}
