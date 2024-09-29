
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using System.Text;
using UserManageService.Model;
using WebApiIdentity_security.Model.DB;
using UserManageService.Service;
using Microsoft.OpenApi.Models;



namespace WebApiIdentity_security
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // connection db 
            builder.Services.AddDbContext<ApplicationDbConnection>(x=>
            x.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

            // for identity 
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbConnection>()
                .AddDefaultTokenProviders();
            // email conformation disk
            builder.Services.Configure<IdentityOptions>(opt=>opt.SignIn.RequireConfirmedEmail=true);

            // Add Email Configration 
            builder.Services.Configure<EmailConfigration>(builder.Configuration.GetSection("EmailConfig"));
            builder.Services.AddTransient<UserManageService.Service.IEmailService, UserManageService.Service.EmailService>();

            // reset password config
            builder.Services.Configure<DataProtectionTokenProviderOptions>(option => option.TokenLifespan = TimeSpan.FromMinutes(10));

            // Adding athentication
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;



            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

           




            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            // swagger token info
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1",new OpenApiInfo { Title="Auth api",Version="v1"});
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In=ParameterLocation.Header,
                    Description="Please enter a valid token",
                    Name="Authorization",
                    Type=SecuritySchemeType.Http,
                    BearerFormat="JWT",
                    Scheme="Bearer"

                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"

                            }
                        },
                        new string[]{}
                    }
                });

            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();



            app.MapControllers();

            app.Run();
        }
    }
}
