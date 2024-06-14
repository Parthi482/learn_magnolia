using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Sprache;
using DateTime = System.DateTime;
namespace Magnolia_cares.authentication.entities
{
    public class Handler
    {
        public static async Task<dynamic> LoginHandler(loginusers body)
        {
            try
            {
                var query = BuildQuery(body);

                var role_type = body.role_type;
                if (string.IsNullOrEmpty(query))
                {
                    return Results.BadRequest(new { status = 400, message = "Invalid role type.", error_msg = "" });
                }

                var users = await Magnolia_cares.helper_service.QueryMethods.ExecuteQueryAsync(query);
                var token = utils.JwtTokenGenerator.GenerateToken(users, role_type);

                if (users == null || users.Count == 0)
                {
                    return Results.BadRequest(new { status = 404, message = "Invalid Email or Password", error_msg = "" });
                }

                foreach (var user in users)
                {
                    if (user.known_as_language != null)
                    {
                        user.known_as_language = JsonConvert.DeserializeObject<List<int>>(user.known_as_language);
                    }
                    if (user.specialization_id != null)
                    {
                        user.specialization_id = JsonConvert.DeserializeObject<List<int>>(user.specialization_id);
                    }
                }

                return new { success = true, message = "Login successfully", data = users, token = token };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Results.BadRequest(new { status = 500, message = "An error occurred while processing your request.", error_msg = ex.Message });
            }
        }

        private static string BuildQuery(loginusers body)
        {
            return body.role_type switch
            {
                "BOU" => $@"
                SELECT id, first_name, last_name, email, phone, designation, address1, address2 ,is_active
                FROM system_users 
                WHERE email = '{body.email}' AND password = '{body.password}' AND user_role = '{body.role_type}'",

                "Pro" => $@" 
                SELECT id, first_name, last_name, email, phone ,profession_type,specialization_id,
                roster_bg_color,roster_text_color,known_as_language,is_active
                FROM healthcare_professionals 
                WHERE email = '{body.email}' AND password = '{body.password}' ",


                "org" => $@" 
                 SELECT id, first_name, last_name, email, phone ,profession_type,specialization_id,
                roster_bg_color,roster_text_color,known_as_language,is_active
                FROM healthcare_professionals 
                WHERE email = '{body.email}' AND password = '{body.password}' ",

                "super-admin" => $@" 
                SELECT id, first_name, last_name, email, phone, designation,  address1, address2 ,is_active
                FROM system_users 
                WHERE email = '{body.email}' AND password = '{body.password}' AND user_role = '{body.role_type}' ",
                _ => string.Empty
            };
        }


        public static async Task<IResult> EmpMobileLoginHandler([FromBody] EmpMobileloginusers body)
        {
            try
            {
                var query = $@" 
                SELECT employees.id,
                employees.first_name ,
                employees.logo_path as profile_pic, 
                employees.last_name,
                employees.email,
                employees.phone,
                employees.status,
                organizations.org_name AS organization_name,
                organizations.logo_path AS organization_logo ,
                organizations.id AS organization_id
                FROM employees
                inner JOIN organizations ON employees.org_id = organizations.id
                where phone = @phone";

                var parameters = new { phone = body.phone };
                var user = await Magnolia_cares.helper_service.QueryMethods.ExecuteQueryAsync(query, parameters);
                foreach (var record in user)
                {
                    if (record?.status != "approved")
                    {
                        var Errmessage = new
                        {
                            status = 404,
                            message = "Waiting for approval contact admin",
                            error_msg = ""
                        };

                        return Results.BadRequest(Errmessage);
                    }
                }

                if (user?.Count == 0)
                {
                    var Errmessage = new
                    {
                        status = 404,
                        message = "Invalid MobileNumber",
                        error_msg = ""
                    };
                    return Results.BadRequest(Errmessage);
                }

                var tk = utils.JwtTokenGenerator.GenerateToken(user, "user");
                var message = new
                {
                    status = 200,
                    message = "Login successful",
                    data = user,
                    token = tk,
                    error_msg = ""
                };

                return Results.Ok(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                var errorResponse = new
                {
                    status = 500,
                    message = "An error occurred while processing your request.",
                    error_msg = ex.Message
                };
                return Results.BadRequest(errorResponse);
            }
        }


        public static async Task<IResult> GenerateOTP(string mobileNumber)
        {
            string query = @"
            SELECT id, first_name, last_name, email, phone, status
            FROM employees
            WHERE phone = @Phone";

            var parameters = new { phone = mobileNumber };

            var user = await Magnolia_cares.helper_service.QueryMethods.ExecuteQueryAsync(query, parameters);
            if (user == null || user.Count == 0)
            {
                var errMessage = new
                {
                    status = 404,
                    message = "Invalid Mobile Number",
                    error_msg = ""
                };

                return Results.NotFound(errMessage);
            }

            foreach (var record in user)
            {
                if (record.status != "approved")
                {
                    var errMessage = new
                    {
                        status = 403,
                        message = "Waiting for approval, contact admin",
                        error_msg = ""
                    };
                    return Results.Problem(detail: errMessage.message, statusCode: StatusCodes.Status403Forbidden);

                }
            }
            // var otp =  GenerateRandomOTP() ; 5 digit
            var otp = 12345;
            var otpgen = System.DateTime.Now;

            var updateQuery = @"
                UPDATE employees
                SET otp = @Otp, otp_gen_date = @OtpGenDate
                WHERE phone = @Phone";

            var updateparameters = new { Otp = otp, OtpGenDate = otpgen, Phone = mobileNumber };

            await Magnolia_cares.helper_service.QueryMethods.ExecuteQueryAsync(updateQuery, updateparameters);

            var successMessage = new
            {
                status = 200,
                message = "OTP Generation Successful",
                data = new { otp = otp },
                error_msg = ""
            };

            return Results.Ok(successMessage);

        }

        public static async Task<IResult> VerifyOTP(int otp, string mobileNumber)
        {
            if (otp == 12345)
            {
                var message = new
                {
                    status = 200,
                    message = "Otp Verified",

                    error_msg = ""
                };
                return Results.Ok(message);

            }
            else
            {
                var message = new
                {
                    status = 404,
                    message = "Invalid OTP",

                    error_msg = ""
                };

                return Results.Ok(message);

            }

        }

        private static int GenerateRandomOTP()
        {
            Random random = new Random();
            return random.Next(10000, 99999);
        }

    }
}
