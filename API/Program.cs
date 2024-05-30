using API.Services.Auth;
using API.Services.Booking;
using API.Services.Package;
using API.Services.User;
using Data.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        var configuration = builder.Configuration;

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                              builder =>
                              {
                                  builder.WithOrigins("https://localhost:7058"
                                    )
                                  .AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                              });
        });

        

        //builder.Services.AddControllers();
        builder.Services.AddControllersWithViews()
                          .AddJsonOptions(options =>
                          {
                              options.JsonSerializerOptions.PropertyNamingPolicy = null;
                          });

        // Add JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["secret"]);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        //DMAExamNewDB
        var contextOptions = new DbContextOptionsBuilder<AppDBContext>()
                     .UseSqlServer("Data Source=DESKTOP-4H1LAGN\\SQLEXPRESS;Initial Catalog=BookingSystem;User ID=admin1;Password=123123;MultipleActiveResultSets=True;TrustServerCertificate=True")
                     .Options;

        builder.Services.AddScoped(s => new AppDBContext(contextOptions));


        //other servies

        builder.Services.AddScoped<IUser>(s => new UserBase(
           s.GetService<AppDBContext>()
           ));

        builder.Services.AddScoped<IPackage>(s => new PackageBase(
           s.GetService<AppDBContext>()
           ));

        builder.Services.AddScoped<IBooking>(s => new BookingBase(
           s.GetService<AppDBContext>()
           ));

        builder.Services.AddScoped<IAuth>(s => new AuthBase(
          s.GetService<AppDBContext>(), configuration
          ));



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