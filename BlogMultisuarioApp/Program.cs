using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BlogMultisuarioApp.Repositories;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios de autenticaci�n
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_vtFErItEI";
    options.ClientId = "6nej8qanjv13q0kobf3glnr9d2";
    options.ClientSecret = "f7bd047tfahdvpufjq72hsu3eqov57hqr9pq73295vtnehia9gm";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.MapInboundClaims = false;
    options.Scope.Add("openid");   // Alcance obligatorio
    options.Scope.Add("email");    // Alcance para correo electr�nico
    options.Scope.Add("phone");    // Alcance para n�mero de tel�fono
    options.Scope.Add("profile");  // Alcance para informaci�n del perfil

    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";  // Esta es la URL a la que Cognito redirigir� despu�s de logout
                                                               //options.PostLogoutRedirectUri = "https://localhost:7084/Home/Index"; // Ajusta esta URL seg�n tu aplicaci�n
    options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddScoped<ArticuloRepository>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
