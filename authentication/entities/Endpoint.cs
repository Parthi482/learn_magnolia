namespace Magnolia_cares.authentication.entities
{
    public static class Endpoint
    {

        public static async void SetupAuthRoutes(IEndpointRouteBuilder app)
        {
            SetupLoginRoutes(app);
        }

        public static void SetupLoginRoutes(IEndpointRouteBuilder app)
        {


            var auth = app.MapGroup("/auth/");

            auth.MapPost("/login", async (loginusers body) =>
            {
                var result = await Handler.LoginHandler(body);

                return result;
            });

            auth.MapPost("/login/employee", async (EmpMobileloginusers body) =>
                    {
                        var result = await Handler.EmpMobileLoginHandler(body);

                        return result;
                    });


            auth.MapGet("/generate/otp", async (string mobileNumber) =>
            {
                var result = await Handler.GenerateOTP(mobileNumber);
                return result;
            });

            auth.MapPost("/verify/otp", async (int otp, string mobileNumber) =>
             {
                 var result = await Handler.VerifyOTP(otp, mobileNumber);
                 return result;
             });
        }
    }

}
