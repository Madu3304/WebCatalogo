using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebCatalogo.Context;
using WebCatalogo.Models;
using WebCatalogo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => //Esse trech é para ter o suporte. 
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ApiCatalogo",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>() // Lista vazia de escopos
        }
    });
});



//Ligar com o banco(com o configuração que deve estar na classe "appsettings.json".
var connectionString = builder.Configuration.GetConnectionString("DefautConnecton");
builder.Services.AddDbContext<AppDbConstext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



//################# SERVIÇO - TOKEN #################

builder.Services.AddDbContext<AppDbConstext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<ITokenServices>(new TokenService());


//################# VAIDAÇÃO DO TOKEN #################

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            //aqui apenas uma chave gerada 
        };
    });

//Serviço de autorização 
builder.Services.AddAuthorization();


//#################
var app = builder.Build();

//ENDPOINT PARA LOGIN

app.MapPost("/login", [AllowAnonymous] (UserModels userModels, ITokenServices tokenService) =>
{

    if (userModels == null) { return Results.BadRequest("Login Invalido"); }

    if (userModels.UserName == "macoratti" && userModels.Password == "numsey#123")
    {
        var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"],
                                                  app.Configuration["Jwt:Issuer"],
                                                  app.Configuration["Jwt:Audience"],
                                                  userModels);

        return Results.Ok(new { token = tokenString });
    }

    else
    {
        return Results.BadRequest("Login Invaido");
    }
}).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status200OK).WithName("Login").WithTags("Autenticacao");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//###################### Categoria ###################### 
app.MapGet("/", () => "Catálogo de Produtos - 2024").ExcludeFromDescription();

app.MapPost("/categoris", async (Categoria categoria, AppDbConstext db) =>
{
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.Id_categoria}", categoria);
});


//###################### PRODUTO ###################### 

app.MapGet("/produtos", async (AppDbConstext db) =>
    await db.Produtos.ToListAsync()).RequireAuthorization();
//&&& RequireAuthorization(); significa que esse endpoint é protegido. &&&


app.MapGet("/produtos/{id:int}", async (int id, AppDbConstext db) => { return await db.Produtos.FindAsync(id) is Produto produto ? Results.Ok(produto) : Results.NotFound(); });

app.MapPost("/produtos", async (Produto produto, AppDbConstext db) =>
    {
        db.Produtos.Add(produto); await db.SaveChangesAsync();
        return Results.Created($"/produtos/{produto.Id_produto}", produto);
    });


app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbConstext db) =>
    {
        if (produto.Id_produto != id) { return Results.NotFound(); }

        var produtosDB = await db.Produtos.FindAsync(id);

        if (produtosDB is null) return Results.NotFound();

        produtosDB.Nome = produto.Nome;
        produtosDB.Descricao = produto.Descricao;
        produtosDB.Precco = produto.Precco;
        produtosDB.Estoque = produto.Estoque;
        produtosDB.Id_categoria = produto.Id_categoria;

        await db.SaveChangesAsync();
        return Results.Ok(produtosDB);
    });


app.MapDelete("/produtos/{id:int}", async (int id, AppDbConstext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto is null)
    {
        return Results.NotFound();
    }

    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


// Configurar middlewares
app.UseAuthentication();
app.UseAuthorization();
app.Run();

